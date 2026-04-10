namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.SDM;

	/// <summary>
	/// Represents a wrapper class for accessing a Link DOM instance.
	/// The <see cref="Link"/> class provides simplified access to the data and functionality of the underlying DOM instance, allowing for easier manipulation and retrieval of data from DOM.
	/// </summary>
	[GenerateExposers]
	////[SdmDomStorage("(slc)object_linking")]
	public sealed class Link : SdmObject<Link>
	{
		public override string Identifier { get; set; } = Guid.NewGuid().ToString();

		public LinkDirection Direction { get; set; }

		public EntityDescriptor Source { get; set; }

		public EntityDescriptor Target { get; set; }

		public DateTime CreatedAt { get; internal set; }

		public string CreatedBy { get; internal set; }

		public DateTime LastModified { get; internal set; }

		public string LastModifiedBy { get; internal set; }

		[SdmIgnore]
		public IEnumerable<EntityDescriptor> EntityDescriptors
		{
			get
			{
				if (Direction != LinkDirection.Backward)
				{
					yield return Source;
					yield return Target;
				}
				else
				{
					yield return Target;
					yield return Source;
				}
			}
		}
	}
}
