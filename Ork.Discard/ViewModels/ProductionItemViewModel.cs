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
using System.ComponentModel;
using Caliburn.Micro;
using Ork.Discard.DomainModelService;
using Ork.Framework;

namespace Ork.Discard.ViewModels
{
  public class ProductionItemViewModel : PropertyChangedBase
  {
    private readonly ProductionItem m_Model;

    public ProductionItemViewModel(ProductionItem model)
    {
      m_Model = model;
      m_Model.PropertyChanged += ModelPropertyChanged;
    }

    public ProductionItem Model
    {
      get { return m_Model; }
    }

    public int Id
    {
      get { return m_Model.Id; }
    }

    public ObservableCollection<InspectionAttribute> InspectionAttributes
    {
      get { return m_Model.InspectionAttributes; }
    }

    public string ItemName
    {
      get { return m_Model.ItemName; }
    }

    public string ItemNumber
    {
      get { return m_Model.ItemNumber; }
    }

    public string ItemCategory
    {
      get { return TranslationProvider.Translate(((ItemCategory) m_Model.ItemCategory).ToString()); }
    }

    public string ItemDescription
    {
      get { return m_Model.ItemDescription; }
    }

    public string Material
    {
      get { return m_Model.Material; }
    }

    public string ToolNumber
    {
      get { return m_Model.ToolNumber; }
    }

    public string ChangeIndex
    {
      get { return m_Model.ChangeIndex; }
    }

    public string ReferenceNumber1
    {
      get { return m_Model.ReferenceNumber1; }
    }

    public string ReferenceNumber2
    {
      get { return m_Model.ReferenceNumber2; }
    }

    public Customer Customer
    {
      get { return m_Model.Customer; }
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(e.PropertyName);
    }
  }
}