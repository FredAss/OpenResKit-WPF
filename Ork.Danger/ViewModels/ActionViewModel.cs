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

using System;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;
using Action = Ork.Danger.DomainModelService.Action;

namespace Ork.Danger.ViewModels
{
  public class ActionViewModel : PropertyChangedBase
  {
    private readonly Action m_Model;

    public ActionViewModel(Action model)
    {
      m_Model = model;
    }

    public Action Model
    {
      get { return m_Model; }
    }

    public int Id
    {
      get { return m_Model.Id; }
    }

    public string Description
    {
      get { return m_Model.Description; }
    }

    public Person Person
    {
      get { return m_Model.Person; }
    }

    public DateTime DueDate
    {
      get { return m_Model.DueDate; }
    }

    public string DueDateShort
    {
      get { return m_Model.DueDate.ToShortDateString(); }
    }

    public string Execution
    {
      get { return m_Model.Execution; }
    }

    public bool Effect
    {
      get { return m_Model.Effect; }
      set { m_Model.Effect = value; }
    }
  }
}