namespace Skyline.DataMiner.Solutions.ObjectLinking.Install.DOM
{
	using System;
	using System.Linq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Modules;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Utils.DOM.Builders;

	internal partial class DomInstaller
	{
		private readonly IEngine _engine;
		private readonly IConnection _connection;
		private readonly Action<string> _logMethod;

		public DomInstaller(IEngine engine, Action<string> logMethod = null)
		{
			_engine = engine;
			_connection = engine.GetUserConnection();
			_logMethod = logMethod;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DomInstaller"/> class.
		/// Used only for unit testing purposes, where we will never need to run a migration script.
		/// </summary>
		/// <param name="connection">Connection to DataMiner.</param>
		/// <param name="logMethod">Optional log method.</param>
		internal DomInstaller(IConnection connection, Action<string> logMethod = null)
		{
			_connection = connection;
			_logMethod = logMethod;
		}

		public void InstallDefaultContent()
		{
			Log("Installation for Object Linking started...");

			var moduleHelper = new ModuleSettingsHelper(_connection.HandleMessages);
			var moduleExist = moduleHelper.ModuleSettings.Count(ModuleSettingsExposers.ModuleId.Equal(LinkDomMapper.ModuleId)) == 0;
			if (!moduleExist)
			{
				Log("Installing Module Settings...");
			}
			else
			{
				Log("Updating Module Settings...");
			}

			var module = new DomModuleBuilder()
				.WithModuleId(LinkDomMapper.ModuleId)
				.WithInformationEvents(false)
				.WithHistory(false)
				.Build();
			Import(moduleHelper.ModuleSettings, ModuleSettingsExposers.ModuleId.Equal(LinkDomMapper.ModuleId), module);

			if (!moduleExist)
			{
				Log("Installed Module Settings");
			}
			else
			{
				Log("Updated Module Settings");
			}

			var needsMigration = false;
			var domHelper = new DomHelper(_connection.HandleMessages, LinkDomMapper.ModuleId);

			var oldEntityDescriptionSectionId = new SectionDefinitionID(new Guid("75dd33f0-204e-4b51-bec3-3ae57065ebc0"))
			{
				ModuleId = "(slc)object_linking",
			};
			var existingLinkDefinition = domHelper.DomDefinitions.Read(DomDefinitionExposers.Id.Equal(LinkDomMapper.DomDefinitionId)).FirstOrDefault();
			if (!(existingLinkDefinition is null) &&
				existingLinkDefinition.SectionDefinitionLinks.Any(link => oldEntityDescriptionSectionId.Equals(link.SectionDefinitionID)))
			{
				Log("Making backup from old module...");

				var moduleSettings = new ModuleSettingsHelper(_engine.SendSLNetMessages);
				var exporter = new DomExporter(moduleSettings, _engine.SendSLNetMessages);
				exporter.Progress += (sender, arg) => Log($"Busy, Exported {arg.Items} items...");
				var backupPath = exporter.Export(LinkDomMapper.ModuleId);

				Log($"Backup exported to: {backupPath}");
			}

			InstallLinks(domHelper);
		}

		internal void Log(string message)
		{
			_logMethod?.Invoke($"ObjectLinking.Installer: {message}");
		}

		private static void Import<T>(ICrudHelperComponent<T> crudHelperComponent, FilterElement<T> equalityFilter, T dataType)
			where T : DataType
		{
			bool exists = crudHelperComponent.Read(equalityFilter).Any();

			if (exists)
			{
				crudHelperComponent.Update(dataType);
			}
			else
			{
				crudHelperComponent.Create(dataType);
			}
		}
	}
}
