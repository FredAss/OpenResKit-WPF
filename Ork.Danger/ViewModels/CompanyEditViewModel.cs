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
using System.Linq;
using Ork.Danger.DomainModelService;
using Action = System.Action;

namespace Ork.Danger.ViewModels
{
  public class CompanyEditViewModel : CompanyAddViewModel
  {
    private readonly Action m_RemoveCompany;

    public CompanyEditViewModel(Company model, Action removeCompanyAction)
      : base(model)
    {
      m_RemoveCompany = removeCompanyAction;
      DisplayName = "Unternehmen bearbeiten";
    }

    public IEnumerable<WorkplaceViewModel> WorkplaceViewModels
    {
      get { return Model.Workplaces.Select(w => new WorkplaceViewModel(w)); }
    }

    public void RemoveCompany()
    {
      m_RemoveCompany();
    }
  }
}