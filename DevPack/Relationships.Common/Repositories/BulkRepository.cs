namespace Skyline.DataMiner.Solutions.Relationships.Repositories
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	using SLDataGateway.API.Querying;
	using SLDataGateway.API.Types.Querying;

	internal abstract class BulkRepository<T> : IBulkRepository<T>
		where T : SdmObject<T>
	{
		private readonly IConnection connection;
		private readonly DomHelper helper;
		private readonly DomDefinitionId domDefinitionId;
		private readonly Exposer<T, string> identifierExposer;

		protected BulkRepository(
			IConnection connection,
			string moduleId,
			DomDefinitionId domDefinitionId,
			Exposer<T, string> identifierExposer)
		{
			this.connection = connection;
			this.helper = new DomHelper(connection.HandleMessages, moduleId);
			this.domDefinitionId = domDefinitionId;
			this.identifierExposer = identifierExposer;
		}

		public T Create(T createObject)
		{
			if (createObject is null)
			{
				throw new ArgumentNullException(nameof(createObject));
			}

			var instance = ToInstance(createObject);
			instance = helper.DomInstances.Create(instance);
			return FromInstance(instance);
		}

		public IReadOnlyCollection<T> Create(IEnumerable<T> createObjects)
		{
			if (createObjects is null || !createObjects.Any())
			{
				return Array.Empty<T>();
			}

			// Check if some of the objects already exist
			var existing = new HashSet<string>();
			foreach (var batch in createObjects.Batch(500))
			{
				existing.UnionWith(Read(new ORFilterElement<T>(batch.Select(obj => identifierExposer.Equal(obj.Identifier)).ToArray())).Select(obj => obj.Identifier));
			}

			// Create the remainder
			var successfulItems = new List<T>();
			var failures = new Dictionary<string, Exception>();
			var objects = createObjects.Where(obj => !existing.Contains(obj.Identifier)).ToDictionary(obj => obj.Identifier);
			foreach (var batch in createObjects.Select(ToInstance).Batch(helper.DomInstances.MaxAmountBulkOperation))
			{
				helper.DomInstances.TryCreateOrUpdate(batch.ToList(), out var result);
				foreach (var failure in result.UnsuccessfulIds)
				{
					failures.Add(failure.Id.ToString(), new CrudFailedException(result.TraceDataPerItem[failure]));
				}

				foreach (var success in result.SuccessfulItems)
				{
					successfulItems.Add(FromInstance(success));
				}
			}

			// If everything went fine, return the successful creations
			if (!existing.Any() && !failures.Any())
			{
				return successfulItems;
			}

			// Otherwise, build and throw an exception
			var exceptionBuilder = new SdmBulkCrudException<T>.Builder();
			foreach (var obj in createObjects)
			{
				if (existing.Contains(obj.Identifier))
				{
					exceptionBuilder.AddFailed(obj, new SdmCrudException<T>(obj, $"Could not create T with guid: '{obj.Identifier}', it already exists."));
					continue;
				}

				if (failures.ContainsKey(obj.Identifier))
				{
					exceptionBuilder.AddFailed(obj, failures[obj.Identifier]);
					continue;
				}

				exceptionBuilder.AddSuccessful(obj);
			}

			throw exceptionBuilder.Build();
		}

		public IReadOnlyCollection<T> CreateOrUpdate(IEnumerable<T> items)
		{
			if (items is null || !items.Any())
			{
				return Array.Empty<T>();
			}

			var successful = new List<T>();
			var exceptionBuilder = new SdmBulkCrudException<T>.Builder();
			var objects = items.ToDictionary(obj => obj.Identifier);
			foreach (var batch in items.Select(ToInstance).Batch(helper.DomInstances.MaxAmountBulkOperation))
			{
				helper.DomInstances.TryCreateOrUpdate(batch.ToList(), out var result);
				foreach (var failure in result.UnsuccessfulIds)
				{
					exceptionBuilder.AddFailed(objects[failure.Id.ToString()], new CrudFailedException(result.TraceDataPerItem[failure]));
				}

				foreach (var success in result.SuccessfulItems)
				{
					var item = FromInstance(success);
					exceptionBuilder.AddSuccessful(item);
					successful.Add(item);
				}
			}

			if (exceptionBuilder.HasFailure)
			{
				throw exceptionBuilder.Build();
			}

			return successful;
		}

		public long Count(FilterElement<T> filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var domFilter = TranslateFullFilter(filter);
			domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(domDefinitionId.Id));
			return helper.DomInstances.Count(domFilter);
		}

		public long Count(IQuery<T> query)
		{
			if (query is null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			var domFilter = TranslateFullFilter(query.Filter);
			domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(domDefinitionId.Id));
			var domOrder = TranslateFullOrderBy(query.Order);
			var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
			return helper.DomInstances.Count(domQuery);
		}

		public IEnumerable<T> Read(FilterElement<T> filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			var domFilter = TranslateFullFilter(filter);
			return Read(domFilter);
		}

		public IEnumerable<T> Read(IQuery<T> query)
		{
			if (query is null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			var domFilter = TranslateFullFilter(query.Filter);
			var domOrder = TranslateFullOrderBy(query.Order);
			var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
			return Read(domQuery);
		}

		public IEnumerable<IPagedResult<T>> ReadPaged(FilterElement<T> filter)
		{
			return ReadPaged(filter, 500);
		}

		public IEnumerable<IPagedResult<T>> ReadPaged(FilterElement<T> filter, int pageSize)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be 1 or higher");
			}

			var domFilter = TranslateFullFilter(filter);
			var paging = ReadPaged(domFilter, pageSize).GetEnumerator();
			var moveNext = paging.MoveNext();
			var i = 0;
			while (moveNext)
			{
				var page = paging.Current.ToList();
				moveNext = paging.MoveNext();
				var result = new PagedResult<T>(page, i, pageSize, moveNext);
				yield return result;
				i++;
			}
		}

		public IEnumerable<IPagedResult<T>> ReadPaged(IQuery<T> query)
		{
			return ReadPaged(query, 500);
		}

		public IEnumerable<IPagedResult<T>> ReadPaged(IQuery<T> query, int pageSize)
		{
			if (query is null)
			{
				throw new ArgumentNullException(nameof(query));
			}

			if (pageSize <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be 1 or higher");
			}

			var domFilter = TranslateFullFilter(query.Filter);
			var domOrder = TranslateFullOrderBy(query.Order);
			var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
			var paging = ReadPaged(domQuery, pageSize).GetEnumerator();
			var moveNext = paging.MoveNext();
			var i = 0;
			while (moveNext)
			{
				var page = paging.Current.ToList();
				moveNext = paging.MoveNext();
				var result = new PagedResult<T>(page, i, pageSize, moveNext);
				yield return result;
				i++;
			}
		}

		public T Update(T updateObject)
		{
			if (updateObject is null)
			{
				throw new ArgumentNullException(nameof(updateObject));
			}

			var instance = ToInstance(updateObject);
			instance = helper.DomInstances.Update(instance);
			return FromInstance(instance);
		}

		public IReadOnlyCollection<T> Update(IEnumerable<T> updateObjects)
		{
			if (updateObjects is null || !updateObjects.Any())
			{
				return Array.Empty<T>();
			}

			// Check if which objects already exist
			var existing = new HashSet<string>();
			foreach (var batch in updateObjects.Batch(500))
			{
				existing.UnionWith(Read(new ORFilterElement<T>(batch.Select(obj => identifierExposer.Equal(obj.Identifier)).ToArray())).Select(obj => obj.Identifier));
			}

			// Update the existing objects
			var successfulItems = new List<T>();
			var failures = new Dictionary<string, Exception>();
			var objects = updateObjects.Where(obj => existing.Contains(obj.Identifier)).ToDictionary(obj => obj.Identifier);
			foreach (var batch in updateObjects.Select(ToInstance).Batch(helper.DomInstances.MaxAmountBulkOperation))
			{
				helper.DomInstances.TryCreateOrUpdate(batch.ToList(), out var result);
				foreach (var failure in result.UnsuccessfulIds)
				{
					failures.Add(failure.Id.ToString(), new CrudFailedException(result.TraceDataPerItem[failure]));
				}

				foreach (var success in result.SuccessfulItems)
				{
					successfulItems.Add(FromInstance(success));
				}
			}

			// Check for failures and build exception if needed
			var exceptionBuilder = new SdmBulkCrudException<T>.Builder();
			foreach (var obj in updateObjects)
			{
				if (!existing.Contains(obj.Identifier))
				{
					exceptionBuilder.AddFailed(obj, new SdmCrudException<T>(obj, "Could not update a non existing T"));
					continue;
				}

				if (failures.ContainsKey(obj.Identifier))
				{
					exceptionBuilder.AddFailed(obj, failures[obj.Identifier]);
					continue;
				}

				exceptionBuilder.AddSuccessful(obj);
			}

			if (exceptionBuilder.HasFailure)
			{
				throw exceptionBuilder.Build();
			}

			return successfulItems;
		}

		public void Delete(T deleteObject)
		{
			if (deleteObject is null)
			{
				throw new ArgumentNullException(nameof(deleteObject));
			}

			var instance = ToInstance(deleteObject);
			helper.DomInstances.Delete(instance);
		}

		public void Delete(IEnumerable<T> deleteObjects)
		{
			if (deleteObjects is null || !deleteObjects.Any())
			{
				return;
			}

			var exceptionBuilder = new SdmBulkCrudException<T>.Builder();
			var objects = deleteObjects.ToDictionary(obj => obj.Identifier);
			foreach (var batch in deleteObjects.Select(ToInstance).Batch(helper.DomInstances.MaxAmountBulkOperation))
			{
				helper.DomInstances.TryDelete(batch.ToList(), out var result);
				foreach (var failure in result.UnsuccessfulIds)
				{
					exceptionBuilder.AddFailed(objects[failure.Id.ToString()], new CrudFailedException(result.TraceDataPerItem[failure]));
				}

				foreach (var success in result.SuccessfulIds)
				{
					exceptionBuilder.AddSuccessful(objects[success.Id.ToString()]);
				}
			}

			if (exceptionBuilder.HasFailure)
			{
				throw exceptionBuilder.Build();
			}
		}

		protected abstract DomInstance ToInstance(T obj);

		protected abstract T FromInstance(DomInstance instance);

		protected abstract FilterElement<DomInstance> CreateFilter(string fieldName, Comparer comparer, object value);

		protected abstract IOrderByElement CreateOrderBy(string fieldName, SortOrder sortOrder, bool naturalSort = false);

		private IEnumerable<T> Read(FilterElement<DomInstance> domFilter)
		{
			if (domFilter is null)
			{
				throw new ArgumentNullException(nameof(domFilter));
			}

			domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(domDefinitionId.Id));
			var domInstances = helper.DomInstances.Read(domFilter);
			return domInstances.Select(FromInstance);
		}

		private IEnumerable<T> Read(IQuery<DomInstance> domQuery)
		{
			if (domQuery is null)
			{
				throw new ArgumentNullException(nameof(domQuery));
			}

			var domFilter = domQuery.Filter.AND(DomInstanceExposers.DomDefinitionId.Equal(domDefinitionId.Id));
			domQuery = domQuery.WithFilter(domFilter);
			var domInstances = helper.DomInstances.Read(domQuery);
			return domInstances.Select(FromInstance);
		}

		private IEnumerable<IEnumerable<T>> ReadPaged(FilterElement<DomInstance> domFilter, int pageSize)
		{
			if (domFilter is null)
			{
				throw new ArgumentNullException(nameof(domFilter));
			}

			domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(domDefinitionId.Id));
			var pagingHelper = helper.DomInstances.PreparePaging(domFilter, pageSize);
			while (pagingHelper.MoveToNextPage())
			{
				yield return pagingHelper.GetCurrentPage().Select(FromInstance);
			}
		}

		private IEnumerable<IEnumerable<T>> ReadPaged(IQuery<DomInstance> domQuery, int pageSize)
		{
			if (domQuery is null)
			{
				throw new ArgumentNullException(nameof(domQuery));
			}

			var domFilter = domQuery.Filter.AND(DomInstanceExposers.DomDefinitionId.Equal(domDefinitionId.Id));
			domQuery = domQuery.WithFilter(domFilter);
			var pagingHelper = helper.DomInstances.PreparePaging(domQuery, pageSize);
			while (pagingHelper.MoveToNextPage())
			{
				yield return pagingHelper.GetCurrentPage().Select(FromInstance);
			}
		}

		private FilterElement<DomInstance> TranslateFullFilter(FilterElement<T> filter)
		{
			if (filter is null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			FilterElement<DomInstance> translated;
			if (filter is ANDFilterElement<T> and)
			{
				translated = new ANDFilterElement<DomInstance>(and.subFilters.Select(TranslateFullFilter).ToArray());
			}
			else if (filter is ORFilterElement<T> or)
			{
				translated = new ORFilterElement<DomInstance>(or.subFilters.Select(TranslateFullFilter).ToArray());
			}
			else if (filter is NOTFilterElement<T> not)
			{
				translated = new NOTFilterElement<DomInstance>(TranslateFullFilter(not));
			}
			else if (filter is TRUEFilterElement<T>)
			{
				translated = new TRUEFilterElement<DomInstance>();
			}
			else if (filter is FALSEFilterElement<T>)
			{
				translated = new FALSEFilterElement<DomInstance>();
			}
			else if (filter is ManagedFilterIdentifier managedFilter)
			{
				translated = TranslateFilter(managedFilter);
			}
			else
			{
				throw new NotSupportedException($"Unsupported filter: {filter}");
			}

			return translated;
		}

		private IOrderBy TranslateFullOrderBy(IOrderBy order)
		{
			if (order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			var translatedElements = new List<IOrderByElement>();
			foreach (var orderByElement in order.Elements)
			{
				var translated = TranslateOrderBy(orderByElement);
				translatedElements.Add(translated);
			}

			return new OrderBy(translatedElements);
		}

		private FilterElement<DomInstance> TranslateFilter(ManagedFilterIdentifier managedFilter)
		{
			if (managedFilter is null)
			{
				throw new ArgumentNullException(nameof(managedFilter));
			}

			var fieldName = managedFilter.getFieldName().fieldName;
			var comparer = managedFilter.getComparer();
			var value = managedFilter.getValue();
			var translated = CreateFilter(fieldName, comparer, value);
			return translated;
		}

		private IOrderByElement TranslateOrderBy(IOrderByElement orderByElement)
		{
			if (orderByElement is null)
			{
				throw new ArgumentNullException(nameof(orderByElement));
			}

			var fieldName = orderByElement.Exposer.fieldName;
			var sortOrder = orderByElement.SortOrder;
			var naturalSort = orderByElement.Options.NaturalSort;
			var translated = CreateOrderBy(fieldName, sortOrder, naturalSort);
			return translated;
		}
	}
}
