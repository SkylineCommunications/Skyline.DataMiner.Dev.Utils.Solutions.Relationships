namespace Skyline.DataMiner.Solutions.ObjectLinking.Validation
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;

	using Skyline.DataMiner.Solutions.ObjectLinking.Exceptions;

	internal class ValidationResult
	{
		private ValidationResult(List<ValidationEntry> entries)
		{
			Entries = new ReadOnlyCollection<ValidationEntry>(entries ?? throw new ArgumentNullException(nameof(entries), "Entries cannot be null."));
			IsValid = !Entries.Any(x => !x.IsValid);
		}

		public bool IsValid { get; }

		public IReadOnlyList<ValidationEntry> Entries { get; }

		public Exception ToException()
		{
			var exceptionMessage = ToString();
			return new ValidationException(exceptionMessage);
		}

		public override string ToString()
		{
			if (Entries == null || Entries.Count == 0)
			{
				return "Validation succeeded.";
			}

			var builder = new StringBuilder();
			for (int i = 0; i < Entries.Count; i++)
			{
				var entry = Entries[i];
				if (entry.IsValid)
				{
					continue; // Skip valid entries
				}

				builder.AppendLine($"Entry {i}:");
				foreach (var ex in entry.Exceptions)
				{
					builder.AppendLine($" - {ex.Message}");
				}
			}

			return builder.ToString();
		}

		internal class Builder
		{
			private readonly List<ValidationEntry> _entries = new List<ValidationEntry>();

			public Builder Add(ValidationEntry result)
			{
				if (result is null)
				{
					throw new ArgumentNullException(nameof(result), "Validation result cannot be null.");
				}

				_entries.Add(result);
				return this;
			}

			public void AddRange(IEnumerable<ValidationEntry> result)
			{
				if (result is null)
				{
					throw new ArgumentNullException(nameof(result), "Validation results cannot be null.");
				}

				_entries.AddRange(result);
			}

			public ValidationResult Build()
			{
				return new ValidationResult(_entries);
			}
		}
	}
}
