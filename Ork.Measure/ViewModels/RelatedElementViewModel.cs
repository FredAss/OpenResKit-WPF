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

using Ork.Framework.ViewModels;

namespace Ork.Measure.ViewModels
{
  public class RelatedElementViewModel : SelectableItemViewModel
  {
    public RelatedElementViewModel(object model, string displayText, bool isSelected = false)
      : base(isSelected)
    {
      Model = model;
      DisplayText = displayText;
    }

    public object Model { get; private set; }
    public string DisplayText { get; private set; }
  }
}