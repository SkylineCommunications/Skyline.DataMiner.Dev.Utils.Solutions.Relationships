namespace Skyline.DataMiner.Solutions.Relationships.Tests
{
	using System.Linq;

	using FluentAssertions;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.Relationships;
	using Skyline.DataMiner.Solutions.Relationships.Install.DOM;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	[TestClass]
	public class TraversalTests
	{
		private IConnection _connection;

		[TestInitialize]
		public void Setup()
		{
			_connection = Mock.CreateConnection();
		}

		[TestMethod]
		public void Traverse_DefaultDepth_ReturnsOnlyDirectNeighbors()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			var entityC = EntityFactory.Create("c", "Model-C");
			manager.Entities.Create(new[] { entityA, entityB, entityC });
			manager.Relate(entityA, entityB);
			manager.Relate(entityB, entityC);

			var results = manager.From(entityA).Traverse().ToList();

			results.Should().ContainSingle(r => r.Entity.Key == entityB.Key);
			results.Should().NotContain(r => r.Entity.Key == entityC.Key);
		}

		[TestMethod]
		public void Traverse_WithDepth2_ReturnsSecondLevelNeighbors()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			var entityC = EntityFactory.Create("c", "Model-C");
			manager.Entities.Create(new[] { entityA, entityB, entityC });
			manager.Relate(entityA, entityB);
			manager.Relate(entityB, entityC);

			var results = manager.From(entityA).Traverse(maxDepth: 2).ToList();

			results.Should().HaveCount(2);
			results.Should().Contain(r => r.Entity.Key == entityB.Key);
			results.Should().Contain(r => r.Entity.Key == entityC.Key);
		}

		[TestMethod]
		public void Traverse_Result_HasCorrectDepth()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			var entityC = EntityFactory.Create("c", "Model-C");
			manager.Entities.Create(new[] { entityA, entityB, entityC });
			manager.Relate(entityA, entityB);
			manager.Relate(entityB, entityC);

			var results = manager.From(entityA).Traverse(maxDepth: 2).ToList();

			results.Single(r => r.Entity.Key == entityB.Key).Depth.Should().Be(1);
			results.Single(r => r.Entity.Key == entityC.Key).Depth.Should().Be(2);
		}

		[TestMethod]
		public void Traverse_Result_HasCorrectViaEntity()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			var entityC = EntityFactory.Create("c", "Model-C");
			manager.Entities.Create(new[] { entityA, entityB, entityC });
			manager.Relate(entityA, entityB);
			manager.Relate(entityB, entityC);

			var results = manager.From(entityA).Traverse(maxDepth: 2).ToList();

			results.Single(r => r.Entity.Key == entityB.Key).Via.Key.Should().Be(entityA.Key);
			results.Single(r => r.Entity.Key == entityC.Key).Via.Key.Should().Be(entityB.Key);
		}

		[TestMethod]
		public void Traverse_WithCycle_DoesNotLoopInfinitely()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			manager.Entities.Create(new[] { entityA, entityB });
			manager.Link(entityA, entityB);  // Creates A→B and B→A

			var results = manager.From(entityA).Traverse(maxDepth: 10).ToList();

			results.Should().ContainSingle(r => r.Entity.Key == entityB.Key);
		}

		[TestMethod]
		public void Traverse_OfModel_YieldsOnlyMatchingEntities()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Element");
			var entityB = EntityFactory.Create("b", "Service");
			var entityC = EntityFactory.Create("c", "Element");
			manager.Entities.Create(new[] { entityA, entityB, entityC });
			manager.Relate(entityA, entityB);
			manager.Relate(entityB, entityC);

			var results = manager.From(entityA).Traverse(maxDepth: 2).Where(r => r.Entity.ModelName == "Element").ToList();

			results.Should().ContainSingle(r => r.Entity.Key == entityC.Key);
			results.Should().NotContain(r => r.Entity.Key == entityB.Key);
		}

		[TestMethod]
		public void Traverse_OfModel_TraversesThroughNonMatchingEntities()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Element");
			var entityB = EntityFactory.Create("b", "Service");
			var entityC = EntityFactory.Create("c", "Element");
			manager.Entities.Create(new[] { entityA, entityB, entityC });
			manager.Relate(entityA, entityB);
			manager.Relate(entityB, entityC);

			var results = manager.From(entityA).Traverse(maxDepth: 2).Where(r => r.Entity.ModelName == "Element").ToList();

			// B (Service) is not an Element so it is excluded from results
			results.Should().NotContain(r => r.Entity.Key == entityB.Key);

			// C (Element) is reachable through B and is included in results
			results.Should().Contain(r => r.Entity.Key == entityC.Key);
		}
	}
}
