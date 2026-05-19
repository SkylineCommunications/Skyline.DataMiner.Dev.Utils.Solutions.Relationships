namespace Skyline.DataMiner.Solutions.Relationships
{
	using System.Collections.Generic;

	internal static class RelationComparison
	{
		public static readonly IEqualityComparer<Relation> ValueComparer = new ValueEqualityComparer();

		private sealed class ValueEqualityComparer : IEqualityComparer<Relation>
		{
			public bool Equals(Relation x, Relation y)
			{
				if (ReferenceEquals(x, y))
					return true;

				if (x is null || y is null)
					return false;

				return x.SourceKey == y.SourceKey
					&& x.TargetKey == y.TargetKey;
			}

			public int GetHashCode(Relation obj)
			{
				if (obj is null)
					return 0;

				unchecked
				{
					int hash = 17;
					hash = (hash * 23) + (obj.SourceKey?.GetHashCode() ?? 0);
					hash = (hash * 23) + (obj.TargetKey?.GetHashCode() ?? 0);
					return hash;
				}
			}
		}
	}
}
