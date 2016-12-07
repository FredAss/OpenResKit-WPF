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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Ork.CarbonFootprint.Converter
{
  public class EnumToIEnumerableConverter : IValueConverter
  {
    private readonly Dictionary<Type, List<object>> cache = new Dictionary<Type, List<object>>();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var type = value.GetType();
      if (!cache.ContainsKey(type))
      {
        var fields = type.GetFields()
                         .Where(field => field.IsLiteral);
        var values = new List<object>();
        foreach (var field in fields)
        {
          var a = (DescriptionAttribute[]) field.GetCustomAttributes(typeof (DescriptionAttribute), false);
          if (a != null &&
              a.Length > 0)
          {
            values.Add(a[0].Description);
          }
          else
          {
            values.Add(field.GetValue(value));
          }
        }
        cache[type] = values;
      }

      return cache[type];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}