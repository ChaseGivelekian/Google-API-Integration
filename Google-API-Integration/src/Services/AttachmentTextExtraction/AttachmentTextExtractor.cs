using Aspose.Words;
using Google.Apis.Classroom.v1.Data;

namespace Google_API_Integration.Services.AttachmentTextExtraction;

public static class AttachmentTextExtractor
{
    public static async Task<List<string>> ExtractTextFromAttachmentsAsync(CourseWork courseWork)
    {
         List<string> attachmentText = [];

        foreach (var material in courseWork.Materials)
        {
            if (material.DriveFile != null)
            {
                var extractedText = await ExtractTextFromDriveFileAsync(material.DriveFile);

                attachmentText.Add($"This is the text from the {material.DriveFile.DriveFile.Title} attachment: {(string.IsNullOrWhiteSpace(extractedText) ? "This file has no content" : extractedText)}");
            }

            if (material.YoutubeVideo != null)
            {
                attachmentText.Add(await ExtractTextFromYoutubeVideoAsync(material.YoutubeVideo));
            }

            if (material.Link != null)
            {
                var extractedText = await ExtractTextFromLinkAsync(material.Link);

                attachmentText.Add($"This is the text from the {material.Link.Title} attachment: {(string.IsNullOrWhiteSpace(extractedText) ? "This file has no content" : extractedText)}");
            }

            if (material.Form != null)
            {
                attachmentText.Add(await ExtractTextFromFormAsync(material.Form));
            }
        }

        return attachmentText;
    }

    private static async Task<string> ExtractTextFromFormAsync(Form form)
    {
        return await GoogleFormTextExtraction.ExtractTextFromGoogleFormIdAsync(form.FormUrl);
    }

    private static async Task<string> ExtractTextFromLinkAsync(Link link)
    {
        return await new HttpClient().GetStringAsync(link.Url);
    }

    private static async Task<string> ExtractTextFromYoutubeVideoAsync(YouTubeVideo youtubeVideo)
    {
        return await YoutubeCaptionExtraction.ExtractTextFromYoutubeIdAsync(youtubeVideo.Id);
    }

    private static async Task<string> ExtractTextFromDriveFileAsync(SharedDriveFile sharedDriveFile)
    {
        var driveService = await GoogleCredentialsManager.CreateDriveServiceAsync();

        // Download the file content into memory
        var request = driveService.Files.Get(sharedDriveFile.DriveFile.Id);
        using var stream = new MemoryStream();
        await request.DownloadAsync(stream);
        stream.Position = 0;

        // Load the document and extract text
        var doc = new Document(stream);
        return doc.GetText();
    }
}