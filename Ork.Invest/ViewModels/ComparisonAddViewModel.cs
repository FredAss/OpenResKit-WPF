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
using Ork.Framework;
using Ork.Invest.DomainModelService;

namespace Ork.Invest.ViewModels
{
  public class ComparisonAddViewModel : Screen
  {
    private Comparison m_ModelComparison;
    private InvestmentPlan m_ModelInvestmentPlan;

    public ComparisonAddViewModel(InvestmentPlan modelInvestmentPlan, Comparison modelComparison)
    {
      DisplayName = TranslationProvider.Translate("AddComparison");
      m_ModelComparison = modelComparison;
      m_ModelInvestmentPlan = modelInvestmentPlan;
    }

    public Comparison ModelComparison
    {
      get { return m_ModelComparison; }
    }

    public DateTime StartYear
    {
      get { return m_ModelInvestmentPlan.StartYear; }
    }

    public int Lifetime
    {
      get { return m_ModelInvestmentPlan.Lifetime; }
      set { ; }
    }

    public float ImputedInterestRate
    {
      get { return m_ModelInvestmentPlan.ImputedInterestRate * 100; }
      set { ; }
    }

    public float OtherCostsChangePA
    {
      get { return m_ModelInvestmentPlan.OtherCostsChangePA * 100; }
      set { ; }
    }

    public float OtherRevenueChangePA
    {
      get { return m_ModelInvestmentPlan.OtherRevenueChangePA * 100; }
      set { ; }
    }

    public float InvestmentSum
    {
      get { return m_ModelInvestmentPlan.InvestmentSum; }
      set { ; }
    }

    public float RecoveryValueToday
    {
      get { return m_ModelInvestmentPlan.RecoveryValueToday; }
      set { ; }
    }

    public float RecoveryValueAfterLifetime
    {
      get { return m_ModelInvestmentPlan.RecoveryValueAfterLifetime; }
      set { ; }
    }

    public float EnergyCostsAnnual
    {
      get { return m_ModelInvestmentPlan.EnergyCostsAnnual; }
      set { ; }
    }

    public float EnergyCostsChangePA
    {
      get { return m_ModelInvestmentPlan.EnergyCostsChangePA * 100; }
      set { ; }
    }

    public float OtherCostsPA
    {
      get { return m_ModelInvestmentPlan.OtherCostsPA; }
      set { ; }
    }

    public float OtherRevenuePA
    {
      get { return m_ModelInvestmentPlan.OtherRevenuePA; }
      set { ; }
    }


    public string NameComparison
    {
      get { return m_ModelComparison.ComparisonName; }
      set { m_ModelComparison.ComparisonName = value; }
    }

    public int LifetimeComparison
    {
      get { return m_ModelComparison.Lifetime; }
      set { m_ModelComparison.Lifetime = value; }
    }

    public float InvestmentSumComparison
    {
      get { return m_ModelComparison.InvestmentSum; }
      set { m_ModelComparison.InvestmentSum = value; }
    }

    public float RecoveryValueTodayComparison
    {
      get { return m_ModelComparison.RecoveryValueToday; }
      set { m_ModelComparison.RecoveryValueToday = value; }
    }

    public float RecoveryValueAfterLifetimeComparison
    {
      get { return m_ModelComparison.RecoveryValueAfterLifetime; }
      set { m_ModelComparison.RecoveryValueAfterLifetime = value; }
    }

    public float EnergyCostsAnnualComparison
    {
      get { return m_ModelComparison.EnergyCostsAnnual; }
      set { m_ModelComparison.EnergyCostsAnnual = value; }
    }

    public float EnergyCostsChangePAComparison
    {
      get { return m_ModelComparison.EnergyCostsChangePA * 100; }
      set { m_ModelComparison.EnergyCostsChangePA = value / 100; }
    }

    public float OtherCostsPAComparison
    {
      get { return m_ModelComparison.OtherCostsPA; }
      set { m_ModelComparison.OtherCostsPA = value; }
    }

    public float OtherRevenuePAComparison
    {
      get { return m_ModelComparison.OtherRevenuePA; }
      set { m_ModelComparison.OtherRevenuePA = value; }
    }
  }
}