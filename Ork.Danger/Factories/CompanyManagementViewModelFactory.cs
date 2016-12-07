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

using System.ComponentModel.Composition;
using Ork.Danger.ViewModels;
using Ork.Framework;

namespace Ork.Danger.Factories
{
  [Export(typeof (ICompanyManagementViewModelFactory))]
  public class CompanyManagementViewModelFactory : ICompanyManagementViewModelFactory
  {
    private readonly IAssessmentViewModelFactory m_AssessmentViewModelFactory;
    private readonly ICompanyContext m_CompanyContext;
    private readonly IDangerViewModelFactory m_DangerViewModelFactory;
    private readonly IDialogManager m_DialogManager;
    private readonly IDangerRepository m_Repository;

    [ImportingConstructor]
    public CompanyManagementViewModelFactory(IDialogManager dialogManager, IDangerRepository repository, IDangerViewModelFactory dangerViewModelFactory,
      IAssessmentViewModelFactory assessmentViewModelFactory, ICompanyContext companyContext)
    {
      m_DialogManager = dialogManager;
      m_Repository = repository;
      m_DangerViewModelFactory = dangerViewModelFactory;
      m_AssessmentViewModelFactory = assessmentViewModelFactory;
      m_CompanyContext = companyContext;
    }

    // Daten  werden an ein ViewModel für die View übergeben, in diesem Fall m_Repository, m_DialogManager, m_DangerManagementViewModelFactory
    public DangerManagementViewModel CreateDangerManagementViewModel()
    {
      return new DangerManagementViewModel(m_DialogManager, m_Repository, m_DangerViewModelFactory, m_CompanyContext);
    }

    public AssessmentManagementViewModel CreateAssessmentManagementViewModel()
    {
      return new AssessmentManagementViewModel(m_Repository, m_AssessmentViewModelFactory, m_CompanyContext);
    }

    public ActionManagementViewModel CreateActionManagementViewModel()
    {
      return new ActionManagementViewModel(m_DialogManager, m_AssessmentViewModelFactory, m_Repository, m_CompanyContext);
    }
  }
}