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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Discard.DomainModelService;
using Ork.Discard.Factories;
using Ork.Framework;

namespace Ork.Discard.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class InspectionManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly BindableCollection<CustomerViewModel> m_CustomerViewModels = new BindableCollection<CustomerViewModel>();
    private readonly IDiscardViewModelFactory m_DiscardViewModelFactory;
    private readonly BindableCollection<InspectionViewModel> m_InspectionViewModels = new BindableCollection<InspectionViewModel>();
    private readonly IDiscardRepository m_Repository;
    private IScreen m_EditItem;
    private bool m_FlyoutActivated = true;
    private bool m_IsEnabled;
    private String m_SearchText;
    private CustomerViewModel m_SelectedCustomer;
    private InspectionViewModel m_SelectedInspectionViewModel;

    [ImportingConstructor]
    public InspectionManagementViewModel([Import] IDiscardRepository contextRepository, [Import] IDiscardViewModelFactory discardViewModelFactory)
    {
      CustomerColumnDataGridVisibility = Visibility.Visible;
      NoCustomerColumnDataGridVisibility = Visibility.Hidden;
      m_Repository = contextRepository;
      m_DiscardViewModelFactory = discardViewModelFactory;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
      Reload();
    }

    public CustomerViewModel SelectedCustomer
    {
      get { return m_SelectedCustomer; }
      set
      {
        if (m_SelectedCustomer == value)
        {
          return;
        }
        m_SelectedCustomer = value;
        ChangeDataGrid();
        NotifyOfPropertyChange(() => FilteredInspections);
        NotifyOfPropertyChange(() => ButtonIsEnabled);
      }
    }

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredCustomers);
      }
    }

    public IEnumerable<CustomerViewModel> FilteredCustomers
    {
      get
      {
        return SearchInCustomerList()
          .ToArray();
      }
    }

    public IEnumerable<InspectionViewModel> FilteredInspections
    {
      get
      {
        if (SelectedCustomer == null ||
            SelectedCustomer.Name == string.Empty)
        {
          return m_InspectionViewModels;
        }

        return SearchInInspectionList()
          .ToArray();
      }
    }

    public bool FlyoutActivated
    {
      get { return m_FlyoutActivated; }
      set
      {
        if (m_FlyoutActivated == value)
        {
          return;
        }
        m_FlyoutActivated = value;
        NotifyOfPropertyChange(() => FlyoutActivated);
      }
    }

    public InspectionViewModel SelectedInspectionViewModel
    {
      get { return m_SelectedInspectionViewModel; }
      set
      {
        m_SelectedInspectionViewModel = value;
        NotifyOfPropertyChange(() => SelectedInspectionViewModel);
      }
    }

    public bool ButtonIsEnabled
    {
      get
      {
        if (m_SelectedCustomer == null ||
            m_Repository.ProductionItems.All(pi => pi.Customer != m_SelectedCustomer.Model))
        {
          return false;
        }
        return true;
      }
    }

    public Visibility CustomerColumnDataGridVisibility { get; set; }
    public Visibility NoCustomerColumnDataGridVisibility { get; set; }

    public int Index
    {
      get { return 20; }
    }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      private set
      {
        if (value.Equals(m_IsEnabled))
        {
          return;
        }
        m_IsEnabled = value;
        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    public string Title
    {
      get { return TranslationProvider.Translate("Inspection"); }
    }

    private IEnumerable<CustomerViewModel> SearchInCustomerList()
    {
      if (string.IsNullOrEmpty(SearchText))
      {
        return m_CustomerViewModels;
      }

      var searchText = SearchText.ToLower();
      var searchResult = m_CustomerViewModels.Where(c => (c.Name != null) && (c.Name.ToLower()
                                                                               .Contains(searchText)));

      return searchResult;
    }

    private IEnumerable<InspectionViewModel> SearchInInspectionList()
    {
      if (SelectedCustomer == null)
      {
        return m_InspectionViewModels;
      }

      return m_InspectionViewModels.Where(ivm => (ivm.ProductionItem.Customer == m_SelectedCustomer.Model));
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConnection;
      if (IsEnabled)
      {
        LoadData();
      }
    }

    private void LoadData()
    {
      if (m_Repository.Inspections == null ||
          m_Repository.Customers == null)
      {
        return;
      }

      m_Repository.Inspections.CollectionChanged += AlterInspectionCollection;
      m_Repository.Customers.CollectionChanged += AlterCustomerCollection;
      LoadInspections();
      LoadCustomers();
    }

    private void LoadInspections()
    {
      m_InspectionViewModels.Clear();

      foreach (var inspection in m_Repository.Inspections)
      {
        m_InspectionViewModels.Add(new InspectionViewModel(inspection));
      }
      NotifyOfPropertyChange(() => FilteredInspections);
    }

    private void LoadCustomers()
    {
      m_CustomerViewModels.Clear();

      foreach (var customer in m_Repository.Customers)
      {
        m_CustomerViewModels.Add(new CustomerViewModel(customer));
      }
      NotifyOfPropertyChange(() => FilteredCustomers);
    }


    private void OpenEditor(object dataContext)
    {
      m_EditItem = (IScreen) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    public void OpenInspectionAddDialog()
    {
      OpenEditor(m_DiscardViewModelFactory.CreateInspectionAddViewModel(m_SelectedCustomer));
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount < 2)
      {
        return;
      }
      OpenInspectionEditDialog();
    }

    public void OpenInspectionEditDialog()
    {
      OpenEditor(m_DiscardViewModelFactory.CreateInspectionEditViewModel(m_SelectedInspectionViewModel.Model, m_SelectedCustomer, RemoveInspection));
    }

    public void RemoveInspection()
    {
      var inspectionViewModel = m_SelectedInspectionViewModel;
      if (m_SelectedInspectionViewModel.Model.DiscardItems.Any())
      {
        Dialogs.ShowMessageBox(TranslationProvider.Translate("InspectionDeleteFailed"), TranslationProvider.Translate("DBError"));
      }
      else
      {
        m_Repository.Inspections.Remove(inspectionViewModel.Model);
        m_Repository.Save();
        CloseEditor();
      }
    }

    public void Accept(object dataContext)
    {
      CloseEditor();
      m_Repository.Save();

      NotifyOfPropertyChange(() => FilteredInspections);
    }

    public void Cancel()
    {
      CloseEditor();
    }

    private void CloseEditor()
    {
      m_EditItem.TryClose();
    }

    public void AddInspection(object dataContext)
    {
      m_Repository.Inspections.Add(((InspectionAddViewModel) dataContext).Model);
      CloseEditor();
      m_Repository.Save();
    }

    private void AlterInspectionCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<Inspection>())
        {
          CreateInspectionViewModel(newItem);
        }
        NotifyOfPropertyChange(() => FilteredInspections);
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        foreach (var oldItem in e.OldItems.OfType<Inspection>())
        {
          var inspectionViewModel = m_InspectionViewModels.Single(r => r.Model == oldItem);

          m_InspectionViewModels.Remove(inspectionViewModel);
        }
        NotifyOfPropertyChange(() => FilteredInspections);
      }
    }

    private void AlterCustomerCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<Customer>())
        {
          CreateCustomerViewModel(newItem);
        }
        NotifyOfPropertyChange(() => FilteredCustomers);
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        foreach (var oldItem in e.OldItems.OfType<Customer>())
        {
          m_CustomerViewModels.Remove(m_CustomerViewModels.Single(r => r.Model == oldItem));
        }
        NotifyOfPropertyChange(() => FilteredCustomers);
      }
    }

    private void CreateCustomerViewModel(Customer newItem)
    {
      var cvm = m_DiscardViewModelFactory.CreateFromExisting(newItem);
      m_CustomerViewModels.Add(cvm);
    }

    private void CreateInspectionViewModel(Inspection newItem)
    {
      var ivm = new InspectionViewModel(newItem);
      m_InspectionViewModels.Add(ivm);
    }

    public void ShowAllInspections()
    {
      m_SelectedCustomer = null;
      ChangeDataGrid();
      NotifyOfPropertyChange(() => SelectedCustomer);
      NotifyOfPropertyChange(() => ButtonIsEnabled);
      NotifyOfPropertyChange(() => FilteredInspections);
    }

    public void ChangeDataGrid()
    {
      if (SelectedCustomer == null)
      {
        CustomerColumnDataGridVisibility = Visibility.Visible;
        NoCustomerColumnDataGridVisibility = Visibility.Collapsed;
      }
      else
      {
        CustomerColumnDataGridVisibility = Visibility.Collapsed;
        NoCustomerColumnDataGridVisibility = Visibility.Visible;
      }
      NotifyOfPropertyChange(() => CustomerColumnDataGridVisibility);
      NotifyOfPropertyChange(() => NoCustomerColumnDataGridVisibility);
    }
  }
}