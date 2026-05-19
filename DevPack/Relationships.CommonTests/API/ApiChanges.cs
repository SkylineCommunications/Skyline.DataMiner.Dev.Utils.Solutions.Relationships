namespace Skyline.DataMiner.Solutions.Relationships.Tests.API
{
	using System.Threading.Tasks;

	using PublicApiGenerator;

	[TestClass]
	[UsesVerify]
	public partial class ApiChanges
	{
		[TestMethod]
		public Task PublicChanges()
		{
			var assembly = typeof(IRelationshipManager).Assembly;
			var publicApi = assembly.GeneratePublicApi();

			return Verify(publicApi)
				.UseFileName("SDM.Abstractions");
		}
	}
}
