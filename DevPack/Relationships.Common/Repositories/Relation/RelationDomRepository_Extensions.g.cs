
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.Solutions.Relationships;
	using Skyline.DataMiner.SDM;

	internal static class RelationDomRepository_Extensions
	{

		public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.Solutions.Relationships.Relation> WithMiddleware(
			this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.Solutions.Relationships.Relation> repository,
			IMiddlewareMarker<Skyline.DataMiner.Solutions.Relationships.Relation> middleware)
		{
			return new RelationDomRepository_Middleware(repository, middleware);
		}
	}
}
