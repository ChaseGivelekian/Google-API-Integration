using Google.Apis.Docs.v1.Data;

namespace Google_Drive_Organizer.Interfaces;

public interface IDocumentContentProcessor
{
    Task<string> ProcessContent(Document document);
}