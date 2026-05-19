namespace Skyline.DataMiner.Solutions.Relationships.Validation
{
	using System;
	using System.Collections.Generic;

	internal class ValidationEntry
	{
		public bool IsValid => Exceptions.Count == 0;

		public List<Exception> Exceptions { get; } = new List<Exception>();

		public Exception ToException()
		{
			return new ValidationResult.Builder()
				.Add(this)
				.Build()
				.ToException();
		}
	}
}
