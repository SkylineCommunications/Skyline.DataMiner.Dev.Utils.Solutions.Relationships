namespace Skyline.DataMiner.Solutions.ObjectLinking.Install.DOM
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Text;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.Modules;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	internal class ItemProgressEventArgs : EventArgs
	{
		public ItemProgressEventArgs(int items) => Items = items;

		public int Items { get; internal set; }
	}

	internal class DomExporter
	{
		private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto,
			ContractResolver = new DefaultContractResolver { IgnoreSerializableInterface = true },
		});

		private readonly ModuleSettingsHelper moduleSettingsHelper;
		private readonly Func<DMSMessage[], DMSMessage[]> sendSLNetMessages;
		private DomHelper domHelper;
		private JsonTextWriter jsonTextWriter;
		private ItemProgressEventArgs itemProgressEventArgs;

		public DomExporter(
			ModuleSettingsHelper moduleSettingsHelper,
			Func<DMSMessage[], DMSMessage[]> sendSLNetMessages)
		{
			this.moduleSettingsHelper =
				moduleSettingsHelper ?? throw new ArgumentNullException(nameof(moduleSettingsHelper));
			this.sendSLNetMessages = sendSLNetMessages ?? throw new ArgumentNullException(nameof(sendSLNetMessages));
		}

		public event EventHandler<ItemProgressEventArgs> Progress;

		public string Export(string moduleId)
		{
			InitProgressCounter();

			string filePath = PrepareFilePath(moduleId);
			using (var writer = new Writer(filePath))
			{
				jsonTextWriter = writer.JsonTextWriter;

				jsonTextWriter.WriteStartArray();
				ExportModule(moduleId);

				jsonTextWriter.WriteEndArray();
			}

			return filePath;
		}

		private static string PrepareFilePath(string moduleId)
		{
			const string Dir = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\DOM Export\";
			string filePath = Path.ChangeExtension(Path.Combine(Dir, GenerateRandomFileName(moduleId)), "zip");
			Directory.CreateDirectory(Dir);
			return filePath;
		}

		private static string GenerateRandomFileName(string moduleId)
		{
			char[] invalidChars = Path.GetInvalidFileNameChars();
			string sanitizedModuleId = new string(moduleId.Where(c => !invalidChars.Contains(c)).ToArray());
			string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
			return $"{sanitizedModuleId}_{timestamp}_UTC";
		}

		private void ExportModule(string moduleId)
		{
			jsonTextWriter.WriteStartObject();
			ExportModuleSettings(moduleId);
			domHelper = new DomHelper(sendSLNetMessages, moduleId);
			ExportSectionDefinitions();
			ExportDomBehaviorDefinitions();
			ExportDomDefinitions();
			ExportDomTemplates();
			ExportDomInstances();

			jsonTextWriter.WriteEndObject();
		}

		private void ExportDomTemplates()
		{
			ExportPaged("DomTemplates", domHelper.DomTemplates);
		}

		private void ExportSectionDefinitions()
		{
			ExportPaged("SectionDefinitions", domHelper.SectionDefinitions);
		}

		private void ExportDomBehaviorDefinitions()
		{
			ExportPaged("DomBehaviorDefinitions", domHelper.DomBehaviorDefinitions);
		}

		private void ExportDomDefinitions()
		{
			ExportPaged("DomDefinitions", domHelper.DomDefinitions);
		}

		private void ExportDomInstances()
		{
			ExportPaged("DomInstances", domHelper.DomInstances);
		}

		private void ExportModuleSettings(string moduleId)
		{
			jsonTextWriter.WritePropertyName("ModuleSettings");

			ModuleSettings moduleSettings = moduleSettingsHelper.ModuleSettings
				.Read(ModuleSettingsExposers.ModuleId.Equal(moduleId))
				.Single();

			JsonSerializer.Serialize(jsonTextWriter, moduleSettings);
			IncrementProgressCounter(1);
		}

		private void ExportPaged<T>(string name, ICrudHelperComponent<T> crudHelperComponent) where T : DataType
		{
			ExportPaged(name, crudHelperComponent.PreparePaging(new TRUEFilterElement<T>()));
		}

		private void ExportPaged<T>(string name, PagingHelper<T> pagingHelper) where T : DataType
		{
			jsonTextWriter.WritePropertyName(name);
			jsonTextWriter.WriteStartArray();
			while (pagingHelper.MoveToNextPage())
			{
				List<T> dataTypes = pagingHelper.GetCurrentPage();
				foreach (T dataType in dataTypes)
				{
					JsonSerializer.Serialize(jsonTextWriter, dataType);
				}

				IncrementProgressCounter(dataTypes.Count);
			}

			jsonTextWriter.WriteEndArray();
		}

		private void InitProgressCounter()
		{
			itemProgressEventArgs = new ItemProgressEventArgs(0);
			Progress?.Invoke(this, itemProgressEventArgs);
		}

		private void IncrementProgressCounter(int items)
		{
			itemProgressEventArgs.Items += items;
			Progress?.Invoke(this, itemProgressEventArgs);
		}

		private sealed class Writer : IDisposable
		{
			private readonly FileStream fileStream;
			private readonly ZipArchive zipArchive;
			private readonly Stream stream;
			private readonly StreamWriter streamWriter;

			public Writer(string path)
			{
				try
				{
					fileStream = new FileStream(path, FileMode.Create);
					zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);
					stream = zipArchive.CreateEntry("module.json", CompressionLevel.Optimal).Open();
					streamWriter = new StreamWriter(stream, Encoding.UTF8);
					JsonTextWriter = new JsonTextWriter(streamWriter);
				}
				catch
				{
					Dispose();
					throw;
				}
			}

			public JsonTextWriter JsonTextWriter { get; }

			public void Dispose()
			{
				((IDisposable)JsonTextWriter)?.Dispose();
				streamWriter?.Dispose();
				stream?.Dispose();
				zipArchive?.Dispose();
				fileStream?.Dispose();
			}
		}
	}
}
