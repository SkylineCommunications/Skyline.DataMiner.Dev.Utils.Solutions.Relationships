namespace InstallDOM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using DomHelpers.SlcObject_Linking;
	using DomHelpers.SlcObject_Linking_Old;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// Represents a DataMiner Automation script.
	/// </summary>
	public class Script
	{
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

		private static void RunSafe(IEngine engine)
		{
			var amount = 0;
			var success = 0;

			var helper = new DomHelper(engine.SendSLNetMessages, SlcObject_LinkingIds.ModuleId);

			var filter = new ANDFilterElement<DomInstance>(
				DomInstanceExposers.DomDefinitionId.Equal(SlcObject_LinkingIds_Old.Definitions.Link.Id),
				DomInstanceExposers.FieldValues.KeyExists(SlcObject_LinkingIds_Old.Sections.EntityDescriptor.ID.Id.ToString()).Equal(true));
			var pagingHelper = helper.DomInstances.PreparePaging(filter, helper.DomInstances.MaxAmountBulkOperation - 5);
			while (pagingHelper.MoveToNextPage())
			{
				var page = pagingHelper.GetCurrentPage();

				var toBeRemoved = new List<DomInstance>();
				var links = new List<LinkInstance>();
				var batch = page.Select(instance => new LinkInstance_Old(instance));
				batch.ForEach(link =>
				{
					try
					{
						var translated = TranslateToNewFormat(link, out var removed);
						links.AddRange(translated);
						amount += translated.Count;
						if (removed is null)
						{
							return;
						}

						toBeRemoved.Add(removed);
					}
					catch
					{
						engine.Log($"Failed to translate link with id {link.ID.Id}, skipping. Link details: {JsonConvert.SerializeObject(link)}");
					}
				});

				if (!links.Any())
				{
					continue;
				}

				try
				{
					helper.DomInstances.CreateOrUpdate(links.Cast<DomInstance>().ToList());
					helper.DomInstances.Delete(toBeRemoved);
					success += page.Count;
				}
				catch (BulkCrudFailedException<DomInstance> ex)
				{
					engine.Log($"Failed to migrate a batch of links: {ex}");
					success += ex.Result.SuccessfulIds.Count;
				}
			}

			engine.AddOrUpdateScriptOutput("amount", Convert.ToString(amount));
			engine.AddOrUpdateScriptOutput("success", Convert.ToString(success));
		}

		private static List<LinkInstance> TranslateToNewFormat(LinkInstance_Old linkInstance, out DomInstance removed)
		{
			removed = null;
			var result = new List<LinkInstance>();

			if (linkInstance is null ||
				linkInstance.EntityDescriptor.Count == 0)
			{
				// link is already migrated
				return result;
			}

			// First check if it's an easy translation, meaning only 2 entity descriptors, one source and one target.
			if (linkInstance.EntityDescriptor.Count == 2)
			{
				result.Add(TranslateToNewFormat(linkInstance.EntityDescriptor[0], linkInstance.EntityDescriptor[1], linkInstance.ID.Id));
				return result;
			}

			// otherwise create a link per combination
			var seen = new HashSet<string>();
			for (int i = 0; i < linkInstance.EntityDescriptor.Count; i++)
			{
				var section1 = linkInstance.EntityDescriptor[i];
				for (int j = 0; j < linkInstance.EntityDescriptor.Count; j++)
				{
					var section2 = linkInstance.EntityDescriptor[j];
					if (section1 == section2)
					{
						continue;
					}

					var forward = $"{section1.ID} - {section2.ID}";
					var backward = $"{section1.ID} - {section2.ID}";
					if (seen.Contains(forward) ||
						seen.Contains(backward))
					{
						continue;
					}

					result.Add(TranslateToNewFormat(section1, section2));
					seen.Add(forward);
					seen.Add(backward);
				}
			}

			removed = linkInstance;
			return result;
		}

		private static LinkInstance TranslateToNewFormat(EntityDescriptorSection_Old section1, EntityDescriptorSection_Old section2, Guid? linkIdentifier = null)
		{
			var identifier = linkIdentifier ?? Guid.NewGuid();
			return new LinkInstance(identifier)
			{
				LinkProperties = new LinkPropertiesSection
				{
					Direction = SlcObject_LinkingIds.Enums.Direction.Symmetric,
				},
				SourceEntityDescriptor = new SourceEntityDescriptorSection
				{
					ID = section1.ID,
					DisplayName = section1.DisplayName,
					ModelName = String.IsNullOrEmpty(section1.ModelName) ? "Migrated" : section1.ModelName,
					SolutionID = String.IsNullOrEmpty(section1.SolutionName) ? "Migrated" : section1.SolutionName, // SolutionID is not available in the old format, so we use the SolutionName as a fallback.
					SolutionName = String.IsNullOrEmpty(section1.SolutionName) ? "Migrated" : section1.SolutionName,
					ParentID = section1.ParentID,
					ParentModelName = section1.ParentModelName,
				},
				TargetEntityDescriptor = new TargetEntityDescriptorSection
				{
					ID = section2.ID,
					DisplayName = section2.DisplayName,
					ModelName = String.IsNullOrEmpty(section2.ModelName) ? "Migrated" : section2.ModelName,
					SolutionID = String.IsNullOrEmpty(section2.SolutionName) ? "Migrated" : section2.SolutionName, // SolutionID is not available in the old format, so we use the SolutionName as a fallback.
					SolutionName = String.IsNullOrEmpty(section2.SolutionName) ? "Migrated" : section2.SolutionName,
					ParentID = section2.ParentID,
					ParentModelName = section2.ParentModelName,
				},
			};
		}
	}
}
