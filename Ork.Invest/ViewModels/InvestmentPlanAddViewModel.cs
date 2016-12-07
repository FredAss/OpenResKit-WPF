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
using System.Linq;
using Ork.Framework;
using Ork.Invest.DomainModelService;

namespace Ork.Invest.ViewModels
{
  public class InvestmentPlanAddViewModel : DocumentBase
  {
    private readonly InvestmentPlan m_Model;
    private readonly IEnumerable<ResponsibleSubjectViewModel> m_ResponsibleSubjectViewModels;
    private string m_ResponsibleSubjectSearchText = string.Empty;
    private ResponsibleSubjectViewModel m_SelectedResponsibleSubjectViewModel;

    public InvestmentPlanAddViewModel(InvestmentPlan model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjectViewModels)
    {
      DisplayName = TranslationProvider.Translate("InvestmentPlanAdd");
      m_Model = model;
      m_ResponsibleSubjectViewModels = responsibleSubjectViewModels;
    }

    public InvestmentPlan Model
    {
      get { return m_Model; }
    }

    public string InvestmentName
    {
      get { return m_Model.InvestmentName; }
      set { m_Model.InvestmentName = value; }
    }

    public string Description
    {
      get { return m_Model.Description; }
      set { m_Model.Description = value; }
    }

    public IEnumerable<ResponsibleSubjectViewModel> FilteredResponsibleSubjects
    {
      get { return SearchInResponsibleSubjectList(); }
    }

    public string ResponsibleSubjectSearchText
    {
      get { return m_ResponsibleSubjectSearchText; }
      set
      {
        m_ResponsibleSubjectSearchText = value;
        NotifyOfPropertyChange(() => FilteredResponsibleSubjects);
      }
    }

    public ResponsibleSubjectViewModel SelectedResponsibleSubject
    {
      get { return m_SelectedResponsibleSubjectViewModel; }
      set
      {
        m_SelectedResponsibleSubjectViewModel = value;
        m_Model.ResponsibleSubject = value.Model;
        NotifyOfPropertyChange(() => m_Model.ResponsibleSubject);
        NotifyOfPropertyChange(() => CanInvestmentPlanAdd);
      }
    }

    public float ImputedInterestRate
    {
      get { return m_Model.ImputedInterestRate * 100; }
      set { m_Model.ImputedInterestRate = value / 100; }
    }

    public float OtherCostsChangePA
    {
      get { return m_Model.OtherCostsChangePA * 100; }
      set { m_Model.OtherCostsChangePA = value / 100; }
    }

    public float OtherRevenueChangePA
    {
      get { return m_Model.OtherRevenueChangePA * 100; }
      set { m_Model.OtherRevenueChangePA = value / 100; }
    }

    public int Lifetime
    {
      get { return m_Model.Lifetime; }
      set { m_Model.Lifetime = value; }
    }

    public float InvestmentSum
    {
      get { return m_Model.InvestmentSum; }
      set { m_Model.InvestmentSum = value; }
    }

    public float RecoveryValueToday
    {
      get { return m_Model.RecoveryValueToday; }
      set { m_Model.RecoveryValueToday = value; }
    }

    public float RecoveryValueAfterLifetime
    {
      get { return m_Model.RecoveryValueAfterLifetime; }
      set { m_Model.RecoveryValueAfterLifetime = value; }
    }

    public float EnergyCostsAnnual
    {
      get { return m_Model.EnergyCostsAnnual; }
      set { m_Model.EnergyCostsAnnual = value; }
    }

    public float EnergyCostsChangePA
    {
      get { return m_Model.EnergyCostsChangePA * 100; }
      set { m_Model.EnergyCostsChangePA = value / 100; }
    }

    public float OtherCostsPA
    {
      get { return m_Model.OtherCostsPA; }
      set { m_Model.OtherCostsPA = value; }
    }

    public float OtherRevenuePA
    {
      get { return m_Model.OtherRevenuePA; }
      set { m_Model.OtherRevenuePA = value; }
    }

    public DateTime StartYear
    {
      get { return m_Model.StartYear; }
      set { m_Model.StartYear = value; }
    }

    public bool CanInvestmentPlanAdd
    {
      get { return !(m_Model.InvestmentName == "" | m_Model.Description == "" | m_Model.ResponsibleSubject == null); }
    }

    private IEnumerable<ResponsibleSubjectViewModel> SearchInResponsibleSubjectList()
    {
      if (string.IsNullOrEmpty(ResponsibleSubjectSearchText))
      {
        return m_ResponsibleSubjectViewModels;
      }
      var searchText = ResponsibleSubjectSearchText.ToLower();

      var searchResultResponsibleSubjects = m_ResponsibleSubjectViewModels.Where(e => (e.Infotext != null) && (e.Infotext.ToLower()
                                                                                                                .Contains(searchText.ToLower())));

      return searchResultResponsibleSubjects;
    }
  }
}