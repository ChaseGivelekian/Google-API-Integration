namespace Google_Drive_Organizer.Exceptions;

public class DocumentNotFoundException(string message, Exception innerException) : Exception(message, innerException);