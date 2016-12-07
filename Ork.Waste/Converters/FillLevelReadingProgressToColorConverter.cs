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
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ork.Waste.Converters
{
  internal class FillLevelReadingFillLevelToColorConverter : IValueConverter
  {
    #region IValueConverter Member

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double fillLevelReadingProgress = 0;

      if (Double.TryParse(value.ToString(), out fillLevelReadingProgress))
      {
        if (fillLevelReadingProgress == 0)
        {
          return (SolidColorBrush) (new BrushConverter().ConvertFrom("#269926"));
        }
        else if (fillLevelReadingProgress == 25)
        {
          return (SolidColorBrush) (new BrushConverter().ConvertFrom("#67E300"));
        }
        else if (fillLevelReadingProgress == 50)
        {
          return (SolidColorBrush) (new BrushConverter().ConvertFrom("#FFBF00"));
        }
        else if (fillLevelReadingProgress == 75)
        {
          return (SolidColorBrush) (new BrushConverter().ConvertFrom("#FF6200"));
        }
        else if (fillLevelReadingProgress == 100)
        {
          return (SolidColorBrush) (new BrushConverter().ConvertFrom("#A30008"));
        }
      }
      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}