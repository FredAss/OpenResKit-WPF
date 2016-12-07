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

using Caliburn.Micro;
using Ork.Framework;

namespace Ork.Meter.ViewModels
{
  public class MapAddViewModel : Screen
  {
    private readonly DomainModelService.Map m_Model;

    public MapAddViewModel(DomainModelService.Map model)
    {
      m_Model = model;
      DisplayName = TranslationProvider.Translate("AddMap");
    }

    public DomainModelService.Map Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
      set { m_Model.Name = value; }
    }
  }
}