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

using System.Collections.ObjectModel;
using System.Data.Services.Client;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;

namespace Ork.Danger.ViewModels
{
  public class GfactorViewModel : Screen
  {
    private readonly GFactor m_Model;
    private ObservableCollection<GfactorViewModel> m_GfactorViewModels;

    public GfactorViewModel(GFactor model)
    {
      m_Model = model;
    }

    public GfactorViewModel(ObservableCollection<GfactorViewModel> gfactorViewModels)
    {
      m_GfactorViewModels = gfactorViewModels;
    }

    public GFactor Model
    {
      get { return m_Model; }
    }

    public int Id
    {
      get { return m_Model.Id; }
    }

    public string Name
    {
      get { return m_Model.Name; }
    }

    public string Number
    {
      get { return m_Model.Number; }
    }

    public DataServiceCollection<Question> Questions
    {
      get { return m_Model.Questions; }
    }

    public bool IsSelected { get; set; }
  }
}