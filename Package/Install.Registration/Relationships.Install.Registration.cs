namespace Relationships.Install.Registration
{
	using System;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.SDM.Registration;

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

			var solution = new SolutionRegistration
			{
				Identifier = "fc085eae-dbc4-4d81-9758-096d46f7330d",
				ID = "fc085eae-dbc4-4d81-9758-096d46f7330d",
				DisplayName = "Relationships DevPack",
				Version = context.Version,
			};

			registrar.Solutions.CreateOrUpdate(new[] { solution });

			var entityDescriptor = new ModelRegistration
			{
				Identifier = "ebd8dc13-4118-4653-a6b0-ecb9f0a7db29",
				Name = "entity_descriptor",
				DisplayName = "Entity Descriptor",
				Version = "1.0.0",
				Solution = solution,
			};

			var relation = new ModelRegistration
			{
				Identifier = "1f87bdd6-9955-4663-a257-94fe79eec96e",
				Name = "relation",
				DisplayName = "Relation",
				Version = "1.0.0",
				Solution = solution,
			};

			registrar.Models.CreateOrUpdate(new[] { entityDescriptor, relation });
		}
	}
}
