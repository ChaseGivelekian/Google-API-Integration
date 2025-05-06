using System.Text;
using Google_API_Integration.Interfaces;
using Google_API_Integration.Services.Docs.ContentProcessing;
using Google.Apis.Docs.v1.Data;

namespace Google_API_Integration.Services.Docs;

public class GoogleDocsContentService
{
    private readonly IEnumerable<IDocumentContentProcessor> _contentProcessors =
    [
        new TabContentProcessor(),
        new BodyContentProcessor()
    ];

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