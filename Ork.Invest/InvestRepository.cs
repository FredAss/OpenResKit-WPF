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
using System.Data.Services.Client;
using System.Linq;
using Ork.Invest.DomainModelService;
using Ork.Setting;

namespace Ork.Invest
{
  [Export(typeof (IInvestRepository))]
  public class InvestRepository : IInvestRepository
  {
    private readonly Func<DomainModelContext> m_CreateMethod;
    private DomainModelContext m_Context;

    [ImportingConstructor]
    public InvestRepository([Import] ISettingsProvider settingsProvider, [Import] Func<DomainModelContext> createMethod)
    {
      m_CreateMethod = createMethod;
      settingsProvider.ConnectionStringUpdated += (s, e) => Initialize();
      Initialize();
    }

    public DataServiceCollection<Comparison> Comparisons { get; private set; }

    public DataServiceCollection<InvestmentPlan> InvestmentPlans { get; private set; }

    public DataServiceCollection<ResponsibleSubject> ResponsibleSubjects { get; private set; }

    public bool HasConncetion { get; private set; }

    public void Save()
    {
      if (m_Context.ApplyingChanges)
      {
        return;
      }

      var result = m_Context.BeginSaveChanges(SaveChangesOptions.Batch, r =>
                                                                        {
                                                                          var dm = (DomainModelContext) r.AsyncState;
                                                                          dm.EndSaveChanges(r);
                                                                          RaiseEvent(SaveCompleted);
                                                                        }, m_Context);
    }

    public event EventHandler ContextChanged;

    public event EventHandler SaveCompleted;

    private void Initialize()
    {
      m_Context = m_CreateMethod();

      try
      {
        LoadComparisons();
        LoadInvestmentPlans();
        //LoadCatalogs();
        LoadResponsibleSubjects();
        HasConncetion = true;
      }
      catch (Exception)
      {
        HasConncetion = false;
      }

      RaiseEvent(ContextChanged);
    }

    private void LoadComparisons()
    {
      Comparisons = new DataServiceCollection<Comparison>(m_Context);

      var query = m_Context.Calculations.Where(c => c is Comparison)
                           .Cast<Comparison>();
      Comparisons.Load(query);
    }

    private void LoadInvestmentPlans()
    {
      InvestmentPlans = new DataServiceCollection<InvestmentPlan>(m_Context);

      var query = m_Context.Calculations.Expand("OpenResKit.DomainModel.InvestmentPlan/Comparisons")
                           .Expand("OpenResKit.DomainModel.InvestmentPlan/StartYear")
                           .Expand("OpenResKit.DomainModel.InvestmentPlan/ResponsibleSubject")
                           .Where(c => c is InvestmentPlan)
                           .Cast<InvestmentPlan>();
      InvestmentPlans.Load(query);
    }

    private void LoadComparisonsToInvestmentPlans()
    {
    }

    private void LoadResponsibleSubjects()
    {
      ResponsibleSubjects = new DataServiceCollection<ResponsibleSubject>(m_Context);

      var query = m_Context.ResponsibleSubjects.Expand("OpenResKit.DomainModel.Employee/Groups");
      ResponsibleSubjects.Load(query);
    }

    private void RaiseEvent(EventHandler eventHandler)
    {
      if (eventHandler != null)
      {
        eventHandler(this, new EventArgs());
      }
    }
  }
}