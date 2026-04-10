/*
****************************************************************************
*  Copyright (c) 2025,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

25/07/2025	1.0.0.1		AMA, Skyline	Initial version
****************************************************************************
*/

using System;
using System.IO;

using Skyline.AppInstaller;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Core.DataMinerSystem.Automation;
using Skyline.DataMiner.Net.AppPackages;
using Skyline.DataMiner.Solutions.ObjectLinking.Install.DevPack;
using Skyline.DataMiner.Solutions.ObjectLinking.Install.DOM;
using Skyline.DataMiner.Solutions.ObjectLinking.Shared;
using Skyline.DataMiner.Utils.SecureCoding.SecureIO;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script
{
	/// <summary>
	/// The script entry point.
	/// </summary>
	/// <param name="engine">Provides access to the Automation engine.</param>
	/// <param name="context">Provides access to the installation context.</param>
	[AutomationEntryPoint(AutomationEntryPointType.Types.InstallAppPackage)]
	public void Install(IEngine engine, AppInstallContext context)
	{
		try
		{
			engine.Timeout = new TimeSpan(0, 10, 0);
			engine.GenerateInformation("Starting installation");
			var installer = new AppInstaller(Engine.SLNetRaw, context);
			if (!PreRequisite(installer))
			{
				engine.ExitFail("Cannot install Object Linking solution. Please make sure you meet the prerequisites and try again.");
			}

			installer.InstallDefaultContent();

			// Install the Object Linking DevPack
			var devPackInstaller = new DevPackInstaller(installer, engine);
			devPackInstaller.DeployAllDevPacks();

			// Install DOM Module
			var domInstaller = new DomInstaller(engine, installer.Log);
			domInstaller.InstallDefaultContent();

			// Register the Object Linking solution in SDM
			Register(engine, context, installer);

			// Remove the subscripts
			TryDeleteScript(engine, installer, Constants.DomScriptName);
			TryDeleteScript(engine, installer, Constants.RegistrationScriptName);
		}
		catch (Exception e)
		{
			engine.ExitFail($"Exception encountered during installation: {e}");
		}
	}

	private static bool PreRequisite(AppInstaller installer)
	{
		// Check if SDM is installed
		var solutionLibrariesFolder = @"C:\Skyline DataMiner\ProtocolScripts\DllImport\SolutionLibraries";
		var devPackFolder = SecurePath.ConstructSecurePathWithSubDirectories(solutionLibrariesFolder, "SDM.Abstractions");
		var devPackPath = SecurePath.ConstructSecurePathWithSubDirectories(devPackFolder, "Skyline.DataMiner.Dev.Utils.SDM.Abstractions.dll");

		var result = File.Exists(devPackPath);
		if (!result)
		{
			installer.Log($"Prerequisite check failed: You need to install SDM first.");
		}

		return result;
	}

	private static void Register(IEngine engine, AppInstallContext context, AppInstaller installer)
	{
		try
		{
			installer.Log($"Registering Solution Object Linking [{Constants.CatalogIdentifier}] with version {context.AppInfo.Version} in SDM..");

			var subScript = engine.PrepareSubScript(Constants.RegistrationScriptName);
			subScript.SelectScriptParam("version", context.AppInfo.Version);
			subScript.Synchronous = true;
			subScript.StartScript();
		}
		catch
		{
			installer.Log("Failed to register the solution in SDM.");
		}
	}

	private void TryDeleteScript(IEngine engine, AppInstaller installer, string name)
	{
		try
		{
			var script = engine.GetDms().GetScript(name);
			if (script == null)
				return;

			installer.Log($"Removing script {name}...");
			script.Delete();
			installer.Log($"Removed script {name}");
		}
		catch (Exception)
		{
			installer.Log($"Unable to remove script {name}");
		}
	}
}