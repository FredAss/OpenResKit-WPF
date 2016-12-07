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
using System.Data.Services.Client;
using Ork.Danger.DomainModelService;
using Ork.Setting;

namespace Ork.Danger
{
  //wird als IDangerRepository exportiert, um dieses Repository später in den FactoryViewModels importieren zu können als Konstruktor (einmaliges Laden für den Rest der Ausführung)
  [Export(typeof (IDangerRepository))]
  public class DangerRepository : IDangerRepository
  {
    private readonly Func<DomainModelContext> m_CreateContext;
    private readonly ISettingsProvider m_SettingsProvider;
    private DomainModelContext m_Context;

    [ImportingConstructor]
    public DangerRepository([Import] ISettingsProvider settingsProvider, [Import] Func<DomainModelContext> createMethod)
    {
      m_SettingsProvider = settingsProvider;
      m_CreateContext = createMethod;
      m_SettingsProvider.ConnectionStringUpdated += (s, e) => Initialize();

      HasConnection = true;
      Initialize();
    }

    public void ReloadSurveyTypes()
    {
      SurveyTypes.Clear();
      LoadSurveyTypes();
    }

    public DataServiceCollection<SurveyType> SurveyTypes { get; private set; }

    public DataServiceCollection<Category> Categories { get; private set; }

    public DataServiceCollection<GFactor> GFactors { get; private set; }

    public DataServiceCollection<Question> Questions { get; private set; }

    public DataServiceCollection<Company> Companies { get; private set; }

    public DataServiceCollection<Workplace> Workplaces { get; private set; }

    public DataServiceCollection<WorkplaceCategory> WorkplaceCategories { get; private set; }

    public DataServiceCollection<Activity> Activities { get; private set; }

    public DataServiceCollection<Person> Persons { get; private set; }

    public DataServiceCollection<Assessment> Assessments { get; private set; }

    public DataServiceCollection<Threat> Threats { get; private set; }

    public bool HasConnection { get; private set; }

    public event EventHandler ContextChanged;

    public event EventHandler SaveCompleted;

    public void Save()
    {
      if (m_Context.ApplyingChanges)
      {
        return;
      }

      var result = m_Context.BeginSaveChanges(SaveChangesOptions.Batch, r =>
                                                                        {
                                                                          var dm = (DomainModelContext) r.AsyncState;
                                                                          dm.EndSaveChanges(r);
                                                                          RaiseEvent(SaveCompleted);
                                                                        }, m_Context);
    }

    private void Initialize()
    {
      m_Context = m_CreateContext();

      try
      {
        LoadSurveyTypes();
        LoadCategories();
        LoadGFactors();
        LoadQuestions();
        LoadAssessments();
        LoadActivities();
        LoadPersons();
        LoadThreats();
        LoadCompanies();
        LoadWorkplaces();
        LoadWorkplaceCategories();
      }
      catch (Exception)
      {
        HasConnection = false;
      }

      RaiseEvent(ContextChanged);
    }

    private void LoadWorkplaces()
    {
      Workplaces = new DataServiceCollection<Workplace>(m_Context);
      var query = m_Context.Workplaces.Expand("SurveyType")
                           .Expand("Activities")
                           .Expand("Assessments/AssessmentDate")
                           .Expand("Assessments/EvaluatingPerson")
                           .Expand("Assessments/Threats");
      Workplaces.Load(query);
    }

    private void LoadThreats()
    {
      Threats = new DataServiceCollection<Threat>(m_Context);
      var query = m_Context.Threats.Expand("GFactor")
                           .Expand("Pictures")
                           .Expand("Status")
                           .Expand("ProtectionGoals")
                           .Expand("GFactor/Dangerpoints")
                           .Expand("Actions")
                           .Expand("Dangerpoint")
                           .Expand("Actions/Person")
                           .Expand("Activity");
      Threats.Load(query);
    }

    private void LoadCompanies()
    {
      Companies = new DataServiceCollection<Company>(m_Context);
      var query = m_Context.Companies.Expand("Workplaces")
                           .Expand("Workplaces/SurveyType")
                           .Expand("Workplaces/Activities")
                           .Expand("Workplaces/Assessments");
      Companies.Load(query);
    }

    private void LoadQuestions()
    {
      Questions = new DataServiceCollection<Question>(m_Context);
      var query = m_Context.Questions;
      Questions.Load(query);
    }

    private void LoadGFactors()
    {
      GFactors = new DataServiceCollection<GFactor>(m_Context);
      var query = m_Context.GFactors.Expand("Questions")
                           .Expand("Dangerpoints");
      GFactors.Load(query);
    }

    private void LoadCategories()
    {
      Categories = new DataServiceCollection<Category>(m_Context);
      var query = m_Context.Categories.Expand("GFactors")
                           .Expand("GFactors/Questions")
                           .Expand("GFactors/Dangerpoints");
      Categories.Load(query);
    }

    private void LoadSurveyTypes()
    {
      SurveyTypes = new DataServiceCollection<SurveyType>(m_Context);
      var query = m_Context.SurveyTypes.Expand("SurveyTypeDocx/DocumentSource/BinarySource")
                           .Expand("Categories")
                           .Expand("Categories/GFactors")
                           .Expand("Categories/GFactors/Questions")
                           .Expand("Categories/GFactors/Dangerpoints");
      SurveyTypes.Load(query);
    }

    private void LoadWorkplaceCategories()
    {
      WorkplaceCategories = new DataServiceCollection<WorkplaceCategory>(m_Context);
      var query = m_Context.WorkplaceCategories.Expand("Workplace")
                           .Expand("Category");
      WorkplaceCategories.Load(query);
    }

    private void LoadActivities()
    {
      Activities = new DataServiceCollection<Activity>(m_Context);
      var query = m_Context.Activities;
      Activities.Load(query);
    }

    private void LoadPersons()
    {
      Persons = new DataServiceCollection<Person>(m_Context);
      var query = m_Context.People;
      Persons.Load(query);
    }

    private void LoadAssessments()
    {
      Assessments = new DataServiceCollection<Assessment>(m_Context);
      var query = m_Context.Assessments.Expand("AssessmentDate")
                           .Expand("EvaluatingPerson")
                           .Expand("Threats");
      Assessments.Load(query);
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