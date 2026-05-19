namespace Skyline.DataMiner.Solutions.Relationships.Query
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.Relationships.Repositories;

	/// <summary>
	/// Provides a fluent API for exploring the relationship graph from a starting entity.
	/// </summary>
	/// <remarks>
	/// Obtain an instance via <see cref="RelationshipManagerExtensions.From"/>.
	///
	/// <para>Traversal is breadth-first and follows edges in both directions
	/// (a relation A→B is traversable from both A and B).</para>
	///
	/// <para>Example usage:</para>
	/// <code>
	/// using var manager = connection.GetRelationshipManager();
	///
	/// // All entities directly related to myElement
	/// IEnumerable&lt;EntityDescriptor&gt; neighbors = manager.From(myElement).GetNeighbors();
	///
	/// // All Service entities reachable within 3 hops (post-traversal LINQ filter)
	/// IEnumerable&lt;TraversalResult&gt; services = manager
	///     .From(myElement)
	///     .Traverse(maxDepth: 3)
	///     .Where(r =&gt; r.Entity.ModelName == "Service");
	/// </code>
	/// </remarks>
	public sealed class EntityTraversal
	{
		private readonly IRelationRepository _relations;
		private readonly IEntityDescriptorRepository _entities;
		private readonly EntityDescriptor _start;
		private FilterElement<EntityDescriptor> _entityFilter;

		internal EntityTraversal(
			IRelationRepository relations,
			IEntityDescriptorRepository entities,
			EntityDescriptor start)
		{
			_relations = relations ?? throw new ArgumentNullException(nameof(relations));
			_entities = entities ?? throw new ArgumentNullException(nameof(entities));
			_start = start ?? throw new ArgumentNullException(nameof(start));
		}

		/// <summary>
		/// Restricts which entities are fetched and returned during traversal using a DOM filter.
		/// </summary>
		/// <param name="filter">A <see cref="FilterElement{T}"/> applied when querying entities from storage.</param>
		/// <returns>The same <see cref="EntityTraversal"/> instance for chaining.</returns>
		/// <remarks>
		/// <para>
		/// The filter is pushed down to the DOM query, so only matching entities are fetched.
		/// Entities that do not match will not be yielded and will not be traversed further.
		/// </para>
		/// <para>Calling <see cref="Where"/> multiple times ANDs the filters together.</para>
		/// <para>Example — only return active entities:</para>
		/// <code>
		/// manager.From(myElement)
		///        .Where(EntityDescriptorExposers.Status.Equal(EntityStatus.Active))
		///        .Traverse();
		/// </code>
		/// <para>Example — combine with post-traversal LINQ filtering:</para>
		/// <code>
		/// manager.From(myElement)
		///        .Where(EntityDescriptorExposers.Status.Equal(EntityStatus.Active))
		///        .Traverse(maxDepth: 3)
		///        .Where(r => r.Entity.ModelName == "Service");
		/// </code>
		/// </remarks>
		public EntityTraversal Where(FilterElement<EntityDescriptor> filter)
		{
			if (filter is null)
				throw new ArgumentNullException(nameof(filter));

			_entityFilter = _entityFilter == null
				? filter
				: new ANDFilterElement<EntityDescriptor>(_entityFilter, filter);

			return this;
		}

		/// <summary>
		/// Returns the entities directly related to the starting entity (one hop).
		/// </summary>
		/// <returns>
		/// The <see cref="EntityDescriptor"/> of each immediate neighbor.
		/// Any <see cref="Where"/> filters are applied.
		/// </returns>
		public IEnumerable<EntityDescriptor> GetNeighbors()
		{
			return Traverse(maxDepth: 1).Select(r => r.Entity);
		}

		/// <summary>
		/// Returns all relations in which the starting entity appears as either source or target (one hop).
		/// </summary>
		/// <returns>
		/// Every <see cref="Relation"/> where <see cref="Relation.SourceKey"/> or
		/// <see cref="Relation.TargetKey"/> matches the starting entity.
		/// </returns>
		public IEnumerable<Relation> GetRelations()
		{
			return _relations.Read(BuildRelationFilter(_start.Key));
		}

		/// <summary>
		/// Performs a breadth-first traversal of the relationship graph.
		/// </summary>
		/// <param name="maxDepth">
		/// Maximum number of hops from the starting entity. Defaults to <c>1</c> (direct neighbors only).
		/// Increase explicitly for deeper traversals — large graphs can trigger many DOM reads per hop.
		/// </param>
		/// <returns>
		/// Each <see cref="TraversalResult"/> contains the discovered entity, its depth,
		/// the entity it was reached through, and all relations that connect them.
		/// When a symmetric link exists (e.g. A→B and B→A) <see cref="TraversalResult.Relations"/> contains both edges.
		/// </returns>
		/// <remarks>
		/// Reads are batched per depth level: one relation query and one entity query per hop,
		/// regardless of how many nodes are in the frontier at that level.
		/// </remarks>
		public IEnumerable<TraversalResult> Traverse(int maxDepth = 1)
		{
			if (maxDepth <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxDepth), "maxDepth must be greater than zero.");

			// visited prevents re-processing the same entity through a different path or cycle.
			// The start entity is pre-marked so it is never returned as its own neighbor.
			var visited = new HashSet<EntityKey> { _start.Key };

			// The frontier holds all entities at the current depth level.
			// Each iteration advances one hop, replacing the frontier with the next level.
			var frontier = new List<EntityDescriptor> { _start };

			for (int depth = 0; depth < maxDepth && frontier.Count > 0; depth++)
			{
				// Single bulk query: fetch all relations touching any entity in the current frontier.
				var allRelations = BulkReadRelations(frontier);

				// Identify every unvisited neighbor reachable from this frontier level.
				// Neighbors are marked visited here to deduplicate across the same frontier level.
				var pending = CollectPendingNeighbors(frontier, allRelations, visited);

				// No new neighbors means the graph is fully explored — stop early.
				if (pending.Count == 0)
					break;

				// Single bulk query: resolve all neighbor keys to their full entity descriptors.
				// Keys not found in the store (orphaned relations) are silently skipped.
				var resolved = LookupEntities(pending.Select(p => p.NeighborKey));

				// Build the next frontier from the resolved neighbors and yield matching results.
				frontier = ResolveNextFrontier(pending, resolved, depth, out var results);

				foreach (var result in results)
					yield return result;
			}
		}

		private List<(EntityDescriptor Via, EntityKey NeighborKey, List<Relation> Relations)> CollectPendingNeighbors(
				List<EntityDescriptor> frontier,
				List<Relation> allRelations,
				HashSet<EntityKey> visited)
			{
				// Keyed by NeighborKey so that multiple edges to the same neighbor (e.g. A→B and B→A)
				// are merged into a single pending entry with all relations accumulated.
				var pendingMap = new Dictionary<EntityKey, (EntityDescriptor Via, List<Relation> Relations)>();

				foreach (var entity in frontier)
				{
					// Filter the bulk-read relations down to only those involving this specific entity.
					var entityRelations = allRelations.Where(r => r.SourceKey == entity.Key || r.TargetKey == entity.Key);

					foreach (var (neighborKey, relation) in GetNeighborPairs(entity.Key, entityRelations))
					{
						if (pendingMap.TryGetValue(neighborKey, out var existing))
							{
								// Same neighbor already queued at this frontier level via a different edge
								// (e.g. the back-edge of a symmetric link A→B / B→A): accumulate the
								// extra relation without creating a duplicate frontier entry.
								existing.Relations.Add(relation);
							}
							else if (!visited.Contains(neighborKey))
							{
								// First time we see this neighbor: mark visited and queue it.
								visited.Add(neighborKey);
								pendingMap[neighborKey] = (entity, new List<Relation> { relation });
							}
					}
				}

				return pendingMap.Select(kv => (kv.Value.Via, kv.Key, kv.Value.Relations)).ToList();
			}

		private List<EntityDescriptor> ResolveNextFrontier(
				List<(EntityDescriptor Via, EntityKey NeighborKey, List<Relation> Relations)> pending,
				IReadOnlyDictionary<EntityKey, EntityDescriptor> resolved,
				int depth,
				out List<TraversalResult> results)
			{
				results = new List<TraversalResult>();
				var nextFrontier = new List<EntityDescriptor>();

				foreach (var (via, neighborKey, relations) in pending)
				{
					// Skip neighbors whose entity record could not be found (orphaned relation).
						if (!resolved.TryGetValue(neighborKey, out var neighbor))
							continue;

						nextFrontier.Add(neighbor);
						results.Add(new TraversalResult(neighbor, depth + 1, via, relations));
				}

				return nextFrontier;
			}

		private List<Relation> BulkReadRelations(IReadOnlyList<EntityDescriptor> entities)
		{
			if (entities.Count == 0)
				return new List<Relation>();

			var perKeyFilters = entities.Select(e => BuildRelationFilter(e.Key)).ToArray();
			var filter = perKeyFilters.Length == 1
				? perKeyFilters[0]
				: new ORFilterElement<Relation>(perKeyFilters);

			return _relations.Read(filter).ToList();
		}

		private static IEnumerable<(EntityKey NeighborKey, Relation Relation)> GetNeighborPairs(
			EntityKey fromKey,
			IEnumerable<Relation> relations)
		{
			foreach (var relation in relations)
			{
				if (relation.SourceKey == fromKey && relation.TargetKey != fromKey)
					yield return (relation.TargetKey, relation);
				else if (relation.TargetKey == fromKey && relation.SourceKey != fromKey)
					yield return (relation.SourceKey, relation);
			}
		}

		private static FilterElement<Relation> BuildRelationFilter(EntityKey key)
		{
			return new ORFilterElement<Relation>(
				RelationExposers.SourceKey.Equal(key),
				RelationExposers.TargetKey.Equal(key));
		}

		private IReadOnlyDictionary<EntityKey, EntityDescriptor> LookupEntities(IEnumerable<EntityKey> keys)
		{
			var keyList = keys.ToList();

			if (keyList.Count == 0)
				return new Dictionary<EntityKey, EntityDescriptor>();

			var keyFilters = keyList.Select(BuildEntityKeyFilter).ToArray();
			FilterElement<EntityDescriptor> filter = keyFilters.Length == 1
				? keyFilters[0]
				: new ORFilterElement<EntityDescriptor>(keyFilters);

			if (_entityFilter != null)
				filter = new ANDFilterElement<EntityDescriptor>(filter, _entityFilter);

			return _entities.Read(filter).ToDictionary(e => e.Key);
		}

		private static FilterElement<EntityDescriptor> BuildEntityKeyFilter(EntityKey key)
		{
			return new ANDFilterElement<EntityDescriptor>(
			[
				EntityDescriptorExposers.SolutionID.Equal(key.SolutionID),
				EntityDescriptorExposers.ModelName.Equal(key.ModelName),
				EntityDescriptorExposers.ID.Equal(key.ID),
			]);
		}
	}
}
