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

using System.Data.Services.Client;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;
using Action = Ork.Danger.DomainModelService.Action;

namespace Ork.Danger.ViewModels
{
  public class ThreatViewModel : Screen
  {
    private readonly string[,] m_Matrix = new string[6, 6];

    private readonly string[] m_MatrixValues =
    {
      "3", "2", "1", "1", "1", "3", "2", "1", "1", "1", "3", "2", "2", "1", "1", "3", "2", "2", "2", "1", "3", "3", "3", "2", "2"
    };

    private readonly Threat m_Model;
    private string m_RiskGroupResult;

    public ThreatViewModel(Threat model, string RiskResult)
    {
      m_RiskGroupResult = RiskResult;
      m_Model = model;
    }

    public Threat Model
    {
      get { return m_Model; }
    }

    public string Description
    {
      get { return m_Model.Description; }
    }

    public int RiskDimension
    {
      get { return m_Model.RiskDimension; }
    }

    public int RiskPossibility
    {
      get { return m_Model.RiskPossibility; }
    }

    public string RiskResult
    {
      get
      {
        CreateRiskGroupMatrix();
        m_RiskGroupResult = m_Matrix[m_Model.RiskPossibility, m_Model.RiskDimension];
        return m_RiskGroupResult;
      }
    }

    public bool Actionneed
    {
      get { return m_Model.Actionneed; }
    }

    public string Status
    {
      get { return m_Model.Status; }
    }

    public GFactor GFactor
    {
      get { return m_Model.GFactor; }
    }

    public DataServiceCollection<Picture> Pictures
    {
      get { return m_Model.Pictures; }
    }

    public DataServiceCollection<ProtectionGoal> ProtectionGoals
    {
      get { return m_Model.ProtectionGoals; }
    }

    public DataServiceCollection<Action> Actions
    {
      get { return m_Model.Actions; }
    }

    public Activity Activity
    {
      get { return m_Model.Activity; }
    }

    public Dangerpoint Dangerpoint
    {
      get { return m_Model.Dangerpoint; }
    }

    public string CreateRiskGroupMatrix()
    {
      for (var i = 0; i < m_MatrixValues.Length; i++)
      {
        // Fill row
        for (var j = 0; j < 5; j++)
        {
          // Fill column of row
          for (var k = 0; k < 5; k++)
          {
            m_Matrix[j, k] = m_MatrixValues[i];
            i++;
          }
        }
      }
      m_RiskGroupResult = "";
      return m_RiskGroupResult;
    }
  }
}