
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.Solutions.Relationships;
	using Skyline.DataMiner.SDM;

	internal static class EntityDescriptorDomRepository_Extensions
	{

		public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.Solutions.Relationships.EntityDescriptor> WithMiddleware(
			this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.Solutions.Relationships.EntityDescriptor> repository,
			IMiddlewareMarker<Skyline.DataMiner.Solutions.Relationships.EntityDescriptor> middleware)
		{
			return new EntityDescriptorDomRepository_Middleware(repository, middleware);
		}
	}
}
