namespace Skyline.DataMiner.Solutions.Relationships.Middleware
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Solutions.Relationships.Validation;

	using SLDataGateway.API.Types.Querying;

	internal class EntityValidationMiddleware : OptionalBulkRepositoryMiddleware<EntityDescriptor>
	{
		public override long OnCount(FilterElement<EntityDescriptor> filter, Func<FilterElement<EntityDescriptor>, long> next)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null");
			}

			return next(filter);
		}

		public override long OnCount(IQuery<EntityDescriptor> query, Func<IQuery<EntityDescriptor>, long> next)
		{
			if (query is null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null");
			}

			return next(query);
		}

		public override EntityDescriptor OnCreate(EntityDescriptor oToCreate, Func<EntityDescriptor, EntityDescriptor> next)
		{
			var result = Validate(oToCreate);
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreate);
		}

		public override IReadOnlyCollection<EntityDescriptor> OnCreate(IEnumerable<EntityDescriptor> oToCreate, Func<IEnumerable<EntityDescriptor>, IReadOnlyCollection<EntityDescriptor>> next)
		{
			var builder = new ValidationResult.Builder();
			foreach (var entity in oToCreate)
			{
				builder.Add(Validate(entity));
			}

			var result = builder.Build();
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreate);
		}

		public override IReadOnlyCollection<EntityDescriptor> OnCreateOrUpdate(IEnumerable<EntityDescriptor> oToCreateOrUpdate, Func<IEnumerable<EntityDescriptor>, IReadOnlyCollection<EntityDescriptor>> next)
		{
			var builder = new ValidationResult.Builder();
			foreach (var entity in oToCreateOrUpdate)
			{
				builder.Add(Validate(entity));
			}

			var result = builder.Build();
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreateOrUpdate);
		}

		public override void OnDelete(EntityDescriptor oToDelete, Action<EntityDescriptor> next)
		{
			if (oToDelete is null)
			{
				throw new ArgumentNullException(nameof(oToDelete), "Entity to delete cannot be null");
			}

			next(oToDelete);
		}

		public override void OnDelete(IEnumerable<EntityDescriptor> oToDelete, Action<IEnumerable<EntityDescriptor>> next)
		{
			if (oToDelete is null)
			{
				throw new ArgumentNullException(nameof(oToDelete), "The collection of relations to delete cannot be null.");
			}

			if (oToDelete is ICollection<EntityDescriptor> collection && collection.Count == 0)
			{
				return;
			}

			next(oToDelete);
		}

		public override IEnumerable<EntityDescriptor> OnRead(FilterElement<EntityDescriptor> filter, Func<FilterElement<EntityDescriptor>, IEnumerable<EntityDescriptor>> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			return next(filter);
		}

		public override IEnumerable<EntityDescriptor> OnRead(IQuery<EntityDescriptor> query, Func<IQuery<EntityDescriptor>, IEnumerable<EntityDescriptor>> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			return next(query);
		}

		public override IEnumerable<IPagedResult<EntityDescriptor>> OnReadPaged(FilterElement<EntityDescriptor> filter, Func<FilterElement<EntityDescriptor>, IEnumerable<IPagedResult<EntityDescriptor>>> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			return next(filter);
		}

		public override IEnumerable<IPagedResult<EntityDescriptor>> OnReadPaged(IQuery<EntityDescriptor> query, Func<IQuery<EntityDescriptor>, IEnumerable<IPagedResult<EntityDescriptor>>> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			return next(query);
		}

		public override IEnumerable<IPagedResult<EntityDescriptor>> OnReadPaged(FilterElement<EntityDescriptor> filter, int pageSize, Func<FilterElement<EntityDescriptor>, int, IEnumerable<IPagedResult<EntityDescriptor>>> next)
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

		public override IEnumerable<IPagedResult<EntityDescriptor>> OnReadPaged(IQuery<EntityDescriptor> query, int pageSize, Func<IQuery<EntityDescriptor>, int, IEnumerable<IPagedResult<EntityDescriptor>>> next)
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

		public override IReadOnlyCollection<EntityDescriptor> OnUpdate(IEnumerable<EntityDescriptor> oToUpdate, Func<IEnumerable<EntityDescriptor>, IReadOnlyCollection<EntityDescriptor>> next)
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

		public override EntityDescriptor OnUpdate(EntityDescriptor oToUpdate, Func<EntityDescriptor, EntityDescriptor> next)
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

		private static ValidationEntry Validate(EntityDescriptor entity)
		{
			var entry = new ValidationEntry();
			if (entity is null)
			{
				entry.Exceptions.Add(new ArgumentNullException(nameof(entity), "EntityDescriptor object cannot be null."));
				return entry;
			}

			// Check for a valid Identifier.
			if (!Guid.TryParse(entity.Identifier, out var guid) ||
				guid == Guid.Empty)
			{
				entry.Exceptions.Add(new ArgumentException("EntityDescriptor Identifier should be a valid non empty guid.", nameof(entity)));
			}

			if (String.IsNullOrEmpty(entity.ID))
			{
				entry.Exceptions.Add(new ArgumentException($"ID of entity cannot be null or empty.", entity.ID));
			}

			if (String.IsNullOrEmpty(entity.ModelName))
			{
				entry.Exceptions.Add(new ArgumentException($"ModelName of entity (Id: {entity.ID}) cannot be null or empty.", nameof(entity.ModelName)));
			}

			if (String.IsNullOrEmpty(entity.DisplayName))
			{
				entry.Exceptions.Add(new ArgumentException($"DisplayName of entity (Id: {entity.ID}) cannot be null or empty.", nameof(entity.DisplayName)));
			}

			if (String.IsNullOrEmpty(entity.SolutionID))
			{
				entry.Exceptions.Add(new ArgumentException($"SolutionID of entity (Id: {entity.ID}) cannot be null or empty.", nameof(entity.SolutionID)));
			}

			if (String.IsNullOrEmpty(entity.SolutionName))
			{
				entry.Exceptions.Add(new ArgumentException($"SolutionName of entity (Id: {entity.ID}) cannot be null or empty.", nameof(entity.SolutionID)));
			}

			if (String.IsNullOrEmpty(entity.ParentID) &&
				String.IsNullOrEmpty(entity.ParentModelName))
			{
				return entry;
			}

			if (String.IsNullOrEmpty(entity.ParentID))
			{
				entry.Exceptions.Add(new ArgumentException($"ParentID of entity (Id: {entity.ID}) cannot be null or empty when ParentModelName is provided.", nameof(entity.ParentID)));
			}

			if (String.IsNullOrEmpty(entity.ParentModelName))
			{
				entry.Exceptions.Add(new ArgumentException($"ParentModelName of entity (Id: {entity.ID}) cannot be null or empty when ParentID is provided.", nameof(entity.ParentModelName)));
			}

			return entry;
		}
	}
}
