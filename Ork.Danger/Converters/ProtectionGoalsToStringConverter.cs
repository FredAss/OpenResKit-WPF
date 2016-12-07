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
using System.Globalization;
using System.Windows.Data;
using Ork.Danger.DomainModelService;

namespace Ork.Danger.Converters
{
  internal class ProtectionGoalsToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var list = value as ICollection<ProtectionGoal>;
      var text = string.Empty;
      var i = 1;
      foreach (var protectionGoal in list)
      {
        if (i < list.Count)
        {
          text += protectionGoal.Description + "\n";
        }
        else
        {
          text += protectionGoal.Description;
        }
        i++;
      }
      return text;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}