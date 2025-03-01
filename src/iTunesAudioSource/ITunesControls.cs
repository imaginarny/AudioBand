﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using iTunesLib;
using Timer = System.Timers.Timer;

namespace iTunesAudioSource
{
    public class ITunesControls
    {
        private iTunesApp _itunesApp;
        private Timer _checkProcessTimer;
        private bool _itunesOpened;

        public ITunesControls()
        {
            _checkProcessTimer = new Timer(50)
            {
                AutoReset = false,
                Enabled = false
            };
            _checkProcessTimer.Elapsed += CheckProcess;
        }

        public bool IsPlaying => GetIsPlaying();

        public Track CurrentTrack => GetTrack();

        public TimeSpan Progress
        {
            get
            {
                return TimeSpan.FromMilliseconds(_itunesApp.PlayerPositionMS);
            }

            set
            {
                _itunesApp.PlayerPositionMS = (int)value.TotalMilliseconds;
            }
        }

        public bool Shuffle
        {
            get
            {
                return _itunesApp.CurrentPlaylist.Shuffle;
            }

            set
            {
                _itunesApp.CurrentPlaylist.Shuffle = value;
            }
        }

        public ITPlaylistRepeatMode RepeatMode
        {
            get
            {
                return _itunesApp.CurrentPlaylist.SongRepeat;
            }

            set
            {
                _itunesApp.CurrentPlaylist.SongRepeat = value;
            }
        }

        /// <summary>
        /// Gets or sets the volume from [0, 100]
        /// </summary>
        public int Volume
        {
            get
            {
                return _itunesApp.SoundVolume;
            }

            set
            {
                _itunesApp.SoundVolume = value;
            }
        }

        public void Start()
        {
            _itunesApp = new iTunesApp();
            _checkProcessTimer.Start();
        }

        public void Stop()
        {
            _checkProcessTimer.Stop();
        }

        public void Play()
        {
            try
            {
                _itunesApp.Play();
            }
            catch (COMException) { }
        }

        public void Pause()
        {
            try
            {
                _itunesApp.Pause();
            }
            catch (COMException) { }
        }

        public void Next()
        {
            try
            {
                _itunesApp.NextTrack();
            }
            catch (COMException) { }
        }

        public void Previous()
        {
            try
            {
                _itunesApp.PreviousTrack();
            }
            catch (COMException) { }
        }

        public bool GetLike()
        {
            var track = _itunesApp.CurrentTrack as IITFileOrCDTrack;

            return track.ratingKind == ITRatingKind.ITRatingKindUser ? true : false;

            // this is how it should be done, but it's readonly
            // track.ratingKind = 1;

            // below is code to give it 5 stars or none, which isn't really equivalent to the like button
            if (_itunesApp.CurrentTrack.Rating == 100)
            {
                _itunesApp.CurrentTrack.Rating = 0;
            }
            else
            {
                _itunesApp.CurrentTrack.Rating = 100;
            }
        }

        private Track GetTrack()
        {
            if (!ITunesIsRunning())
            {
                return null;
            }

            try
            {
                var track = _itunesApp.CurrentTrack as IITTrack;
                if (track == null)
                {
                    return null;
                }

                return new Track
                {
                    Album = track.Album,
                    Artist = track.Artist,
                    Length = TimeSpan.FromSeconds(track.Duration),
                    Name = track.Name,
                    Artwork = GetArtwork(track.Artwork)
                };
            }
            catch (COMException)
            {
                return null;
            }
        }

        private Image GetArtwork(IITArtworkCollection collection)
        {
            if (collection.Count == 0)
            {
                return null;
            }

            // 1-based index
            var artwork = collection[1];
            var tempPath = Path.GetTempFileName();

            artwork.SaveArtworkToFile(tempPath);

            Image image;
            using (var tmp = new Bitmap(tempPath))
            {
                image = new Bitmap(tmp);
            }

            File.Delete(tempPath);
            return image;
        }

        private bool ITunesIsRunning()
        {
            return Process.GetProcessesByName("iTunes").Length > 0;
        }

        private bool GetIsPlaying()
        {
            try
            {
                return _itunesApp.PlayerState.HasFlag(ITPlayerState.ITPlayerStatePlaying);
            }
            catch (COMException)
            {
                return false;
            }
        }

        private void CheckProcess(object sender, ElapsedEventArgs e)
        {
            var opened = ITunesIsRunning();
            if (!_itunesOpened && opened)
            {
                _itunesApp = new iTunesApp();
            }

            _itunesOpened = opened;

            _checkProcessTimer.Enabled = true;
        }
    }
}
