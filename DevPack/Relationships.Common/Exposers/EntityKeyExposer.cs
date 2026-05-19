namespace Skyline.DataMiner.Solutions.Relationships
{
	using System;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <summary>
	/// A composite exposer for an <see cref="EntityKey"/> property.
	/// Provides filter methods for the whole key and exposes each key component
	/// individually so callers can build component-level filters.
	/// </summary>
	/// <typeparam name="TObject">The model type that owns the <see cref="EntityKey"/> property.</typeparam>
	public class EntityKeyExposer<TObject> : Exposer<TObject, EntityKey>
	{
		/// <summary>Filters on the <see cref="EntityKey.ID"/> component of this key.</summary>
		public readonly Exposer<TObject, string> ID;

		/// <summary>Filters on the <see cref="EntityKey.ModelName"/> component of this key.</summary>
		public readonly Exposer<TObject, string> ModelName;

		/// <summary>Filters on the <see cref="EntityKey.SolutionID"/> component of this key.</summary>
		public readonly Exposer<TObject, string> SolutionID;

		internal EntityKeyExposer(Func<TObject, EntityKey> getter, string prefix)
			: base(prefix)
		{
			ID = new Exposer<TObject, string>(obj => getter(obj)?.ID, prefix + ".ID");
			ModelName = new Exposer<TObject, string>(obj => getter(obj)?.ModelName, prefix + ".ModelName");
			SolutionID = new Exposer<TObject, string>(obj => getter(obj)?.SolutionID, prefix + ".SolutionID");
		}

		internal EntityKeyExposer(
			Exposer<TObject, string> idExposer,
			Exposer<TObject, string> modelNameExposer,
			Exposer<TObject, string> solutionIdExposer)
		{
			ID = idExposer ?? throw new ArgumentNullException(nameof(idExposer));
			ModelName = modelNameExposer ?? throw new ArgumentNullException(nameof(modelNameExposer));
			SolutionID = solutionIdExposer ?? throw new ArgumentNullException(nameof(solutionIdExposer));
		}

		/// <summary>
		/// Returns a filter that matches objects whose key equals <paramref name="key"/>
		/// (all three components — ID, ModelName and SolutionID — must match).
		/// </summary>
		/// <param name="key">The entity key to compare against.</param>
		/// <returns>A filter that matches when all components of the key are equal.</returns>
		public FilterElement<TObject> Equal(EntityKey key)
		{
			return new ANDFilterElement<TObject>(
				ID.Equal(key.ID),
				ModelName.Equal(key.ModelName),
				SolutionID.Equal(key.SolutionID));
		}

		/// <summary>
		/// Returns a filter that matches objects whose key does not equal <paramref name="key"/>
		/// (at least one component — ID, ModelName or SolutionID — must differ).
		/// </summary>
		/// <param name="key">The entity key to compare against.</param>
		/// <returns>A filter that matches when at least one of the components of the key differ.</returns>
		public FilterElement<TObject> NotEqual(EntityKey key)
		{
			// De Morgan: NOT(A AND B AND C) = NOT A OR NOT B OR NOT C
			return new ORFilterElement<TObject>(
				ID.NotEqual(key.ID),
				ModelName.NotEqual(key.ModelName),
				SolutionID.NotEqual(key.SolutionID));
		}
	}
}
