using MicrosoftZuneLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zune = MicrosoftZunePlayback.PlayerInterop;
using System.Threading;
using MicrosoftZunePlayback;
using ZuneUI;

namespace ZuneDiscordRPC {
    public class TrackChangedEventArgs : EventArgs {
        public string Title;
        public string Artist;
        public string Album;
        public string ArtUrl;
        public double Duration;
    }

    class ZuneAPI {
        public event EventHandler<TrackChangedEventArgs> OnTrackChanged;
        public event EventHandler OnStopped;
        public event EventHandler OnPlaying;

        public volatile bool IsRunning;

        public ZuneAPI(IAlbumArtProvider albumArtProvider) {
            m_AlbumArtProvider = albumArtProvider;

            m_ZuneThread = new Thread(LaunchZune);
            m_ZuneThread.Start();

            while (Zune.Instance == null)
            {
                Thread.Sleep(100);
            }
             
            Zune.Instance.TransportPositionChanged += Instance_TransportPositionChanged;
            Zune.Instance.TransportStatusChanged += Instance_TransportStatusChanged;
            Zune.Instance.Initialize();

            m_TransportState = MicrosoftZunePlayback.MCTransportState.Stopped;

            IsRunning = true;
        }

        private void LaunchZune() {
            Microsoft.Zune.Shell.ZuneApplication.Launch("", IntPtr.Zero);
            IsRunning = false;
        }

        /*
         * Event Handlers
         */
        private void OnPlay() {
            var track = ZuneUI.SingletonModelItem<ZuneUI.TransportControls>.Instance.CurrentTrack;

            if (track is ZuneUI.LibraryPlaybackTrack)
            {
                ZuneUI.LibraryPlaybackTrack libraryPlaybackTrack = (ZuneUI.LibraryPlaybackTrack)track;
                if (libraryPlaybackTrack.AlbumLibraryId > 0)
                {
                    AlbumMetadata albumMetadata = ZuneUI.FindAlbumInfoHelper.GetAlbumMetadata(libraryPlaybackTrack.AlbumLibraryId);

                    TrackChangedEventArgs args = new TrackChangedEventArgs();
                    args.Artist = albumMetadata.AlbumArtist;
                    args.Album = albumMetadata.AlbumTitle;
                    args.Title = track.Title;
                    args.Duration = track.Duration.TotalSeconds;
                    args.ArtUrl = m_AlbumArtProvider.GetAlbumArt(albumMetadata.AlbumTitle, albumMetadata.AlbumArtist);
                    OnTrackChanged?.Invoke(this, args);
                }
            }

            OnPlaying?.Invoke(this, EventArgs.Empty);
        }

        private void OnStop() {
            Console.WriteLine("STOPPED");
            OnStopped?.Invoke(this, EventArgs.Empty);
        }

        private void Instance_TransportStatusChanged(object sender, EventArgs e) {
            if (m_TransportState == Zune.Instance.TransportState)
                return;

            switch (Zune.Instance.TransportState)
            {
                case MicrosoftZunePlayback.MCTransportState.Playing:
                    OnPlay();
                    break;
                case MicrosoftZunePlayback.MCTransportState.Stopped:
                case MicrosoftZunePlayback.MCTransportState.Paused:
                    OnStop();
                    break;
            }
        }

        private void Instance_TransportPositionChanged(object sender, EventArgs e) {

        }

        private MicrosoftZunePlayback.MCTransportState m_TransportState;
        private Thread m_ZuneThread;
        private IAlbumArtProvider m_AlbumArtProvider;
    }
}
