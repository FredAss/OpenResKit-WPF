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
using Caliburn.Micro;
using Ork.Framework;
using Ork.Invest.DomainModelService;
using Ork.Invest.Utilities;

namespace Ork.Invest.ViewModels
{
  [Export(typeof (IWorkspace))]
  internal class EvaluationManagementViewModel : LocalizableScreenConductor, IWorkspace
  {
    private readonly BindableCollection<FinancialCalculationViewModel> m_FinancialCalculationViewModels = new BindableCollection<FinancialCalculationViewModel>();
    private readonly IInvestRepository m_Repository;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private string m_SearchText;
    private FinancialCalculation m_SelectedFinancialCalculation;

    [ImportingConstructor]
    public EvaluationManagementViewModel([Import] IInvestRepository repository, [ImportMany] IEnumerable<InvestmentEvaluationContentBaseViewModel> investmentEvaluationContentBaseViewModels)
    {
      m_Repository = repository;
      FlyoutActivated = true;
      
      investmentEvaluationContentBaseViewModels = investmentEvaluationContentBaseViewModels.OrderBy(m => m.Index);
      Items.AddRange(investmentEvaluationContentBaseViewModels);
   
      foreach (Screen item in Items)
      {
        item.Parent = this;
      }

      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);

      Reload();

      if(AllFinancialCalculations.Any())
        SelectedFinancialCalculation = AllFinancialCalculations.First();
    }

    public FinancialCalculation SelectedFinancialCalculation
    {
      get { return m_SelectedFinancialCalculation; }
      set
      {
        m_SelectedFinancialCalculation = value;
        NotifyOfPropertyChange(() => SelectedFinancialCalculation);
        foreach (var item in Items.Cast<InvestmentEvaluationContentBaseViewModel>())
        {
          item.SelectedFinancialCalculation = SelectedFinancialCalculation;
        }
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

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredFinancialCalculations);
      }
    }

    public IEnumerable<FinancialCalculation> AllFinancialCalculations
    {
      get { return FilteredFinancialCalculations.SelectMany(fc => fc.FinancialCalculations); }
    }

    public IEnumerable<FinancialCalculationViewModel> FilteredFinancialCalculations
    {
      get
      {
        return SearchInInvestmentPlanList()
          .ToArray();
      }
    }

    public int Index
    {
      get { return 450; }
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

    public string Title
    {
      get { return TranslationProvider.Translate("TitleEvaluationViewModel"); }
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
      m_Repository.InvestmentPlans.CollectionChanged += AlterFinancialCalculationViewModelCollection;
      foreach (var investmentPlan in m_Repository.InvestmentPlans)
      {
        CreateFinancialCalculationViewModel(investmentPlan);
      }
      NotifyOfPropertyChange(() => FilteredFinancialCalculations);
    }

    private void AlterFinancialCalculationViewModelCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (var newItem in e.NewItems.OfType<InvestmentPlan>())
          {
            CreateFinancialCalculationViewModel(newItem);
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (var investmentPlanViewModel in e.OldItems.OfType<InvestmentPlan>()
                                                   .Select(oldItem => m_FinancialCalculationViewModels.Single(r => r.Model == oldItem)))
          {
            m_FinancialCalculationViewModels.Remove(investmentPlanViewModel);
          }
          break;
      }
      NotifyOfPropertyChange(() => FilteredFinancialCalculations);
    }

    private void CreateFinancialCalculationViewModel(InvestmentPlan investmentPlan)
    {
      m_FinancialCalculationViewModels.Add(new FinancialCalculationViewModel(investmentPlan));
    }

    private IEnumerable<FinancialCalculationViewModel> SearchInInvestmentPlanList()
    {
      if (string.IsNullOrEmpty(m_SearchText))
      {
        return m_FinancialCalculationViewModels;
      }

      var searchText = SearchText.ToLower();
      var filteredInvestmentPlans = m_FinancialCalculationViewModels.Where(ipvm => (ipvm.InvestmentName != null) && (ipvm.InvestmentName.ToLower()
                                                                                                                         .Contains(searchText)) || (ipvm.ResponsibleSubjectName.ToLower()
                                                                                                                                                        .Contains(searchText)));

      return filteredInvestmentPlans;
    }
  }
}