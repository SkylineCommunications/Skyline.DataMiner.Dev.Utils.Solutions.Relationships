namespace Relationships.Install.Migration
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SdmRelationships;
	using DomHelpers.SlcObject_Linking;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Modules;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
		private Dictionary<(string ID, string ModelName, string SolutionID), Guid> _keyToIdentifierMap = new Dictionary<(string ID, string ModelName, string SolutionID), Guid>();
		private HashSet<(string SourceID, string SourceModelName, string SourceSolutionID, string TargetID, string TargetModelName, string TargetSolutionID)> _relations = new HashSet<(string SourceID, string SourceModelName, string SourceSolutionID, string TargetID, string TargetModelName, string TargetSolutionID)>();

		/// <summary>
		/// The script entry point.
		/// </summary>
		/// <param name="engine">Link with SLAutomation process.</param>
		public void Run(IEngine engine)
		{
			try
			{
				RunSafe(engine);
			}
			catch (ScriptAbortException)
			{
				// Catch normal abort exceptions (engine.ExitFail or engine.ExitSuccess)
				throw; // Comment if it should be treated as a normal exit of the script.
			}
			catch (ScriptForceAbortException)
			{
				// Catch forced abort exceptions, caused via external maintenance messages.
				throw;
			}
			catch (ScriptTimeoutException)
			{
				// Catch timeout exceptions for when a script has been running for too long.
				throw;
			}
			catch (InteractiveUserDetachedException)
			{
				// Catch a user detaching from the interactive script by closing the window.
				// Only applicable for interactive scripts, can be removed for non-interactive scripts.
				throw;
			}
			catch (Exception e)
			{
				engine.ExitFail("Run|Something went wrong: " + e);
			}
		}

		private void RunSafe(IEngine engine)
		{
			var moduleHelper = new ModuleSettingsHelper(engine.SendSLNetMessages);
			if (moduleHelper.ModuleSettings.Count(ModuleSettingsExposers.ModuleId.Equal(SlcObject_LinkingIds.ModuleId)) <= 0)
			{
				engine.Log("Nothing to migrate");
				return;
			}

			var linkHelper = new DomHelper(engine.SendSLNetMessages, SlcObject_LinkingIds.ModuleId);
			var relationHelper = new DomHelper(engine.SendSLNetMessages, SdmRelationshipsIds.ModuleId);

			var filter = new ANDFilterElement<DomInstance>(
				DomInstanceExposers.DomDefinitionId.Equal(SlcObject_LinkingIds.Definitions.Link.Id),
				DomInstanceExposers.FieldValues.KeyExists(SlcObject_LinkingIds.Sections.EntityDescriptor.ID.Id.ToString()).Equal(true));
			var linkCount = linkHelper.DomInstances.Count(filter);
			var pagingHelper = linkHelper.DomInstances.PreparePaging(filter, linkHelper.DomInstances.MaxAmountBulkOperation);
			while (pagingHelper.MoveToNextPage())
			{
				var page = pagingHelper.GetCurrentPage();
				var contexts = TranslateLinkBatch(page.Select(instance => new LinkInstance(instance)));
				var relationSuccessBatch = CreateOrUpdateBatch(relationHelper, contexts.SelectMany(c => c.Relations).Select(c => (DomInstance)c).ToList());
				var entitySuccessBatch = CreateOrUpdateBatch(relationHelper, contexts.SelectMany(c => c.EntityDescriptors).Select(c => (DomInstance)c).ToList());

				var linkAmountBatch = page.Count;
				var linkSuccessBatch = DeleteBatch(linkHelper, contexts.Select(c => (DomInstance)c.Link).ToList());
			}

			if (linkHelper.DomInstances.Count(new TRUEFilterElement<DomInstance>()) > 0)
			{
				engine.ExitFail("RunSafe|Not all links could be migrated.");
			}
			else
			{
				var objectLinkingSettings = moduleHelper.ModuleSettings.Read(ModuleSettingsExposers.ModuleId.Equal(SlcObject_LinkingIds.ModuleId)).FirstOrDefault();
				if (objectLinkingSettings is null)
				{
					return;
				}

				moduleHelper.ModuleSettings.Delete(objectLinkingSettings);
			}
		}

		private IList<LinkMigrationContext> TranslateLinkBatch(IEnumerable<LinkInstance> links)
		{
			var result = new List<LinkMigrationContext>();
			foreach (var link in links)
			{
				var context = new LinkMigrationContext(link);
				context.Relations.AddRange(TranslateRelations(link));
				context.EntityDescriptors.AddRange(TranslateEntities(link));

				result.Add(context);
			}

			return result;
		}

		private IEnumerable<RelationInstance> TranslateRelations(LinkInstance linkInstace)
		{
			foreach (var source in linkInstace.EntityDescriptor)
			{
				foreach (var target in linkInstace.EntityDescriptor.Where(e => !e.Equals(source)))
				{
					var key = CreateRelationKey(source, target);
					if (!_relations.Add(key))
					{
						continue;
					}

					yield return new RelationInstance
					{
						RelationProperties = new RelationPropertiesSection
						{
							SourceID = source.ID,
							SourceModelName = String.IsNullOrEmpty(source.ModelName) ? "Migrated" : source.ModelName,
							SourceSolutionID = String.IsNullOrEmpty(source.SolutionName) ? "Migrated" : source.SolutionName, // SolutionID is not available in the old format, so we use the SolutionName as a fallback.
							SourceStatus = SdmRelationshipsIds.Enums.Sourcestatus.Active,
							TargetID = target.ID,
							TargetModelName = String.IsNullOrEmpty(target.ModelName) ? "Migrated" : target.ModelName,
							TargetSolutionID = String.IsNullOrEmpty(target.SolutionName) ? "Migrated" : target.SolutionName, // SolutionID is not available in the old format, so we use the SolutionName as a fallback.
							TargetStatus = SdmRelationshipsIds.Enums.Targetstatus.Active,
						},
					};
				}
			}
		}

		private IEnumerable<EntityDescriptorInstance> TranslateEntities(LinkInstance linkInstance)
		{
			foreach (var entity in linkInstance.EntityDescriptor)
			{
				var identifier = Guid.NewGuid();
				var key = CreateEntityKey(entity);
				if (_keyToIdentifierMap.TryGetValue(key, out var existingIdentifier))
				{
					identifier = existingIdentifier;
				}
				else
				{
					_keyToIdentifierMap.Add(key, identifier);
				}

				yield return new EntityDescriptorInstance(identifier)
				{
					EntityDescriptorProperties = new EntityDescriptorPropertiesSection
					{
						ID = entity?.ID ?? String.Empty,
						DisplayName = entity?.DisplayName ?? String.Empty,
						ModelName = String.IsNullOrEmpty(entity.ModelName) ? "Migrated" : entity.ModelName,
						SolutionID = String.IsNullOrEmpty(entity.SolutionName) ? "Migrated" : entity.SolutionName, // SolutionID is not available in the old format, so we use the SolutionName as a fallback.
						SolutionName = String.IsNullOrEmpty(entity.SolutionName) ? "Migrated" : entity.SolutionName,
						ParentID = entity?.ParentID ?? String.Empty,
						ParentModelName = entity?.ParentModelName ?? String.Empty,
						Status = SdmRelationshipsIds.Enums.Status.Active,
					},
				};
			}
		}

		private static (string ID, string ModelName, string SolutionID) CreateEntityKey(EntityDescriptorSection entity)
		{
			return (entity?.ID ?? String.Empty,
				entity?.ModelName ?? String.Empty,
				String.IsNullOrEmpty(entity.SolutionName) ? "Migrated" : entity.SolutionName);
		}

		private static (string SourceID, string SourceModelName, string SourceSolutionID, string TargetID, string TargetModelName, string TargetSolutionID) CreateRelationKey(EntityDescriptorSection source, EntityDescriptorSection target)
		{
			return (source?.ID ?? String.Empty,
				source?.ModelName ?? String.Empty,
				String.IsNullOrEmpty(source.SolutionName) ? "Migrated" : source.SolutionName,
				target?.ID ?? String.Empty,
				target?.ModelName ?? String.Empty,
				String.IsNullOrEmpty(target.SolutionName) ? "Migrated" : target.SolutionName);
		}

		private static int DeleteBatch(DomHelper helper, List<DomInstance> instances)
		{
			if (instances.Count <= 0)
			{
				return 0;
			}

			foreach (var batch in instances.Batch(helper.DomInstances.MaxAmountBulkOperation))
			{
				helper.DomInstances.Delete(batch.ToList());
			}

			return instances.Count;
		}

		private static int CreateOrUpdateBatch(DomHelper helper, List<DomInstance> instances)
		{
			if (instances.Count <= 0)
			{
				return 0;
			}

			foreach (var batch in instances.Batch(helper.DomInstances.MaxAmountBulkOperation))
			{
				helper.DomInstances.CreateOrUpdate(batch.ToList());
			}

			return instances.Count;
		}
	}
}
