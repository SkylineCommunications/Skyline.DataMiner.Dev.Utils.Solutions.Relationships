namespace Skyline.DataMiner.Solutions.ObjectLinking.Exceptions
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
	/// Represents errors that occur during link validation operations.
	/// </summary>
	public class LinkValidationException : ValidationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinkValidationException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		protected LinkValidationException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkValidationException"/> class with a specified error message and link identifier.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="linkIdentifier">The unique identifier of the link related to the validation error.</param>
		public LinkValidationException(string message, string linkIdentifier)
			: base($"{message} Identifier: {linkIdentifier}")
		{
			Link = new SdmObjectReference<Link>(linkIdentifier);
		}

		/// <summary>
		/// Gets or sets the reference to the <see cref="Link"/> associated with the validation error.
		/// </summary>
		public SdmObjectReference<Link> Link { get; protected set; }
	}

	/// <summary>
	/// Represents errors that occur during link entity validation operations.
	/// </summary>
	public class LinkEntityValidationException : LinkValidationException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinkEntityValidationException"/> class with a specified error message, link identifier, and entity identifier.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="linkIdentifier">The unique identifier of the link related to the validation error.</param>
		/// <param name="entityId">The identifier of the entity related to the validation error.</param>
		public LinkEntityValidationException(string message, string linkIdentifier, string entityId)
			: base($"{message} (Identifier: {linkIdentifier}, entity: {entityId})")
		{
			Link = new SdmObjectReference<Link>(linkIdentifier);
			EntityID = entityId;
		}

		/// <summary>
		/// Gets the identifier of the entity associated with the validation error.
		/// </summary>
		public string EntityID { get; }
	}
}
