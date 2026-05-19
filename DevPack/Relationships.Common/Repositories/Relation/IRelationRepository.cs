namespace Skyline.DataMiner.Solutions.Relationships.Repositories
{
	using Skyline.DataMiner.SDM;

	[AllowSdmMiddleware]
	public interface IRelationRepository :
		IReadableRepository<Relation>,
		IPageableRepository<Relation>,
		IBulkCreatableRepository<Relation>,
		IBulkDeletableRepository<Relation>
	{
	}
}
