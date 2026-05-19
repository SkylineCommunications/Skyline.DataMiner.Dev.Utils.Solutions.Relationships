namespace Relationships.Install.Migration
{
	using System.Collections.Generic;

	using DomHelpers.SdmRelationships;
	using DomHelpers.SlcObject_Linking;

	internal sealed class LinkMigrationContext
	{
		public LinkMigrationContext(LinkInstance link)
		{
			Link = link;
		}

		public LinkInstance Link { get; }

		public List<RelationInstance> Relations { get; set; } = new List<RelationInstance>();

		public List<EntityDescriptorInstance> EntityDescriptors { get; set; } = new List<EntityDescriptorInstance>();

		public bool LinkRemoved { get; set; }

		public bool RelationsCreated { get; set; }

		public bool Successful => LinkRemoved && RelationsCreated;
	}
}
