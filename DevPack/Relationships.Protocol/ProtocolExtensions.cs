namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	using System;

	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.Solutions.Relationships;

	/// <summary>
	/// Provides extension methods for the <see cref="SLProtocol"/> interface to support relation functionality.
	/// </summary>
	public static class ProtocolExtensions
	{
		/// <summary>
		/// Allows an override of the behavior of <see cref="GetRelationshipManager"/> to return a Fake or Mock of <see cref="IRelationshipManager"/>.
		/// Important: When this is used, unit tests should never be run in parallel.
		/// </summary>
		public static Func<SLProtocol, IRelationshipManager> OverrideGetRelationshipManager = (SLProtocol protocol) => new RelationshipManager(protocol.SLNet.RawConnection);

		/// <summary>
		/// Gets an <see cref="IRelationshipManager"/> instance for the specified <see cref="SLProtocol"/>.
		/// </summary>
		/// <param name="protocol">The protocol instance for which to get the object linker.</param>
		/// <returns>
		/// An <see cref="IRelationshipManager"/> that provides access to object linking operations using the protocol's connection.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="protocol"/> is <c>null</c>.
		/// </exception>
		public static IRelationshipManager GetRelationshipManager(this SLProtocol protocol)
		{
			if (protocol is null)
			{
				throw new ArgumentNullException(nameof(protocol), "protocol cannot be null.");
			}

			return OverrideGetRelationshipManager(protocol);
		}
	}
}