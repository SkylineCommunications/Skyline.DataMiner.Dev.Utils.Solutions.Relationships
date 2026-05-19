namespace Skyline.DataMiner.Solutions.Relationships.Install
{
	using System;

	using Skyline.DataMiner.Core.DataMinerSystem.Common;
	using Skyline.DataMiner.Net;

	internal static class ConnectionExtensions
	{
		public static bool TryDeleteScript(this IConnection connection, string name, Action<string> logMethod = null)
		{
			try
			{
				var dms = connection.GetDms();
				if (!dms.ScriptExists(name))
					return true;

				var script = dms.GetScript(name);

				logMethod?.Invoke($"Removing script {name}...");
				script.Delete();
				logMethod?.Invoke($"Removed script {name}");
				return true;
			}
			catch (Exception)
			{
				logMethod?.Invoke($"Unable to remove script {name}");
				return false;
			}
		}
	}
}
