namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	using System;

	using Skyline.DataMiner.Automation;

	/// <summary>
	/// Provides extension methods for the <see cref="IEngine"/> interface to support object linking functionality.
	/// </summary>
	public static class EngineExtensions
	{
		/// <summary>
		/// Allows an override of the behavior of GetObjectLinker to return a Fake or Mock of Skyline.DataMiner.Solutions.ObjectLinking.IObjectLinker.
		/// Important: When this is used, unit tests should never be run in parallel.
		/// </summary>
		public static Func<IEngine, IObjectLinker> OverrideGetObjectLinker = (IEngine engine) => new ObjectLinker(engine.GetUserConnection());

		/// <summary>
		/// Gets an <see cref="ObjectLinker"/> instance for the specified <see cref="IEngine"/>.
		/// </summary>
		/// <param name="engine">The engine for which to get the <see cref="ObjectLinker"/>.</param>
		/// <returns>An <see cref="ObjectLinker"/> instance associated with the specified engine.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is <c>null</c>.</exception>
		public static IObjectLinker GetObjectLinker(this IEngine engine)
		{
			if (engine is null)
			{
				throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
			}

			return OverrideGetObjectLinker(engine);
		}
	}
}