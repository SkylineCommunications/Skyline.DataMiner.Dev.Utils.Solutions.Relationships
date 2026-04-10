namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.SDM;

	/// <summary>
	/// Provides functionality to create and query links between entities in the SDM object model.
	/// </summary>
	public interface IObjectLinker
	{
		/// <summary>
		/// Gets the storage provider for <see cref="Link"/> objects.
		/// </summary>
		/// <value>An <see cref="IBulkRepository{Link}"/> instance used for CRUD operations on links.</value>
		IBulkRepository<Link> Links { get; }

		/// <summary>
		/// Creates a new link between two entities.
		/// </summary>
		/// <param name="source">The first entity to link.</param>
		/// <param name="target">The second entity to link.</param>
		/// <returns>The newly created <see cref="Link"/> with <see cref="LinkDirection.Symmetric"/> direction.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="target"/> is <c>null</c>.</exception>
		/// <remarks>
		/// A symmetric link has no implied direction; both entities are considered equal participants in the relationship.
		/// To create a directional link, use the <see cref="Create(EntityDescriptor, EntityDescriptor, LinkDirection)"/> overload.
		/// </remarks>
		Link Create(EntityDescriptor source, EntityDescriptor target);

		/// <summary>
		/// Creates a new link between two entities with the specified direction.
		/// </summary>
		/// <param name="source">The first entity to link.</param>
		/// <param name="target">The second entity to link.</param>
		/// <param name="direction">The direction of the link, controlling traversal semantics.</param>
		/// <returns>The newly created <see cref="Link"/> with the specified <see cref="LinkDirection"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> or <paramref name="target"/> is <c>null</c>.</exception>
		/// <remarks>
		/// <para>The <paramref name="direction"/> parameter determines how the link is traversed when using
		/// <see cref="GetLinkedEntities(string)"/>:</para>
		/// <list type="bullet">
		///   <item><description><see cref="LinkDirection.Symmetric"/> – the link can be traversed from either side.</description></item>
		///   <item><description><see cref="LinkDirection.Forward"/> – the link is traversed from source to target.</description></item>
		///   <item><description><see cref="LinkDirection.Backward"/> – the link is traversed from target to source.</description></item>
		/// </list>
		/// </remarks>
		Link Create(EntityDescriptor source, EntityDescriptor target, LinkDirection direction);

		/// <summary>
		/// Gets a link by its unique identifier.
		/// </summary>
		/// <param name="linkIdentifier">The unique identifier of the link, expressed as a <see cref="Guid"/> string.</param>
		/// <returns>The <see cref="Link"/> with the specified identifier.</returns>
		/// <exception cref="ArgumentException">Thrown if <paramref name="linkIdentifier"/> is not a valid <see cref="Guid"/> or is <see cref="Guid.Empty"/>.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if no link with the specified identifier is found.</exception>
		Link GetLinkById(string linkIdentifier);

		/// <summary>
		/// Gets all entities that are linked to the specified entity.
		/// </summary>
		/// <param name="entity">The entity to find linked entities for.</param>
		/// <returns>A list of <see cref="EntityDescriptor"/> objects linked to the specified entity.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
		IList<EntityDescriptor> GetLinkedEntities(EntityDescriptor entity);

		/// <summary>
		/// Gets all entities that are linked to the specified entity ID.
		/// </summary>
		/// <param name="entityId">The ID of the entity to find linked entities for.</param>
		/// <returns>
		/// A list of <see cref="EntityDescriptor"/> objects linked to the specified entity ID.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="entityId"/> is <c>null</c> or empty.</exception>
		/// <remarks>
		/// <para>This method retrieves all links associated with the entity via <see cref="GetLinksByEntity(string)"/>
		/// and returns the opposite side of each link.</para>
		/// </remarks>
		IList<EntityDescriptor> GetLinkedEntities(string entityId);

		/// <summary>
		/// Gets all entities that are linked to the specified SDM object.
		/// </summary>
		/// <typeparam name="T">The type of the SDM object.</typeparam>
		/// <param name="sdmObject">The SDM object to find linked entities for.</param>
		/// <returns>A list of <see cref="EntityDescriptor"/> objects linked to the specified SDM object.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="sdmObject"/> is <c>null</c>.</exception>
		/// <remarks>
		/// This is a convenience overload that extracts the <see cref="SdmObject{T}.Identifier"/> from the provided
		/// SDM object and delegates to <see cref="GetLinkedEntities(EntityDescriptor)"/>.
		/// </remarks>
		IList<EntityDescriptor> GetLinkedEntities<T>(SdmObject<T> sdmObject) where T : SdmObject<T>;

		/// <summary>
		/// Gets all links that reference the specified entity, respecting link directionality.
		/// </summary>
		/// <param name="entity">The entity to search for. Must have a non-empty <see cref="EntityDescriptor.ID"/>and <see cref="EntityDescriptor.ModelName"/>.</param>
		/// <returns>A list of <see cref="Link"/> objects referencing the entity.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="entity"/> has a <c>null</c> or empty <see cref="EntityDescriptor.ID"/>.</exception>
		/// <remarks>
		/// <para>The search matches on both <see cref="EntityDescriptor.ID"/> and <see cref="EntityDescriptor.ModelName"/>,
		/// ensuring that only links for the exact entity type are returned.</para>
		/// <para>Directionality is respected: forward links match only when the entity is the source,
		/// backward links match only when the entity is the target, and symmetric links match on either side.</para>
		/// </remarks>
		IList<Link> GetLinksByEntity(EntityDescriptor entity);

		/// <summary>
		/// Gets all links that reference the specified entity ID, respecting link directionality.
		/// </summary>
		/// <param name="entityId">The ID of the entity to search for.</param>
		/// <returns>A list of <see cref="Link"/> objects referencing the entity ID.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="entityId"/> is <c>null</c> or empty.</exception>
		/// <remarks>
		/// <para>Unlike the <see cref="GetLinksByEntity(EntityDescriptor)"/> overload, this method matches only on
		/// <see cref="EntityDescriptor.ID"/> without filtering by <see cref="EntityDescriptor.ModelName"/>.</para>
		/// <para>Directionality is respected: forward links match only when the entity is the source,
		/// backward links match only when the entity is the target, and symmetric links match on either side.</para>
		/// </remarks>
		IList<Link> GetLinksByEntity(string entityId);

		/// <summary>
		/// Gets all links that reference the specified SDM object.
		/// </summary>
		/// <typeparam name="T">The type of the SDM object, which must derive from <see cref="SdmObject{T}"/>.</typeparam>
		/// <param name="sdmObject">The SDM object to search for.</param>
		/// <returns>A list of <see cref="Link"/> objects referencing the SDM object.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="sdmObject"/> is <c>null</c>.</exception>
		/// <remarks>
		/// This is a convenience overload that extracts the <see cref="SdmObject{T}.Identifier"/> from the provided
		/// SDM object and delegates to <see cref="GetLinksByEntity(string)"/>.
		/// </remarks>
		IList<Link> GetLinksByEntity<T>(SdmObject<T> sdmObject) where T : SdmObject<T>;
	}
}