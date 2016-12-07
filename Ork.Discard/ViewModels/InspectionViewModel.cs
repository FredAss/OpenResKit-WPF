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
using System.ComponentModel;
using System.Data.Services.Client;
using System.Linq;
using Caliburn.Micro;
using Ork.Discard.DomainModelService;
using Ork.Framework;

namespace Ork.Discard.ViewModels
{
  public class InspectionViewModel : PropertyChangedBase
  {
    private readonly Inspection m_Model;

    public InspectionViewModel(Inspection inspection)
    {
      m_Model = inspection;
      m_Model.PropertyChanged += ModelPropertyChanged;
    }

    public Inspection Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
    }

    public string Description
    {
      get { return m_Model.Description; }
    }

    public int SampleSize
    {
      get { return m_Model.SampleSize; }
    }

    public string InspectionDate
    {
      get { return m_Model.InspectionDate.ToShortDateString(); }
    }

    public string Unit
    {
      get { return m_Model.Unit; }
    }

    public string InspectionType
    {
      get { return TranslationProvider.Translate(((InspectionType) m_Model.InspectionType).ToString()); }
    }

    public string InspectionShift
    {
      get { return TranslationProvider.Translate(((Shift) m_Model.InspectionShift).ToString()); }
    }

    public string ProductionShift
    {
      get { return TranslationProvider.Translate(((Shift) m_Model.ProductionShift).ToString()); }
    }

    public DateTime ProductionDate
    {
      get { return m_Model.ProductionDate; }
    }

    public Customer Customer
    {
      get { return m_Model.ProductionItem.Customer; }
    }

    public string ResponsibleSubject
    {
      get
      {
        if (m_Model.ResponsibleSubject is Employee)
        {
          var employee = (Employee) m_Model.ResponsibleSubject;
          return employee.FirstName + " " + employee.LastName;
        }
        if (m_Model.ResponsibleSubject is EmployeeGroup)
        {
          var employeeGroup = (EmployeeGroup) m_Model.ResponsibleSubject;
          return employeeGroup.Name;
        }
        return TranslationProvider.Translate("NotAvailable");
      }
    }

    public ProductionItem ProductionItem
    {
      get { return m_Model.ProductionItem; }
    }

    public DataServiceCollection<DiscardItem> DiscardItems
    {
      get { return m_Model.DiscardItems; }
    }

    public int TotalDiscardItems
    {
      get { return m_Model.DiscardItems.Sum(di => di.Quantity); }
    }

    public int TotalAmount
    {
      get { return m_Model.TotalAmount; }
    }

    public string Finished
    {
      get
      {
        if (m_Model.Finished)
        {
          return TranslationProvider.Translate("Finished");
        }
        return TranslationProvider.Translate("NotFinished");
      }
    }

    public string SampleSizeText
    {
      get { return string.Format("{0} {1}", m_Model.SampleSize, m_Model.Unit); }
    }

    public string TotalAmountText
    {
      get { return string.Format("{0} {1}", m_Model.TotalAmount, m_Model.Unit); }
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Unit" ||
          e.PropertyName == "SampleSize")
      {
        NotifyOfPropertyChange(() => SampleSizeText);
      }
      if (e.PropertyName == "Unit" ||
          e.PropertyName == "TotalAmount")
      {
        NotifyOfPropertyChange(() => TotalAmountText);
      }
      NotifyOfPropertyChange(e.PropertyName);
    }
  }
}