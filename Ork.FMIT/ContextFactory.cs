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
using System.Data.Services.Client;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Ork.FMIT.DomainModelService;
using Ork.Setting;

namespace Ork.FMIT
{
  // Class implements IContextFactory
  class ContextFactory : IContextFactory
  {
    private readonly ISettingsProvider m_SettingsProvider;

    // Constructor imports the settings from SettingsProvider
    [ImportingConstructor]
    public ContextFactory([Import] ISettingsProvider settingsProvider)
    {
      m_SettingsProvider = settingsProvider;
    }

    /// <summary>
    /// Methode, which creates a new DomainModelContext with the uri and returns it
    /// </summary>
    /// <returns></returns>
    [Export]
    public DomainModelContext CreateContext()
    {
      // Creates the uri for the connection to the OpenResKitHub
      var uri = new Uri(m_SettingsProvider.ConnectionString + "/OpenResKitHub");
      // Creates a new DomainmodelContext and passes the uri to the DomainModelContext
      var dms = new DomainModelContext(uri);
      dms.Credentials = new NetworkCredential(m_SettingsProvider.User, m_SettingsProvider.Password);
      dms.MergeOption = MergeOption.PreserveChanges;
      return dms;
    }
  }

 
  public interface IContextFactory
  {
    [Export]
    DomainModelContext CreateContext();
  }
}
