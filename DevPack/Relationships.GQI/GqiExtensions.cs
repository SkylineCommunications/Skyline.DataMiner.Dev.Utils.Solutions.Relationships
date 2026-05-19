namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;

	using Skyline.DataMiner.Analytics.GenericInterface;

	/// <summary>
	/// Provides extension methods for use within GQI to support relation functionality.
	/// </summary>
	public static class GqiExtensions
	{
		/// <summary>
		/// Allows an override of the behavior of <see cref="GetRelationshipManager(GQIDMS)"/> to return a Fake or Mock of <see cref="IRelationshipManager"/>.
		/// Important: When this is used, unit tests should never be run in parallel.
		/// </summary>
		public static Func<GQIDMS, IRelationshipManager> OverrideGetRelationshipManager = (GQIDMS dms) => new RelationshipManager(dms.GetConnection());

		/// <summary>
		/// Creates a new <see cref="IRelationshipManager"/> instance using the specified <see cref="GQIDMS"/> connection.
		/// </summary>
		/// <param name="dms">The GQI DMS connection.</param>
		/// <returns>An <see cref="IRelationshipManager"/> instance initialized with the provided DMS connection.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="dms"/> is <c>null</c>.</exception>
		public static IRelationshipManager GetRelationshipManager(this GQIDMS dms)
		{
			if (dms is null)
			{
				throw new ArgumentNullException(nameof(dms), "dms cannot be null.");
			}

			return OverrideGetRelationshipManager(dms);
		}

		/// <summary>
		/// Creates a new <see cref="IRelationshipManager"/> instance using the DMS connection from the specified <see cref="OnInitInputArgs"/>.
		/// </summary>
		/// <param name="args">The initialization arguments containing the DMS connection.</param>
		/// <returns>An <see cref="IRelationshipManager"/> instance initialized with the DMS connection from <paramref name="args"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <c>null</c>.</exception>
		public static IRelationshipManager GetRelationshipManager(this OnInitInputArgs args)
		{
			if (args is null)
			{
				throw new ArgumentNullException(nameof(args), "args cannot be null.");
			}

			return GetRelationshipManager(args.DMS);
		}
	}
}