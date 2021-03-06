﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DoubanFM.Desktop.API.Services;
using DoubanFM.Desktop.Audio;
using Moq;
using Prism.Events;

namespace DoubanFM.Desktop.NowPlaying.Tests
{
    [TestClass]
    public class NowPlayingViewModelFixture
    {
        [TestMethod]
        public void CanInitViewModel()
        {
            var eventAggregator = new Mock<IEventAggregator>();
            var songService = new Mock<ISongService>();
            var audioEngine = new Mock<IAudioEngine>();
            var lyricService = new Mock<ILyricsService>();

            var viewModel = new ViewModels.NowPlayingViewModel(
                eventAggregator.Object,
                audioEngine.Object,
                songService.Object,
                lyricService.Object);        



        }
    }
}
