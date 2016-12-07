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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ork.Framework;

namespace Ork.Measure.ViewModels
{
  public class MeasureEditViewModel : MeasureAddViewModel
  {
    private readonly Action m_RemoveMeasure;
    private readonly IEnumerable m_Stati;

    public MeasureEditViewModel(DomainModelService.Measure model, Action removeMeasureAction, ResponsibleSubjectViewModel[] responsibleSubjectViewModels,
      IRelatedElementProvider[] relatedElementProvider)
      : base(model, responsibleSubjectViewModels, relatedElementProvider)
    {
      DisplayName = TranslationProvider.Translate("TitleMeasureEditViewModel");
      m_Stati = Enum.GetValues(typeof (Status));
      m_RemoveMeasure = removeMeasureAction;
      SelectedResponsibleSubject = responsibleSubjectViewModels.Single(rsvm => model.ResponsibleSubject == rsvm.Model);
    }

    public override IEnumerable<RelatedElementProviderViewModel> RelatedElementProviders
    {
      get
      {
        if (m_RelatedElementProviderViewModels == null)
        {
          m_RelatedElementProviderViewModels = m_RelatedElementProviders.Select(r => r.GetViewModel(Model.Id))
                                                                        .ToArray();
          foreach (var relatedElementProviderViewModel in m_RelatedElementProviderViewModels)
          {
            relatedElementProviderViewModel.PropertyChanged += RelatedElementViewModelIsSelected;
          }
        }
        return m_RelatedElementProviderViewModels;
      }
    }

    public double EvaluationValue
    {
      get { return Model.EvaluationRating; }
      set
      {
        Model.EvaluationRating = value;
        NotifyOfPropertyChange(() => EvaluationValue);
      }
    }

    public string Evaluation
    {
      get { return Model.Evaluation; }
      set { Model.Evaluation = value; }
    }

    public DateTime? EntryDate
    {
      get { return Model.EntryDate; }
      set
      {
        Model.EntryDate = value.Value;
        NotifyOfPropertyChange(() => CanMeasureAdd);
      }
    }

    public int Status
    {
      get { return Model.Status; }
      set { Model.Status = value; }
    }

    public IEnumerable Stati
    {
      get { return m_Stati; }
    }

    public void RemoveMeasures()
    {
      foreach (var relatedElementProvider in m_RelatedElementProviders)
      {
        relatedElementProvider.DeleteRelatedElement(Model.Id);
      }
      m_RemoveMeasure();
    }
  }
}