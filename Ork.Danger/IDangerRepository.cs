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
using System.Data.Services.Client;
using Ork.Danger.DomainModelService;

namespace Ork.Danger
{
  public interface IDangerRepository
  {
    DataServiceCollection<SurveyType> SurveyTypes { get; }
    DataServiceCollection<Category> Categories { get; }
    DataServiceCollection<GFactor> GFactors { get; }
    DataServiceCollection<Question> Questions { get; }
    DataServiceCollection<Company> Companies { get; }
    DataServiceCollection<Workplace> Workplaces { get; }
    DataServiceCollection<WorkplaceCategory> WorkplaceCategories { get; }
    DataServiceCollection<Activity> Activities { get; }
    DataServiceCollection<Threat> Threats { get; }
    DataServiceCollection<Person> Persons { get; }
    DataServiceCollection<Assessment> Assessments { get; }
    bool HasConnection { get; }
    event EventHandler ContextChanged;
    event EventHandler SaveCompleted;
    void Save();
    void ReloadSurveyTypes();
  }
}