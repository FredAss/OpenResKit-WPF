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

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Danger.Commands;
using Ork.Danger.DomainModelService;
using Ork.Danger.Factories;
using Ork.Framework;

namespace Ork.Danger.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class CompanyManagementViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IWorkspace
  {
    private readonly ICompanyContext m_CompanyContext;
    private readonly ICompanyManagementViewModelFactory m_CompanyManagementViewModelFactory;

    private readonly BindableCollection<CompanyViewModel> m_CompanyViewModels = new BindableCollection<CompanyViewModel>();

    private readonly IDangerViewModelFactory m_DangerViewModelFactory;
    private readonly IDangerRepository m_Repository;
    private int m_ActivatedManagementViewModel;
    private IScreen m_EditItem;
    private bool m_IsEnabled;
    private CompanyViewModel m_SelectedCompanyViewModel;

    [ImportingConstructor]
    public CompanyManagementViewModel(IDialogManager dialogManager, [Import] IDangerRepository mRepository, [Import] ICompanyManagementViewModelFactory companyManagementViewModelFactory,
      [Import] IDangerViewModelFactory dangerViewModelFactory, [Import] ICompanyContext companyContext)
    {
      m_Repository = mRepository;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(LoadData);
      m_CompanyManagementViewModelFactory = companyManagementViewModelFactory;
      m_DangerViewModelFactory = dangerViewModelFactory;
      m_CompanyContext = companyContext;
      m_CompanyContext.CompanyChanged += (s, e) => UpdateSelectedCompanyViewModel();
      ActivatedManagementViewModel = 0;
      AddNewCompany = new AddNewCompanyCommand(this);
      LoadData();
      Dialogs = dialogManager;
      SelectFirstCompanyViewModel();
    }

    public ICommand AddNewCompany { get; set; }

    public CompanyViewModel SelectedCompanyViewModel
    {
      get { return m_SelectedCompanyViewModel; }
      set
      {
        m_SelectedCompanyViewModel = value;
        m_CompanyContext.companyViewModel = value;
        m_CompanyContext.Save();
        NotifyOfPropertyChange(() => SelectedCompanyViewModel);
      }
    }

    public ObservableCollection<CompanyViewModel> CompanyViewModels
    {
      get { return m_CompanyViewModels; }
    }

    public int ActivatedManagementViewModel
    {
      get { return m_ActivatedManagementViewModel; }
      set
      {
        m_ActivatedManagementViewModel = value;
        ChecksActivatedManagementViewModel();
        NotifyOfPropertyChange(() => ActivatedManagementViewModel);
      }
    }

    public bool ActivateTabEnabled
    {
      get { return m_CompanyViewModels.Any(); }
    }

    public IDialogManager Dialogs { get; set; }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      set
      {
        if (m_Repository.SurveyTypes.Any())
        {
          m_IsEnabled = true;
        }
        else
        {
          m_IsEnabled = false;
        }

        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    public int Index
    {
      get { return 100; }
    }

    public string Title
    {
      get { return "Beurteilungen"; }
    }

    public void UpdateSelectedCompanyViewModel()
    {
      m_SelectedCompanyViewModel = m_CompanyContext.companyViewModel;
    }

    public void AddCompany(object dataContext)
    {
      var companyAddViewModel = ((CompanyAddViewModel) dataContext);
      m_Repository.Companies.Add(companyAddViewModel.Model);
      CloseEditor();
      Save();
      SelectedCompanyViewModel = m_CompanyViewModels.Last();
      NotifyOfPropertyChange(() => ActivateTabEnabled);
    }

    private void OpenEditor(object dataContext)
    {
      m_EditItem = (IScreen) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    public void OpenCompanyAddDialog()
    {
      OpenEditor(m_DangerViewModelFactory.CreateCompanyAddViewModel());
    }

    public void OpenCompanyEditDialog(object dataContext)
    {
      SelectedCompanyViewModel = (CompanyViewModel) dataContext;
      OpenEditor(m_DangerViewModelFactory.CreateCompanyEditViewModel(m_SelectedCompanyViewModel.Model, RemoveCompany));
    }

    public void CloseEditor()
    {
      m_EditItem.TryClose();
    }

    public void Accept()
    {
      Save();
      CloseEditor();
    }

    public void Cancel()
    {
      CloseEditor();
    }

    private void Save()
    {
      m_Repository.Save();
    }

    public void RemoveCompany()
    {
      if (m_SelectedCompanyViewModel.Model.Workplaces.Any())
      {
        CloseEditor();
        Dialogs.ShowMessageBox("Zu diesem Unternehmen gibt es Arbeitsplätze. Das Löschen ist nicht möglich!", "Löschen nicht möglich");
      }
      else
      {
        m_Repository.Companies.Remove(m_SelectedCompanyViewModel.Model);
        CloseEditor();
        Save();
      }
    }

    private void LoadData()
    {
      IsEnabled = m_Repository.HasConnection;

      if (IsEnabled)
      {
        m_CompanyViewModels.Clear();
        LoadCompanies();

        if (CompanyViewModels.Any())
        {
          SelectFirstCompanyViewModel();
        }
      }
    }


    private void LoadCompanies()
    {
      m_Repository.Companies.CollectionChanged += AlterCompanyCollection;

      if (m_Repository.Companies != null)
      {
        foreach (var company in m_Repository.Companies)
        {
          m_CompanyViewModels.Add(m_DangerViewModelFactory.CreateCompanyViewModel(company));
        }
      }
      NotifyOfPropertyChange(() => CompanyViewModels);
    }

    private void CreateCompanyViewModel(Company company)
    {
      m_CompanyViewModels.Add(m_DangerViewModelFactory.CreateCompanyViewModel(company));
    }

    private void AlterCompanyCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<Company>())
        {
          CreateCompanyViewModel(newItem);
        }
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        foreach (var oldItem in e.OldItems.OfType<Company>())
        {
          m_CompanyViewModels.Remove(m_CompanyViewModels.Single(m => m.Model == oldItem));
        }
      }
    }

    public void SelectFirstCompanyViewModel()
    {
      if (m_CompanyViewModels.Any())
      {
        SelectedCompanyViewModel = m_CompanyViewModels.First();
      }
    }

    private void ChecksActivatedManagementViewModel()
    {
      if (m_ActivatedManagementViewModel == 0)
      {
        ShowDangerManagementViewModel();
      }
      if (m_ActivatedManagementViewModel == 1)
      {
        ShowAssessmentManagementViewModel();
      }
      if (m_ActivatedManagementViewModel == 2)
      {
        ShowActionManagementViewModel();
      }
    }

    public void ShowActionManagementViewModel()
    {
      ActivateItem(m_CompanyManagementViewModelFactory.CreateActionManagementViewModel());
    }

    public void ShowDangerManagementViewModel()
    {
      ActivateItem(m_CompanyManagementViewModelFactory.CreateDangerManagementViewModel());
    }

    public void ShowAssessmentManagementViewModel()
    {
      ActivateItem(m_CompanyManagementViewModelFactory.CreateAssessmentManagementViewModel());
    }
  }
}