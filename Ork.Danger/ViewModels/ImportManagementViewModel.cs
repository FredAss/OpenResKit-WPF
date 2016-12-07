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
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Ork.Danger.DomainModelService;
using Ork.Danger.Factories;
using Ork.Danger.SurveyParserService;
using Ork.Framework;
using Ork.Framework.Utilities;
using Ork.Setting;

namespace Ork.Danger.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class ImportManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly IDangerRepository m_Repository;
    private readonly ISettingsProvider m_SettingsProvider;

    private readonly BindableCollection<SurveyTypeViewModel> m_SurveyTypeList;
    private readonly ISurveyTypeViewModelFactory m_SurveyTypeViewModelFactory;
    private SurveyParserServiceClient m_SurveyParserServiceClient;

    [ImportingConstructor]
    public ImportManagementViewModel([Import] ISettingsProvider settingsProvider, [Import] IDangerRepository contextRepository, [Import] ISurveyTypeViewModelFactory surveytypeViewModelFactory)
    {
      m_SurveyTypeViewModelFactory = surveytypeViewModelFactory;
      m_Repository = contextRepository;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(LoadSurveyTypes);
      m_SettingsProvider = settingsProvider;
      m_SurveyTypeList = new BindableCollection<SurveyTypeViewModel>();


      Initialize();
      if (m_SurveyTypeList != null &&
          m_SurveyTypeList.Any())
      {
        SelectedSurvey = m_SurveyTypeViewModelFactory.CreateSurveyTypeViewModelFromExisting(m_SurveyTypeList.First()
                                                                                                            .Model);
      }
    }


    public IEnumerable<SurveyTypeViewModel> AllSurveys
    {
      get { return m_SurveyTypeList; }
    }

    public SurveyTypeViewModel SelectedSurvey { get; set; }


    public string SelectedSurveyName { get; set; }

    public int Index
    {
      get { return 120; }
    }

    public bool IsEnabled
    {
      get { return true; }
    }

    public string Title
    {
      get { return "Import/Export"; }
    }


    public void AddSurvey()
    {
      if (!string.IsNullOrEmpty(SelectedSurveyName))
      {
        m_Repository.SaveCompleted += RepositoryOnSaveCompleted;

        var filename = string.Empty;
        var binarySourceDocument = FileHelpers.GetByeArrayFromUserSelectedFile(string.Empty, out filename);
        var newSurvey = new SurveyType();
        if (binarySourceDocument != null)
        {
          newSurvey.SurveyTypeDocx = (new Document
                                      {
                                        DocumentSource = new DocumentSource
                                                         {
                                                           BinarySource = binarySourceDocument
                                                         },
                                        Name = filename
                                      });
        }
        newSurvey.Name = SelectedSurveyName;
        m_Repository.SurveyTypes.Add(newSurvey);
        m_Repository.Save();

        m_SettingsProvider.Refresh();
      }
      else
      {
        Dialogs.ShowMessageBox("Bitte vergeben Sie einen Namen", "Kein Name");
      }
    }

    private void Initialize()
    {
      m_SurveyParserServiceClient = new SurveyParserServiceClient("BasicHttpBinding_SurveyParserService", m_SettingsProvider.ConnectionString + "/SurveyParserService");
      m_SurveyParserServiceClient.ClientCredentials.UserName.UserName = m_SettingsProvider.User;
      m_SurveyParserServiceClient.ClientCredentials.UserName.Password = m_SettingsProvider.Password;
      LoadSurveyTypes();
    }

    private void LoadSurveyTypes()
    {
      m_SurveyTypeList.Clear();
      if (m_Repository.SurveyTypes != null)
      {
        foreach (var st in m_Repository.SurveyTypes)
        {
          m_SurveyTypeList.Add(m_SurveyTypeViewModelFactory.CreateSurveyTypeViewModelFromExisting(st));
        }
      }
      NotifyOfPropertyChange(() => AllSurveys);
    }

    private void RepositoryOnSaveCompleted(object sender, EventArgs eventArgs)
    {
      m_Repository.ReloadSurveyTypes();
      LoadSurveyTypes();

      SelectedSurvey = m_SurveyTypeList.Last();
      SelectedSurveyName = null;
      //m_SurveyParserServiceClient.GenerateSurveyAsync(SelectedSurvey.Model.Id);
      m_SurveyParserServiceClient.GenerateSurvey(SelectedSurvey.Model.Id);
    }
  }
}