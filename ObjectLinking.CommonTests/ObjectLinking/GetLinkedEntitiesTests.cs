namespace Skyline.DataMiner.Solutions.ObjectLinking.Tests
{
	using FluentAssertions;

	using Skyline.DataMiner.Solutions.ObjectLinking.Install.DOM;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	[TestClass]
	public class GetLinkedEntitiesTests
	{
		private DomConnectionMock _connection;

		[TestInitialize]
		public void Setup()
		{
			_connection = new DomConnectionMock();
			new DomInstaller(_connection).InstallDefaultContent();
		}

		[TestMethod]
		public void ObjectLinker_GetLinkedEntities_Symmetric()
		{
			var linker = new ObjectLinker(_connection);

			var entity1 = new EntityDescriptor
			{
				ID = "entity1",
				DisplayName = "Entity 1",
				ModelName = "Model A",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution X",
				ParentID = "parent1",
				ParentModelName = "Parent Model A",
			};
			var entity2 = new EntityDescriptor
			{
				ID = "entity2",
				DisplayName = "Entity 2",
				ModelName = "Model B",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution Y",
				ParentID = "parent2",
				ParentModelName = "Parent Model B",
			};
			var entity3 = new EntityDescriptor
			{
				ID = "entity3",
				DisplayName = "Entity 3",
				ModelName = "Model C",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution Z",
				ParentID = "parent3",
				ParentModelName = "Parent Model C",
			};

			linker.Create(entity1, entity2);
			linker.Create(entity2, entity3);
			linker.Create(entity3, entity1);

			IList<EntityDescriptor> result = new List<EntityDescriptor>();
			var act = () => result = linker.GetLinkedEntities(entity1);

			act.Should().NotThrow();
			result.Should().HaveCount(2);
			result.Should().NotContain(entity1);
			result.Should().Contain(entity2);
			result.Should().Contain(entity3);
		}

		[TestMethod]
		public void ObjectLinker_GetLinkedEntities_Forward()
		{
			var linker = new ObjectLinker(_connection);

			var entity1 = new EntityDescriptor
			{
				ID = "entity1",
				DisplayName = "Entity 1",
				ModelName = "Model A",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution X",
				ParentID = "parent1",
				ParentModelName = "Parent Model A",
			};
			var entity2 = new EntityDescriptor
			{
				ID = "entity2",
				DisplayName = "Entity 2",
				ModelName = "Model B",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution Y",
				ParentID = "parent2",
				ParentModelName = "Parent Model B",
			};
			var entity3 = new EntityDescriptor
			{
				ID = "entity3",
				DisplayName = "Entity 3",
				ModelName = "Model C",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution Z",
				ParentID = "parent3",
				ParentModelName = "Parent Model C",
			};

			linker.Create(entity1, entity2, LinkDirection.Forward);
			linker.Create(entity2, entity3, LinkDirection.Forward);
			linker.Create(entity3, entity1, LinkDirection.Forward);

			IList<EntityDescriptor> result = new List<EntityDescriptor>();
			var act = () => result = linker.GetLinkedEntities(entity1);

			act.Should().NotThrow();
			result.Should().HaveCount(1);
			result.Should().NotContain(entity1);
			result.Should().Contain(entity2);
			result.Should().NotContain(entity3);
		}

		[TestMethod]
		public void ObjectLinker_GetLinkedEntities_Backward()
		{
			var linker = new ObjectLinker(_connection);

			var entity1 = new EntityDescriptor
			{
				ID = "entity1",
				DisplayName = "Entity 1",
				ModelName = "Model A",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution X",
				ParentID = "parent1",
				ParentModelName = "Parent Model A",
			};
			var entity2 = new EntityDescriptor
			{
				ID = "entity2",
				DisplayName = "Entity 2",
				ModelName = "Model B",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution Y",
				ParentID = "parent2",
				ParentModelName = "Parent Model B",
			};
			var entity3 = new EntityDescriptor
			{
				ID = "entity3",
				DisplayName = "Entity 3",
				ModelName = "Model C",
				SolutionID = Guid.NewGuid().ToString(),
				SolutionName = "Solution Z",
				ParentID = "parent3",
				ParentModelName = "Parent Model C",
			};

			linker.Create(entity1, entity2, LinkDirection.Backward);
			linker.Create(entity2, entity3, LinkDirection.Backward);
			linker.Create(entity3, entity1, LinkDirection.Backward);

			IList<EntityDescriptor> result = new List<EntityDescriptor>();
			var act = () => result = linker.GetLinkedEntities(entity1);

			act.Should().NotThrow();
			result.Should().HaveCount(1);
			result.Should().NotContain(entity1);
			result.Should().NotContain(entity2);
			result.Should().Contain(entity3);
		}
	}
}