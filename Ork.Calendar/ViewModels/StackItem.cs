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

using System.Windows;
using System.Windows.Media;

namespace Ork.Calendar.ViewModels
{
  public class StackItem
  {
    public StackItem(double top, double height, Brush color)
    {
      Margin = new Thickness
               {
                 Bottom = 0,
                 Left = 0,
                 Right = 0,
                 Top = top
               };
      Height = height;
      Color = color;
    }

    public Brush Color { get; private set; }
    public double Height { get; private set; }
    public Thickness Margin { get; private set; }
  }
}