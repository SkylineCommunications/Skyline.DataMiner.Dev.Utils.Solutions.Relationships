namespace Skyline.DataMiner.Solutions.Relationships.Query
{
	using System.Collections.Generic;

	/// <summary>
	/// Represents an entity encountered while traversing the relationship graph.
	/// </summary>
	public sealed class TraversalResult
	{
		internal TraversalResult(EntityDescriptor entity, int depth, EntityDescriptor via, IReadOnlyList<Relation> relations)
		{
			Entity = entity;
			Depth = depth;
			Via = via;
			Relations = relations;
		}

		/// <summary>
		/// Gets the entity discovered during traversal.
		/// </summary>
		public EntityDescriptor Entity { get; }

		/// <summary>
		/// Gets the number of hops from the starting entity to <see cref="Entity"/>.
		/// </summary>
		public int Depth { get; }

		/// <summary>
		/// Gets the entity from which <see cref="Entity"/> was reached.
		/// </summary>
		public EntityDescriptor Via { get; }

		/// <summary>
		/// Gets all relations that connect <see cref="Via"/> to <see cref="Entity"/>.
		/// Contains more than one entry when a symmetric link exists (e.g. A→B and B→A),
		/// so all edges between the two entities are visible to the caller.
		/// </summary>
		public IReadOnlyList<Relation> Relations { get; }
	}
}
