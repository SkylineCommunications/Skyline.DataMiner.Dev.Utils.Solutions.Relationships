namespace Skyline.DataMiner.Solutions.ObjectLinking.Middleware
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Solutions.ObjectLinking.Exceptions;
	using Skyline.DataMiner.Solutions.ObjectLinking.Validation;

	using SLDataGateway.API.Types.Querying;

	internal class LinkValidationMiddleware : IBulkRepositoryMiddleware<Link>
	{
		public long OnCount(FilterElement<Link> filter, Func<FilterElement<Link>, long> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			return next(filter);
		}

		public long OnCount(IQuery<Link> query, Func<IQuery<Link>, long> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			return next(query);
		}

		public IReadOnlyCollection<Link> OnCreate(IEnumerable<Link> oToCreate, Func<IEnumerable<Link>, IReadOnlyCollection<Link>> next)
		{
			var builder = new ValidationResult.Builder();
			foreach (var link in oToCreate)
			{
				var entry = Validate(link);
				builder.Add(entry);
			}

			var result = builder.Build();
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreate);
		}

		public Link OnCreate(Link oToCreate, Func<Link, Link> next)
		{
			var result = Validate(oToCreate);
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreate);
		}

		public IReadOnlyCollection<Link> OnCreateOrUpdate(IEnumerable<Link> oToCreateOrUpdate, Func<IEnumerable<Link>, IReadOnlyCollection<Link>> next)
		{
			var builder = new ValidationResult.Builder();
			foreach (var link in oToCreateOrUpdate)
			{
				var entry = Validate(link);
				builder.Add(entry);
			}

			var result = builder.Build();
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToCreateOrUpdate);
		}

		public void OnDelete(IEnumerable<Link> oToDelete, Action<IEnumerable<Link>> next)
		{
			if (oToDelete is null)
			{
				throw new ArgumentNullException(nameof(oToDelete), "The collection of links to delete cannot be null.");
			}

			next(oToDelete);
		}

		public void OnDelete(Link oToDelete, Action<Link> next)
		{
			if (oToDelete is null)
			{
				throw new ArgumentNullException(nameof(oToDelete), "The link to delete cannot be null.");
			}

			next(oToDelete);
		}

		public IEnumerable<Link> OnRead(FilterElement<Link> filter, Func<FilterElement<Link>, IEnumerable<Link>> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			return next(filter);
		}

		public IEnumerable<Link> OnRead(IQuery<Link> query, Func<IQuery<Link>, IEnumerable<Link>> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			return next(query);
		}

		public IEnumerable<IPagedResult<Link>> OnReadPaged(FilterElement<Link> filter, Func<FilterElement<Link>, IEnumerable<IPagedResult<Link>>> next)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
			}

			return next(filter);
		}

		public IEnumerable<IPagedResult<Link>> OnReadPaged(IQuery<Link> query, Func<IQuery<Link>, IEnumerable<IPagedResult<Link>>> next)
		{
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query), "Query cannot be null.");
			}

			return next(query);
		}

		public IEnumerable<IPagedResult<Link>> OnReadPaged(FilterElement<Link> filter, int pageSize, Func<FilterElement<Link>, int, IEnumerable<IPagedResult<Link>>> next)
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

		public IEnumerable<IPagedResult<Link>> OnReadPaged(IQuery<Link> query, int pageSize, Func<IQuery<Link>, int, IEnumerable<IPagedResult<Link>>> next)
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

		public IReadOnlyCollection<Link> OnUpdate(IEnumerable<Link> oToUpdate, Func<IEnumerable<Link>, IReadOnlyCollection<Link>> next)
		{
			var builder = new ValidationResult.Builder();
			foreach (var link in oToUpdate)
			{
				var entry = Validate(link);
				builder.Add(entry);
			}

			var result = builder.Build();
			if (!result.IsValid)
			{
				throw result.ToException();
			}

			return next(oToUpdate);
		}

		public Link OnUpdate(Link oToUpdate, Func<Link, Link> next)
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

		private static ValidationEntry Validate(Link link)
		{
			var entry = new ValidationEntry();
			if (link is null)
			{
				entry.Exceptions.Add(new ArgumentNullException(nameof(link), "Link object cannot be null."));
				return entry;
			}

			// Validate that the Link has a source and target entities.
			if (link.Source is null ||
				link.Target is null)
			{
				entry.Exceptions.Add(new ArgumentException($"A Link (Identifier: {link.Identifier}) must contain a source and a target entity.", nameof(link)));
			}

			// Optionally, check for a valid Link Id (if required).
			if (!Guid.TryParse(link.Identifier, out var guid) ||
				guid == Guid.Empty)
			{
				entry.Exceptions.Add(new ArgumentException("Link Identifier should be a valid non empty guid.", nameof(link)));
			}

			var entitySet = new HashSet<string>();
			IEnumerable<Exception> ValidateEntity(EntityDescriptor entity, string entityPosition)
			{
				if (entity is null)
				{
					// Already checked in the beginning of the method.
					yield break;
				}

				if (!entitySet.Add(entity.ID))
				{
					yield return new ArgumentException($"{entityPosition} entity (Id: {entity.ID}) is a duplicate in Link (Identifier: {link.Identifier}).", nameof(link));
					yield break;
				}

				if (String.IsNullOrEmpty(entity.ID))
				{
					yield return new LinkEntityValidationException($"ID of {entityPosition} entity cannot be null or empty.", link.Identifier, entity.ID);
					yield break;
				}

				if (String.IsNullOrEmpty(entity.ModelName))
				{
					yield return new ValidationException($"ModelName of {entityPosition} entity (Id: {entity.ID}) in Link (Identifier: {link.Identifier}) cannot be null or empty.");
				}

				if (String.IsNullOrEmpty(entity.DisplayName))
				{
					yield return new ValidationException($"DisplayName of {entityPosition} entity (Id: {entity.ID}) in Link (Identifier: {link.Identifier}) cannot be null or empty.");
				}

				if (String.IsNullOrEmpty(entity.SolutionID))
				{
					yield return new ValidationException($"SolutionID of {entityPosition} entity (Id: {entity.ID}) in Link (Identifier: {link.Identifier}) cannot be null or empty.");
				}

				if (String.IsNullOrEmpty(entity.SolutionName))
				{
					yield return new ValidationException($"SolutionName of {entityPosition} entity (Id: {entity.ID}) in Link (Identifier: {link.Identifier}) cannot be null or empty.");
				}
			}

			entry.Exceptions.AddRange(ValidateEntity(link.Source, nameof(Link.Source)));
			entry.Exceptions.AddRange(ValidateEntity(link.Target, nameof(Link.Target)));

			return entry;
		}
	}
}