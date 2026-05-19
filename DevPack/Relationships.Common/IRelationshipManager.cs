namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Solutions.Relationships.Repositories;

	/// <summary>
	/// Provides functionality to create and query relations between entities.
	/// </summary>
	public interface IRelationshipManager : IDisposable
	{
		/// <summary>
		/// Gets the repository for <see cref="Relation"/> objects.
		/// </summary>
		/// <value>An <see cref="IRelationRepository"/> instance used for operations on relations.</value>
		IRelationRepository Relations { get; }

		/// <summary>
		/// Gets the repository for <see cref="EntityDescriptor"/> objects.
		/// </summary>
		/// <value>An <see cref="IEntityDescriptorRepository"/> instance used for operations on entity descriptors.</value>
		IEntityDescriptorRepository Entities { get; }
	}
}