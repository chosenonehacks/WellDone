using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace WellDone.Converters
{
    public class DateTimeToDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value == null) return null;

                DateTime date = (DateTime)value;
                return new DateTimeOffset(date);
            }
            catch (Exception )
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                DateTimeOffset dto = (DateTimeOffset)value;
                return dto.DateTime;
            }
            catch (Exception )
            {
                return null;
            }
        }
    }
}
