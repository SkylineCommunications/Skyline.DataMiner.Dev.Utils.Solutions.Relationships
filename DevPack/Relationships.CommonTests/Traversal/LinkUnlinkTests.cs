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
	public class LinkUnlinkTests
	{
		private IConnection _connection;

		[TestInitialize]
		public void Setup()
		{
			_connection = Mock.CreateConnection();
		}

		[TestMethod]
		public void Link_CreatesBothDirections()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");

			manager.Link(entityA, entityB);

			var relations = manager.Relations.Read(new TRUEFilterElement<Relation>()).ToList();

			relations.Should().HaveCount(2);
			relations.Should().Contain(r => r.SourceKey == entityA.Key && r.TargetKey == entityB.Key);
			relations.Should().Contain(r => r.SourceKey == entityB.Key && r.TargetKey == entityA.Key);
		}

		[TestMethod]
		public void Unlink_RemovesBothDirections()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			manager.Link(entityA, entityB);

			manager.Unlink(entityA, entityB);

			var relations = manager.Relations.Read(new TRUEFilterElement<Relation>()).ToList();

			relations.Should().BeEmpty();
		}

		[TestMethod]
		public void Unlink_WhenNoRelationsExist_DoesNotThrow()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");

			var act = () => manager.Unlink(entityA, entityB);

			act.Should().NotThrow();
		}

		[TestMethod]
		public void Relate_CreatesSingleDirectedRelation()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");

			manager.Relate(entityA, entityB);

			var relations = manager.Relations.Read(new TRUEFilterElement<Relation>()).ToList();

			relations.Should().ContainSingle();
			relations[0].SourceKey.Should().Be(entityA.Key);
			relations[0].TargetKey.Should().Be(entityB.Key);
		}

		[TestMethod]
		public void Unrelate_RemovesOnlySpecifiedDirection()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");
			manager.Link(entityA, entityB);

			manager.Unrelate(entityA, entityB);

			var relations = manager.Relations.Read(new TRUEFilterElement<Relation>()).ToList();

			relations.Should().ContainSingle();
			relations[0].SourceKey.Should().Be(entityB.Key);
			relations[0].TargetKey.Should().Be(entityA.Key);
		}

		[TestMethod]
		public void Unrelate_WhenNoRelationExists_DoesNotThrow()
		{
			var manager = new RelationshipManager(_connection);
			var entityA = EntityFactory.Create("a", "Model-A");
			var entityB = EntityFactory.Create("b", "Model-B");

			var act = () => manager.Unrelate(entityA, entityB);

			act.Should().NotThrow();
		}
	}
}
