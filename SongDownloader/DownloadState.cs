using TootTallyCore.Graphics.ProgressCounter;

namespace TootTallySongDownloader;

internal record DownloadState
{
    /// <summary>
    /// Waiting for a response from the TootTally API to see if this chart has a download/some metadata
    /// </summary>
    internal record Waiting : DownloadState;

    /// <summary>
    /// User does not have the chart but a download is available
    /// </summary>
    internal record DownloadAvailable : DownloadState;

    /// <summary>
    /// User does not have the chart and no download could be found
    /// </summary>
    internal record DownloadUnavailable : DownloadState;

    /// <summary>
    /// The chart is currently downloading
    /// </summary>
    internal record Downloading : DownloadState
    {
        internal ProgressCounter Progress;
    }

    /// <summary>
    /// The user has the chart
    /// </summary>
    internal record Owned : DownloadState;
}
