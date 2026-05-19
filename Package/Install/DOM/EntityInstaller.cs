namespace Skyline.DataMiner.Solutions.Relationships.Install.DOM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Concatenation;
	using Skyline.DataMiner.Net.GenericEnums;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Utils.DOM.Builders;

	internal partial class DomInstaller
	{
		private void InstallEntity(DomHelper helper)
		{
			var entitySection = new SectionDefinitionBuilder()
				.WithName("Entity Descriptor Properties")
				.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId)
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.ID)
					.WithName("ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The unique identifier of the entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.DisplayName)
					.WithName("Display Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The display name of the entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName)
					.WithName("Model Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The name of the model the entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionID)
					.WithName("Solution ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The unique identifier of the solution the entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionName)
					.WithName("Solution Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The name of the solution the entity is associated with."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.ParentID)
					.WithName("Parent ID")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The unique identifier of the parent entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.ParentModelName)
					.WithName("Parent Model Name")
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The name of the model the parent entity is associated with."))
				.AddFieldDescriptor(CreateBaseEntityStatusEnumFieldDescriptorBuilder()
					.WithID(EntityDescriptorDomMapper.EntityDescriptorProperties.Status)
					.WithName("Status")
					.WithTooltip("The current status of the entity. Active indicates the entity is in use, Inactive indicates it is disabled."))
				.Build();

			Import(helper.SectionDefinitions, SectionDefinitionExposers.ID.Equal(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId), entitySection);
			Log("Installed Entity Descriptor section definition.");

			var entityDefinition = new DomDefinitionBuilder()
				.WithID(EntityDescriptorDomMapper.DomDefinitionId)
				.WithName("Entity Descriptor")
				.AddSectionDefinitionLink(new Net.Apps.Sections.SectionDefinitions.SectionDefinitionLink
				{
					AllowMultipleSections = false,
					IsOptional = true,
					IsSoftDeleted = false,
					SectionDefinitionID = EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId,
				})
				.Build();

			// Name definition
			var overrides = entityDefinition.ModuleSettingsOverrides ?? new ModuleSettingsOverrides();
			var nameDefinition = overrides.NameDefinition ?? new DomInstanceNameDefinition();
			nameDefinition.ConcatenationItems = new List<IDomInstanceConcatenationItem>
			{
				new StaticValueConcatenationItem("["),
				new FieldValueConcatenationItem(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionName),
				new StaticValueConcatenationItem(", "),
				new FieldValueConcatenationItem(EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName),
				new StaticValueConcatenationItem("] "),
				new FieldValueConcatenationItem(EntityDescriptorDomMapper.EntityDescriptorProperties.DisplayName),
			};

			overrides.NameDefinition = nameDefinition;
			entityDefinition.ModuleSettingsOverrides = overrides;

			Import(helper.DomDefinitions, DomDefinitionExposers.Id.Equal(EntityDescriptorDomMapper.DomDefinitionId), entityDefinition);
			Log("Installed Entity Descriptor DOM definition.");
		}
	}
}
