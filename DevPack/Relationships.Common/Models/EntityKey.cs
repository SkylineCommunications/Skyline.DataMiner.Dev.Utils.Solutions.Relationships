namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;
	using System.Text.RegularExpressions;

	public class EntityKey : IEquatable<EntityKey>
	{
		public static readonly EntityKey Empty = new EntityKey(String.Empty, String.Empty, String.Empty);

		private static readonly Regex _entityKeyRegex = new Regex(
			@"\[(?<SolutionID>.+?)\]\s(?<ModelName>.+?)\s\(ID:\s(?<ID>.+?)\)",
			RegexOptions.Multiline | RegexOptions.Compiled);

		public EntityKey(string solutionID, string modelName, string id)
		{
			ID = id ?? String.Empty;
			ModelName = modelName ?? String.Empty;
			SolutionID = solutionID ?? String.Empty;
		}

		public string ID { get; }

		public string ModelName { get; }

		public string SolutionID { get; }

		public static bool operator ==(EntityKey left, EntityKey right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(EntityKey left, EntityKey right)
		{
			return !Equals(left, right);
		}

		public bool Equals(EntityKey other)
		{
			return String.Equals(SolutionID, other.SolutionID, StringComparison.OrdinalIgnoreCase)
				&& String.Equals(ModelName, other.ModelName, StringComparison.OrdinalIgnoreCase)
				&& String.Equals(ID, other.ID, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			return obj is EntityKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 31) + (SolutionID ?? String.Empty).GetHashCode();
				hash = (hash * 31) + (ModelName ?? String.Empty).GetHashCode();
				hash = (hash * 31) + (ID ?? String.Empty).GetHashCode();
				return hash;
			}
		}

		public override string ToString()
		{
			return String.Format("[{0}] {1} (ID: {2})", SolutionID, ModelName, ID);
		}

		public static EntityKey Parse(string key)
		{
			if (String.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
			}

			var match = _entityKeyRegex.Match(key);
			if (!match.Success)
			{
				throw new FormatException("Key is not in the correct format. [{SolutionID}] {ModelName} (ID: {ID})");
			}

			return new EntityKey(
				match.Groups["SolutionID"].Value,
				match.Groups["ModelName"].Value,
				match.Groups["ID"].Value);
		}
	}
}
