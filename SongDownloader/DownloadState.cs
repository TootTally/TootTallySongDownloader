using System;

namespace TootTallySongDownloader;

internal record DownloadState
{
    internal record Waiting : DownloadState;
    internal record DownloadAvailable : DownloadState;
    internal record Downloading : DownloadState
    {
        private uint BytesDownloaded;
        private uint BytesTotal;
    }
    internal record Owned : DownloadState;
}
