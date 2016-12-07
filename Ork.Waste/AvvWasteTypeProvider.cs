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
using System.Net;
using System.Net.Sockets;
using Ork.Setting;
using Ork.Waste.AVVCatalog;

namespace Ork.Waste
{
  [Export(typeof (IAvvWasteTypeProvider))]
  public class AvvWasteTypeProvider : IAvvWasteTypeProvider
  {
    private readonly ISettingsProvider m_SettingsProvider;
    private AVVProviderServiceClient m_AvvProviderServiceClient;
    private IEnumerable<AvvWasteType> m_AvvWasteTypes;

    [ImportingConstructor]
    public AvvWasteTypeProvider([Import] ISettingsProvider settingsProvider)
    {
      m_SettingsProvider = settingsProvider;
      m_SettingsProvider.ConnectionStringUpdated += (s, e) => Initialize();
      Initialize();
    }

    public IEnumerable<AvvWasteType> AvvWasteTypes
    {
      get { return m_AvvWasteTypes; }
    }

    public event EventHandler Loaded;

    private void AvvWasteTypesReceived(object sender, GetAvvCatalogueCompletedEventArgs e)
    {
      if (e.Error != null &&
          e.Error.InnerException != null &&
          (e.Error.InnerException.InnerException is SocketException || e.Error.InnerException is WebException))
      {
        return;
      }
      m_AvvWasteTypes = e.Result;
      OnLoaded();
    }

    private void Initialize()
    {
      m_AvvProviderServiceClient = new AVVProviderServiceClient("BasicHttpBinding_AVVProviderService", m_SettingsProvider.ConnectionString + "/AvvCatalogue");
      m_AvvProviderServiceClient.ClientCredentials.UserName.UserName = m_SettingsProvider.User;
      m_AvvProviderServiceClient.ClientCredentials.UserName.Password = m_SettingsProvider.Password;
      m_AvvProviderServiceClient.GetAvvCatalogueCompleted += AvvWasteTypesReceived;
      m_AvvProviderServiceClient.GetAvvCatalogueAsync();
    }

    private void OnLoaded()
    {
      if (Loaded != null)
      {
        Loaded(this, new EventArgs());
      }
    }
  }
}