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
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;
using Ork.Danger.Factories;
using Ork.Framework;

namespace Ork.Danger.ViewModels
{
  [Export(typeof (DangerManagementViewModel))]
  public class DangerManagementViewModel : Conductor<IScreen>.Collection.OneActive, IWorkspaceOptions
  {
    private readonly ICompanyContext m_CompanyContext;
    private readonly IDangerViewModelFactory m_DangerViewModelFactory;
    private readonly IDangerRepository m_Repository;
    private bool m_IsEnabled;
    private string m_SearchText = "";
    private CompanyViewModel m_SelectedCompanyViewModel;
    private WorkplaceViewModel m_SelectedWorkplaceViewModel;
    private Workplace m_deleteWorkplace;

    [ImportingConstructor]
    public DangerManagementViewModel(IDialogManager dialogManager, IDangerRepository contextRepository, IDangerViewModelFactory dangerViewModelFactory, ICompanyContext companyContext)
    {
      Dialogs = dialogManager;
      m_Repository = contextRepository;
      m_DangerViewModelFactory = dangerViewModelFactory;
      m_CompanyContext = companyContext;
      SelectedCompanyViewModel = m_CompanyContext.companyViewModel;
      m_CompanyContext.CompanyChanged += (s, e) => UpdateSelectedCompanyViewModel();
    }

    public IDialogManager Dialogs { get; set; }

    public CompanyViewModel SelectedCompanyViewModel
    {
      get { return m_SelectedCompanyViewModel; }
      set
      {
        m_SelectedCompanyViewModel = value;
        if (m_SelectedCompanyViewModel != null)
        {
          ShowWorkplaceAddView();
          SelectedWorkplaceViewModel = null;
        }


        NotifyOfPropertyChange(() => SelectedCompanyViewModel);
        NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
      }
    }

    public IEnumerable<WorkplaceViewModel> FilteredWorkplacesByCompany
    {
      get
      {
        if (m_SelectedCompanyViewModel == null)
        {
          return null;
        }

        var filteredWorkplaces = FilterWorkplaceViewModelsBySearchtext();

        return filteredWorkplaces;
      }
    }

    public WorkplaceViewModel SelectedWorkplaceViewModel
    {
      get { return m_SelectedWorkplaceViewModel; }
      set
      {
        m_SelectedWorkplaceViewModel = value;

        if (m_SelectedWorkplaceViewModel != null)
        {
          ShowWorkplaceEditView();
        }
        NotifyOfPropertyChange(() => SelectedWorkplaceViewModel);
      }
    }

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        if (value != m_SearchText)
        {
          m_SearchText = value;
          NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
        }
      }
    }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      set
      {
        m_IsEnabled = value;
        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    public void CancelObject(object dataContext)
    {
      ShowWorkplaceAddView();
      SelectedWorkplaceViewModel = null;
    }

    public void AcceptObject(object dataContext)
    {
      if (dataContext.GetType() == typeof (WorkplaceAddViewModel))
      {
        ExecuteInsertOfWorkplace(((WorkplaceAddViewModel) dataContext).Workplace);
      }
      if (dataContext.GetType() == typeof (WorkplaceEditViewModel))
      {
        ExecuteChangeOfWorkplace();
      }
    }

    public void RemoveObject(object dataContext)
    {
      if (dataContext is WorkplaceEditViewModel)
      {
        DeleteWorkplace(((WorkplaceEditViewModel) dataContext).Workplace);
        NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
        ShowWorkplaceAddView();
      }
    }

    public void CopyObject(object dataContext)
    {
      if (dataContext is WorkplaceEditViewModel)
      {
        CreateCopyOfWorkplace(((WorkplaceEditViewModel) dataContext).Workplace);
        NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
        ShowWorkplaceAddView();
      }
    }


    private void UpdateSelectedCompanyViewModel()
    {
      SelectedCompanyViewModel = m_CompanyContext.companyViewModel;
      NotifyOfPropertyChange(() => SelectedCompanyViewModel);
    }

    public IEnumerable<WorkplaceViewModel> FilterWorkplaceViewModelsBySearchtext()
    {
      var searchText = m_SearchText.ToLower();

      return m_SelectedCompanyViewModel.Workplaces.Where(w => w.Name.ToLower()
                                                               .Contains(searchText) || w.NameCompany.ToLower()
                                                                                         .Contains(searchText) || w.Description.ToLower()
                                                                                                                   .Contains(searchText))
                                       .Select(w => m_DangerViewModelFactory.CreateWorkplaceViewModel(w));
    }

    public void ShowWorkplaceAddView()
    {
      ActivateItem(m_DangerViewModelFactory.CreateWorkplaceAddViewModel(m_SelectedCompanyViewModel.Model));
    }

    public void ShowWorkplaceEditView()
    {
      ActivateItem(m_DangerViewModelFactory.CreateWorkplaceEditViewModel(m_SelectedWorkplaceViewModel.Model, m_SelectedCompanyViewModel.Model));
    }

    public void ShowWorkplaceEditView(WorkplaceViewModel workplaceOfCompany)
    {
      ActivateItem(m_DangerViewModelFactory.CreateWorkplaceEditViewModel(workplaceOfCompany.Model, m_SelectedCompanyViewModel.Model));
    }

    private void Save()
    {
      m_Repository.Save();
    }

    private void ExecuteInsertOfWorkplace(Workplace workplaceToInsert)
    {
      InsertWorkplace(workplaceToInsert);
      //InsertThreatsOfWorklplace(workplaceToInsert);
      NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
      ShowWorkplaceAddView();
    }


    private void ExecuteChangeOfWorkplace()
    {
      Save();
      NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);
      ShowWorkplaceAddView();
    }

    private void InsertWorkplace(Workplace workplaceToInsert)
    {
      if (!workplaceToInsert.Activities.Any())
      {
        InsertDefaultActivity(workplaceToInsert);
      }

      m_Repository.Workplaces.Add(workplaceToInsert);
      SelectedCompanyViewModel.Workplaces.Add(workplaceToInsert);
      Save();
    }

    private Workplace InsertDefaultActivity(Workplace workplaceToInsert)
    {
      var defaultActivity = new Activity
                            {
                              Name = "Sonstige Tätigkeit"
                            };
      workplaceToInsert.Activities.Add(defaultActivity);
      return workplaceToInsert;
    }

    private void DeleteWorkplace(Workplace workplace)
    {
      m_deleteWorkplace = workplace;
      if (workplace.Assessments.Any())
      {
        Dialogs.ShowMessageBox("Wollen Sie den Arbeitsplatz wirklich löschen? Es existieren bereits Begehungen", "Arbeitsplatz löschen nicht möglich", MessageBoxOptions.OkCancel, RemoveWorkplaceResult);
      }

      else
      {
        RemoveWorkplaceAction(m_deleteWorkplace);
      }
    }

    private void RemoveWorkplaceResult(IMessageBox obj)
    {
      if (obj.WasSelected(MessageBoxOptions.Ok))
      {
        RemoveWorkplaceAction(m_deleteWorkplace);
      }
    }

    private void RemoveWorkplaceAction(Workplace workplace)
    {
      m_Repository.Workplaces.Remove(workplace);
      SelectedCompanyViewModel.Workplaces.Remove(workplace);
      NotifyOfPropertyChange(() => FilteredWorkplacesByCompany);

      Save();
    }


    private void CreateCopyOfWorkplace(Workplace workplace)
    {
      var copyOfWorkplace = new Workplace
                            {
                              Name = workplace.Name,
                              NameCompany = workplace.NameCompany,
                              Description = workplace.Description,
                              SurveyType = workplace.SurveyType
                            };

      copyOfWorkplace = CreateCopiesActivitiesOfWorkplace(workplace, copyOfWorkplace);

      InsertWorkplace(copyOfWorkplace);
    }

    private Workplace CreateCopiesActivitiesOfWorkplace(Workplace originalWorkplace, Workplace copyWorkplace)
    {
      foreach (var copyOfActivity in originalWorkplace.Activities.Select(activityToCopy => new Activity
                                                                                           {
                                                                                             Name = activityToCopy.Name
                                                                                           }))
      {
        copyWorkplace.Activities.Add(copyOfActivity);
      }

      return copyWorkplace;
    }
  }
}