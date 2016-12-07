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
using Ork.Danger.DomainModelService;

namespace Ork.Danger.ViewModels
{
  public class WorkplaceEditViewModel : WorkplaceAddViewModel
  {
    public WorkplaceEditViewModel(Workplace workplace, IEnumerable<SurveyTypeViewModel> surveytypeViewModel, Company company)
      : base(workplace, surveytypeViewModel, company)
    {
    }


    public new bool CancelIsEnabled
    {
      get { return true; }
    }

    public new bool AcceptIsEnabled
    {
      get { return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(NameInCompany); }
    }

    public new bool RemoveIsEnabled
    {
      get { return true; }
    }

    public new bool CopyIsEnabled
    {
      get { return true; }
    }

    public new bool ExportIsEnabled
    {
      get { return true; }
    }
  }
}