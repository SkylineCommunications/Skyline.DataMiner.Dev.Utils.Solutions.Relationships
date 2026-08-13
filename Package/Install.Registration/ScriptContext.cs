namespace Skyline.DataMiner.Automation
{
	using System;
	using System.Linq;

	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public class ScriptContext
	{
		public ScriptContext(IEngine engine)
		{
			Engine = engine;

			Version = GetScriptParam("version").Single();
		}

		public IEngine Engine { get; }

		public string Version { get; }

		private string[] GetScriptParam(string name)
		{
			var rawValue = Engine.GetScriptParam(name).Value;
			if (String.IsNullOrEmpty(rawValue))
			{
				throw new ArgumentException($"Script Param '{name}' cannot be left empty.");
			}

			if (IsJsonArray(rawValue))
			{
				return JsonConvert.DeserializeObject<string[]>(rawValue);
			}
			else
			{
				return new[] { rawValue };
			}
		}

		private static bool IsJsonArray(string json)
		{
			try
			{
				JArray.Parse(json);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
