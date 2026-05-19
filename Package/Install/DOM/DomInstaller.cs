namespace Skyline.DataMiner.Solutions.Relationships.Install.DOM
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Modules;
	using Skyline.DataMiner.Net.Exceptions;
	using Skyline.DataMiner.Net.GenericEnums;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.Solutions.Relationships.Shared;
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
			Log("Installation for Relationships started...");

			var moduleHelper = new ModuleSettingsHelper(_connection.HandleMessages);
			var moduleExist = moduleHelper.ModuleSettings.Count(ModuleSettingsExposers.ModuleId.Equal(RelationDomMapper.ModuleId)) != 0;
			var module = new DomModuleBuilder()
				.WithModuleId(RelationDomMapper.ModuleId)
				.WithInformationEvents(false)
				.WithHistory(false)
				.Build();
			if (!moduleExist)
			{
				Log("Installing Module Settings...");
				moduleHelper.ModuleSettings.Create(module);
				Log("Installed Module Settings");
			}
			else
			{
				Log("Updating Module Settings...");
				moduleHelper.ModuleSettings.Update(module);
				Log("Updated Module Settings");
			}

			ReinitializeManagerStore(module.ModuleId);

			var domHelper = new DomHelper(_connection.HandleMessages, RelationDomMapper.ModuleId);

			InstallEntity(domHelper);
			InstallRelation(domHelper);
			CreateDummyObjects(domHelper);
		}

		internal void Log(string message)
		{
			_logMethod?.Invoke($"ObjectLinking.Installer: {message}");
		}

		private void ReinitializeManagerStore(string moduleId)
		{
			var request = new ManagerStoreReinitializeCustomManagerRequest(moduleId, "DomManager");
			IEnumerable<ManagerStoreReinitializeCustomManagerResponse> responses =
				_connection.HandleMessage(request).Cast<ManagerStoreReinitializeCustomManagerResponse>();

			var aggregatedResponse = (IManagerStoreCustomResponse)request.AggregateResponses(responses);

			if (aggregatedResponse.TraceData.ErrorData.Any())
			{
				throw new DataMinerException(String.Join("\n", aggregatedResponse.TraceData.ErrorData));
			}
		}

		private GenericEnumFieldDescriptorBuilder CreateBaseEntityStatusEnumFieldDescriptorBuilder()
		{
			return new GenericEnumFieldDescriptorBuilder()
				.WithEnumType(GenericEnumFieldDescriptorBuilder.EnumType.Int)
				.AddEnumValue(new GenericEnumEntry<int>("Active", 0))
				.AddEnumValue(new GenericEnumEntry<int>("Inactive", 1))
				.WithIsOptional(true)
				.WithSoftDeleted(false)
				.WithReadonly(false)
				.WithHidden(false);
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
