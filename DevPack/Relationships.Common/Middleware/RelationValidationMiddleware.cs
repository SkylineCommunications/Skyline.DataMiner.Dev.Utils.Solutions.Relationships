namespace Skyline.DataMiner.Solutions.Relationships.Middleware
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Solutions.Relationships.Exceptions;
	using Skyline.DataMiner.Solutions.Relationships.Validation;

	using SLDataGateway.API.Types.Querying;

	internal class RelationValidationMiddleware : IBulkRepositoryMiddleware<Relation>
	{
		public long OnCount(FilterElement<Relation> filter, Func<FilterElement<Relation>, long> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			return next(filter);
		}

		public long OnCount(IQuery<Relation> query, Func<IQuery<Relation>, long> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			return next(query);
		}

		public IReadOnlyCollection<Relation> OnCreate(IEnumerable<Relation> oToCreate, Func<IEnumerable<Relation>, IReadOnlyCollection<Relation>> next)
		{
			var builder = new ValidationResult.Builder();
			foreach (var relation in oToCreate)
			{
				var entry = Validate(relation);
				builder.Add(entry);
			}

			var result = builder.Build();
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreate);
		}

		public Relation OnCreate(Relation oToCreate, Func<Relation, Relation> next)
		{
			var result = Validate(oToCreate);
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreate);
		}

		public IReadOnlyCollection<Relation> OnCreateOrUpdate(IEnumerable<Relation> oToCreateOrUpdate, Func<IEnumerable<Relation>, IReadOnlyCollection<Relation>> next)
		{
			var builder = new ValidationResult.Builder();
			foreach (var relation in oToCreateOrUpdate)
			{
				var entry = Validate(relation);
				builder.Add(entry);
			}

			var result = builder.Build();
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreateOrUpdate);
		}

		public void OnDelete(IEnumerable<Relation> oToDelete, Action<IEnumerable<Relation>> next)
		{
			if (oToDelete is null)
			{
				throw new ArgumentNullException(nameof(oToDelete), "The collection of relations to delete cannot be null.");
			}

			next(oToDelete);
		}

		public void OnDelete(Relation oToDelete, Action<Relation> next)
		{
			if (oToDelete is null)
			{
				throw new ArgumentNullException(nameof(oToDelete), "The relations to delete cannot be null.");
			}

			next(oToDelete);
		}

		public IEnumerable<Relation> OnRead(FilterElement<Relation> filter, Func<FilterElement<Relation>, IEnumerable<Relation>> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			return next(filter);
		}

		public IEnumerable<Relation> OnRead(IQuery<Relation> query, Func<IQuery<Relation>, IEnumerable<Relation>> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			return next(query);
		}

		public IEnumerable<IPagedResult<Relation>> OnReadPaged(FilterElement<Relation> filter, Func<FilterElement<Relation>, IEnumerable<IPagedResult<Relation>>> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			return next(filter);
		}

		public IEnumerable<IPagedResult<Relation>> OnReadPaged(IQuery<Relation> query, Func<IQuery<Relation>, IEnumerable<IPagedResult<Relation>>> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			return next(query);
		}

		public IEnumerable<IPagedResult<Relation>> OnReadPaged(FilterElement<Relation> filter, int pageSize, Func<FilterElement<Relation>, int, IEnumerable<IPagedResult<Relation>>> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
			}

			return next(filter, pageSize);
		}

		public IEnumerable<IPagedResult<Relation>> OnReadPaged(IQuery<Relation> query, int pageSize, Func<IQuery<Relation>, int, IEnumerable<IPagedResult<Relation>>> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
			}

			return next(query, pageSize);
		}

		public IReadOnlyCollection<Relation> OnUpdate(IEnumerable<Relation> oToUpdate, Func<IEnumerable<Relation>, IReadOnlyCollection<Relation>> next)
		{
			var builder = new ValidationResult.Builder();
			foreach (var relation in oToUpdate)
			{
				var entry = Validate(relation);
				builder.Add(entry);
			}

			var result = builder.Build();
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToUpdate);
		}

		public Relation OnUpdate(Relation oToUpdate, Func<Relation, Relation> next)
		{
			var result = Validate(oToUpdate);
			if (!result.IsValid)
			{
				throw new ValidationResult.Builder()
					.Add(result)
					.Build()
					.ToException();
			}

			return next(oToUpdate);
		}

		private static ValidationEntry Validate(Relation relation)
		{
			var entry = new ValidationEntry();
			if (relation is null)
			{
				entry.Exceptions.Add(new ArgumentNullException(nameof(relation), "Relation object cannot be null."));
				return entry;
			}

			// Check for a valid Relation Id (if required).
			if (!Guid.TryParse(relation.Identifier, out var guid) ||
				guid == Guid.Empty)
			{
				entry.Exceptions.Add(new ArgumentException("Relation Identifier should be a valid non empty guid.", nameof(relation)));
			}

			// Validate that the Relation has a source and target entities.
			if (relation.SourceKey is null ||
				relation.TargetKey is null)
			{
				entry.Exceptions.Add(new ArgumentException($"A Relation (Identifier: {relation.Identifier}) must contain a source and a target entity.", nameof(relation)));
			}

			if (relation.SourceKey == EntityKey.Empty)
			{
				entry.Exceptions.Add(new ArgumentException($"SourceKey of relation (Identifier: {relation.Identifier}) cannot be empty", nameof(relation.SourceKey)));
			}

			if (relation.TargetKey == EntityKey.Empty)
			{
				entry.Exceptions.Add(new ArgumentException($"TargetKey of relation (Identifier: {relation.Identifier}) cannot be empty", nameof(relation.TargetKey)));
			}

			return entry;
		}
	}
}