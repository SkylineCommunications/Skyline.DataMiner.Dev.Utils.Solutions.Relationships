namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;

	using Skyline.DataMiner.Net;

	/// <summary>
	/// Provides extension methods for the <see cref="IConnection"/> interface to support relation functionality.
	/// </summary>
	public static class ConnectionExtensions
	{
		/// <summary>
		/// Allows an override of the behavior of <see cref="GetRelationshipManager"/> to return a Fake or Mock of <see cref="IRelationshipManager"/>.
		/// Important: When this is used, unit tests should never be run in parallel.
		/// </summary>
		public static Func<IConnection, IRelationshipManager> OverrideGetRelationshipManager = (IConnection connection) => new RelationshipManager(connection);

		/// <summary>
		/// Gets an <see cref="RelationshipManager"/> instance for the specified <see cref="IConnection"/>.
		/// </summary>
		/// <param name="connection">The connection for which to get the <see cref="RelationshipManager"/>.</param>
		/// <returns>An <see cref="RelationshipManager"/> instance associated with the specified engine.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> is <c>null</c>.</exception>
		public static IRelationshipManager GetRelationshipManager(this IConnection connection)
		{
			if (connection is null)
			{
				throw new ArgumentNullException(nameof(connection), "Connection cannot be null.");
			}

			return OverrideGetRelationshipManager(connection);
		}
	}
}
