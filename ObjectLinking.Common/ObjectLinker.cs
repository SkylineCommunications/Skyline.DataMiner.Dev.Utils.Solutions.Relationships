// Ignore Spelling: SDM

namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.Solutions.ObjectLinking.Middleware;

	/// <inheritdoc/>
	public class ObjectLinker : IObjectLinker
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectLinker"/> class.
		/// </summary>
		/// <param name="connection">The connection to use for link storage operations.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="connection"/> is <c>null</c>.</exception>
		internal ObjectLinker(IConnection connection)
		{
			Links = new LinkDomRepository(connection)
				.WithMiddleware(new LinkValidationMiddleware());
		}

		/// <inheritdoc/>
		public IBulkRepository<Link> Links { get; }

		/// <inheritdoc/>
		public Link Create(EntityDescriptor source, EntityDescriptor target)
		{
			return Links.Create(new Link
			{
				Direction = LinkDirection.Symmetric,
				Source = source ?? throw new ArgumentNullException(nameof(source)),
				Target = target ?? throw new ArgumentNullException(nameof(target)),
			});
		}

		/// <inheritdoc/>
		public Link Create(EntityDescriptor source, EntityDescriptor target, LinkDirection direction)
		{
			return Links.Create(new Link
			{
				Direction = direction,
				Source = source ?? throw new ArgumentNullException(nameof(source)),
				Target = target ?? throw new ArgumentNullException(nameof(target)),
			});
		}

		/// <inheritdoc/>
		public IList<Link> GetLinksByEntity(EntityDescriptor entity)
		{
			if (entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (String.IsNullOrEmpty(entity.ID) ||
				String.IsNullOrEmpty(entity.ModelName))
			{
				throw new ArgumentException($"Entity needs to have at least an id and a model name.");
			}

			// Filter to get links where the entity is either the source or the target
			var filter = new ORFilterElement<Link>(
				new ANDFilterElement<Link>( // Symmetric, meaning id can be either source or target
					LinkExposers.Direction.Equal(LinkDirection.Symmetric),
					new ORFilterElement<Link>(
						new ANDFilterElement<Link>(
							LinkExposers.Source.ID.Equal(entity.ID),
							LinkExposers.Source.ModelName.Equal(entity.ModelName)),
						new ANDFilterElement<Link>(
							LinkExposers.Target.ID.Equal(entity.ID),
							LinkExposers.Target.ModelName.Equal(entity.ModelName)))),
				new ANDFilterElement<Link>( // Forward, meaning id is source
					LinkExposers.Direction.Equal(LinkDirection.Forward),
					new ANDFilterElement<Link>(
						LinkExposers.Source.ID.Equal(entity.ID),
						LinkExposers.Source.ModelName.Equal(entity.ModelName))),
				new ANDFilterElement<Link>( // Backward, meaning id is target
					LinkExposers.Direction.Equal(LinkDirection.Backward),
					new ANDFilterElement<Link>(
						LinkExposers.Target.ID.Equal(entity.ID),
						LinkExposers.Target.ModelName.Equal(entity.ModelName)))
				);

			var result = new List<Link>();
			foreach (var page in Links.ReadPaged(filter))
			{
				result.AddRange(page);
			}

			return result;
		}

		/// <inheritdoc/>
		public IList<Link> GetLinksByEntity<T>(SdmObject<T> sdmObject)
			where T : SdmObject<T>
		{
			if (sdmObject is null)
			{
				throw new ArgumentNullException(nameof(sdmObject));
			}

			return GetLinksByEntity(sdmObject.Identifier);
		}

		/// <inheritdoc/>
		public IList<Link> GetLinksByEntity(string entityId)
		{
			if (String.IsNullOrEmpty(entityId))
			{
				throw new ArgumentNullException(nameof(entityId));
			}

			// Filter to get links where the entity is either the source or the target
			var filter = new ORFilterElement<Link>(
				new ANDFilterElement<Link>( // Symmetric, meaning id can be either source or target
					LinkExposers.Direction.Equal(LinkDirection.Symmetric),
					new ORFilterElement<Link>(
						LinkExposers.Source.ID.Equal(entityId),
						LinkExposers.Target.ID.Equal(entityId))),
				new ANDFilterElement<Link>( // Forward, meaning id is source
					LinkExposers.Direction.Equal(LinkDirection.Forward),
					LinkExposers.Source.ID.Equal(entityId)),
				new ANDFilterElement<Link>( // Backward, meaning id is target
					LinkExposers.Direction.Equal(LinkDirection.Backward),
					LinkExposers.Target.ID.Equal(entityId))
				);

			var result = new List<Link>();
			foreach (var page in Links.ReadPaged(filter))
			{
				result.AddRange(page);
			}

			return result;
		}

		/// <inheritdoc/>
		public IList<EntityDescriptor> GetLinkedEntities(EntityDescriptor entity)
		{
			var result = new List<EntityDescriptor>();
			foreach (var link in GetLinksByEntity(entity))
			{
				// Exclude the entity itself from the results, in case it's present as source or target in the link
				result.AddRange(link.EntityDescriptors.Where(e => e.ID != entity.ID && e.ModelName != entity.ModelName));
			}

			return result;
		}

		/// <inheritdoc/>
		public IList<EntityDescriptor> GetLinkedEntities<T>(SdmObject<T> sdmObject)
			where T : SdmObject<T>
		{
			if (sdmObject is null)
			{
				throw new ArgumentNullException(nameof(sdmObject));
			}

			return GetLinkedEntities(sdmObject.Identifier);
		}

		/// <inheritdoc/>
		public IList<EntityDescriptor> GetLinkedEntities(string entityId)
		{
			if (String.IsNullOrEmpty(entityId))
			{
				throw new ArgumentNullException(nameof(entityId));
			}

			var result = new List<EntityDescriptor>();
			foreach (var link in GetLinksByEntity(entityId))
			{
				// Exclude the entity itself from the results, in case it's present as source or target in the link
				result.AddRange(link.EntityDescriptors.Where(e => e.ID != entityId));
			}

			return result;
		}

		/// <inheritdoc/>
		public Link GetLinkById(string linkIdentifier)
		{
			if (!Guid.TryParse(linkIdentifier, out var guid) ||
				guid == Guid.Empty)
			{
				throw new ArgumentException("Identifier should be a valid non empty guid.", nameof(linkIdentifier));
			}

			var link = Links.Read(LinkExposers.Identifier.Equal(linkIdentifier)).FirstOrDefault();
			if (link is null)
			{
				throw new KeyNotFoundException($"No link found with ID: {linkIdentifier}");
			}

			return link;
		}
	}
}
