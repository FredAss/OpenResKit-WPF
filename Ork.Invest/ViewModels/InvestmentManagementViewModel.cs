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
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Invest.DomainModelService;
using Ork.Invest.Factories;

namespace Ork.Invest.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class InvestmentManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly IComparisonViewModelFactory m_ComparisonViewModelFactory;
    private readonly IInvestmentPlanViewModelFactory m_InvestmentPlanViewModelFactory;
    private readonly BindableCollection<InvestmentPlanViewModel> m_InvestmentPlanViewModels = new BindableCollection<InvestmentPlanViewModel>();
    private readonly IInvestRepository m_Repository;
    private IScreen m_EditItem;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private string m_SearchText;
    private InvestmentPlanViewModel m_SelectedInvestmentPlan;

    [ImportingConstructor]
    public InvestmentManagementViewModel([Import] IInvestRepository repository, [Import] IInvestmentPlanViewModelFactory investmentPlanViewModelFactory,
      [Import] IComparisonViewModelFactory comparisonViewModelFactory)
    {
      m_Repository = repository;
      m_InvestmentPlanViewModelFactory = investmentPlanViewModelFactory;
      m_ComparisonViewModelFactory = comparisonViewModelFactory;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);

      Reload();

      FlyoutActivated = true;
      SelectFirstInvestmentPlan();
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

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredInvestmentPlans);
      }
    }

    public IEnumerable<InvestmentPlanViewModel> FilteredInvestmentPlans
    {
      get
      {
        return SearchInInvestmentPlanList()
          .ToArray();
      }
    }

    public InvestmentPlanViewModel SelectedInvestmentPlanViewModel
    {
      get { return m_SelectedInvestmentPlan; }
      set
      {
        m_SelectedInvestmentPlan = value;
        NotifyOfPropertyChange(() => SelectedInvestmentPlanViewModel);
        NotifyOfPropertyChange(() => Comparisons);
      }
    }

    public IEnumerable<ComparisonViewModel> Comparisons
    {
      get
      {
        if (SelectedInvestmentPlanViewModel == null)
        {
          return null;
        }

        var oldnewComparison = new Comparison()
                               {
                                 ComparisonName = SelectedInvestmentPlanViewModel.InvestmentName,
                                 EnergyCostsAnnual = SelectedInvestmentPlanViewModel.EnergyCostsAnnual,
                                 EnergyCostsChangePA = SelectedInvestmentPlanViewModel.EnergyCostsChangePA,
                                 InvestmentSum = SelectedInvestmentPlanViewModel.InvestmentSum,
                                 Lifetime = SelectedInvestmentPlanViewModel.Lifetime,
                                 OtherCostsPA = SelectedInvestmentPlanViewModel.OtherCostsPA,
                                 OtherRevenuePA = SelectedInvestmentPlanViewModel.OtherRevenueChangePA,
                                 RecoveryValueAfterLifetime = SelectedInvestmentPlanViewModel.RecoveryValueAfterLifetime,
                                 RecoveryValueToday = SelectedInvestmentPlanViewModel.RecoveryValueToday
                               };

        var allComparisons = new List<ComparisonViewModel>();
        var oldnewComparisonViewModel = new ComparisonViewModel(oldnewComparison)
                                        {
                                          IsReadOnly = true
                                        };
        var comparisons = SelectedInvestmentPlanViewModel.Model.Comparisons.Select(m_ComparisonViewModelFactory.CreateFromExisting);
        allComparisons.Add(oldnewComparisonViewModel);
        allComparisons.AddRange(comparisons);


        return allComparisons;
      }
    }

    public int Index
    {
      get { return 55; }
    }

    public string Title
    {
      get { return TranslationProvider.Translate("TitleInvestmentManagementViewModel"); }
    }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      private set
      {
        m_IsEnabled = value;
        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    private void SelectFirstInvestmentPlan()
    {
      if (m_InvestmentPlanViewModels.Any())
      {
        SelectedInvestmentPlanViewModel = m_InvestmentPlanViewModels.First();
      }
    }

    private IEnumerable<InvestmentPlanViewModel> SearchInInvestmentPlanList()
    {
      if (string.IsNullOrEmpty(m_SearchText))
      {
        return m_InvestmentPlanViewModels;
      }

      var searchText = SearchText.ToLower();
      var filteredInvestmentPlans = m_InvestmentPlanViewModels.Where(ipvm => (ipvm.InvestmentName != null) && (ipvm.InvestmentName.ToLower()
                                                                                                                   .Contains(searchText)) || (ipvm.ResponsibleSubjectName.ToLower()
                                                                                                                                                  .Contains(searchText)));

      return filteredInvestmentPlans;
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConncetion;
      if (IsEnabled)
      {
        LoadData();
      }
    }

    private void LoadData()
    {
      LoadInvestmentPlans();
    }

    private void LoadInvestmentPlans()
    {
      m_InvestmentPlanViewModels.Clear();
      m_Repository.InvestmentPlans.CollectionChanged += AlterInvestmentPlanCollection;
      foreach (var investmentPlan in m_Repository.InvestmentPlans)
      {
        CreateInvestmentPlanViewModel(investmentPlan);
      }
      NotifyOfPropertyChange(() => FilteredInvestmentPlans);
    }

    private void CreateInvestmentPlanViewModel(InvestmentPlan investmentPlan)
    {
      var ipvm = m_InvestmentPlanViewModelFactory.CreateFromExisting(investmentPlan);
      m_InvestmentPlanViewModels.Add(ipvm);
    }

    private void AlterInvestmentPlanCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (var newItem in e.NewItems.OfType<InvestmentPlan>())
          {
            CreateInvestmentPlanViewModel(newItem);
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (var investmentPlanViewModel in e.OldItems.OfType<InvestmentPlan>()
                                                   .Select(oldItem => m_InvestmentPlanViewModels.Single(r => r.Model == oldItem)))
          {
            m_InvestmentPlanViewModels.Remove(investmentPlanViewModel);
          }
          break;
      }
      NotifyOfPropertyChange(() => FilteredInvestmentPlans);
    }

    private void OpenEditor(object dataContext)
    {
      m_EditItem = (IScreen) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2)
      {
        if (dataContext is InvestmentPlanViewModel)
        {
          OpenInvestmentPlanEditDialog(dataContext);
        }
        else if (dataContext is ComparisonViewModel)
        {
          OpenComparisonEditDialog(dataContext);
        }
      }
    }

    public void OpenInvestmentPlanAddDialog()
    {
      OpenEditor(m_InvestmentPlanViewModelFactory.CreateInvestmentPlanAddViewModel());
    }

    public void OpenComparisonEditDialog(object dataContext)
    {
      OpenEditor(m_ComparisonViewModelFactory.CreateComparisonEditViewModel((((ComparisonViewModel) dataContext).Model), m_SelectedInvestmentPlan.Model));
    }

    public void OpenInvestmentPlanEditDialog(object dataContext)
    {
      OpenEditor(m_InvestmentPlanViewModelFactory.CreateInvestmentPlanEditViewModel((((InvestmentPlanViewModel) dataContext).Model), RemoveInvestmentPlan));
      m_SelectedInvestmentPlan = (InvestmentPlanViewModel) dataContext;
      NotifyOfPropertyChange(() => Comparisons);
    }

    public void OpenComparisonAddDialog()
    {
      OpenEditor(m_ComparisonViewModelFactory.CreateComparisonAddViewModel(m_SelectedInvestmentPlan.Model));
    }

    public void AddInvestmentPlan(object dataContext)
    {
      var investmentPlanAddViewModel = ((InvestmentPlanAddViewModel) dataContext);
      m_Repository.InvestmentPlans.Add(investmentPlanAddViewModel.Model);
      Save();
      CloseEditor();
      NotifyOfPropertyChange(() => FilteredInvestmentPlans);
      NotifyOfPropertyChange(() => Comparisons);
    }

    public void RemoveInvestmentPlan()
    {
      m_Repository.InvestmentPlans.Remove(m_SelectedInvestmentPlan.Model);
      Save();
      CloseEditor();
      NotifyOfPropertyChange(() => FilteredInvestmentPlans);
    }

    public void AddComparison(object dataContext)
    {
      var comparisonAddViewModel = ((ComparisonAddViewModel) dataContext);
      m_Repository.Comparisons.Add(comparisonAddViewModel.ModelComparison);
      m_SelectedInvestmentPlan.Model.Comparisons.Add(comparisonAddViewModel.ModelComparison);
      Save();
      CloseEditor();
      NotifyOfPropertyChange(() => FilteredInvestmentPlans);
    }

    public void SaveComparison(object dataContext)
    {
      Save();
      CloseEditor();
      NotifyOfPropertyChange(() => FilteredInvestmentPlans);
    }


    public void CloseEditor()
    {
      m_EditItem.TryClose();
    }

    public void Cancel()
    {
      CloseEditor();
    }

    private void Save()
    {
      m_Repository.Save();
      NotifyOfPropertyChange(() => Comparisons);
    }

    public void Accept(object dataContext)
    {
      Save();
      CloseEditor();
    }
  }
}