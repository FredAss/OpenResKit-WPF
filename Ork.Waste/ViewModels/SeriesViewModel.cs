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
using Ork.Waste.DomainModelService;

namespace Ork.Waste.ViewModels
{
  public class SeriesViewModel : PropertyChangedBase
  {
    private readonly Series m_Model;

    public SeriesViewModel(Series model)
    {
      m_Model = model;
    }

    public Series Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
    }

    public string TypeIndicator
    {
      get { return "Series"; }
    }
  }
}