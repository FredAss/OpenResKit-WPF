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
using System.ComponentModel.Composition;
using Ork.CarbonFootprint.PositionDataProvider;
using Ork.Setting;

namespace Ork.CarbonFootprint
{
  [Export]
  public class PositionDataAccessor
  {
    private readonly ISettingsProvider m_SettingsProvider;
    private IEnumerable<GlobalAirport> m_AirportData;
    private bool m_AirportsCompleted;
    private IEnumerable<FullyQualifiedCarData> m_CarData;
    private bool m_CarsCompleted;
    private PositionDataContextClient m_PositionDataContextClient;

    [ImportingConstructor]
    public PositionDataAccessor(ISettingsProvider settingsProvider)
    {
      m_SettingsProvider = settingsProvider;
      m_SettingsProvider.ConnectionStringUpdated += (s, e) => Initialize();
      Initialize();
    }

    public IEnumerable<GlobalAirport> AirportData
    {
      get { return m_AirportData; }
    }


    public IEnumerable<FullyQualifiedCarData> CarData
    {
      get { return m_CarData; }
    }

    private void Initialize()
    {
      m_PositionDataContextClient = new PositionDataContextClient("BasicHttpBinding_PositionDataContext",
        string.Format("http://{0}:{1}/PositionDataContext", m_SettingsProvider.Url, m_SettingsProvider.Port));
      m_PositionDataContextClient.ClientCredentials.UserName.UserName = m_SettingsProvider.User;
      m_PositionDataContextClient.ClientCredentials.UserName.Password = m_SettingsProvider.Password;
      m_PositionDataContextClient.GetFullyQualifiedCarDataAsync();
      m_PositionDataContextClient.GetFullyQualifiedCarDataCompleted += FullyQualifiedCarDataCompleted;
      m_PositionDataContextClient.GetAirportDataAsync();
      m_PositionDataContextClient.GetAirportDataCompleted += GetAirportDataCompleted;
    }

    public event EventHandler Loaded;

    private void OnLoaded()
    {
      if (Loaded != null)
      {
        Loaded(this, new EventArgs());
      }
    }


    private void GetAirportDataCompleted(object sender, GetAirportDataCompletedEventArgs e)
    {
      if (e.Result == null)
      {
        return;
      }

      m_AirportData = e.Result;
      m_AirportsCompleted = true;

      if (m_CarsCompleted)
      {
        OnLoaded();
      }
    }

    private void FullyQualifiedCarDataCompleted(object sender, GetFullyQualifiedCarDataCompletedEventArgs e)
    {
      if (e.Result == null)
      {
        return;
      }

      m_CarData = e.Result;
      m_CarsCompleted = true;

      if (m_AirportsCompleted)
      {
        OnLoaded();
      }
    }
  }
}