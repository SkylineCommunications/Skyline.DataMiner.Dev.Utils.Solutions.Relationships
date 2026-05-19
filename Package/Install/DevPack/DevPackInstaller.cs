namespace Skyline.DataMiner.Solutions.Relationships.Install.DevPack
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.ServiceProcess;

	using Skyline.AppInstaller;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Automation.Messages.Requests;
	using Skyline.DataMiner.Utils.SecureCoding.SecureIO;

	internal class DevPackInstaller
	{
		public const string DevPackNamePrefix = "Skyline.DataMiner.Dev.Utils.";
		private const string DataMinerGqiServiceName = "DataMiner GQI";

		public DevPackInstaller(AppInstaller installer, IEngine engine)
		{
			Installer = installer ?? throw new ArgumentNullException(nameof(installer));
			Engine = engine ?? throw new ArgumentNullException(nameof(engine));
		}

		public AppInstaller Installer { get; }

		public IEngine Engine { get; }

		public IEnumerable<string> GetAvailableDevPacks()
		{
			var setupContentPath = Installer.GetSetupContentDirectory();
			var devPackFolderPath = SecurePath.ConstructSecurePathWithSubDirectories(setupContentPath, "DevPack");

			if (!Directory.Exists(devPackFolderPath))
			{
				yield break;
			}

			foreach (var devPackFile in Directory.EnumerateFiles(devPackFolderPath, "*.dll"))
			{
				var devPackName = Path.GetFileNameWithoutExtension(devPackFile);

				if (IsValidDevPackName(devPackName))
				{
					yield return devPackName;
				}
			}
		}

		public bool IsDevPackInstalled(string devPackName)
		{
			if (String.IsNullOrWhiteSpace(devPackName))
			{
				return false;
			}

			var solutionLibrariesFolder = @"C:\Skyline DataMiner\ProtocolScripts\DllImport\SolutionLibraries";
			var devPackFolder = SecurePath.ConstructSecurePathWithSubDirectories(solutionLibrariesFolder, GetDevPackShortName(devPackName));
			var devPackPath = SecurePath.ConstructSecurePathWithSubDirectories(devPackFolder, $"{devPackName}.dll");

			return File.Exists(devPackPath);
		}

		public void DeployAllDevPacks()
		{
			var stoppedGqiService = StopDataMinerGQI();
			var devPackNames = GetAvailableDevPacks();

			foreach (var devPackName in devPackNames)
			{
				if (!IsValidDevPackName(devPackName))
				{
					Installer.Log($"DevPack {devPackName} has an invalid name and will be skipped. It must start with '{DevPackNamePrefix}'.");
					continue;
				}

				DeployDevPack(devPackName);
			}

			StartDataMinerGQI(stoppedGqiService);
		}

		private static bool IsValidDevPackName(string devPackName)
		{
			return !String.IsNullOrWhiteSpace(devPackName) &&
				devPackName.StartsWith(DevPackNamePrefix, StringComparison.OrdinalIgnoreCase);
		}

		private static string GetDevPackShortName(string devPackName)
		{
			if (!IsValidDevPackName(devPackName))
			{
				throw new InvalidOperationException($"DevPack name '{devPackName}' is invalid. It must start with '{DevPackNamePrefix}'.");
			}

			return devPackName.Substring(DevPackNamePrefix.Length);
		}

		private void DeployDevPack(string devPackName, bool skipIfInstalled = false)
		{
			if (!IsValidDevPackName(devPackName))
			{
				throw new InvalidOperationException($"DevPack name '{devPackName}' is invalid. It must start with '{DevPackNamePrefix}'.");
			}

			try
			{
				if (skipIfInstalled && IsDevPackInstalled(devPackName))
				{
					Installer.Log($@"DevPack {devPackName} is already installed. Skipping installation.");
					return;
				}

				Installer.Log($@"Installing DevPack {devPackName}...");

				var devPackFileName = $"{devPackName}.dll";

				var setupContentPath = Installer.GetSetupContentDirectory();
				var devPackFolderPath = SecurePath.ConstructSecurePathWithSubDirectories(setupContentPath, "DevPack");
				var devPackLibraryPath = SecurePath.ConstructSecurePathWithSubDirectories(devPackFolderPath, devPackFileName);

				var path = GetDevPackShortName(devPackName);
				var fileContent = File.ReadAllBytes(devPackLibraryPath);

				var uploadDependencyMessage = new UploadScriptDependencyMessage
				{
					DependencyName = devPackFileName,
					Path = path,
					Bytes = fileContent,
					DependencyFolder = ScriptDependencyFolder.SolutionLibraries,
				};

				Engine.SendSLNetSingleResponseMessage(uploadDependencyMessage);

				Installer.Log($@"DevPack {devPackName} installed successfully.");
			}
			catch (Exception ex)
			{
				Installer.Log($"Failed to install DevPack {devPackName}: {ex}");
				throw;
			}
		}

		private ServiceController StopDataMinerGQI()
		{
			ServiceController service = new ServiceController(DataMinerGqiServiceName);
			try
			{
				service.Stop();
				service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20));

				Installer.Log("Successfully stopped DataMiner GQI service.");
			}
			catch (System.ServiceProcess.TimeoutException)
			{
				Installer.Log($"Timeout while waiting for DataMiner GQI service to stop");
			}
			catch (Exception e)
			{
				Installer.Log($"Failed to stop DataMiner GQI Service due to {e}");
			}

			return service;
		}

		private void StartDataMinerGQI(ServiceController service)
		{
			try
			{
				service.Start();
				service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(20));

				Installer.Log("Successfully started DataMiner GQI service.");
			}
			catch (System.ServiceProcess.TimeoutException)
			{
				Installer.Log($"Timeout while waiting for DataMiner GQI service to stop/start");
			}
			catch (Exception e)
			{
				Installer.Log($"Failed to start DataMiner GQI Service due to {e}");
			}
		}
	}
}
