﻿#region License

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
using Ork.Framework;

namespace Ork.Waste.ViewModels
{
  public class MapEditViewModel : MapAddViewModel
  {
    private readonly Action m_RemoveMap;

    public MapEditViewModel(DomainModelService.Map model, Action removeMapAction)
      : base(model)
    {
      m_RemoveMap = removeMapAction;
      DisplayName = TranslationProvider.Translate("TitleMapEditViewModel");
    }

    public void RemoveMap()
    {
      m_RemoveMap();
    }
  }
}