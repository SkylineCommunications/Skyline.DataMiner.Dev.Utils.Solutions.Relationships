namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;

	using Skyline.DataMiner.SDM;

	/// <summary>
	/// Represents a directed relation from a source entity to a target entity.
	/// </summary>
	/// <remarks>
	/// A single relation is directed: Source → Target.
	/// For a bidirectional (symmetric) link, create two complementary relations using
	/// <see cref="RelationshipManagerExtensions.Link"/>, or call
	/// <see cref="Between"/> twice in opposite directions.
	/// </remarks>
	[GenerateExposers]
	////[SdmDomStorage("(sdm)relationships")]
	public sealed class Relation : SdmObject<Relation>
	{
		/// <inheritdoc/>
		public override string Identifier { get; set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// Gets the key identifying the source entity of this relation.
		/// </summary>
		[SdmIgnore]
		public EntityKey SourceKey { get; internal set; }

		/// <summary>
		/// Gets the current status of the source entity.
		/// </summary>
		public EntityStatus SourceState { get; internal set; }

		/// <summary>
		/// Gets the key identifying the target entity of this relation.
		/// </summary>
		[SdmIgnore]
		public EntityKey TargetKey { get; internal set; }

		/// <summary>
		/// Gets the current status of the target entity.
		/// </summary>
		public EntityStatus TargetState { get; internal set; }

		/// <summary>
		/// Creates a directed relation from <paramref name="source"/> to <paramref name="target"/>.
		/// </summary>
		/// <param name="source">The source entity.</param>
		/// <param name="target">The target entity.</param>
		/// <returns>A new <see cref="Relation"/> pointing from source to target.</returns>
		/// <remarks>
		/// To create a bidirectional link, use <see cref="RelationshipManagerExtensions.Link"/>
		/// or call <see cref="Between"/> in both directions.
		/// </remarks>
		public static Relation Between(EntityDescriptor source, EntityDescriptor target)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (target == null)
				throw new ArgumentNullException(nameof(target));

			return new Relation
			{
				SourceKey = source.Key,
				SourceState = source.Status,
				TargetKey = target.Key,
				TargetState = target.Status,
			};
		}
	}
}