using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace fwv.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        /// <summary>
        /// enum -> bool
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                return DependencyProperty.UnsetValue;
            }
            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }

            // a param that the radio button has.
            string param = parameter as string;
            object radioValue = Enum.Parse(value.GetType(), param);

            // return true if the radio value equals selected value.
            return (int)value == (int)radioValue;
        }

        /// <summary>
        /// bool -> enum
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                return DependencyProperty.UnsetValue;
            }

            string param = parameter as string;
            return Enum.Parse(targetType, param);
        }
    }
}
