using Google.Apis.Docs.v1.Data;

namespace Google_API_Integration.Interfaces;

public interface IDocumentContentProcessor
{
    Task<string> ProcessContent(Document document);
}