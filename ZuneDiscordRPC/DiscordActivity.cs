using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZuneUI;

namespace ZuneDiscordRPC
{
    class DiscordActivity {
        public DiscordActivity() {
            m_Discord = new Discord.Discord(904704369998561321, (UInt64)Discord.CreateFlags.Default);  

            m_Activity = new Discord.Activity
            {
                Type = ActivityType.Playing,
                State = "Idle",
                Details = "Not playing",
                
                Timestamps = {
                    Start = 0,
                    End = 0,
                },
               
                Assets = {
                    LargeImage = "modern_zune_logo",
                    SmallImage = "modern_zune_logo"
                }
            };
        }

        public void Set(TrackChangedEventArgs track) {
            if(m_Discord == null) {
                m_Discord = new Discord.Discord(904704369998561321, (UInt64)Discord.CreateFlags.Default);
            }

            double currentTimestamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
            currentTimestamp += track.Duration;
            Console.WriteLine(track.Duration);

            m_Activity.Details = track.Artist + " - " + track.Title;
            m_Activity.State = track.Album;
            m_Activity.Timestamps.End = (long)currentTimestamp;

            m_Discord.GetActivityManager().UpdateActivity(m_Activity, result => { });
        }

        public void Update() {
            try {
                m_Discord?.RunCallbacks();
            } catch (Exception ex) { }
        }

        public void Stop() {
            m_Discord?.Dispose();
            m_Discord = null;
        }

        private static Discord.Discord m_Discord;
        private static Discord.Activity m_Activity;
    }
}
