#region License

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//  
// http://www.apache.org/licenses/LICENSE-2.0.html
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  
// Copyright (c) 2013, HTW Berlin

#endregion

using System;
using System.Globalization;
using System.Windows.Data;

namespace Ork.Calendar.Converters
{
  /// <summary>
  ///   Takes a date (value) and an hour offset (parameter) and returns a date at that day:hour.
  /// </summary>
  /// <example>
  ///   Value:			2009-12-2 09:30:45
  ///   Parameter:	14
  ///   Result:			2009-12-2 14:00:00
  /// </example>
  public class HourOfDay : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is DateTime))
      {
        return null;
      }
      var date = (DateTime) value;

      if (string.IsNullOrEmpty((string) parameter))
      {
        return null;
      }

      int hour;

      if (!int.TryParse((string) parameter, out hour))
      {
        return null;
      }

      return new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}