namespace Skyline.DataMiner.Solutions.Relationships.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.Relationships.Exceptions;
	using Skyline.DataMiner.Solutions.Relationships.Repositories;
	using Skyline.DataMiner.Solutions.Relationships.Validation;

	/// <summary>
	/// Middleware that verifies both the source and target <see cref="EntityDescriptor"/> exist
	/// before a <see cref="Relation"/> is created or updated.
	/// </summary>
	internal class RelationEntityExistenceMiddleware : OptionalBulkRepositoryMiddleware<Relation>
	{
		private readonly IEntityDescriptorRepository _entities;

		public RelationEntityExistenceMiddleware(IEntityDescriptorRepository entities)
		{
			_entities = entities ?? throw new ArgumentNullException(nameof(entities));
		}

		public override Relation OnCreate(Relation oToCreate, Func<Relation, Relation> next)
		{
			Validate([oToCreate]);
			return next(oToCreate);
		}

		public override IReadOnlyCollection<Relation> OnCreate(IEnumerable<Relation> oToCreate, Func<IEnumerable<Relation>, IReadOnlyCollection<Relation>> next)
		{
			var list = oToCreate.ToList();
			Validate(list);
			return next(list);
		}

		public override IReadOnlyCollection<Relation> OnCreateOrUpdate(IEnumerable<Relation> oToCreateOrUpdate, Func<IEnumerable<Relation>, IReadOnlyCollection<Relation>> next)
		{
			var list = oToCreateOrUpdate.ToList();
			Validate(list);
			return next(list);
		}

		private void Validate(IReadOnlyCollection<Relation> relations)
		{
			// Collect the unique entity keys referenced across all relations.
			var keysToCheck = relations
				.Where(r => r?.SourceKey != null && r.TargetKey != null)
				.SelectMany(r => new[] { r.SourceKey, r.TargetKey })
				.Distinct()
				.ToList();

			if (keysToCheck.Count == 0)
				return;

			var existingKeys = LookupExistingKeys(keysToCheck);

			var builder = new ValidationResult.Builder();
			foreach (var relation in relations)
			{
				var entry = new ValidationEntry();

				if (relation?.SourceKey != null && !existingKeys.Contains(relation.SourceKey))
				{
					entry.Exceptions.Add(new InvalidOperationException($"Source entity '{relation.SourceKey}' of relation (Identifier: {relation?.Identifier}) does not exist."));
				}

				if (relation?.TargetKey != null && !existingKeys.Contains(relation.TargetKey))
				{
					entry.Exceptions.Add(new InvalidOperationException($"Target entity '{relation.TargetKey}' of relation (Identifier: {relation?.Identifier}) does not exist."));
				}

				builder.Add(entry);
			}

			var result = builder.Build();
			if (!result.IsValid)
				throw result.ToException();
		}

		private HashSet<EntityKey> LookupExistingKeys(IReadOnlyCollection<EntityKey> keys)
		{
			var keyFilters = keys.Select(BuildEntityKeyFilter).ToArray();
			FilterElement<EntityDescriptor> filter = keyFilters.Length == 1
				? keyFilters[0]
				: new ORFilterElement<EntityDescriptor>(keyFilters);

			return _entities.Read(filter).Select(e => e.Key).ToHashSet();
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
