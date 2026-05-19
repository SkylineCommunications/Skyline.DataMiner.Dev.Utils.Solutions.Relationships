namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;
	using System.Collections.Generic;

	internal static class EntityComparison
	{
		public static readonly IEqualityComparer<EntityDescriptor> ValueComparer = new ValueEqualityComparer();

		private sealed class ValueEqualityComparer : IEqualityComparer<EntityDescriptor>
		{
			public bool Equals(EntityDescriptor x, EntityDescriptor y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				if (x is null || y is null)
				{
					return false;
				}

				return String.Equals(x.ID, y.ID, StringComparison.InvariantCulture)
					&& String.Equals(x.DisplayName, y.DisplayName, StringComparison.InvariantCulture)
					&& String.Equals(x.ModelName, y.ModelName, StringComparison.InvariantCulture)
					&& String.Equals(x.SolutionID, y.SolutionID, StringComparison.InvariantCulture)
					&& String.Equals(x.SolutionName, y.SolutionName, StringComparison.InvariantCulture)
					&& String.Equals(x.ParentID, y.ParentID, StringComparison.InvariantCulture)
					&& String.Equals(x.ParentModelName, y.ParentModelName, StringComparison.InvariantCulture)
					&& x.Status == y.Status;
			}

			public int GetHashCode(EntityDescriptor obj)
			{
				if (obj is null)
				{
					return 0;
				}

				unchecked
				{
					int hash = 17;

					hash = (hash * 23) + StringComparer.InvariantCulture.GetHashCode(obj.ID ?? String.Empty);
					hash = (hash * 23) + StringComparer.InvariantCulture.GetHashCode(obj.DisplayName ?? String.Empty);
					hash = (hash * 23) + StringComparer.InvariantCulture.GetHashCode(obj.ModelName ?? String.Empty);
					hash = (hash * 23) + StringComparer.InvariantCulture.GetHashCode(obj.SolutionID ?? String.Empty);
					hash = (hash * 23) + StringComparer.InvariantCulture.GetHashCode(obj.SolutionName ?? String.Empty);
					hash = (hash * 23) + StringComparer.InvariantCulture.GetHashCode(obj.ParentID ?? String.Empty);
					hash = (hash * 23) + StringComparer.InvariantCulture.GetHashCode(obj.ParentModelName ?? String.Empty);
					hash = (hash * 23) + obj.Status.GetHashCode();

					return hash;
				}
			}
		}
	}
}
