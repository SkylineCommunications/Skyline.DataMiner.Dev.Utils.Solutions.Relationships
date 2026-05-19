namespace Skyline.DataMiner.Solutions.Relationships.Caching
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;

	/// <summary>
	/// A generic thread-safe in-memory cache for SDM objects.
	/// On a cache miss, the item is fetched using a caller-supplied fetch delegate and stored for future lookups.
	/// Concurrent reads are served in parallel; writes and fetches are serialized per key.
	/// </summary>
	/// <typeparam name="TItem">The type of items stored in the cache.</typeparam>
	internal class SdmRepositoryCache<TItem> : RepositoryCache<string, TItem>
		where TItem : SdmObject<TItem>
	{
		private static readonly Exposer<TItem, string> _identifierExposer = (Exposer<TItem, string>)ExposerRegistry.findExposer(typeof(TItem), nameof(SdmObject<>.Identifier));

		private readonly IReadableRepository<TItem> _reader;

		/// <summary>
		/// Initializes a new instance of the <see cref="SdmRepositoryCache{TItem}"/> class.
		/// </summary>
		/// <param name="reader">The read repository for the SDM object.</param>
		public SdmRepositoryCache(IReadableRepository<TItem> reader)
			: base(
				  (item) => item.Identifier,
				  (identifier) => reader.Read(
					  FilterElementFactory.Create<TItem>(
						  _identifierExposer,
						  Comparer.Equals,
						  identifier))?.SingleOrDefault())
		{
			_reader = reader ?? throw new ArgumentNullException(nameof(reader));
		}

		/// <summary>
		/// Retrieves multiple items by key.
		/// Returns cached instances when available; otherwise calls the fetch delegate for each cache miss,
		/// stores the results, and returns them. Items where the fetch returns <c>null</c> are omitted from the result.
		/// </summary>
		public IEnumerable<TItem> Get(IEnumerable<string> keys)
		{
			if (keys == null)
				throw new ArgumentNullException(nameof(keys));

			ThrowIfDisposed();

			var results = new List<TItem>();
			var misses = new List<string>();

			_lock.EnterReadLock();
			try
			{
				foreach (var key in keys)
				{
					if (key == null)
						throw new ArgumentException("Sequence contains a null key.", nameof(keys));

					if (_cache.TryGetValue(key, out var cached))
						results.Add(cached);
					else
						misses.Add(key);
				}
			}
			finally
			{
				_lock.ExitReadLock();
			}

			if (misses.Count == 0)
				return results;

			_lock.EnterWriteLock();
			try
			{
				var filters = new List<FilterElement<TItem>>();
				foreach (var key in misses)
				{
					if (_cache.TryGetValue(key, out var cached))
					{
						results.Add(cached);
						continue;
					}

					filters.Add(_identifierExposer.Equal(key));
				}

				var fetched = _reader.Read(new ORFilterElement<TItem>(filters.ToArray()));
				fetched.ForEach(item => _cache[item.Identifier] = item);
			}
			finally
			{
				_lock.ExitWriteLock();
			}

			return results;
		}

		/// <summary>
		/// Retrieves multiple items by item.
		/// Returns cached instances when available; otherwise calls the fetch delegate for each cache miss,
		/// stores the results, and returns them. Items where the fetch returns <c>null</c> are omitted from the result.
		/// </summary>
		public IEnumerable<TItem> Get(IEnumerable<TItem> items)
		{
			return Get(items.Select(e => e.Identifier));
		}
	}
}
