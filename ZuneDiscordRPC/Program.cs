using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZuneDiscordRPC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MicrosoftZunePlayback.PlayerInterop.Instance.AlertSent += Instance_AlertSent;
            MicrosoftZunePlayback.PlayerInterop.Instance.PlayerBandwithUpdate += Instance_PlayerBandwithUpdate;
            MicrosoftZunePlayback.PlayerInterop.Instance.PlayerPropertyChanged += Instance_PlayerPropertyChanged;
            MicrosoftZunePlayback.PlayerInterop.Instance.StatusChanged += Instance_StatusChanged;
            MicrosoftZunePlayback.PlayerInterop.Instance.TransportPositionChanged += Instance_TransportPositionChanged;
            MicrosoftZunePlayback.PlayerInterop.Instance.TransportStatusChanged += Instance_TransportStatusChanged;
            MicrosoftZunePlayback.PlayerInterop.Instance.UriSet += Instance_UriSet;
            MicrosoftZunePlayback.PlayerInterop.Instance.Initialize();


            Microsoft.Zune.Shell.ZuneApplication.Launch("", IntPtr.Zero);
        }

        private static void Instance_UriSet(object sender, EventArgs e)
        {
            Console.WriteLine("Uri Set");

        }

        private static void Instance_TransportStatusChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Transport Status Changed");
            ZuneUI.PlaybackTrack track = ZuneUI.SingletonModelItem<ZuneUI.TransportControls>.Instance.CurrentTrack;
            Console.WriteLine(track.Title);
        }

        private static void Instance_TransportPositionChanged(object sender, EventArgs e)
        {
            Console.WriteLine("TPC");

        }

        private static void Instance_StatusChanged(object sender, EventArgs e)
        {
            Console.WriteLine("Status Changed: " + MicrosoftZunePlayback.PlayerInterop.Instance.State);

        }

        private static void Instance_PlayerPropertyChanged(object sender, MicrosoftZunePlayback.PlayerPropertyChangedEventArgs args)
        {
            Console.WriteLine("Player Property Changed");

        }

        private static void Instance_PlayerBandwithUpdate(object sender, MicrosoftZunePlayback.BandwidthUpdateArgs args)
        {
            Console.WriteLine("Bandwidth Update");

        }

        private static void Instance_AlertSent(MicrosoftZunePlayback.Announcement value)
        {
            Console.WriteLine("Alert Sent");
        }
    }
}
