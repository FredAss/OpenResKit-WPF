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
using Ork.Danger.ViewModels;

namespace Ork.Danger
{
  [Export(typeof (ICompanyContext))]
  public class CompanyContext : ICompanyContext
  {
    public CompanyContext()
    {
      Initialize();
    }

    public CompanyViewModel companyViewModel { get; set; }
    public event EventHandler CompanyChanged;

    public void Save()
    {
      RaiseEvent(CompanyChanged);
    }

    private void Initialize()
    {
      RaiseEvent(CompanyChanged);
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