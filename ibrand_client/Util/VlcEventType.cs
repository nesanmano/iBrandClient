namespace NomadLogic.iBrandDesktop.Util
{
    internal enum VlcEventType
    {
        LibvlcMediaMetaChanged = 0, LibvlcMediaSubItemAdded, LibvlcMediaDurationChanged, LibvlcMediaParsedChanged,
        LibvlcMediaFreed, LibvlcMediaStateChanged, LibvlcMediaPlayerMediaChanged = 0x100, LibvlcMediaPlayerNothingSpecial,
        LibvlcMediaPlayerOpening, LibvlcMediaPlayerBuffering, LibvlcMediaPlayerPlaying, LibvlcMediaPlayerPaused,
        LibvlcMediaPlayerStopped, LibvlcMediaPlayerForward, LibvlcMediaPlayerBackward, LibvlcMediaPlayerEndReached,
        LibvlcMediaPlayerEncounteredError, LibvlcMediaPlayerTimeChanged, LibvlcMediaPlayerPositionChanged, LibvlcMediaPlayerSeekableChanged,
        LibvlcMediaPlayerPausableChanged, LibvlcMediaPlayerTitleChanged, LibvlcMediaPlayerSnapshotTaken, LibvlcMediaPlayerLengthChanged,
        LibvlcMediaPlayerVout, LibvlcMediaListItemAdded = 0x200, LibvlcMediaListWillAddItem, LibvlcMediaListItemDeleted,
        LibvlcMediaListWillDeleteItem, LibvlcMediaListViewItemAdded = 0x300, LibvlcMediaListViewWillAddItem, LibvlcMediaListViewItemDeleted,
        LibvlcMediaListViewWillDeleteItem, LibvlcMediaListPlayerPlayed = 0x400, LibvlcMediaListPlayerNextItemSet, LibvlcMediaListPlayerStopped,
        LibvlcMediaDiscovererStarted = 0x500, LibvlcMediaDiscovererEnded, LibvlcVlmMediaAdded = 0x600, LibvlcVlmMediaRemoved,
        LibvlcVlmMediaChanged, LibvlcVlmMediaInstanceStarted, LibvlcVlmMediaInstanceStopped, LibvlcVlmMediaInstanceStatusInit,
        LibvlcVlmMediaInstanceStatusOpening, LibvlcVlmMediaInstanceStatusPlaying, LibvlcVlmMediaInstanceStatusPause, LibvlcVlmMediaInstanceStatusEnd,
        LibvlcVlmMediaInstanceStatusError 
    }
}