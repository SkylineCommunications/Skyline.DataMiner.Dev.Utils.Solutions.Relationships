namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;

	using Skyline.DataMiner.Automation;

	/// <summary>
	/// Provides extension methods for the <see cref="IEngine"/> interface to support relation functionality.
	/// </summary>
	public static class EngineExtensions
	{
		/// <summary>
		/// Allows an override of the behavior of <see cref="GetRelationshipManager"/> to return a Fake or Mock of <see cref="IRelationshipManager"/>.
		/// Important: When this is used, unit tests should never be run in parallel.
		/// </summary>
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable S2223 // Non-constant static fields should not be visible
		public static Func<IEngine, IRelationshipManager> OverrideGetRelationshipManager = (IEngine engine) => new RelationshipManager(engine.GetUserConnection());
#pragma warning restore S2223 // Non-constant static fields should not be visible
#pragma warning restore SA1401 // Fields should be private

		/// <summary>
		/// Gets an <see cref="IRelationshipManager"/> instance for the specified <see cref="IEngine"/>.
		/// </summary>
		/// <param name="engine">The engine for which to get the <see cref="IRelationshipManager"/>.</param>
		/// <returns>An <see cref="IRelationshipManager"/> instance associated with the specified engine.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is <c>null</c>.</exception>
		public static IRelationshipManager GetRelationshipManager(this IEngine engine)
		{
			if (engine is null)
			{
				throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
			}

			return OverrideGetRelationshipManager(engine);
		}
	}
}