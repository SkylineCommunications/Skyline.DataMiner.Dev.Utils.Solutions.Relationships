namespace Skyline.DataMiner.Solutions.Relationships.Repositories
{
	using Skyline.DataMiner.SDM;

	[AllowSdmMiddleware]
	public interface IEntityDescriptorRepository :
		IBulkRepository<EntityDescriptor>
	{
	}
}
