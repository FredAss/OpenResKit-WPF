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

using System.ComponentModel;
using System.Data.Services.Client;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;

namespace Ork.Danger.ViewModels
{
  public class CompanyViewModel : Screen
  {
    private readonly Company m_Model;

    public CompanyViewModel(Company model)
    {
      m_Model = model;
      m_Model.PropertyChanged += ModelPropertyChanged;
    }

    public Company Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
    }

    public string Adresse
    {
      get { return m_Model.Adress; }
    }

    public string Phone
    {
      get { return m_Model.Telephone; }
    }

    public string TypeOfBusiness
    {
      get { return m_Model.TypeOfBusiness; }
    }

    public DataServiceCollection<Workplace> Workplaces
    {
      get { return m_Model.Workplaces; }
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(e.PropertyName);
    }
  }
}