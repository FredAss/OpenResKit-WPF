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
using Caliburn.Micro;
using Ork.Approval.DomainModelService;

namespace Ork.Approval.ViewModels
{
    public class MeasureViewModel: Screen
    {
        private Approval_Measure m_Model;

        public MeasureViewModel(Approval_Measure model)
        {
            m_Model = model;
        }

        public Approval_Measure Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
        }

        public string Description
        {
            get { return m_Model.Description; }
        }

        public int Progress
        {
            get { return m_Model.Progress; }
        }

        public int Priority
        {
            get { return m_Model.Priority; }
        }

        public string ResponsibleSubject
        {
            get
            {
                if (m_Model.ResponsibleSubject.IsOfType<Employee>())
                {
                    var a = (Employee) m_Model.ResponsibleSubject;
                    return a.FirstName + " " + a.LastName;
                }
                else
                {
                    var a = (EmployeeGroup) m_Model.ResponsibleSubject;
                    return a.Name;
                }
                    
            }
        }

        public DateTime DueDate
        {
            get { return m_Model.DueDate; }
        }

        public DateTime? EntryDate
        {
            get { return m_Model.EntryDate; }
        }

        public DataServiceCollection<Document> AttachedDocuments
        {
            get { return m_Model.AttachedDocuments; }
        }
    }
}
