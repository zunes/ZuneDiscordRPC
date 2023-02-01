using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZuneUI;

namespace ZuneDiscordRPC
{
    class DiscordActivity {
        public DiscordActivity() {
            InitDiscord();

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
                InitDiscord();
            }

            m_StopTimer?.Dispose();
            m_StopTimer = null;

            double currentTimestamp = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
            currentTimestamp += track.Duration;
  
            m_Activity.Details = track.Artist + " - " + track.Title;
            m_Activity.State = track.Album;
            m_Activity.Timestamps.End = (long)currentTimestamp;
            m_Activity.Assets.LargeImage = track.ArtUrl;

            m_Discord.GetActivityManager().UpdateActivity(m_Activity, result => { });
        }

        public void Update() {
            try {
                m_Discord?.RunCallbacks();
            } catch (Exception ex) { }
        }

        public void Pause() {
            m_Activity.Details = "Idle";
            m_Activity.State = "";
            m_Activity.Timestamps.Start = 0;
            m_Activity.Timestamps.End = 0;
            m_Activity.Assets.LargeImage = "modern_zune_logo";
            m_Activity.Assets.SmallImage = "";
            m_Discord.GetActivityManager().UpdateActivity(m_Activity, result => { });

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(5);


            m_StopTimer = new System.Threading.Timer((obj) =>
            {
                Stop();
                m_StopTimer.Dispose();
                m_StopTimer = null;
            }, null, (int)TimeSpan.FromMinutes(1).TotalMilliseconds, System.Threading.Timeout.Infinite);

        }

        private void Stop() {
            m_Discord?.Dispose();
            m_Discord = null;
        }

        private void InitDiscord() {
            m_Discord = new Discord.Discord(904704369998561321, (UInt64)Discord.CreateFlags.Default);
        }

        private static Discord.Discord m_Discord;
        private static Discord.Activity m_Activity;
        private static Timer m_StopTimer;
    }
}
