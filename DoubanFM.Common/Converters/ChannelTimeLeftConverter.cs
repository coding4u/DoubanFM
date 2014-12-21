﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DoubanFM.Common.Converters
{
    public class ChannelTimeLeftConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var channelLength = (double)values[0];
            var channelPosition = (double)values[1];
            var timeLeft = channelPosition - channelLength;

            var ts = TimeSpan.FromSeconds(timeLeft);
            return string.Format("-{0:m\\:ss}", ts);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
