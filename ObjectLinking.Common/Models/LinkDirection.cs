namespace Skyline.DataMiner.Solutions.ObjectLinking
{
	/// <summary>
	/// Specifies the direction of a link between two entities.
	/// </summary>
	public enum LinkDirection
	{
		/// <summary>
		/// Symmetric link where the order of entities doesn't matter.
		/// </summary>
		Symmetric = 0,

		/// <summary>
		/// Directed link from source to target (Source → Target).
		/// </summary>
		Forward,

		/// <summary>
		/// Directed link from target to source (Target → Source), representing a logical reversal.
		/// </summary>
		Backward,
	}
}
