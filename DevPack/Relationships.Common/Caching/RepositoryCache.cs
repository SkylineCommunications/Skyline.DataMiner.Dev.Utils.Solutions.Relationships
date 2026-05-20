namespace Skyline.DataMiner.Solutions.Relationships.Caching
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	/// <summary>
	/// A generic thread-safe in-memory cache keyed by <typeparamref name="TKey"/>.
	/// On a cache miss, the item is fetched using a caller-supplied fetch delegate and stored for future lookups.
	/// Concurrent reads are served in parallel; writes and fetches are serialized per key.
	/// </summary>
	/// <typeparam name="TKey">The type used as the cache key.</typeparam>
	/// <typeparam name="TItem">The type of items stored in the cache.</typeparam>
	internal class RepositoryCache<TKey, TItem> : IDisposable
		where TItem : class
	{
#pragma warning disable SA1401 // Fields should be private
		protected readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		protected readonly Dictionary<TKey, TItem> _cache;
#pragma warning restore SA1401 // Fields should be private

		private readonly Func<TKey, TItem> _fetch;
		private readonly Func<TItem, TKey> _keySelector;
		private bool _disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="RepositoryCache{TKey, TItem}"/> class.
		/// </summary>
		/// <param name="keySelector">Extracts the cache key from an item.</param>
		/// <param name="fetch">Called on a cache miss to retrieve an item by key. May return <c>null</c> when not found.</param>
		/// <param name="keyComparer">Optional equality comparer for <typeparamref name="TKey"/>.</param>
		public RepositoryCache(Func<TItem, TKey> keySelector, Func<TKey, TItem> fetch, IEqualityComparer<TKey> keyComparer = null)
		{
			_keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
			_fetch = fetch ?? throw new ArgumentNullException(nameof(fetch));
			_cache = keyComparer != null
				? new Dictionary<TKey, TItem>(keyComparer)
				: new Dictionary<TKey, TItem>();
		}

		/// <summary>
		/// Retrieves an item by key.
		/// Returns the cached instance when available; otherwise calls the fetch delegate,
		/// stores the result, and returns it. Returns <c>null</c> when the fetch returns nothing.
		/// </summary>
		/// <param name="key">The key of the item to retrieve.</param>
		/// <returns>The cached or fetched item, or <c>null</c> if not found.</returns>
		public TItem Get(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			ThrowIfDisposed();

			_lock.EnterReadLock();
			try
			{
				if (_cache.TryGetValue(key, out var cached))
					return cached;
			}
			finally
			{
				_lock.ExitReadLock();
			}

			// Cache miss: acquire write lock and double-check before fetching,
			// so only one thread calls the (potentially expensive) fetch delegate.
			_lock.EnterWriteLock();
			try
			{
				if (_cache.TryGetValue(key, out var cached))
					return cached;

				var fetched = _fetch(key);

				if (fetched != null)
					_cache[key] = fetched;

				return fetched;
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Adds or updates a single item in the cache.
		/// </summary>
		/// <param name="item">The item to add or update.</param>
		public void Add(TItem item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			ThrowIfDisposed();

			_lock.EnterWriteLock();
			try
			{
				_cache[_keySelector(item)] = item;
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Adds or updates multiple items in the cache.
		/// </summary>
		/// <param name="items">The sequence of items to add or update.</param>
		public void Add(IEnumerable<TItem> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));

			ThrowIfDisposed();

			_lock.EnterWriteLock();
			try
			{
				foreach (var item in items)
				{
					if (item == null)
						throw new ArgumentException("Sequence contains a null item.", nameof(items));

					_cache[_keySelector(item)] = item;
				}
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Removes an entry from the cache.
		/// </summary>
		/// <param name="key">The key of the entry to remove.</param>
		/// <returns><c>true</c> if the entry was present and removed; otherwise <c>false</c>.</returns>
		public bool Invalidate(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));

			ThrowIfDisposed();

			_lock.EnterWriteLock();
			try
			{
				return _cache.Remove(key);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			_lock.Dispose();
		}

		protected void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().FullName);
		}
	}
}
