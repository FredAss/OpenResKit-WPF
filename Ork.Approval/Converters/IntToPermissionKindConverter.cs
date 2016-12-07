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
using System.Windows.Data;
using Ork.Framework;

namespace Ork.Approval.Converters
{
    public class IntToPermissionKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((int) value)
            {
                case 0:
                    return TranslationProvider.Translate(((KindOfPermission) value).ToString()); 
                case 1:
                    return TranslationProvider.Translate(((KindOfPermission)value).ToString());
                case 2:
                    return TranslationProvider.Translate(((KindOfPermission)value).ToString());
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
