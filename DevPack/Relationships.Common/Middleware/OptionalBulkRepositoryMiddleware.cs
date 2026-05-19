namespace Skyline.DataMiner.Solutions.Relationships.Middleware
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	using SLDataGateway.API.Types.Querying;

	internal class OptionalBulkRepositoryMiddleware<T> : IBulkRepositoryMiddleware<T>
		where T : class
	{
		public virtual long OnCount(FilterElement<T> filter, Func<FilterElement<T>, long> next)
		{
			return next(filter);
		}

		public virtual long OnCount(IQuery<T> query, Func<IQuery<T>, long> next)
		{
			return next(query);
		}

		public virtual IReadOnlyCollection<T> OnCreate(IEnumerable<T> oToCreate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
		{
			return next(oToCreate);
		}

		public virtual T OnCreate(T oToCreate, Func<T, T> next)
		{
			return next(oToCreate);
		}

		public virtual IReadOnlyCollection<T> OnCreateOrUpdate(IEnumerable<T> oToCreateOrUpdate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
		{
			return next(oToCreateOrUpdate);
		}

		public virtual void OnDelete(IEnumerable<T> oToDelete, Action<IEnumerable<T>> next)
		{
			next(oToDelete);
		}

		public virtual void OnDelete(T oToDelete, Action<T> next)
		{
			next(oToDelete);
		}

		public virtual IEnumerable<T> OnRead(FilterElement<T> filter, Func<FilterElement<T>, IEnumerable<T>> next)
		{
			return next(filter);
		}

		public virtual IEnumerable<T> OnRead(IQuery<T> query, Func<IQuery<T>, IEnumerable<T>> next)
		{
			return next(query);
		}

		public virtual IEnumerable<IPagedResult<T>> OnReadPaged(FilterElement<T> filter, Func<FilterElement<T>, IEnumerable<IPagedResult<T>>> next)
		{
			return next(filter);
		}

		public virtual IEnumerable<IPagedResult<T>> OnReadPaged(IQuery<T> query, Func<IQuery<T>, IEnumerable<IPagedResult<T>>> next)
		{
			return next(query);
		}

		public virtual IEnumerable<IPagedResult<T>> OnReadPaged(FilterElement<T> filter, int pageSize, Func<FilterElement<T>, int, IEnumerable<IPagedResult<T>>> next)
		{
			return next(filter, pageSize);
		}

		public virtual IEnumerable<IPagedResult<T>> OnReadPaged(IQuery<T> query, int pageSize, Func<IQuery<T>, int, IEnumerable<IPagedResult<T>>> next)
		{
			return next(query, pageSize);
		}

		public virtual IReadOnlyCollection<T> OnUpdate(IEnumerable<T> oToUpdate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
		{
			return next(oToUpdate);
		}

		public virtual T OnUpdate(T oToUpdate, Func<T, T> next)
		{
			return next(oToUpdate);
		}
	}
}
