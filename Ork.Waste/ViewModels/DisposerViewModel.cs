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

using System.Collections.Generic;
using Caliburn.Micro;
using Ork.Waste.DomainModelService;

namespace Ork.Waste.ViewModels
{
  public class DisposerViewModel : PropertyChangedBase
  {
    private readonly Disposer m_Model;

    public DisposerViewModel(Disposer disposer)
    {
      m_Model = disposer;
    }

    public string Name
    {
      get { return m_Model.Name; }
    }

    public string Number
    {
      get { return m_Model.Number; }
    }

    public string Street
    {
      get { return m_Model.Street; }
    }

    public string ZipCode
    {
      get { return m_Model.ZipCode; }
    }

    public string City
    {
      get { return m_Model.City; }
    }

    public IEnumerable<WasteContainer> Containers
    {
      get { return m_Model.Containers; }
    }

    public string EMail
    {
      get { return m_Model.EMail; }
    }

    public string AdditionalInformation
    {
      get { return m_Model.AdditionalInformation; }
    }

    public IEnumerable<Document> Documents
    {
      get { return m_Model.Documents; }
    }

    public Disposer Model
    {
      get { return m_Model; }
    }

    public string TypeIndicator
    {
      get { return "Disposer"; }
    }
  }
}