namespace Skyline.DataMiner.Solutions.Relationships.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Solutions.Relationships.Repositories;

	internal class EntityDeletionMiddleware : OptionalBulkRepositoryMiddleware<EntityDescriptor>
	{
		private readonly IRelationRepository _relationRepo;

		public EntityDeletionMiddleware(IRelationRepository relationRepo)
		{
			_relationRepo = relationRepo ?? throw new ArgumentNullException(nameof(relationRepo));
		}

		public override void OnDelete(EntityDescriptor oToDelete, Action<EntityDescriptor> next)
		{
			DeleteRelations([oToDelete]);
			next(oToDelete);
		}

		public override void OnDelete(IEnumerable<EntityDescriptor> oToDelete, Action<IEnumerable<EntityDescriptor>> next)
		{
			foreach (var batch in oToDelete.Batch(50))
			{
				DeleteRelations(batch);
			}

			next(oToDelete);
		}

		private void DeleteRelations(IEnumerable<EntityDescriptor> batch)
		{
			var filter = new ORFilterElement<Relation>(
				batch.Select(e => new ORFilterElement<Relation>(
						RelationExposers.SourceKey.Equal(e.Key),
						RelationExposers.TargetKey.Equal(e.Key)))
					.ToArray());

			var relations = _relationRepo.ReadPaged(
				filter,
				100);

			foreach (var page in relations)
			{
				_relationRepo.Delete(page);
			}
		}
	}
}
