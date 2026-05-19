namespace Skyline.DataMiner.Solutions.Relationships.Install.DOM
{
	using System.Collections.Generic;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Concatenation;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Utils.DOM.Builders;

	internal partial class DomInstaller
	{
		private void InstallRelation(DomHelper helper)
		{
			var relationProperties = new SectionDefinitionBuilder()
				.WithName("Relation Properties")
				.WithID(RelationDomMapper.RelationProperties.SectionDefinitionId)
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithName("Source ID")
					.WithID(RelationDomMapper.RelationProperties.SourceID)
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The unique identifier of the source entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithName("Source Model Name")
					.WithID(RelationDomMapper.RelationProperties.SourceModelName)
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The model name of the source entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithName("Source Solution ID")
					.WithID(RelationDomMapper.RelationProperties.SourceSolutionID)
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The solution identifier associated with the source entity."))
				.AddFieldDescriptor(CreateBaseEntityStatusEnumFieldDescriptorBuilder()
					.WithID(RelationDomMapper.RelationProperties.SourceState)
					.WithName("Source Status")
					.WithTooltip("The current status of the source entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithName("Target ID")
					.WithID(RelationDomMapper.RelationProperties.TargetID)
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The unique identifier of the target entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithName("Target Model Name")
					.WithID(RelationDomMapper.RelationProperties.TargetModelName)
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The model name of the target entity."))
				.AddFieldDescriptor(new FieldDescriptorBuilder()
					.WithName("Target Solution ID")
					.WithID(RelationDomMapper.RelationProperties.TargetSolutionID)
					.WithType(typeof(string))
					.WithIsOptional(true)
					.WithSoftDeleted(false)
					.WithReadonly(false)
					.WithHidden(false)
					.WithTooltip("The solution identifier associated with the target entity."))
				.AddFieldDescriptor(CreateBaseEntityStatusEnumFieldDescriptorBuilder()
					.WithID(RelationDomMapper.RelationProperties.TargetState)
					.WithName("Target Status")
					.WithTooltip("The current status of the target entity."))
				.Build();

			Import(helper.SectionDefinitions, SectionDefinitionExposers.ID.Equal(RelationDomMapper.RelationProperties.SectionDefinitionId), relationProperties);
			Log("Installed Relation Properties section definition.");

			var relation = new DomDefinitionBuilder()
				.WithID(RelationDomMapper.DomDefinitionId)
				.WithName("Relation")
				.AddSectionDefinitionLink(new Net.Apps.Sections.SectionDefinitions.SectionDefinitionLink
				{
					AllowMultipleSections = false,
					IsOptional = true,
					IsSoftDeleted = false,
					SectionDefinitionID = RelationDomMapper.RelationProperties.SectionDefinitionId,
				})
				.Build();

			// Name definition
			var overrides = relation.ModuleSettingsOverrides ?? new ModuleSettingsOverrides();
			var nameDefinition = overrides.NameDefinition ?? new DomInstanceNameDefinition();
			nameDefinition.ConcatenationItems = new List<IDomInstanceConcatenationItem>
			{
				new FieldValueConcatenationItem(RelationDomMapper.RelationProperties.SourceID),
				new StaticValueConcatenationItem(" -> "),
				new FieldValueConcatenationItem(RelationDomMapper.RelationProperties.TargetID),
			};

			overrides.NameDefinition = nameDefinition;
			relation.ModuleSettingsOverrides = overrides;

			Import(helper.DomDefinitions, DomDefinitionExposers.Id.Equal(RelationDomMapper.DomDefinitionId), relation);
			Log("Installed Relation DOM definition.");
		}
	}
}
