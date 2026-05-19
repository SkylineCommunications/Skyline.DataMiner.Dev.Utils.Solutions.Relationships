namespace Skyline.DataMiner.Solutions.Relationships.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Collections;
	using FluentAssertions.Execution;

	internal static class FluentAssertionsExtensions
	{
		public static AndConstraint<GenericCollectionAssertions<T>> Contain<T>(
			this GenericCollectionAssertions<T> assertions,
			T expected,
			IEqualityComparer<T> comparer,
			string because = "",
			params object[] becauseArgs)
		{
			Execute.Assertion
				.ForCondition(assertions.Subject.Any(x => comparer.Equals(x, expected)))
				.BecauseOf(because, becauseArgs)
				.FailWith("Expected collection to contain {0} using custom comparer{reason}, but it does not.", expected);

			return new AndConstraint<GenericCollectionAssertions<T>>(assertions);
		}

		public static AndConstraint<GenericCollectionAssertions<T>> NotContain<T>(
			this GenericCollectionAssertions<T> assertions,
			T unexpected,
			IEqualityComparer<T> comparer,
			string because = "",
			params object[] becauseArgs)
		{
			Execute.Assertion
				.ForCondition(assertions.Subject == null ||
							  !assertions.Subject.Any(x => comparer.Equals(x, unexpected)))
				.BecauseOf(because, becauseArgs)
				.FailWith(
					"Did not expect {context:collection} to contain {0} using {1}{reason}, but it does.",
					unexpected,
					comparer);

			return new AndConstraint<GenericCollectionAssertions<T>>(assertions);
		}
	}
}
