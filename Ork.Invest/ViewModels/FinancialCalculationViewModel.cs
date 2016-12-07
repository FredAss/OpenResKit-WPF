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
using System.Linq;
using Caliburn.Micro;
using Ork.Invest.DomainModelService;
using Ork.Invest.Utilities;

namespace Ork.Invest.ViewModels
{
  public class FinancialCalculationViewModel : Screen
  {
    private readonly IList<FinancialCalculation> m_FinancialCalculations = new List<FinancialCalculation>();
    private readonly InvestmentPlan m_Model;

    public FinancialCalculationViewModel(InvestmentPlan model)
    {
      m_Model = model;
      var calculations = model.Comparisons.Select(c => new FinancialCalculation(model, c));
      foreach (var financialCalculation in calculations)
      {
        m_FinancialCalculations.Add(financialCalculation);
      }
      model.Comparisons.CollectionChanged += AlterFinancialCalculationCollection;
    }

    public InvestmentPlan Model
    {
      get { return m_Model; }
    }

    public string InvestmentName
    {
      get { return m_Model.InvestmentName; }
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

    public IEnumerable<FinancialCalculation> FinancialCalculations
    {
      get { return m_FinancialCalculations; }
    }

    private void AlterFinancialCalculationCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          foreach (var newItem in e.NewItems.OfType<Comparison>())
          {
            m_FinancialCalculations.Add(new FinancialCalculation(m_Model, newItem));
          }
          break;
        case NotifyCollectionChangedAction.Remove:
          foreach (var item in e.OldItems.OfType<Comparison>()
                                .Select(oldItem => m_FinancialCalculations.Single(r => r.ComparisonModel == oldItem)))
          {
            m_FinancialCalculations.Remove(item);
          }
          break;
      }
    }
  }
}