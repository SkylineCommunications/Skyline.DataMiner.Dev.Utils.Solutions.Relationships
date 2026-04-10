namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	using System;

	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Provides extension methods for the <see cref="SLProtocol"/> class to support object linking functionality.
	/// </summary>
	public static class ProtocolExtensions
	{
		/// <summary>
		/// Allows an override of the behavior of GetObjectLinker to return a Fake or Mock of Skyline.DataMiner.Solutions.ObjectLinking.IObjectLinker.
		/// Important: When this is used, unit tests should never be run in parallel.
		/// </summary>
		public static Func<SLProtocol, IObjectLinker> OverrideGetObjectLinker = (SLProtocol protocol) => new ObjectLinker(protocol.SLNet.RawConnection);

		/// <summary>
		/// Gets an <see cref="ObjectLinker"/> instance for the specified <see cref="SLProtocol"/>.
		/// </summary>
		/// <param name="protocol">The protocol instance for which to get the object linker.</param>
		/// <returns>
		/// An <see cref="ObjectLinker"/> that provides access to object linking operations using the protocol's connection.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		public static IObjectLinker GetObjectLinker(this SLProtocol protocol)
		{
			if (protocol is null)
			{
				throw new ArgumentNullException(nameof(protocol), "protocol cannot be null.");
			}

			return OverrideGetObjectLinker(protocol);
		}
	}
}