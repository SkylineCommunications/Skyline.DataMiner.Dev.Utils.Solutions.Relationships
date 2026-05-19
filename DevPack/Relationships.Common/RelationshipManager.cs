namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Solutions.Relationships.Middleware;
	using Skyline.DataMiner.Solutions.Relationships.Repositories;

	/// <inheritdoc/>
	public class RelationshipManager : IRelationshipManager
	{
		private bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelationshipManager"/> class.
		/// </summary>
		/// <param name="connection">The connection to use for relation storage operations.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> is <c>null</c>.</exception>
		internal RelationshipManager(IConnection connection)
		{
			var entities = new EntityDescriptorDomRepository(connection);
			var relations = new RelationDomRepository(connection);

			Entities = (entities as IEntityDescriptorRepository)
				.WithMiddleware(new EntityDeletionMiddleware(relations))
				.WithMiddleware(new EntityStateMiddleware(relations, entities))
				.WithMiddleware(new EntityValidationMiddleware());
			Relations = (relations as IRelationRepository)
				.WithMiddleware(new RelationEntityExistenceMiddleware(Entities))
				.WithMiddleware(new RelationValidationMiddleware());
		}

		/// <inheritdoc/>
		public IEntityDescriptorRepository Entities { get; }

		/// <inheritdoc/>
		public IRelationRepository Relations { get; }

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="RelationshipManager"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				(Entities as IDisposable)?.Dispose();
				(Relations as IDisposable)?.Dispose();
			}

			disposed = true;
		}
	}
}
