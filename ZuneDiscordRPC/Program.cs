using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZuneDiscordRPC
{
    class Program {
        static void Main(string[] args) {
            object dlock = new object();

            var albumArtProvider = new DeezerAlbumArtProvider();
            ZuneAPI zune = new ZuneAPI(albumArtProvider);
            DiscordActivity activity = new DiscordActivity();


            zune.OnTrackChanged += (object sender, TrackChangedEventArgs e) => {
                lock(dlock) {
                    activity.Set(e);
                }
            };

            zune.OnStopped += (object sender, EventArgs e) => {
                lock (dlock) {
                    activity.Pause();
                }
            };

            while(zune.IsRunning) {
                lock (dlock) {
                    activity.Update();
                }

                Thread.Sleep(500);
            }
        }
    }
}
