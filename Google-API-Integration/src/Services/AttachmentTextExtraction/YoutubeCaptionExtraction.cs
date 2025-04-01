using System.Text;
using Google.Apis.YouTube.v3;

namespace Google_API_Integration.Services.AttachmentTextExtraction;

public static class YoutubeCaptionExtraction
{
    private static readonly YouTubeService YouTubeService = GoogleCredentialsManager.CreateYouTubeServiceAsync().GetAwaiter().GetResult() ?? throw new NullReferenceException();

    public static async Task<string> ExtractTextFromYoutubeIdAsync(string youtubeId)
    {
        var captionsListRequest = YouTubeService.Captions.List("snippet", youtubeId);
        var captionsListResponse = await captionsListRequest.ExecuteAsync();

        if (captionsListResponse.Items == null || !captionsListResponse.Items.Any())
        {
            return "No captions available for this video.";
        }

        var captionTrack = captionsListResponse.Items.First();

        using var captionsStream = new MemoryStream();
        await YouTubeService.Captions.Download(captionTrack.Id).DownloadAsync(captionsStream);

        captionsStream.Position = 0;
        using var reader = new StreamReader(captionsStream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}