namespace Skyline.DataMiner.Solutions.Relationships.Install.DOM
{
	using System;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Utils.DOM.Builders;
	using Skyline.DataMiner.Utils.DOM.Extensions;

	internal partial class DomInstaller
	{
		public void CreateDummyObjects(DomHelper helper)
		{
			// Create some entities first
			var sdmAbstractionsEntity_ID = new DomInstanceId(Guid.Parse("93c81292-351b-49c0-8048-06e1180f9c36"));
			var sdmAbstractionsEntity = new DomInstanceBuilder(EntityDescriptorDomMapper.DomDefinitionId)
				.WithID(sdmAbstractionsEntity_ID)
				.AddSection(new DomSectionBuilder(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId)
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.ID, "52173e49-9185-4772-9b60-c186ee365a81.Abstractions")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.DisplayName, "SDM Abstractions")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName, "standard_data_model_solution_registration")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionID, "52173e49-9185-4772-9b60-c186ee365a81")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionName, "SDM Registrations")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.Status, 0))
				.Build();

			Import(helper.DomInstances, DomInstanceExposers.Id.Equal(sdmAbstractionsEntity_ID), sdmAbstractionsEntity);

			var sdmRegistrationEntity_ID = new DomInstanceId(Guid.Parse("7fd7bf85-21e7-4dea-8bd3-3720ffded7f5"));
			var sdmRegistrationEntity = new DomInstanceBuilder(EntityDescriptorDomMapper.DomDefinitionId)
				.WithID(sdmRegistrationEntity_ID)
				.AddSection(new DomSectionBuilder(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId)
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.ID, "52173e49-9185-4772-9b60-c186ee365a81")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.DisplayName, "SDM Registrations")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName, "standard_data_model_solution_registration")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionID, "52173e49-9185-4772-9b60-c186ee365a81")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionName, "SDM Registrations")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.Status, 0))
				.Build();

			Import(helper.DomInstances, DomInstanceExposers.Id.Equal(sdmRegistrationEntity_ID), sdmRegistrationEntity);

			var relationships_ID = new DomInstanceId(Guid.Parse("b147ea22-3257-495f-a5a5-39805b7793a6"));
			var relationshipsEntity = new DomInstanceBuilder(EntityDescriptorDomMapper.DomDefinitionId)
				.WithID(relationships_ID)
				.AddSection(new DomSectionBuilder(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId)
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.ID, "fc085eae-dbc4-4d81-9758-096d46f7330d")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.DisplayName, "Relationships")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName, "standard_data_model_solution_registration")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionID, "52173e49-9185-4772-9b60-c186ee365a81")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionName, "SDM Registrations")
					.WithFieldValue(EntityDescriptorDomMapper.EntityDescriptorProperties.Status, 0))
				.Build();

			Import(helper.DomInstances, DomInstanceExposers.Id.Equal(relationships_ID), relationshipsEntity);

			// Create some relations between them
			var registration_abstractions_id = new DomInstanceId(Guid.Parse("7c1c8baa-0247-48d6-ba16-b67e0805d8d7"));
			var registration_abstractions = new DomInstanceBuilder(RelationDomMapper.DomDefinitionId)
				.WithID(registration_abstractions_id)
				.AddSection(new DomSectionBuilder(RelationDomMapper.RelationProperties.SectionDefinitionId)
					.WithFieldValue(RelationDomMapper.RelationProperties.SourceID, sdmRegistrationEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.ID).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.SourceModelName, sdmRegistrationEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.SourceSolutionID, sdmRegistrationEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionID).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.SourceState, sdmRegistrationEntity.GetFieldValue<int>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.Status).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.TargetID, sdmAbstractionsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.ID).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.TargetModelName, sdmAbstractionsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.TargetSolutionID, sdmAbstractionsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionID).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.TargetState, sdmAbstractionsEntity.GetFieldValue<int>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.Status).Value))
				.Build();

			Import(helper.DomInstances, DomInstanceExposers.Id.Equal(registration_abstractions_id), registration_abstractions);

			var relationships_abstractions_id = new DomInstanceId(Guid.Parse("fbaeab16-3828-4124-9a1e-3299ee8280b7"));
			var relationships_abstractions = new DomInstanceBuilder(RelationDomMapper.DomDefinitionId)
				.WithID(relationships_abstractions_id)
				.AddSection(new DomSectionBuilder(RelationDomMapper.RelationProperties.SectionDefinitionId)
					.WithFieldValue(RelationDomMapper.RelationProperties.SourceID, relationshipsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.ID).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.SourceModelName, relationshipsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.SourceSolutionID, relationshipsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionID).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.SourceState, relationshipsEntity.GetFieldValue<int>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.Status).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.TargetID, sdmAbstractionsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.ID).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.TargetModelName, sdmAbstractionsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.ModelName).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.TargetSolutionID, sdmAbstractionsEntity.GetFieldValue<string>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.SolutionID).Value)
					.WithFieldValue(RelationDomMapper.RelationProperties.TargetState, sdmAbstractionsEntity.GetFieldValue<int>(EntityDescriptorDomMapper.EntityDescriptorProperties.SectionDefinitionId, EntityDescriptorDomMapper.EntityDescriptorProperties.Status).Value))
				.Build();

			Import(helper.DomInstances, DomInstanceExposers.Id.Equal(relationships_abstractions_id), relationships_abstractions);
		}
	}
}
