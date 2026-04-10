namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	using System;

	/// <summary>
	/// Represents a description of an entity.
	/// </summary>
	public sealed partial class EntityDescriptor : IEquatable<EntityDescriptor>
	{
		/// <summary>
		/// Gets or sets the identifier of the entity.
		/// </summary>
		/// <remarks>
		/// This property is required and must be set before saving it.
		/// </remarks>
		public string ID { get; set; }

		/// <summary>
		/// Gets or sets the DisplayName of the entity.
		/// </summary>
		/// <remarks>
		/// This property is required and must be set before saving it.
		/// </remarks>
		public string DisplayName { get; set; }

		/// <summary>
		/// Gets or sets the ModelName field of the entity.
		/// </summary>
		/// <remarks>
		/// This property is required and must be set before saving it.
		/// </remarks>
		public string ModelName { get; set; }

		/// <summary>
		/// Gets or sets the SolutionID field of the entity.
		/// </summary>
		/// <remarks>
		/// This property is required and must be set before saving it.
		/// </remarks>
		public string SolutionID { get; set; }

		/// <summary>
		/// Gets or sets the SolutionName field of the entity.
		/// </summary>
		/// <remarks>
		/// This property is required and must be set before saving it.
		/// </remarks>
		public string SolutionName { get; set; }

		/// <summary>
		/// Gets or sets the ParentID field of the entity.
		/// </summary>
		public string ParentID { get; set; }

		/// <summary>
		/// Gets or sets the ParentModelName field of the entity.
		/// </summary>
		public string ParentModelName { get; set; }

		/// <summary>
		/// Gets or sets the current status of the entity.
		/// </summary>
		public EntityStatus Status { get; set; }

		/// <summary>
		/// Determines whether the specified <see cref="EntityDescriptor"/> is equal to the current instance.
		/// Two instances are considered equal if they have the same <see cref="ID"/>, <see cref="ModelName"/>,
		/// <see cref="SolutionName"/>, <see cref="ParentID"/>, and <see cref="ParentModelName"/> values
		/// using an invariant culture comparison.
		/// </summary>
		/// <param name="other">The <see cref="EntityDescriptor"/> to compare with the current instance.</param>
		/// <returns><c>true</c> if the specified <see cref="EntityDescriptor"/> is equal to the current instance; otherwise, <c>false</c>.</returns>
		public bool Equals(EntityDescriptor other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return String.Equals(ID, other.ID, StringComparison.InvariantCulture)
				&& String.Equals(ModelName, other.ModelName, StringComparison.InvariantCulture)
				&& String.Equals(SolutionName, other.SolutionName, StringComparison.InvariantCulture)
				&& String.Equals(ParentID, other.ParentID, StringComparison.InvariantCulture)
				&& String.Equals(ParentModelName, other.ParentModelName, StringComparison.InvariantCulture);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as EntityDescriptor);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + (ID?.GetHashCode() ?? 0);
				hash = (hash * 23) + (ModelName?.GetHashCode() ?? 0);
				hash = (hash * 23) + (SolutionName?.GetHashCode() ?? 0);
				hash = (hash * 23) + (ParentID?.GetHashCode() ?? 0);
				hash = (hash * 23) + (ParentModelName?.GetHashCode() ?? 0);
				return hash;
			}
		}
	}
}
