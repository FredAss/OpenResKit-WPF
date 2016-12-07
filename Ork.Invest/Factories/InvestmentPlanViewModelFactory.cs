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
using System.ComponentModel.Composition;
using System.Linq;
using Ork.Invest.DomainModelService;
using Ork.Invest.ViewModels;

namespace Ork.Invest.Factories
{
  [Export(typeof (IInvestmentPlanViewModelFactory))]
  public class InvestmentPlanViewModelFactory : IInvestmentPlanViewModelFactory
  {
    private readonly IComparisonViewModelFactory m_ComparisonViewModelFactory;
    private readonly IResponsibleSubjectViewModelFactory m_ResponsibleSubjectViewModelFactory;
    private readonly IInvestRepository m_repository;

    [ImportingConstructor]
    public InvestmentPlanViewModelFactory([Import] IInvestRepository repository, [Import] IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory,
      [Import] IComparisonViewModelFactory comparisonViewModelFactory)
    {
      m_repository = repository;
      m_ResponsibleSubjectViewModelFactory = responsibleSubjectViewModelFactory;
      m_ComparisonViewModelFactory = comparisonViewModelFactory;
    }


    public InvestmentPlanViewModel CreateFromExisting(InvestmentPlan investmentPlan)
    {
      return new InvestmentPlanViewModel(investmentPlan, m_ComparisonViewModelFactory);
    }

    public InvestmentPlanAddViewModel CreateInvestmentPlanAddViewModel()
    {
      return new InvestmentPlanAddViewModel(new InvestmentPlan
                                            {
                                              StartYear = DateTime.Today
                                            }, CreateResponsibleSubjects());
    }

    public InvestmentPlanEditViewModel CreateInvestmentPlanEditViewModel(InvestmentPlan investmentPlan, Action removeInvestmentPlanAction)
    {
      return new InvestmentPlanEditViewModel(investmentPlan, CreateResponsibleSubjects(), removeInvestmentPlanAction);
    }


    private ResponsibleSubjectViewModel[] CreateResponsibleSubjects()
    {
      return m_repository.ResponsibleSubjects.Select(m_ResponsibleSubjectViewModelFactory.CreateFromExisting)
                         .ToArray();
    }
  }
}