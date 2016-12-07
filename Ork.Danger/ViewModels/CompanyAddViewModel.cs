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
using Ork.Danger.DomainModelService;

namespace Ork.Danger.ViewModels
{
  public class CompanyAddViewModel : Screen
  {
    private readonly Company m_Model;

    public CompanyAddViewModel(Company model)
    {
      DisplayName = "Unternehmen anlegen";
      m_Model = model;
    }

    public Company Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
      set
      {
        m_Model.Name = value;
        NotifyOfPropertyChange(() => Name);
        NotifyOfPropertyChange(() => EnableSaving);
      }
    }

    public string Adresse
    {
      get { return m_Model.Adress; }
      set
      {
        m_Model.Adress = value;
        NotifyOfPropertyChange(() => Adresse);
      }
    }

    public string Phone
    {
      get { return m_Model.Telephone; }
      set
      {
        m_Model.Telephone = value;
        NotifyOfPropertyChange(() => Phone);
      }
    }

    public string TypeOfBusiness
    {
      get { return m_Model.TypeOfBusiness; }
      set
      {
        m_Model.TypeOfBusiness = value;
        NotifyOfPropertyChange(() => TypeOfBusiness);
      }
    }

    public bool EnableSaving
    {
      get { return !string.IsNullOrEmpty(Name); }
    }
  }
}