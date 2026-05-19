namespace Skyline.DataMiner.Solutions.Relationships.Tests
{
	using Skyline.DataMiner.Solutions.Relationships;

	internal static class EntityFactory
	{
		internal static EntityDescriptor Create(string id, string modelName, string solutionId = "solution-1")
		{
			return new EntityDescriptor
			{
				ID = id,
				DisplayName = id,
				ModelName = modelName,
				SolutionID = solutionId,
				SolutionName = "Test Solution",
			};
		}
	}
}
