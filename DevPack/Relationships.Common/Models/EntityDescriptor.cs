namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;

	using Skyline.DataMiner.SDM;

	/// <summary>
	/// Represents a description of an entity.
	/// </summary>
	[GenerateExposers]
	public sealed partial class EntityDescriptor : SdmObject<EntityDescriptor>
	{
		public sealed override string Identifier { get; set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// Gets the key of the entity.
		/// </summary>
		[SdmIgnore]
		public EntityKey Key => new EntityKey(SolutionID, ModelName, ID);

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
		public string ParentID { get; set; } = String.Empty;

		/// <summary>
		/// Gets or sets the ParentModelName field of the entity.
		/// </summary>
		public string ParentModelName { get; set; } = String.Empty;

		/// <summary>
		/// Gets or sets the current status of the entity.
		/// </summary>
		public EntityStatus Status { get; set; }
	}
}
