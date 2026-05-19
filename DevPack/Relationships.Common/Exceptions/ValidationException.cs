namespace Skyline.DataMiner.Solutions.Relationships.Exceptions
{
	using System;
	using System.Runtime.Serialization;

	using Skyline.DataMiner.SDM;

	/// <summary>
	/// Represents errors that occur during validation operations.
	/// </summary>
	[Serializable]
	public class ValidationException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		public ValidationException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ValidationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public ValidationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class with serialized data.
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}

	/// <summary>
	/// Represents errors that occur during relation validation operations.
	/// </summary>
	public class InvalidRelationException : ValidationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRelationException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		protected InvalidRelationException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRelationException"/> class with a specified error message and relation identifier.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="relationIdentifier">The unique identifier of the relation referenced in the validation error.</param>
		public InvalidRelationException(string message, string relationIdentifier)
			: base($"{message} Identifier: {relationIdentifier}")
		{
			Relation = new SdmObjectReference<Relation>(relationIdentifier);
		}

		/// <summary>
		/// Gets or sets the reference to the <see cref="Relation"/> associated with the validation error.
		/// </summary>
		public SdmObjectReference<Relation> Relation { get; protected set; }
	}

	/// <summary>
	/// Represents errors that occur during relation entity validation operations.
	/// </summary>
	public class InvalidRelationEntityException : InvalidRelationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidRelationEntityException"/> class with a specified error message, relation identifier, and entity identifier.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="relationIdentifier">The unique identifier of the relation referenced in the validation error.</param>
		/// <param name="entityId">The identifier of the entity related to the validation error.</param>
		public InvalidRelationEntityException(string message, string relationIdentifier, string entityId)
			: base($"{message} (Identifier: {relationIdentifier}, entity: {entityId})")
		{
			Relation = new SdmObjectReference<Relation>(relationIdentifier);
			EntityID = entityId;
		}

		/// <summary>
		/// Gets the identifier of the entity associated with the validation error.
		/// </summary>
		public string EntityID { get; }
	}
}
