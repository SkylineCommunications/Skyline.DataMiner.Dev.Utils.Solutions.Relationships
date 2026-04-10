namespace Skyline.DataMiner.Solutions.ObjectLinking.Tests
{
	using FluentAssertions;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.ObjectLinking.Exceptions;
	using Skyline.DataMiner.Solutions.ObjectLinking.Install.DOM;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	[TestClass]
	public class ObjectLinkerTests
	{
		private DomConnectionMock _connection;

		[TestInitialize]
		public void Setup()
		{
			_connection = new DomConnectionMock();
			new DomInstaller(_connection).InstallDefaultContent();
		}

		[TestMethod]
		public void ObjectLinker_Creation()
		{
			var act = () => new ObjectLinker(_connection);

			act.Should().NotThrow();
		}

		[TestMethod]
		public void ObjectLinker_Create()
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

			var link = new Link
			{
				Source = entity1,
			};

			var act = () => linker.Links.Create(link);

			act.Should().Throw<ValidationException>();

			link.Target = entity2;

			act.Should().NotThrow();

			linker.Links.Read(new TRUEFilterElement<Link>()).Should().ContainSingle(l => l.EntityDescriptors.Contains(entity1));
		}

		[TestMethod]
		public void ObjectLinker_Count()
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

			long result = -1;
			var act = () => result = linker.Links.Count(
				new ORFilterElement<Link>(
					LinkExposers.Source.SolutionName.Equal("Solution Z"),
					LinkExposers.Target.SolutionName.Equal("Solution Z")));

			act.Should().NotThrow();
			result.Should().Be(2);
		}
	}
}