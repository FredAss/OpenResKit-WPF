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

using Ork.Framework.ViewModels;

namespace Ork.Employee.UI.ViewModels
{
  public class SelectableEmployeeViewModel : SelectableItemViewModel
  {
    private readonly DomainModelService.Employee m_Model;

    public SelectableEmployeeViewModel(DomainModelService.Employee model, bool isSelected)
      : base(isSelected)
    {
      m_Model = model;
    }

    public DomainModelService.Employee Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.LastName; }
    }
  }
}