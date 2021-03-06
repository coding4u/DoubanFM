﻿using NAudio.Wave;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFSoundVisualizationLib;


namespace DoubanFM.Desktop.Audio
{
    public class NAudioEngine 
    {
        #region Fields
        private static NAudioEngine instance;
        private readonly DispatcherTimer positionTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private readonly BackgroundWorker waveformGenerateWorker = new BackgroundWorker();
        private readonly int fftDataSize = (int)FFTDataSize.FFT2048;
        private bool disposed;
        private bool canPlay;
        private bool canPause;
        private bool canStop;
        private bool isPlaying;
        private bool isLiked;
        private bool inChannelTimerUpdate;
        private double channelLength;
        private double channelPosition;
        private bool inChannelSet;
        private WaveOut waveOutDevice;
        private WaveStream activeStream;
        private WaveChannel32 inputStream;
        private SampleAggregator sampleAggregator;
        //private SampleAggregator waveformAggregator;
        //private string pendingWaveformPath;
        //private float[] fullLevelData;
        private float[] waveformData;
        private TagLib.File fileTag;
        private TimeSpan repeatStart;
        private TimeSpan repeatStop;
        private bool inRepeatSet;
        private BitmapImage albumImage;
        #endregion

        #region events
        public event EventHandler TrackEnded;
        #endregion

        #region Constants
        private const int waveformCompressedPointCount = 2000;
        private const int repeatThreshold = 200;
        #endregion

        #region Singleton Pattern
        public static NAudioEngine Instance
        {
            get
            {
                if (instance == null)
                    instance = new NAudioEngine();
                return instance;
            }
        }
        #endregion

        #region Constructor
        private NAudioEngine()
        {

            positionTimer.Interval = TimeSpan.FromMilliseconds(50);
            positionTimer.Tick += positionTimer_Tick;

            waveOutDevice.PlaybackStopped += waveOutDevice_PlaybackStopped;

            this.PlayCommand = new DelegateCommand(() =>
                {
                    if (IsPlaying)
                        Pause();
                    else
                        Play();
                });

            this.StopCommand = new DelegateCommand(() => Stop(), () => CanStop);
        }

        #endregion

        #region Notification Properties


        public bool CanPlay
        {
            get
            {
                return canPlay;
            }
            set
            {
                if (value != canPlay)
                {
                    canPlay = value;
                    NotifyPropertyChanged("CanPlay");
                }
            }
        }


        public bool CanPause
        {
            get
            {
                return canPause;
            }
            set
            {
                if (value != canPause)
                {
                    canPause = value;
                    NotifyPropertyChanged("CanPause");
                }
            }
        }



        public bool CanStop
        {
            get
            {
                return canStop;
            }
            set
            {
                if (value != canStop)
                {
                    canStop = value;
                    NotifyPropertyChanged("CanStop");
                }
            }
        }



        //public bool IsPlaying
        //{
        //    get
        //    {
        //        return isPlaying;
        //    }
        //    set
        //    {
        //        if (value != isPlaying)
        //        {
        //            isPlaying = value;
        //            NotifyPropertyChanged("IsPlaying");
        //        }
        //        positionTimer.IsEnabled = value;
        //    }
        //}

        public bool IsPlaying
        {
            get { return isPlaying; }
            protected set
            {
                bool oldValue = isPlaying;
                isPlaying = value;
                if (oldValue != isPlaying)
                    NotifyPropertyChanged("IsPlaying");
                positionTimer.IsEnabled = value;
            }
        }

        public bool IsLiked
        {
            get { return isLiked; }
            set
            {
                if(value!=isLiked)
                {
                    isLiked = value;
                    NotifyPropertyChanged("IsLiked");
                }
            }
        }



        public WaveStream ActiveStream
        {
            get
            {
                return activeStream;
            }
            set
            {
                if (value != activeStream)
                {
                    activeStream = value;
                    NotifyPropertyChanged("ActiveStream");
                }
            }
        }

        public BitmapImage AlbumImage
        {
            get
            {
                return albumImage;
            }
            set
            {
                if (value != albumImage)
                {
                    albumImage = value;
                    NotifyPropertyChanged("AlbumImage");
                }
            }
        }

        #endregion

        #region Commands
        public ICommand PlayCommand { get; set; }

		public ICommand PauseCommand { get; set; }

		public ICommand PlayNextCommand { get; set; }

        public ICommand StopCommand { get; set; }
        #endregion

        #region Public Methods

        public void OpenFile(string path)
        {

            Stop();

            if (ActiveStream != null)
            {
                SelectionBegin = TimeSpan.Zero;
                SelectionEnd = TimeSpan.Zero;
                ChannelPosition = 0;
            }

            StopAndCloseStream();

            if (File.Exists(path))
            {
                try
                {
                    waveOutDevice = new WaveOut()
                    {
                        DesiredLatency = 100
                    };
                    ActiveStream = new Mp3FileReader(path);
                    inputStream = new WaveChannel32(ActiveStream);
                    sampleAggregator = new SampleAggregator(fftDataSize);
                    inputStream.Sample += inputStream_Sample;
                    waveOutDevice.Init(inputStream);
                    ChannelLength = inputStream.TotalTime.TotalSeconds;
                    fileTag = TagLib.File.Create(path);
                    GetAlbumImage();
                    CanPlay = true;
                }
                catch (Exception ex)
                {
                    ActiveStream = null;
                    CanPlay = false;

                    Debug.WriteLine("Open file:{0} failed !", path);
                    Debug.WriteLine("Exception:{0}", ex.Message);
                }
            }
            else
            {
                throw (new FileNotFoundException());
            }
        }

        public void OpenUrl(string url)
        {

        }


        public void Play()
        {

            if (CanPlay)
            {
                waveOutDevice.Play();
                IsPlaying = true;
                CanPause = true;
                CanPlay = false;
                CanStop = true;
            }
        }


        public void Pause()
        {

            if (IsPlaying && CanPause)
            {
                waveOutDevice.Pause();
                IsPlaying = false;
                CanPause = false;
                CanPlay = true;
            }

        }


        public void Stop()
        {

            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }

            IsPlaying = false;
            CanStop = false;
            CanPlay = true;
            CanPause = false;
        }

        #endregion

        #region Private Methods
        private void waveOutDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            OnTrackEnded();
        }

        private void StopAndCloseStream()
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }
            if (activeStream != null)
            {
                inputStream.Close();
                inputStream = null;
                ActiveStream.Close();
                ActiveStream = null;
            }
            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }

        private void GetAlbumImage()
        {
            if (fileTag != null)
            {
                var tag = fileTag.Tag;
                if (tag.Pictures.Length > 0)
                {
                    using (var ms = new MemoryStream(tag.Pictures[0].Data.Data))
                    {
                        try
                        {
                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.StreamSource = ms;
                            bmp.EndInit();
                            AlbumImage = bmp;
                        }
                        catch (NotSupportedException)
                        {
                            AlbumImage = null;
                        }
                        ms.Close();
                    }
                }
                else
                {
                    AlbumImage = null;
                }
            }
            else
            {
                albumImage = null;
            }
        }

        #endregion

        #region Event Handlers
        private void OnTrackEnded()
        {
            if (TrackEnded != null)
                TrackEnded(this, null);
        }

        private void inputStream_Sample(object sender, SampleEventArgs e)
        {
            sampleAggregator.Add(e.Left, e.Right);
            long repeatStartPosition = (long)((SelectionBegin.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
            long repeatStopPosition = (long)((SelectionEnd.TotalSeconds / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
            if (((SelectionEnd - SelectionBegin) >= TimeSpan.FromMilliseconds(repeatThreshold)) && ActiveStream.Position >= repeatStopPosition)
            {
                sampleAggregator.Clear();
                ActiveStream.Position = repeatStartPosition;
            }
        }

        void positionTimer_Tick(object sender, EventArgs e)
        {
            inChannelTimerUpdate = true;
            ChannelPosition = ((double)ActiveStream.Position / (double)ActiveStream.Length) * ActiveStream.TotalTime.TotalSeconds;
            inChannelTimerUpdate = false;
        }
        #endregion

        #region ISpectrumPlayer
        public bool GetFFTData(float[] fftDataBuffer)
        {
            sampleAggregator.GetFFTResults(fftDataBuffer);
            return isPlaying;
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            double maxFrequency;
            if (ActiveStream != null)
                maxFrequency = ActiveStream.WaveFormat.SampleRate / 2.0d;
            else
                maxFrequency = 22050; // Assume a default 44.1 kHz sample rate.
            return (int)((frequency / maxFrequency) * (fftDataSize / 2));
        }
        #endregion

        #region IWaveformPlayer
        public TimeSpan SelectionBegin
        {
            get { return repeatStart; }
            set
            {
                if (!inRepeatSet)
                {
                    inRepeatSet = true;
                    if (value != repeatStart)
                    {
                        repeatStart = value;
                        NotifyPropertyChanged("SelectionBegin");
                    }
                    inRepeatSet = false;
                }
            }
        }

        public TimeSpan SelectionEnd
        {
            get { return repeatStop; }
            set
            {
                if (!inChannelSet)
                {
                    inRepeatSet = true;
                    if (value != repeatStop)
                    {
                        repeatStop = value;
                        NotifyPropertyChanged("SelectionEnd");
                    }
                    inRepeatSet = false;
                }
            }
        }

        public float[] WaveformData
        {
            get { return waveformData; }
            protected set
            {
                if (value != waveformData)
                {
                    waveformData = value;
                    NotifyPropertyChanged("WaveformData");
                }
            }
        }

        public double ChannelLength
        {
            get { return channelLength; }
            protected set
            {
                if (value != channelLength)
                {
                    channelLength = value;
                    NotifyPropertyChanged("ChannelLength");
                }
            }
        }

        public double ChannelPosition
        {
            get { return channelPosition; }
            set
            {
                if (!inChannelSet)
                {
                    inChannelSet = true; // Avoid recursion
                    double position = Math.Max(0, Math.Min(value, ChannelLength));
                    if (!inChannelTimerUpdate && ActiveStream != null)
                        ActiveStream.Position = (long)((position / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);
                    if (position != channelPosition)
                    {
                        channelPosition = position;
                        NotifyPropertyChanged("ChannelPosition");
                    }
                    inChannelSet = false;
                }
            }
        }
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    StopAndCloseStream();
                }
                disposed = true;
            }
        }

        #endregion



        public bool IsMuted
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double Volume
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }

}

