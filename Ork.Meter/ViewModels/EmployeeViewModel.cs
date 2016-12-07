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
using Ork.Meter.DomainModelService;

namespace Ork.Meter.ViewModels
{
  public class EmployeeViewModel : ResponsibleSubjectViewModel
  {
    private readonly Employee m_Model;

    public EmployeeViewModel(Employee model)
      : base(model)
    {
      m_Model = model;
    }

    public string FirstName
    {
      get { return m_Model.FirstName; }
    }

    public string FullName
    {
      get
      {
        if (m_Model.FirstName.IsNotEmptyOrWhiteSpace() &&
            m_Model.LastName.IsNotEmptyOrWhiteSpace())
        {
          return m_Model.LastName + ", " + m_Model.FirstName;
        }
        else if (m_Model.FirstName.IsNotEmptyOrWhiteSpace())
        {
          return m_Model.FirstName;
        }
        else
        {
          return m_Model.LastName;
        }
      }
    }

    public ObservableCollection<EmployeeGroup> Groups
    {
      get { return m_Model.Groups; }
    }

    public string LastName
    {
      get { return m_Model.LastName; }
    }

    public string Number
    {
      get { return m_Model.Number; }
    }
  }
}