using TootTallyCore.Graphics.ProgressCounters;

namespace TootTallySongDownloader;

internal abstract record DownloadState
{
    /// <summary>
    /// Waiting for a response from the TootTally API to see if this chart has a download/some metadata
    /// </summary>
    internal sealed record Waiting : DownloadState;

    /// <summary>
    /// User does not have the chart but a download is available
    /// </summary>
    internal sealed record DownloadAvailable : DownloadState;

    /// <summary>
    /// User does not have the chart and no download could be found
    /// </summary>
    internal sealed record DownloadUnavailable : DownloadState;

    /// <summary>
    /// The chart is currently downloading
    /// </summary>
    internal sealed record Downloading : DownloadState
    {
        internal ProgressCounter Progress;
    }

    /// <summary>
    /// The user has the chart
    /// </summary>
    internal sealed record Owned : DownloadState;
}
