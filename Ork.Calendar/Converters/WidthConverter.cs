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
using Ork.Calendar.ViewModels;

namespace Ork.Calendar.Converters
{
  public class WidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      var dayEntryViewModel = values[0];
      var actualWidth = values[1];

      if (dayEntryViewModel is DayEntryViewModel &&
          actualWidth is double)
      {
        var devm = (DayEntryViewModel) dayEntryViewModel;

        return (double) actualWidth / devm.Intersection; // [* (devm.Intersection - devm.StackLevel + 1)];
      }

      return actualWidth;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}