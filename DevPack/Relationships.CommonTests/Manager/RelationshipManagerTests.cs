namespace Skyline.DataMiner.Solutions.Relationships.Tests
{
	using FluentAssertions;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Solutions.Relationships;
	using Skyline.DataMiner.Solutions.Relationships.Exceptions;
	using Skyline.DataMiner.Solutions.Relationships.Install.DOM;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	[TestClass]
	public class RelationshipManagerTests
	{
		private IConnection _connection;

		[TestInitialize]
		public void Setup()
		{
			_connection = Mock.CreateConnection();
		}

		[TestMethod]
		public void RelationshipManager_Creation()
		{
			var act = () => new RelationshipManager(_connection);

			act.Should().NotThrow();
		}

		[TestMethod]
		public void Relation_Between_SetsCorrectSourceAndTargetKeys()
		{
			var source = EntityFactory.Create("src-1", "Model-A");
			var target = EntityFactory.Create("tgt-1", "Model-B");

			var relation = Relation.Between(source, target);

			relation.SourceKey.Should().Be(source.Key);
			relation.TargetKey.Should().Be(target.Key);
		}

		[TestMethod]
		public void Relation_Between_WithNullSource_ThrowsArgumentNullException()
		{
			var target = EntityFactory.Create("tgt-1", "Model-B");

			var act = () => Relation.Between(null, target);

			act.Should().Throw<ArgumentNullException>().WithParameterName("source");
		}

		[TestMethod]
		public void Relation_Between_WithNullTarget_ThrowsArgumentNullException()
		{
			var source = EntityFactory.Create("src-1", "Model-A");

			var act = () => Relation.Between(source, null);

			act.Should().Throw<ArgumentNullException>().WithParameterName("target");
		}

		[TestMethod]
		public void Relation_Create_WithEmptyKeys_ThrowsValidationException()
		{
			var manager = new RelationshipManager(_connection);

			var act = () => manager.Relations.Create(new Relation());

			act.Should().Throw<ValidationException>();
		}
	}
}