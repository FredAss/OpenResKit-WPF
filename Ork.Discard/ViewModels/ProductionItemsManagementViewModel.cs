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
using System.Collections.Specialized;
using System.ComponentModel;
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
  public class ProductionItemsManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly BindableCollection<CustomerViewModel> m_Customers = new BindableCollection<CustomerViewModel>();
    private readonly IDiscardViewModelFactory m_DiscardViewModelFactory;
    private readonly BindableCollection<ProductionItemViewModel> m_ProductionItemViewModels = new BindableCollection<ProductionItemViewModel>();
    private readonly IDiscardRepository m_Repository;
    private IScreen m_EditItem;
    private bool m_FlyoutActivated = true;
    private bool m_IsEnabled;
    private string m_SearchText = string.Empty;
    private CustomerViewModel m_SelectedCustomer;
    private ProductionItemViewModel m_SelectedProductionItemViewModel;

    [ImportingConstructor]
    public ProductionItemsManagementViewModel([Import] IDiscardRepository contextRepository, [Import] IDiscardViewModelFactory dicardViewModelFactory)
    {
      m_Repository = contextRepository;
      m_DiscardViewModelFactory = dicardViewModelFactory;

      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(LoadData);

      LoadData();
      CustomerColumnDataGridVisibility = Visibility.Visible;
      NoCustomerColumnDataGridVisibility = Visibility.Hidden;
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

    public bool ButtonIsEnabled
    {
      get { return m_SelectedCustomer != null; }
    }

    public IEnumerable<ProductionItemViewModel> FilteredProductionItems
    {
      get
      {
        if (SelectedCustomer == null ||
            SelectedCustomer.Name == string.Empty)
        {
          return m_ProductionItemViewModels;
        }

        return m_ProductionItemViewModels.Where(pivm => pivm.Customer == SelectedCustomer.Model);
      }
    }

    public ProductionItemViewModel SelectedProductionItemViewModel
    {
      get { return m_SelectedProductionItemViewModel; }
      set { m_SelectedProductionItemViewModel = value; }
    }

    public Visibility CustomerColumnDataGridVisibility { get; set; }
    public Visibility NoCustomerColumnDataGridVisibility { get; set; }

    public IEnumerable<CustomerViewModel> FilteredCustomers
    {
      get
      {
        var filteredCustomers = SearchInCustomerList();
        return filteredCustomers;
      }
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

        NotifyOfPropertyChange(() => SelectedCustomer);
        NotifyOfPropertyChange(() => FilteredProductionItems);
        NotifyOfPropertyChange(() => ButtonIsEnabled);
        ChangeDataGrid();
      }
    }

    public int Index
    {
      get { return 10; }
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
      get { return TranslationProvider.Translate("TitleProductionItemManagementViewModel"); }
    }

    public IEnumerable<CustomerViewModel> SearchInCustomerList()
    {
      if (SearchText != null &&
          SearchText == string.Empty)
      {
        return m_Customers;
      }
      var searchText = SearchText.ToLower();
      return m_Customers.Where(c => (c.Name != null) && (c.Name.ToLower()
                                                          .Contains(searchText)));
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


    public void ShowAllDiscards()
    {
      m_SelectedCustomer = null;
      ChangeDataGrid();
      NotifyOfPropertyChange(() => SelectedCustomer);
      NotifyOfPropertyChange(() => ButtonIsEnabled);
      NotifyOfPropertyChange(() => FilteredProductionItems);
    }

    private void LoadData()
    {
      IsEnabled = m_Repository.HasConnection;
      if (!IsEnabled)
      {
        return;
      }

      m_ProductionItemViewModels.Clear();
      m_Customers.Clear();
      if (m_Repository.ProductionItems == null ||
          m_Repository.Customers == null)
      {
        return;
      }
      m_Repository.Customers.CollectionChanged += AlterCustomerCollection;
      m_Repository.ProductionItems.CollectionChanged += AlterProductionItemCollection;

      foreach (var productionItem in m_Repository.ProductionItems)
      {
        m_ProductionItemViewModels.Add(new ProductionItemViewModel(productionItem));
      }

      foreach (var customer in m_Repository.Customers)
      {
        m_Customers.Add(new CustomerViewModel(customer));
      }
    }

    public void OpenProductionItemAddDialog()
    {
      OpenEditor(m_DiscardViewModelFactory.CreateAddViewModel(new ProductionItem(), SelectedCustomer.Model));
    }

    public void OpenProductionItemDialog(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2)
      {
        if (dataContext is ProductionItemViewModel)
        {
          OpenEditor(m_DiscardViewModelFactory.CreateEditViewModel((((ProductionItemViewModel) dataContext).Model), RemoveProductionItem));
        }
      }
    }


    public void OpenProductionItemEditDialog(object dataContext)
    {
      OpenEditor(m_DiscardViewModelFactory.CreateEditViewModel((((ProductionItemViewModel) dataContext).Model), RemoveProductionItem));
    }

    public void AddProductionItem(object dataContext)
    {
      var productionItemAddViewModel = ((ProductionItemAddViewModel) dataContext);

      if (!productionItemAddViewModel.InspectionAttributes.Any())
      {
        var defaultInspectionAttribute = new InspectionAttribute
                                         {
                                           Number = "1",
                                           Description = TranslationProvider.Translate("Other")
                                         };

        productionItemAddViewModel.InspectionAttributes.Add(defaultInspectionAttribute);
        m_Repository.InspectionAttributes.Add(defaultInspectionAttribute);
      }

      m_Repository.ProductionItems.Add(productionItemAddViewModel.Model);
      m_Repository.Save();
      CloseEditor();
    }

    public void RemoveProductionItem()
    {
      var productionItemViewModel = SelectedProductionItemViewModel;
      if (m_Repository.Inspections.Any(i => i.ProductionItem == SelectedProductionItemViewModel.Model))
      {
        Dialogs.ShowMessageBox(TranslationProvider.Translate("ProductionItemDeleteFailed"), TranslationProvider.Translate("DBError"));
      }
      else
      {
        m_Repository.ProductionItems.Remove(productionItemViewModel.Model);
        m_Repository.Save();
        CloseEditor();
      }
    }

    public void RemoveCustomer()
    {
      var customerViewModel = SelectedCustomer;
      if (m_Repository.ProductionItems.Any(pi => pi.Customer == SelectedCustomer.Model))
      {
        Dialogs.ShowMessageBox(TranslationProvider.Translate("CustomerDeleteFailed"), TranslationProvider.Translate("DBError"));
      }
      else
      {
        m_Repository.Customers.Remove(customerViewModel.Model);
        m_Repository.Save();
        CloseEditor();
      }
    }


    private void OpenEditor(object dataContext)
    {
      m_EditItem = (IScreen) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount < 2)
      {
        return;
      }
      if (dataContext is CustomerViewModel)
      {
        OpenCustomerEditDialog(dataContext);
      }
      else if (dataContext is ProductionItemsManagementViewModel)
      {
        if (m_SelectedProductionItemViewModel != null)
        {
          OpenProductionItemEditDialog(m_SelectedProductionItemViewModel);
        }
      }
    }

    public void Accept(object dataContext)
    {
      CloseEditor();

      var pievm = ((ProductionItemEditViewModel) dataContext).Model;
      if (!pievm.InspectionAttributes.Any())
      {
        var defaultInspectionAttribute = new InspectionAttribute
                                         {
                                           Number = "1",
                                           Description = TranslationProvider.Translate("Other")
                                         };

        pievm.InspectionAttributes.Add(defaultInspectionAttribute);
        m_Repository.InspectionAttributes.Add(defaultInspectionAttribute);
      }

      m_Repository.Save();

      NotifyOfPropertyChange(() => FilteredProductionItems);
    }

    public void AcceptCustomer()
    {
      m_Repository.Save();
      CloseEditor();
      NotifyOfPropertyChange(() => SelectedCustomer);
      NotifyOfPropertyChange(() => FilteredCustomers);
    }

    public void Cancel()
    {
      CloseEditor();
    }

    private void CloseEditor()
    {
      m_EditItem.TryClose();
    }

    private void CreateCustomerViewModel(Customer newItem)
    {
      var cvm = m_DiscardViewModelFactory.CreateFromExisting(newItem);
      cvm.PropertyChanged += CustomerPropertyChanged;
      m_Customers.Add(cvm);
    }

    private void CreateProductionItemViewModel(ProductionItem newItem)
    {
      var pivm = new ProductionItemViewModel(newItem);
      pivm.PropertyChanged += ProductionItemPropertyChanged;
      m_ProductionItemViewModels.Add(pivm);
    }

    private void CustomerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Model")
      {
        NotifyOfPropertyChange(() => FilteredCustomers);
      }
    }

    private void ProductionItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Model")
      {
        NotifyOfPropertyChange(() => FilteredProductionItems);
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
          var customerViewModel = m_Customers.Single(r => r.Model == oldItem);
          m_Customers.Remove(customerViewModel);
        }
        NotifyOfPropertyChange(() => FilteredCustomers);
      }
    }

    private void AlterProductionItemCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<ProductionItem>())
        {
          CreateProductionItemViewModel(newItem);
        }
        NotifyOfPropertyChange(() => FilteredProductionItems);
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        foreach (var oldItem in e.OldItems.OfType<ProductionItem>())
        {
          var productionItemViewModel = m_ProductionItemViewModels.Single(r => r.Model == oldItem);

          m_ProductionItemViewModels.Remove(productionItemViewModel);
        }
        NotifyOfPropertyChange(() => FilteredProductionItems);
        NotifyOfPropertyChange(() => FilteredCustomers);
      }
    }

    public void AddCustomer(object dataContext)
    {
      var customerAddViewModel = ((CustomerAddViewModel) dataContext);
      m_Repository.Customers.Add(customerAddViewModel.Model);
      m_Repository.Save();
      CloseEditor();
      SelectedCustomer = m_Customers.Last();
    }


    public void OpenCustomerAddDialog()
    {
      OpenEditor(m_DiscardViewModelFactory.CreateCustomerAddViewModel());
    }

    public void OpenCustomerEditDialog(object dataContext)
    {
      SelectedCustomer = (CustomerViewModel) dataContext;
      OpenEditor(m_DiscardViewModelFactory.CreateCustomerEditViewModel((CustomerViewModel) dataContext, RemoveCustomer));
    }
  }
}