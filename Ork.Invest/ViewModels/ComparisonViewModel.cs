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

using System.ComponentModel;
using Caliburn.Micro;
using Ork.Invest.DomainModelService;

namespace Ork.Invest.ViewModels
{
  public class ComparisonViewModel : Screen
  {
    private readonly Comparison m_Model;

    public ComparisonViewModel(Comparison model)
    {
      m_Model = model;
      m_Model.PropertyChanged += ModelPropertyChanged;
    }

    public Comparison Model
    {
      get { return m_Model; }
    }

    public string ComparisonName
    {
      get { return m_Model.ComparisonName; }
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

    public bool IsReadOnly { get; set; }

    private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      NotifyOfPropertyChange(e.PropertyName);
    }
  }
}