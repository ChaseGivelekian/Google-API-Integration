namespace Google_API_Integration.Exceptions;

public class DocumentNotFoundException(string message, Exception innerException) : Exception(message, innerException);