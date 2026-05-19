namespace Skyline.DataMiner.Solutions.Relationships.Tests
{
	using System.Linq;

	using FluentAssertions;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Solutions.Relationships;
	using Skyline.DataMiner.Solutions.Relationships.Install.DOM;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	/// <summary>
	/// Verifies that <see cref="EntityKeyExposer{TObject}"/> produces filters that correctly
	/// translate through the DOM repository pipeline end-to-end.
	/// Each test creates relations, then reads them back with a specific filter so that
	/// both the filter-building logic in <see cref="EntityKeyExposer{TObject}"/> and the
	/// <c>CreateFilter</c> / <c>CreateOrderBy</c> switch cases in the repository are covered.
	/// </summary>
	[TestClass]
	public class EntityKeyExposerTests
	{
		private IConnection _connection;
		private RelationshipManager _manager;

		private EntityDescriptor _entityA;
		private EntityDescriptor _entityB;
		private EntityDescriptor _entityC;

		[TestInitialize]
		public void Setup()
		{
			_connection = Mock.CreateConnection();
			_manager = new RelationshipManager(_connection);

			// Three distinct entities spanning different models and solutions.
			_entityA = EntityFactory.Create("id-a", "Model-Alpha", "sol-1");
			_entityB = EntityFactory.Create("id-b", "Model-Beta", "sol-1");
			_entityC = EntityFactory.Create("id-c", "Model-Alpha", "sol-2");
		}

		// ─── SourceKey.Equal ───────────────────────────────────────────────────────

		[TestMethod]
		public void SourceKey_Equal_ReturnsOnlyRelationsWithMatchingSourceKey()
		{
			// A→B, B→A, A→C — filter should return only A→B and A→C.
			_manager.Relate(_entityA, _entityB);
			_manager.Relate(_entityB, _entityA);
			_manager.Relate(_entityA, _entityC);

			var results = _manager.Relations
				.Read(RelationExposers.SourceKey.Equal(_entityA.Key))
				.ToList();

			results.Should().HaveCount(2);
			results.Should().OnlyContain(r => r.SourceKey == _entityA.Key);
		}

		[TestMethod]
		public void SourceKey_Equal_ReturnsEmpty_WhenNoMatchingRelations()
		{
			_manager.Relate(_entityB, _entityC);

			var results = _manager.Relations
				.Read(RelationExposers.SourceKey.Equal(_entityA.Key))
				.ToList();

			results.Should().BeEmpty();
		}

		// ─── TargetKey.Equal ───────────────────────────────────────────────────────

		[TestMethod]
		public void TargetKey_Equal_ReturnsOnlyRelationsWithMatchingTargetKey()
		{
			// A→B, C→B, A→C — filter should return only A→B and C→B.
			_manager.Relate(_entityA, _entityB);
			_manager.Relate(_entityC, _entityB);
			_manager.Relate(_entityA, _entityC);

			var results = _manager.Relations
				.Read(RelationExposers.TargetKey.Equal(_entityB.Key))
				.ToList();

			results.Should().HaveCount(2);
			results.Should().OnlyContain(r => r.TargetKey == _entityB.Key);
		}

		// ─── SourceKey.NotEqual ────────────────────────────────────────────────────

		[TestMethod]
		public void SourceKey_NotEqual_ExcludesRelationsWithMatchingSourceKey()
		{
			// A→B, B→C, C→A — NotEqual(A) should return B→C and C→A.
			_manager.Relate(_entityA, _entityB);
			_manager.Relate(_entityB, _entityC);
			_manager.Relate(_entityC, _entityA);

			var results = _manager.Relations
				.Read(RelationExposers.SourceKey.NotEqual(_entityA.Key))
				.ToList();

			results.Should().HaveCount(2);
			results.Should().NotContain(r => r.SourceKey == _entityA.Key);
		}

		// ─── TargetKey.NotEqual ────────────────────────────────────────────────────

		[TestMethod]
		public void TargetKey_NotEqual_ExcludesRelationsWithMatchingTargetKey()
		{
			// A→B, B→C — NotEqual(B) as target should return only B→C.
			_manager.Relate(_entityA, _entityB);
			_manager.Relate(_entityB, _entityC);

			var results = _manager.Relations
				.Read(RelationExposers.TargetKey.NotEqual(_entityB.Key))
				.ToList();

			results.Should().ContainSingle();
			results[0].TargetKey.Should().Be(_entityC.Key);
		}

		// ─── Component sub-exposers (SourceKey) ───────────────────────────────────

		[TestMethod]
		public void SourceKey_ID_Equal_FiltersByIdComponent()
		{
			// A→B and C→B share Model-Alpha but differ in solution; ID is unique per entity.
			_manager.Relate(_entityA, _entityB);
			_manager.Relate(_entityC, _entityB);

			var results = _manager.Relations
				.Read(RelationExposers.SourceKey.ID.Equal(_entityA.Key.ID))
				.ToList();

			results.Should().ContainSingle();
			results[0].SourceKey.ID.Should().Be(_entityA.Key.ID);
		}

		[TestMethod]
		public void SourceKey_ModelName_Equal_FiltersByModelNameComponent()
		{
			// A and C both have Model-Alpha as source; B has Model-Beta.
			_manager.Relate(_entityA, _entityB);
			_manager.Relate(_entityC, _entityB);
			_manager.Relate(_entityB, _entityA);

			var results = _manager.Relations
				.Read(RelationExposers.SourceKey.ModelName.Equal("Model-Alpha"))
				.ToList();

			results.Should().HaveCount(2);
			results.Should().OnlyContain(r => r.SourceKey.ModelName == "Model-Alpha");
		}

		[TestMethod]
		public void SourceKey_SolutionID_Equal_FiltersBySolutionIDComponent()
		{
			// A (sol-1) and B (sol-1) vs C (sol-2).
			_manager.Relate(_entityA, _entityC);
			_manager.Relate(_entityB, _entityC);
			_manager.Relate(_entityC, _entityA);

			var results = _manager.Relations
				.Read(RelationExposers.SourceKey.SolutionID.Equal("sol-1"))
				.ToList();

			results.Should().HaveCount(2);
			results.Should().OnlyContain(r => r.SourceKey.SolutionID == "sol-1");
		}

		// ─── Component sub-exposers (TargetKey) ───────────────────────────────────

		[TestMethod]
		public void TargetKey_ID_Equal_FiltersByIdComponent()
		{
			_manager.Relate(_entityA, _entityB);
			_manager.Relate(_entityA, _entityC);

			var results = _manager.Relations
				.Read(RelationExposers.TargetKey.ID.Equal(_entityB.Key.ID))
				.ToList();

			results.Should().ContainSingle();
			results[0].TargetKey.ID.Should().Be(_entityB.Key.ID);
		}

		[TestMethod]
		public void TargetKey_ModelName_Equal_FiltersByModelNameComponent()
		{
			// A→B (target Model-Beta), A→C (target Model-Alpha), B→C (target Model-Alpha).
			_manager.Relate(_entityA, _entityB);
			_manager.Relate(_entityA, _entityC);
			_manager.Relate(_entityB, _entityC);

			var results = _manager.Relations
				.Read(RelationExposers.TargetKey.ModelName.Equal("Model-Alpha"))
				.ToList();

			results.Should().HaveCount(2);
			results.Should().OnlyContain(r => r.TargetKey.ModelName == "Model-Alpha");
		}

		[TestMethod]
		public void TargetKey_SolutionID_Equal_FiltersBySolutionIDComponent()
		{
			// C belongs to sol-2; A and B belong to sol-1.
			_manager.Relate(_entityA, _entityC);
			_manager.Relate(_entityB, _entityC);
			_manager.Relate(_entityA, _entityB);

			var results = _manager.Relations
				.Read(RelationExposers.TargetKey.SolutionID.Equal("sol-2"))
				.ToList();

			results.Should().HaveCount(2);
			results.Should().OnlyContain(r => r.TargetKey.SolutionID == "sol-2");
		}
	}
}
