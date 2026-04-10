namespace ObjectLinkingInstallRegistration
{
	using System;
	using System.Linq;

	using Skyline.DataMiner.Analytics.GenericInterface.JoinFilter;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.Registration;
	using Skyline.DataMiner.Solutions.ObjectLinking.Shared;

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
				var context = new ScriptContext(engine);
				RunSafe(context);
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

		private void RunSafe(ScriptContext context)
		{
			var registrar = context.Engine.GetSdmRegistrar();

			// Cleanup old registrations if they exist, to ensure a clean state.
			var oldRegistartion = registrar.Solutions.Read(SolutionRegistrationExposers.Identifier.Equal("a5e01a18-7704-40fc-b2d4-b4b02b096ba9")).FirstOrDefault();
			if (!(oldRegistartion is null))
			{
				var oldModels = registrar.Models.Read(ModelRegistrationExposers.Solution.Equal(oldRegistartion)).ToArray();
				registrar.Models.Delete(oldModels);
				registrar.Solutions.Delete(oldRegistartion);
			}

			// Register the solution
			var solution = new SolutionRegistration
			{
				Identifier = "f949ad1f-367c-4966-8c69-8525ceb267cc",
				ID = Constants.CatalogIdentifier,
				DisplayName = "SDM Object Linking",
				Version = context.Version,
			};

			registrar.Solutions.CreateOrUpdate(new[] { solution });

			var linkModel = new ModelRegistration
			{
				Identifier = "ebd8dc13-4118-4653-a6b0-ecb9f0a7db29",
				Name = "standard_data_model_object_link",
				DisplayName = "SDM Object Link",
				Version = "1.0.0",
				Solution = solution,
			};

			registrar.Models.CreateOrUpdate(new[] { linkModel });
		}
	}
}
