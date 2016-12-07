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
using System.Linq;
using Ork.Danger.DomainModelService;
using Ork.Danger.ViewModels;
using Action = System.Action;

namespace Ork.Danger.Factories
{
  [Export(typeof (IDangerViewModelFactory))]
  public class DangerViewModelFactory : IDangerViewModelFactory
  {
    private readonly IDangerRepository m_Repository;
    private readonly ISurveyTypeViewModelFactory m_SurveyTypeViewModelFactory;

    [ImportingConstructor]
    public DangerViewModelFactory([Import] IDangerRepository repository, [Import] ISurveyTypeViewModelFactory surveytypeViewModelFactory)
    {
      m_Repository = repository;
      m_SurveyTypeViewModelFactory = surveytypeViewModelFactory;
    }

    public CompanyAddViewModel CreateCompanyAddViewModel()
    {
      var company = new Company();
      return new CompanyAddViewModel(company);
    }

    public CompanyEditViewModel CreateCompanyEditViewModel(Company company, Action removeCompanyAction)
    {
      return new CompanyEditViewModel(company, removeCompanyAction);
    }

    public WorkplaceAddViewModel CreateWorkplaceAddViewModel(Company company)
    {
      var workplace = new Workplace
                      {
/*EvaluatingPerson = new Person()*/
                      };
      return new WorkplaceAddViewModel(workplace, GetSurveytypsFromRepository(), company);
    }

    public WorkplaceEditViewModel CreateWorkplaceEditViewModel(Workplace workplace, Company company)
    {
      return new WorkplaceEditViewModel(workplace, GetSurveytypsFromRepository(), company);
    }

    public CompanyViewModel CreateCompanyViewModel(Company company)
    {
      return new CompanyViewModel(company);
    }

    public WorkplaceViewModel CreateWorkplaceViewModel(Workplace workplace)
    {
      return new WorkplaceViewModel(workplace);
    }

    public WorkplaceCategoryViewModel CreateWorkplaceCategoryViewModel(WorkplaceCategory workplaceCategory)
    {
      return new WorkplaceCategoryViewModel(workplaceCategory);
    }

    private SurveyTypeViewModel[] GetSurveytypsFromRepository()
    {
      return m_Repository.SurveyTypes.Select(m_SurveyTypeViewModelFactory.CreateSurveyTypeViewModelFromExisting)
                         .ToArray();

      //var surveytypes = new Collection<SurveyTypeViewModel>();

      //foreach (var surveytype in m_Repository.SurveyTypes)
      //{
      //    surveytypes.Add(m_SurveyTypeViewModelFactory.CreateSurveyTypeViewModelFromExisting(surveytype));
      //}

      //return surveytypes.ToArray();
    }


    //public SurveyTypeViewModelFactory CreateSurveyTypeViewModel(SurveyType surveytype)
    //{
    //    return new SurveyTypeViewModelFactory(surveytype);
    //}

    //public AssViewModel CreateAssessmentViewModel(Assessment assessment)
    //{
    //    return new AssViewModel(assessment);
    //}

    //public AssessmentAddViewModel CreateWorkplaceAddViewModel(Assessment assessment)
    //{
    //    var workplace = new Workplace { EvaluatingPerson = new Person() };
    //    return new AssessmentAddViewModel(assessment);
    //}

    //public AssessmentEditViewModel CreateWorkplaceEditViewModel(Assessment assessment)
    //{
    //    return new AssessmentEditViewModel(assessment);
    //}
  }
}