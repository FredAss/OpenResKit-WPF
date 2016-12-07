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
using Caliburn.Micro;
using Ork.Invest.DomainModelService;
using Ork.Invest.Factories;

namespace Ork.Invest.ViewModels
{
  public class InvestmentPlanViewModel : Screen
  {
    private readonly InvestmentPlan m_Model;
    private IComparisonViewModelFactory m_ComparisonViewModelFactory;

    public InvestmentPlanViewModel(InvestmentPlan model, IComparisonViewModelFactory comparisonViewModelFactory)
    {
      m_Model = model;
      m_Model.PropertyChanged += ModelPropertyChanged;
      m_ComparisonViewModelFactory = comparisonViewModelFactory;
    }

    public InvestmentPlan Model
    {
      get { return m_Model; }
    }

    public string InvestmentName
    {
      get { return m_Model.InvestmentName; }
    }

    public string Description
    {
      get { return m_Model.Description; }
    }

    public DateTime StartYear
    {
      get { return m_Model.StartYear; }
    }

    public float ImputedInterestRate
    {
      get { return m_Model.ImputedInterestRate; }
    }

    public float OtherCostsChangePA
    {
      get { return m_Model.OtherCostsChangePA; }
    }

    public float OtherRevenueChangePA
    {
      get { return m_Model.OtherRevenueChangePA; }
    }

    public int Lifetime
    {
      get { return m_Model.Lifetime; }
    }

    public float InvestmentSum
    {
      get { return m_Model.InvestmentSum; }
    }

    public float RecoveryValueToday
    {
      get { return m_Model.RecoveryValueToday; }
    }

    public float RecoveryValueAfterLifetime
    {
      get { return m_Model.RecoveryValueAfterLifetime; }
    }

    public float EnergyCostsAnnual
    {
      get { return m_Model.EnergyCostsAnnual; }
    }

    public float EnergyCostsChangePA
    {
      get { return m_Model.EnergyCostsChangePA; }
    }

    public float OtherCostsPA
    {
      get { return m_Model.OtherCostsPA; }
    }

    public float OtherRevenuePA
    {
      get { return m_Model.OtherRevenuePA; }
    }

    public string ResponsibleSubjectName
    {
      get
      {
        if (m_Model.ResponsibleSubject is Employee)
        {
          var employee = (Employee) m_Model.ResponsibleSubject;
          return employee.FirstName + " " + employee.LastName;
        }
        var group = (EmployeeGroup) m_Model.ResponsibleSubject;
        return @group.Name;
      }
    }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(e.PropertyName);
    }
  }
}