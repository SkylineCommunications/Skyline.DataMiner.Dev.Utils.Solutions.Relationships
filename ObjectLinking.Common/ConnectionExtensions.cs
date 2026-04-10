namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	using System;

	using Skyline.DataMiner.Net;

	/// <summary>
	/// Provides extension methods for the <see cref="IConnection"/> interface to support object linking functionality.
	/// </summary>
	public static class ConnectionExtensions
	{
		/// <summary>
		/// Allows an override of the behavior of GetObjectLinker to return a Fake or Mock of Skyline.DataMiner.Solutions.ObjectLinking.IObjectLinker.
		/// Important: When this is used, unit tests should never be run in parallel.
		/// </summary>
		public static Func<IConnection, IObjectLinker> OverrideGetObjectLinker = (IConnection connection) => new ObjectLinker(connection);

		/// <summary>
		/// Gets an <see cref="ObjectLinker"/> instance for the specified <see cref="IConnection"/>.
		/// </summary>
		/// <param name="connection">The connection for which to get the <see cref="ObjectLinker"/>.</param>
		/// <returns>An <see cref="ObjectLinker"/> instance associated with the specified engine.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> is <c>null</c>.</exception>
		public static IObjectLinker GetObjectLinker(this IConnection connection)
		{
			if (connection is null)
			{
				throw new ArgumentNullException(nameof(connection), "Connection cannot be null.");
			}

			return OverrideGetObjectLinker(connection);
		}
	}
}
