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

namespace Ork.Map.Converters
{
  public class KeepSizeConstantConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      var itemSize = System.Convert.ToDouble(parameter);
      double ratio;
      try
      {
        var originalSize = System.Convert.ToDouble(values[0]);
        var actualSize = System.Convert.ToDouble(values[1]);

        ratio = actualSize / originalSize;
      }
      catch (Exception)
      {
        ratio = 1d;
      }

      return itemSize / ratio;
    }


    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}