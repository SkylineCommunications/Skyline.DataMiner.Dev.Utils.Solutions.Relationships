namespace Skyline.DataMiner.Solutions.Relationships.Tests
{
	using System.Linq;

	using FluentAssertions;

	using Moq;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Solutions.Relationships;
	using Skyline.DataMiner.Solutions.Relationships.Install.DOM;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	[TestClass]
	public class GetRelationsByEntityTests
	{
		private IConnection _connection;

		[TestInitialize]
		public void Setup()
		{
			_connection = Mock.CreateConnection();
		}

		[TestMethod]
		public void GetRelations_WithBidirectionalLink_ReturnsBothRelations()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");

			manager.Link(entityA, entityB);

			var result = manager.From(entityA).GetRelations().ToList();

			result.Should().HaveCount(2);
			result.Should().Contain(r => r.SourceKey == entityA.Key && r.TargetKey == entityB.Key);
			result.Should().Contain(r => r.SourceKey == entityB.Key && r.TargetKey == entityA.Key);
		}

		[TestMethod]
		public void GetRelations_WithDirectedRelation_ReturnsRelationWhereEntityIsSource()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");

			manager.Relate(entityA, entityB);

			var result = manager.From(entityA).GetRelations().ToList();

			result.Should().ContainSingle();
			result[0].SourceKey.Should().Be(entityA.Key);
			result[0].TargetKey.Should().Be(entityB.Key);
		}

		[TestMethod]
		public void GetRelations_WithDirectedRelation_ReturnsRelationWhereEntityIsTarget()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");

			manager.Relate(entityB, entityA);

			var result = manager.From(entityA).GetRelations().ToList();

			result.Should().ContainSingle();
			result[0].SourceKey.Should().Be(entityB.Key);
			result[0].TargetKey.Should().Be(entityA.Key);
		}

		[TestMethod]
		public void GetRelations_WithNoRelations_ReturnsEmpty()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");

			var result = manager.From(entityA).GetRelations().ToList();

			result.Should().BeEmpty();
		}

		[TestMethod]
		public void GetRelations_WithMultipleRelations_ReturnsAll()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			var entityC = EntityFactory.Create("c", "Model-C");

			manager.Link(entityA, entityB);    // Creates A→B and B→A
			manager.Relate(entityC, entityA);  // Creates C→A

			var result = manager.From(entityA).GetRelations().ToList();

			result.Should().HaveCount(3);
			result.Should().Contain(r => r.SourceKey == entityA.Key && r.TargetKey == entityB.Key);
			result.Should().Contain(r => r.SourceKey == entityB.Key && r.TargetKey == entityA.Key);
			result.Should().Contain(r => r.SourceKey == entityC.Key && r.TargetKey == entityA.Key);
		}
	}
}