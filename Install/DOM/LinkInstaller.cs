namespace Skyline.DataMiner.Solutions.ObjectLinking.Install.DOM
{
	using System;
	using System.Linq;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.GenericEnums;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.ObjectLinking.Shared;
	using Skyline.DataMiner.Utils.DOM.Builders;

	internal partial class DomInstaller
	{
		private void InstallLinks(DomHelper helper)
		{
			var linkPropertiesSection = new SectionDefinitionBuilder()
				.WithName("Link Properties")
				.WithID(LinkDomMapper.LinkProperties.SectionDefinitionId)
				.AddFieldDescriptor(new GenericEnumFieldDescriptorBuilder()
					.WithEnumType(GenericEnumFieldDescriptorBuilder.EnumType.Int)
					.WithID(LinkDomMapper.LinkProperties.Direction)
					.WithName("Direction")
					.AddEnumValue(new GenericEnumEntry<int>("Symmetric", 0))
					.AddEnumValue(new GenericEnumEntry<int>("Forward", 1))
					.AddEnumValue(new GenericEnumEntry<int>("Backward", 2))
					.WithIsOptional(true)
					.WithTooltip("Indicates the direction of the link."))
				.Build();

			Import(helper.SectionDefinitions, SectionDefinitionExposers.ID.Equal(LinkDomMapper.LinkProperties.SectionDefinitionId), linkPropertiesSection);
			Log("Installed Link Properties section definition");

			var sourceSection = new SectionDefinitionBuilder()
				.WithName("Source Entity Descriptor")
				.WithID(LinkDomMapper.Source.SectionDefinitionId)
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Source.ID)
					.WithName("ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The unique identifier of the source entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Source.DisplayName)
					.WithName("Display Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The display name of the source entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Source.ModelName)
					.WithName("Model Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The name of the model the source entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Source.SolutionID)
					.WithName("Solution ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The unique identifier of the solution the source entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Source.SolutionName)
					.WithName("Solution Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The name of the solution the source entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Source.ParentID)
					.WithName("Parent ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The unique identifier of the parent entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Source.ParentModelName)
					.WithName("Parent Model Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The model name of the parent entity"))
				.AddFieldDescriptor(new GenericEnumFieldDescriptorBuilder()
					.WithEnumType(GenericEnumFieldDescriptorBuilder.EnumType.Int)
					.WithID(LinkDomMapper.Source.Status)
					.WithName("Status")
					.AddEnumValue(new GenericEnumEntry<int>("Active", 0))
					.AddEnumValue(new GenericEnumEntry<int>("Inactive", 1))
					.WithIsOptional(true)
					.WithTooltip("Indicates the Status of the source entity."))
				.Build();

			Import(helper.SectionDefinitions, SectionDefinitionExposers.ID.Equal(LinkDomMapper.Source.SectionDefinitionId), sourceSection);
			Log("Installed Source Entity Descriptor section definition");

			var targetSection = new SectionDefinitionBuilder()
				.WithName("Target Entity Descriptor")
				.WithID(LinkDomMapper.Target.SectionDefinitionId)
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Target.ID)
					.WithName("ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The unique identifier of the target entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Target.DisplayName)
					.WithName("Display Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The display name of the target entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Target.ModelName)
					.WithName("Model Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The name of the model the target entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Target.SolutionID)
					.WithName("Solution ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The unique identifier of the solution the target entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Target.SolutionName)
					.WithName("Solution Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The name of the solution the target entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Target.ParentID)
					.WithName("Parent ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The unique identifier of the parent entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(LinkDomMapper.Target.ParentModelName)
					.WithName("Parent Model Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithTooltip("The model name of the parent entity"))
				.AddFieldDescriptor(new GenericEnumFieldDescriptorBuilder()
					.WithEnumType(GenericEnumFieldDescriptorBuilder.EnumType.Int)
					.WithID(LinkDomMapper.Target.Status)
					.WithName("Status")
					.AddEnumValue(new GenericEnumEntry<int>("Active", 0))
					.AddEnumValue(new GenericEnumEntry<int>("Inactive", 1))
					.WithIsOptional(true)
					.WithTooltip("Indicates the Status of the target entity."))
				.Build();

			Import(helper.SectionDefinitions, SectionDefinitionExposers.ID.Equal(LinkDomMapper.Target.SectionDefinitionId), targetSection);
			Log("Installed Target Entity Descriptor section definition");

			var linkDefinitionBuilder = new DomDefinitionBuilder()
				.WithID(LinkDomMapper.DomDefinitionId)
				.WithName("Link")
				.AddSectionDefinitionLink(new Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions.SectionDefinitionLink
				{
					SectionDefinitionID = LinkDomMapper.LinkProperties.SectionDefinitionId,
					AllowMultipleSections = false,
					IsOptional = true,
				})
				.AddSectionDefinitionLink(new Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions.SectionDefinitionLink
				{
					SectionDefinitionID = LinkDomMapper.Source.SectionDefinitionId,
					AllowMultipleSections = false,
					IsOptional = true,
				})
				.AddSectionDefinitionLink(new Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions.SectionDefinitionLink
				{
					SectionDefinitionID = LinkDomMapper.Target.SectionDefinitionId,
					AllowMultipleSections = false,
					IsOptional = true,
				});

			var needsMigration = false;
			var linkDefinition = linkDefinitionBuilder.Build();
			var normalLinkDefinition = (DomDefinition)linkDefinitionBuilder.Build().Clone();
			var oldEntityDescriptionSectionId = new SectionDefinitionID(new Guid("75dd33f0-204e-4b51-bec3-3ae57065ebc0"))
			{
				ModuleId = "(slc)object_linking",
			};
			var existingLinkDefinition = helper.DomDefinitions.Read(DomDefinitionExposers.Id.Equal(LinkDomMapper.DomDefinitionId)).FirstOrDefault();
			if (!(existingLinkDefinition is null) &&
				existingLinkDefinition.SectionDefinitionLinks.Any(link => oldEntityDescriptionSectionId.Equals(link.SectionDefinitionID)))
			{
				// A migration needs to happen and we cannot simply remove it.
				needsMigration = true;
				linkDefinitionBuilder.AddSectionDefinitionLink(new Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions.SectionDefinitionLink
				{
					SectionDefinitionID = oldEntityDescriptionSectionId,
					AllowMultipleSections = true,
					IsOptional = true,
				});

				linkDefinition = linkDefinitionBuilder.Build();
			}

			Import(helper.DomDefinitions, DomDefinitionExposers.Id.Equal(LinkDomMapper.DomDefinitionId), linkDefinition);
			Log("Installed DOM definition for Link");

			if (!needsMigration)
			{
				return;
			}

			// Trigger migration script
			Log("Found a previous object linking installation, starting migration...");

			try
			{
				Log($"Migrating object linking data to the new format...");

				var subScript = _engine.PrepareSubScript(Constants.DomScriptName);
				subScript.Synchronous = true;
				subScript.InheritScriptOutput = true;
				subScript.ExtendedErrorInfo = true;
				subScript.StartScript();

				var output = subScript.GetScriptResult();
				var amount = output["amount"];
				var successfull = output["success"];
				Log($"Migrated {successfull} of the {amount} old links.");

				var errors = subScript.GetErrorMessages();
				if (errors.Length <= 0)
				{
					Import(helper.DomDefinitions, DomDefinitionExposers.Id.Equal(LinkDomMapper.DomDefinitionId), normalLinkDefinition);
					var oldSectionDefinition = helper.SectionDefinitions.Read(SectionDefinitionExposers.ID.Equal(oldEntityDescriptionSectionId)).FirstOrDefault();
					if (!(oldSectionDefinition is null))
					{
						helper.SectionDefinitions.Delete(oldSectionDefinition);
					}

					return;
				}

				foreach (var error in errors)
				{
					Log($"Error during migration: {error}");
					_engine.ExitFail($"Error during migration: {error}");
				}
			}
			catch (Exception ex)
			{
				Log("Failed to remove previous definitions from the old object linking.");
				Log(ex.Message);
				Log(ex.StackTrace);
				_engine.ExitFail("Failed to remove previous definitions from the old object linking.");
			}
		}
	}
}
