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
using System.ComponentModel.Composition;
using Ork.Framework;
using Ork.Invest.Utilities;

namespace Ork.Invest.ViewModels
{
  [Export(typeof (InvestmentEvaluationContentBaseViewModel))]
  public class PaymentPlanViewModel : InvestmentEvaluationContentBaseViewModel
  {
    private IEnumerable<Payment> m_Payments;
    private FinancialCalculation m_SelectedFinancialCalculation;

    public PaymentPlanViewModel()
    {
      Index = 2;
    }

    public string Title
    {
      get { return TranslationProvider.Translate("PaymentPlan"); }
    }

    public IEnumerable<Payment> Payments
    {
      get { return m_Payments; }
    }

    protected override void OnFinancialCalculationChanged(FinancialCalculation value)
    {
      m_SelectedFinancialCalculation = value;
      m_Payments = m_SelectedFinancialCalculation.Payments;
      NotifyOfPropertyChange(() => Payments);
    }
  }
}