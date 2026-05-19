namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.Relationships.Query;

	/// <summary>
	/// Extension methods on <see cref="IRelationshipManager"/> for creating, removing,
	/// and traversing relations in a user-friendly way.
	/// </summary>
	public static class RelationshipManagerExtensions
	{
		/// <summary>
		/// Starts a relationship graph traversal from <paramref name="entity"/>.
		/// </summary>
		/// <param name="manager">The relationship manager.</param>
		/// <param name="entity">The entity to start from.</param>
		/// <returns>
		/// An <see cref="EntityTraversal"/> that can be further configured and executed.
		/// </returns>
		/// <example>
		/// <code>
		/// using var manager = connection.GetRelationshipManager();
		///
		/// // Direct neighbors
		/// var neighbors = manager.From(myElement).GetNeighbors();
		///
		/// // All Service entities reachable within 3 hops (post-traversal LINQ filter)
		/// var services = manager.From(myElement)
		///                       .Traverse(maxDepth: 3)
		///                       .Where(r => r.Entity.ModelName == "Service");
		/// </code>
		/// </example>
		public static EntityTraversal From(this IRelationshipManager manager, EntityDescriptor entity)
		{
			if (manager is null)
				throw new ArgumentNullException(nameof(manager));
			if (entity is null)
				throw new ArgumentNullException(nameof(entity));

			return new EntityTraversal(manager.Relations, manager.Entities, entity);
		}

		/// <summary>
		/// Starts a relationship graph traversal from the entity identified by <paramref name="key"/>.
		/// </summary>
		/// <param name="manager">The relationship manager.</param>
		/// <param name="key">The unique key of the entity to start from.</param>
		/// <returns>
		/// An <see cref="EntityTraversal"/> that can be further configured and executed.
		/// </returns>
		/// <exception cref="KeyNotFoundException">
		/// Thrown when no entity matching <paramref name="key"/> is found in the store.
		/// </exception>
		/// <example>
		/// <code>
		/// using var manager = connection.GetRelationshipManager();
		///
		/// var services = manager.From(myElementKey)
		///                       .Traverse(maxDepth: 3);
		/// </code>
		/// </example>
		public static EntityTraversal From(this IRelationshipManager manager, EntityKey key)
		{
			if (manager is null)
				throw new ArgumentNullException(nameof(manager));
			if (key is null)
				throw new ArgumentNullException(nameof(key));

			var filter = new ANDFilterElement<EntityDescriptor>(
				EntityDescriptorExposers.SolutionID.Equal(key.SolutionID),
				EntityDescriptorExposers.ModelName.Equal(key.ModelName),
				EntityDescriptorExposers.ID.Equal(key.ID));

			var entity = manager.Entities.Read(filter).FirstOrDefault();
			if (entity is null)
				throw new KeyNotFoundException($"No entity found for key: {key}");

			return new EntityTraversal(manager.Relations, manager.Entities, entity);
		}

		/// <summary>
		/// Creates a bidirectional (symmetric) link between <paramref name="a"/> and <paramref name="b"/>
		/// by registering two complementary relations: <c>a → b</c> and <c>b → a</c>.
		/// </summary>
		/// <param name="manager">The relationship manager.</param>
		/// <param name="a">One end of the link.</param>
		/// <param name="b">The other end of the link.</param>
		/// <remarks>
		/// If either entity does not yet exist in the store it is created automatically.
		/// If it already exists it is updated to reflect the current descriptor state.
		/// </remarks>
		public static void Link(this IRelationshipManager manager, EntityDescriptor a, EntityDescriptor b)
		{
			if (manager is null)
				throw new ArgumentNullException(nameof(manager));
			if (a is null)
				throw new ArgumentNullException(nameof(a));
			if (b is null)
				throw new ArgumentNullException(nameof(b));

			manager.Entities.CreateOrUpdate([a, b]);

			manager.Relations.Create(
			[
				Relation.Between(a, b),
				Relation.Between(b, a),
			]);
		}

		/// <summary>
		/// Creates a single directed relation from <paramref name="source"/> to <paramref name="target"/>.
		/// </summary>
		/// <param name="manager">The relationship manager.</param>
		/// <param name="source">The source entity.</param>
		/// <param name="target">The target entity.</param>
		/// <remarks>
		/// If either entity does not yet exist in the store it is created automatically.
		/// If it already exists it is updated to reflect the current descriptor state.
		/// </remarks>
		public static void Relate(this IRelationshipManager manager, EntityDescriptor source, EntityDescriptor target)
		{
			if (manager is null)
				throw new ArgumentNullException(nameof(manager));
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			manager.Entities.CreateOrUpdate([source, target]);

			manager.Relations.Create(Relation.Between(source, target));
		}

		/// <summary>
		/// Removes the single directed relation from <paramref name="source"/> to <paramref name="target"/>
		/// (<c>source → target</c> only). The reverse relation, if any, is left intact.
		/// </summary>
		/// <param name="manager">The relationship manager.</param>
		/// <param name="source">The source entity.</param>
		/// <param name="target">The target entity.</param>
		public static void Unrelate(this IRelationshipManager manager, EntityDescriptor source, EntityDescriptor target)
		{
			if (manager is null)
				throw new ArgumentNullException(nameof(manager));
			if (source is null)
				throw new ArgumentNullException(nameof(source));
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			var filter = new ANDFilterElement<Relation>(
				RelationExposers.SourceKey.Equal(source.Key),
				RelationExposers.TargetKey.Equal(target.Key));

			var relations = manager.Relations.Read(filter).ToList();
			if (relations.Count > 0)
				manager.Relations.Delete(relations);
		}

		/// <summary>
		/// Removes all relations between <paramref name="a"/> and <paramref name="b"/> in either direction
		/// (<c>a → b</c> and <c>b → a</c>).
		/// </summary>
		/// <param name="manager">The relationship manager.</param>
		/// <param name="a">One end of the link.</param>
		/// <param name="b">The other end of the link.</param>
		public static void Unlink(this IRelationshipManager manager, EntityDescriptor a, EntityDescriptor b)
		{
			if (manager is null)
				throw new ArgumentNullException(nameof(manager));
			if (a is null)
				throw new ArgumentNullException(nameof(a));
			if (b is null)
				throw new ArgumentNullException(nameof(b));

			var filter = new ORFilterElement<Relation>(
				new ANDFilterElement<Relation>(
					RelationExposers.SourceKey.Equal(a.Key),
					RelationExposers.TargetKey.Equal(b.Key)),
				new ANDFilterElement<Relation>(
					RelationExposers.SourceKey.Equal(b.Key),
					RelationExposers.TargetKey.Equal(a.Key)));

			var relations = manager.Relations.Read(filter).ToList();
			if (relations.Count > 0)
				manager.Relations.Delete(relations);
		}
	}
}
