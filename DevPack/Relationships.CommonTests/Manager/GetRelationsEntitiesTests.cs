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
	public class GetNeighborsTests
	{
		private IConnection _connection;

		[TestInitialize]
		public void Setup()
		{
			_connection = Mock.CreateConnection();
		}

		[TestMethod]
		public void GetNeighbors_WithBidirectionalLink_ReturnsNeighbor()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			manager.Entities.Create(new[] { entityA, entityB });

			manager.Link(entityA, entityB);

			var result = manager.From(entityA).GetNeighbors().ToList();

			result.Should().ContainSingle(e => e.Key == entityB.Key);
		}

		[TestMethod]
		public void GetNeighbors_WithDirectedRelationAsSource_ReturnsTarget()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			manager.Entities.Create(new[] { entityA, entityB });

			manager.Relate(entityA, entityB);

			var result = manager.From(entityA).GetNeighbors().ToList();

			result.Should().ContainSingle(e => e.Key == entityB.Key);
		}

		[TestMethod]
		public void GetNeighbors_WithDirectedRelationAsTarget_ReturnsSource()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			manager.Entities.Create(new[] { entityA, entityB });

			manager.Relate(entityB, entityA);

			var result = manager.From(entityA).GetNeighbors().ToList();

			result.Should().ContainSingle(e => e.Key == entityB.Key);
		}

		[TestMethod]
		public void GetNeighbors_WithMultipleNeighbors_ReturnsAll()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			var entityC = EntityFactory.Create("c", "Model-C");
			manager.Entities.Create(new[] { entityA, entityB, entityC });

			manager.Link(entityA, entityB);
			manager.Link(entityA, entityC);

			var result = manager.From(entityA).GetNeighbors().ToList();

			result.Should().HaveCount(2);
			result.Should().Contain(e => e.Key == entityB.Key);
			result.Should().Contain(e => e.Key == entityC.Key);
		}

		[TestMethod]
		public void GetNeighbors_WithNoRelations_ReturnsEmpty()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			manager.Entities.Create(entityA);

			var result = manager.From(entityA).GetNeighbors().ToList();

			result.Should().BeEmpty();
		}

		[TestMethod]
		public void GetNeighbors_WithOfModel_FiltersToMatchingModelOnly()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityElement = EntityFactory.Create("elem-1", "Element");
			var entityService = EntityFactory.Create("svc-1", "Service");
			manager.Entities.Create(new[] { entityA, entityElement, entityService });

			manager.Link(entityA, entityElement);
			manager.Link(entityA, entityService);

			var result = manager.From(entityA).Where(EntityDescriptorExposers.ModelName.Equal("Element")).GetNeighbors().ToList();

			result.Should().ContainSingle(e => e.Key == entityElement.Key);
			result.Should().NotContain(e => e.Key == entityService.Key);
		}
	}
}