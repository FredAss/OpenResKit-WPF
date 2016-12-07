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
using System.Linq;
using Ork.Framework;
using Ork.Measure.DomainModelService;

namespace Ork.Measure.ViewModels
{
  public class MeasureViewModel
  {
    private readonly DomainModelService.Measure m_Model;
    private readonly IEnumerable<IRelatedElementProvider> m_RelatedElementProviders;

    public MeasureViewModel(DomainModelService.Measure measure, IEnumerable<IRelatedElementProvider> relatedElementProviders, CatalogViewModel catalog)
    {
      m_Model = measure;
      Catalog = catalog;
      m_RelatedElementProviders = relatedElementProviders;
    }

    public DateTime? CreationDate
    {
      get { return m_Model.CreationDate; }
    }

    public string CreationDateString
    {
      get { return m_Model.CreationDate.ToShortDateString(); }
    }

    public double EvaluationRating
    {
      get { return m_Model.EvaluationRating; }
    }

    public string Name
    {
      get { return m_Model.Name; }
    }

    public DomainModelService.Measure Model
    {
      get { return m_Model; }
    }

    public string Description
    {
      get { return m_Model.Description; }
    }

    public DateTime DueDate
    {
      get { return m_Model.DueDate; }
    }

    public string DueDateString
    {
      get { return m_Model.DueDate.ToShortDateString(); }
    }

    public DateTime? EntryDate
    {
      get { return m_Model.EntryDate; }
    }

    public string EntryDateString
    {
      get
      {
        if (!m_Model.EntryDate.HasValue)
        {
          return "";
        }

        return m_Model.EntryDate.GetValueOrDefault()
                      .ToShortDateString();
      }
    }

    public string Evaluation
    {
      get { return m_Model.Evaluation; }
    }

    public int Priority
    {
      get { return m_Model.Priority; }
    }

    public string PriorityName
    {
      get { return TranslationProvider.Translate(((Priority) Priority).ToString()); }
    }

    public int Status
    {
      get { return m_Model.Status; }
    }

    public string StatusName
    {
      get { return TranslationProvider.Translate(((Status) Status).ToString()); }
    }

    public ResponsibleSubject ResponsibleSubject
    {
      get { return m_Model.ResponsibleSubject; }
    }

    public string ResponsibleSubjectName
    {
      get
      {
        if (ResponsibleSubject is Employee)
        {
          var employee = (Employee) ResponsibleSubject;
          return employee.FirstName + " " + employee.LastName;
        }
        else
        {
          var group = (EmployeeGroup) ResponsibleSubject;
          return group.Name;
        }
      }
    }

    public CatalogViewModel Catalog { get; private set; }

    public string DueDateIsDelayed
    {
      get { return TranslationProvider.Translate("DueDateIsDelayed"); }
    }

    public string EntryDateIsDelayed
    {
      get { return TranslationProvider.Translate("EntryDateIsDelayed"); }
    }

    public bool Delayed
    {
      get
      {
        if (DueDate < DateTime.Today &&
            Status != 2)
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    public bool DelayedCompleted
    {
      get
      {
        if (EntryDate != null &&
            (EntryDate.Value.Date > DueDate.Date && Status == 2))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    public string RelatedElementName
    {
      get
      {
        var repvm = m_RelatedElementProviders.Select(r => r.GetViewModel(Model.Id))
                                             .SingleOrDefault(r => r.IsExpanded);
        return repvm != null
          ? repvm.Elements.Single(e => e.IsSelected)
                 .DisplayText
          : string.Empty;
      }
    }
  }
}