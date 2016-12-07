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
using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;
using Ork.Framework;
using Ork.Framework.Utilities;
using File = System.IO.File;


namespace Ork.Approval.ViewModels
{
    public class MeasureAddViewModel : Screen
    {
        private Approval_Measure m_Model;
        private IEnumerable m_Progresses;
        private IEnumerable m_Priorities;
        private IEnumerable<ResponsibleSubjectViewModel> m_ResponsibleSubjectViewModels;
        private ResponsibleSubjectViewModel m_SelectedResponsibleSubjectViewModel;


        public MeasureAddViewModel(Approval_Measure model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjectViewModels )
        {
            DisplayName = TranslationProvider.Translate("MeasureAdd");

            m_Model = model;
            m_Progresses = Enum.GetValues(typeof (Progress));
            m_Priorities = Enum.GetValues(typeof (Priority));
            m_ResponsibleSubjectViewModels = responsibleSubjectViewModels;

            if (model.DueDate == new DateTime())
            {
                m_Model.DueDate = DateTime.Now;
                m_Model.EntryDate = DateTime.Now;
                m_Model.Progress = 0;
            }
        }

        public Approval_Measure Model
        {
            get { return m_Model; }
        }

        public string Name
        {
            get { return m_Model.Name; }
            set { m_Model.Name = value; }
        }

        public string Description
        {
            get { return m_Model.Description; }
            set { m_Model.Description = value; }
        }

        public int Progress
        {
            get { return m_Model.Progress; }
            set { m_Model.Progress = value; }
        }

        public int Priority
        {
            get { return m_Model.Priority; }
            set { m_Model.Priority = value; }
        }

        public ResponsibleSubjectViewModel SelectedResponsibleSubjectViewModel
        {
            get { return m_SelectedResponsibleSubjectViewModel; }
            set
            {
                m_SelectedResponsibleSubjectViewModel = value;
                m_Model.ResponsibleSubject = value.Model;
                NotifyOfPropertyChange(() => m_Model.ResponsibleSubject);
            }
        }

        public IEnumerable Progresses
        {
            get { return m_Progresses; }
        }

        public IEnumerable Priorities
        {
            get { return m_Priorities; }
        }

        public IEnumerable<ResponsibleSubjectViewModel> ResponsibleSubjectViewModels
        {
            get { return m_ResponsibleSubjectViewModels; }
        }
        
        public IEnumerable<Document> AttachedDocuments
        {
            get
            {
                if (m_Model.AttachedDocuments != null)
                {
                    return m_Model.AttachedDocuments;
                }
                return null;
            }
        }

        public void AddDocument()
        {
            var filename = string.Empty;
            var binarySourceDocument = FileHelpers.GetByeArrayFromUserSelectedFile(string.Empty, out filename);

            if (binarySourceDocument != null)
            {
                m_Model.AttachedDocuments.Add(new Document()
                {
                    DocumentSource = new DocumentSource()
                    {
                        BinarySource = binarySourceDocument
                    },
                    Name = filename
                });

                NotifyOfPropertyChange(() => AttachedDocuments);
            }
        }

        public void OpenDocument(Document context)
        {
            File.WriteAllBytes(Path.GetTempPath() + context.Name, context.DocumentSource.BinarySource);

            Process.Start(Path.GetTempPath() + context.Name);
        }

        public void DeleteDocument(Document context)
        {
            m_Model.AttachedDocuments.Remove(context);
            NotifyOfPropertyChange(() => AttachedDocuments);
        }
    }
}
