namespace Skyline.DataMiner.Solutions.Relationships.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Solutions.Relationships.Caching;
	using Skyline.DataMiner.Solutions.Relationships.Repositories;

	internal class EntityStateMiddleware : OptionalBulkRepositoryMiddleware<EntityDescriptor>
	{
		private readonly IBulkRepository<Relation> _relationRepo;
		private readonly IEntityDescriptorRepository _entityRepo;

		private readonly SdmRepositoryCache<EntityDescriptor> _cache;

		public EntityStateMiddleware(
			IBulkRepository<Relation> relationRepo,
			IEntityDescriptorRepository entityRepo)
		{
			_relationRepo = relationRepo ?? throw new ArgumentNullException(nameof(relationRepo));
			_entityRepo = entityRepo ?? throw new ArgumentNullException(nameof(entityRepo));
			_cache = new SdmRepositoryCache<EntityDescriptor>(entityRepo);
		}

		public override IReadOnlyCollection<EntityDescriptor> OnCreateOrUpdate(IEnumerable<EntityDescriptor> oToCreateOrUpdate, Func<IEnumerable<EntityDescriptor>, IReadOnlyCollection<EntityDescriptor>> next)
		{
			var previous = _cache.Get(oToCreateOrUpdate).ToDictionary(e => e.Identifier);
			var stateChanges = new HashSet<EntityDescriptor>();
			foreach (var entity in oToCreateOrUpdate)
			{
				if (!previous.TryGetValue(entity.Identifier, out var prevItem))
				{
					continue;
				}

				if (entity.Status != prevItem.Status)
				{
					stateChanges.Add(entity);
				}
			}

			if (stateChanges.Count == 0)
			{
				// No state changes
				return next(oToCreateOrUpdate);
			}

			foreach (var batch in stateChanges.Batch(100))
			{
				UpdateRelations(batch, oToCreateOrUpdate.ToDictionary(e => e.Key));
			}

			return next(oToCreateOrUpdate);
		}

		public override EntityDescriptor OnUpdate(EntityDescriptor oToUpdate, Func<EntityDescriptor, EntityDescriptor> next)
		{
			var previous = _cache.Get(oToUpdate.Identifier);
			if (oToUpdate.Status == previous.Status)
			{
				// Status hasn't changed
				return next(oToUpdate);
			}

			UpdateRelations([oToUpdate], new Dictionary<EntityKey, EntityDescriptor>
			{
				[oToUpdate.Key] = oToUpdate,
			});

			return next(oToUpdate);
		}

		public override IReadOnlyCollection<EntityDescriptor> OnUpdate(IEnumerable<EntityDescriptor> oToUpdate, Func<IEnumerable<EntityDescriptor>, IReadOnlyCollection<EntityDescriptor>> next)
		{
			var previous = _cache.Get(oToUpdate).ToDictionary(e => e.Identifier);
			var stateChanges = new HashSet<EntityDescriptor>();
			foreach (var entity in oToUpdate)
			{
				if (entity.Status != previous[entity.Identifier].Status)
				{
					stateChanges.Add(entity);
				}
			}

			if (stateChanges.Count == 0)
			{
				// No state changes
				return next(oToUpdate);
			}

			foreach (var batch in stateChanges.Batch(100))
			{
				UpdateRelations(batch, oToUpdate.ToDictionary(e => e.Key));
			}

			return next(oToUpdate);
		}

		private void UpdateRelations(IEnumerable<EntityDescriptor> batch, IDictionary<EntityKey, EntityDescriptor> entities)
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
				page.ForEach(r => Apply(r, entities));
				_relationRepo.Update(page);
			}
		}

		private static void Apply(Relation relation, IDictionary<EntityKey, EntityDescriptor> entities)
		{
			if (entities.TryGetValue(relation.SourceKey, out var sourceEntity))
			{
				relation.SourceState = sourceEntity.Status;
			}

			if (entities.TryGetValue(relation.TargetKey, out var targetEntity))
			{
				relation.TargetState = targetEntity.Status;
			}
		}
	}
}
