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
using Action = Ork.Danger.DomainModelService.Action;

namespace Ork.Danger.Converters
{
  internal class ActionsToStringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var list = value as ICollection<Action>;
      var text = string.Empty;
      var i = 1;
      foreach (var action in list)
      {
        if (i < list.Count)
        {
          text += action.Description + "\n";
        }
        else
        {
          text += action.Description;
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