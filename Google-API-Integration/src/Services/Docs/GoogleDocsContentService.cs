using System.Text;
using Google_Drive_Organizer.Exceptions;
using Google_Drive_Organizer.Interfaces;
using Google_Drive_Organizer.Services.Docs.DocsContentProcessing;
using Google;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;

namespace Google_Drive_Organizer.Services.Docs;

public class GoogleDocsContentService(DocsService docsService)
{
    private readonly DocsService _docsService = docsService ?? throw new ArgumentNullException(nameof(docsService));

    private readonly IEnumerable<IDocumentContentProcessor> _contentProcessors =
    [
        new TabContentProcessor(),
        new BodyContentProcessor()
    ];

    public async Task<Document> GetDocumentAsync(string documentId)
    {
        if (string.IsNullOrEmpty(documentId)) throw new ArgumentNullException(nameof(documentId));

        try
        {
            return await _docsService.Documents.Get(documentId).ExecuteAsync();
        }
        catch (GoogleApiException e) when (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new DocumentNotFoundException(documentId, e);
        }
    }

    public async Task<string> ExtractDocumentContent(Document document)
    {
        var results = new StringBuilder();

        foreach (var processor in _contentProcessors)
        {
            var content = await processor.ProcessContent(document);
            if (!string.IsNullOrEmpty(content))
            {
                results.AppendLine(content);
            }
        }

        return results.ToString();
    }
}