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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Services.Client;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Ork.Discard.DomainModelService;
using Ork.Framework;

namespace Ork.Discard.ViewModels
{
  public class InspectionAddViewModel : DocumentBase
  {
    private readonly IDiscardRepository m_Repository;
    private CustomerViewModel m_Customer;
    private IEnumerable m_InspectionShift;
    private IEnumerable m_InspectionType;
    private Inspection m_Model;
    private ObservableCollection<ProductionItem> m_ProductionItems = new ObservableCollection<ProductionItem>();
    private IEnumerable m_ProductionShift;
    private string m_ResponsibleSubjectSearchText = string.Empty;
    private IEnumerable<ResponsibleSubjectViewModel> m_ResponsibleSubjects;
    private ResponsibleSubjectViewModel m_SelectedResponsibleSubject;


    [ImportingConstructor]
    public InspectionAddViewModel(Inspection model, CustomerViewModel customer, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjectViewModels, IDiscardRepository repository)
    {
      m_Model = model;
      m_Repository = repository;
      DisplayName = TranslationProvider.Translate("TitleAddInspection");

      m_Customer = customer;
      m_ProductionShift = Enum.GetValues(typeof (Shift));
      m_InspectionShift = Enum.GetValues(typeof (Shift));
      m_InspectionType = Enum.GetValues(typeof (InspectionType));
      m_ResponsibleSubjects = responsibleSubjectViewModels;
      m_ProductionItems = repository.ProductionItems;
      if (m_Model.ResponsibleSubject != null)
      {
        m_SelectedResponsibleSubject = m_ResponsibleSubjects.First(rs => rs.Model == m_Model.ResponsibleSubject);
      }


      if (m_Model.InspectionDate == new DateTime())
      {
        m_Model.InspectionDate = DateTime.Now;
      }
      if (m_Model.ProductionDate == new DateTime())
      {
        m_Model.ProductionDate = DateTime.Now;
      }


      if (m_Model.ProductionItem == null)
      {
        ProductionItem = FilteredProductionItems.First();
      }
      else
      {
        ProductionItem = m_ProductionItems.First(pi => pi == m_Model.ProductionItem);
      }


      NotifyOfPropertyChange(() => FilteredProductionItems);
      NotifyOfPropertyChange(() => ProductionItem);
    }

    public IEnumerable<ProductionItem> FilteredProductionItems
    {
      get
      {
        if (m_Customer != null)
        {
          return m_ProductionItems.Where(pivm => pivm.Customer == m_Customer.Model);
        }
        else
        {
          return m_ProductionItems.Where(pivm => pivm.Customer == m_Model.ProductionItem.Customer);
        }
      }
    }

    public Inspection Model
    {
      get { return m_Model; }
    }

    public string Name
    {
      get { return m_Model.Name; }
      set
      {
        m_Model.Name = value;
        NotifyOfPropertyChange(() => CanInspectionAdd);
        NotifyOfPropertyChange(() => Name);
      }
    }

    public string Description
    {
      get { return m_Model.Description; }
      set
      {
        m_Model.Description = value;
        NotifyOfPropertyChange(() => Description);
      }
    }

    public int SampleSize
    {
      get { return m_Model.SampleSize; }
      set
      {
        m_Model.SampleSize = value;
        NotifyOfPropertyChange(() => SampleSize);
      }
    }

    public DateTime InspectionDate
    {
      get { return m_Model.InspectionDate; }
      set
      {
        m_Model.InspectionDate = value;
        NotifyOfPropertyChange(() => InspectionDate);
      }
    }

    public string InspectionDateShort
    {
      get { return m_Model.InspectionDate.ToShortDateString(); }
    }

    public string Unit
    {
      get { return m_Model.Unit; }
      set
      {
        m_Model.Unit = value;
        NotifyOfPropertyChange(() => Unit);
      }
    }

    public int InspectionType
    {
      get { return m_Model.InspectionType; }
      set
      {
        m_Model.InspectionType = value;
        NotifyOfPropertyChange(() => InspectionType);
      }
    }

    public IEnumerable InspectionTypes
    {
      get { return m_InspectionType; }
    }

    public int InspectionShift
    {
      get { return m_Model.InspectionShift; }
      set
      {
        m_Model.InspectionShift = value;
        NotifyOfPropertyChange(() => InspectionShift);
      }
    }

    public IEnumerable InspectionShifts
    {
      get { return m_InspectionShift; }
    }


    public int ProductionShift
    {
      get { return m_Model.ProductionShift; }
      set
      {
        m_Model.ProductionShift = value;
        NotifyOfPropertyChange(() => ProductionShift);
      }
    }

    public IEnumerable ProductionShifts
    {
      get { return m_ProductionShift; }
    }

    public DateTime ProductionDate
    {
      get { return m_Model.ProductionDate; }
      set
      {
        m_Model.ProductionDate = value;
        NotifyOfPropertyChange(() => ProductionDate);
      }
    }

    public IEnumerable<ResponsibleSubjectViewModel> ResponsibleSubjects
    {
      get { return SearchInResponsibleObjectList(); }
    }

    public string ResponsibleSubjectSearchText
    {
      get { return m_ResponsibleSubjectSearchText; }
      set
      {
        m_ResponsibleSubjectSearchText = value;
        NotifyOfPropertyChange(() => ResponsibleSubjects);
      }
    }

    public ResponsibleSubjectViewModel ResponsibleSubject
    {
      get { return m_SelectedResponsibleSubject; }
      set
      {
        if (value != null)
        {
          m_SelectedResponsibleSubject = value;
          m_Model.ResponsibleSubject = value.Model;
        }
        NotifyOfPropertyChange(() => ResponsibleSubject);
      }
    }


    public ProductionItem ProductionItem
    {
      get { return m_ProductionItems.First(pi => pi == m_Model.ProductionItem); }
      set
      {
        m_Model.ProductionItem = value;
        NotifyOfPropertyChange(() => ProductionItem);
      }
    }


    public DataServiceCollection<DiscardItem> DiscardItems
    {
      get { return m_Model.DiscardItems; }
      set
      {
        m_Model.DiscardItems = value;
        NotifyOfPropertyChange(() => DiscardItems);
      }
    }

    public int TotalAmount
    {
      get { return m_Model.TotalAmount; }
      set
      {
        m_Model.TotalAmount = value;
        NotifyOfPropertyChange(() => TotalAmount);
      }
    }

    public Customer Customer
    {
      get { return m_Model.ProductionItem.Customer; }
    }


    public bool CanInspectionAdd
    {
      get { return !(m_Model.Name == null | m_Model.Name == ""); }
    }

    private IEnumerable<ResponsibleSubjectViewModel> SearchInResponsibleObjectList()
    {
      if (string.IsNullOrEmpty(ResponsibleSubjectSearchText))
      {
        return m_ResponsibleSubjects;
      }
      var searchText = ResponsibleSubjectSearchText.ToLower();

      var searchResultResponsibleSubjects = m_ResponsibleSubjects.Where(e => (e.Infotext != null) && (e.Infotext.ToLower()
                                                                                                       .Contains(searchText.ToLower())));

      return searchResultResponsibleSubjects;
    }

    public void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
      var regex = new Regex("[0-9]");
      e.Handled = !regex.IsMatch(e.Text);
    }
  }
}