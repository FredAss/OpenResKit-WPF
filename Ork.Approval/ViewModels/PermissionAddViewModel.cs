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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Ork.Approval.DomainModelService;
using Ork.Framework;
using Ork.Framework.Utilities;
using File = System.IO.File;

namespace Ork.Approval.ViewModels
{
    public class PermissionAddViewModel : DocumentBase
    {
        private Permission m_Model;
        private readonly IEnumerable m_KindOfPermissions;
        private ObservableCollection<SelectedPlantViewModel> m_PlantViewModels;
        private string m_conditionName;
        private string m_SearchText;
        private IApprovalRepository m_Repository;

        public PermissionAddViewModel(Permission model, ObservableCollection<SelectedPlantViewModel> plantViewModels, IApprovalRepository repository)
        {
            m_Model = model;
            DisplayName = TranslationProvider.Translate("PermissionAdd");
            m_Repository = repository;

            m_KindOfPermissions = Enum.GetValues(typeof (KindOfPermission));

            if (m_Model.EndOfPermission == new DateTime() || m_Model.StartOfPermission == new DateTime())
            {
                m_Model.StartOfPermission = DateTime.Now;
                m_Model.EndOfPermission = DateTime.Now;
                m_Model.DateOfApplication = DateTime.Now;
                m_Model.InEffect = true;
            }

            m_PlantViewModels = plantViewModels;
        }

        public ObservableCollection<SelectedPlantViewModel> PlantViewModels
        {
            get { return FilterPlantsViewModelsBySearchText(); }
        }

        private ObservableCollection<SelectedPlantViewModel> FilterPlantsViewModelsBySearchText()
        {
            if (string.IsNullOrEmpty(m_SearchText))
                return m_PlantViewModels;

            var searchText = m_SearchText.ToLower();

            var filteredPlants = m_PlantViewModels.Where(p => (p.Name.ToLower().Contains(searchText)) || (p.Number.ToLower().Contains(searchText)) || (p.Position.ToLower().Contains(searchText)));

            return new ObservableCollection<SelectedPlantViewModel>(filteredPlants);
        }

        public Permission Model
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

        public string FileNumber
        {
            get { return m_Model.FileNumber; }
            set { m_Model.FileNumber = value; }
        }

        public int PermissionKind
        {
            get { return m_Model.PermissionKind; }
            set { m_Model.PermissionKind = value; }
        }

        public IEnumerable Permissions
        {
            get { return m_KindOfPermissions; }
        }

        public DateTime DateOfApplication
        {
            get { return m_Model.DateOfApplication; }
            set { m_Model.DateOfApplication = value; }
        }

        public DateTime StartPermission
        {
            get { return m_Model.StartOfPermission; }
            set { m_Model.StartOfPermission = value; }
        }

        public DateTime EndPermission
        {
            get { return m_Model.EndOfPermission; }
            set { m_Model.EndOfPermission = value; }
        }

        public bool InEffect
        {
            get { return m_Model.InEffect; }
            set { m_Model.InEffect = value; }
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

        public bool CanPermissionAdd
        {
            get { return m_PlantViewModels.Any(p => p.IsSelected); }
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set
            {
                if (m_SearchText == value)
                    return;

                m_SearchText = value;
                NotifyOfPropertyChange(() => PlantViewModels);
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

        public string Condition
        {
            get { return m_conditionName; }
            set
            {
                m_conditionName = value;
                NotifyOfPropertyChange(Condition);
            }
        }

        public void AddConditionToPermissionEvent(KeyEventArgs e, string text)
        {
            if (e.Key == Key.Return)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    AddConditionToPermission(text);
                    Condition = string.Empty;
                }            
            }
        }

        public void AddAuxillaryCondition(string text)
        {
            AddConditionToPermission(text);
            Condition = string.Empty;
        }

        private void AddConditionToPermission(string text)
        {
            var condition = new AuxillaryCondition();
            condition.Condition = text;
            condition.InEffect = true;

            m_Model.AuxillaryConditions.Add(condition);
            NotifyOfPropertyChange(() => AuxillaryConditionViewModels);
        }

        public void RemoveConditionFromPermission(Object dataContext)
        {
            var auvw = (AuxillaryConditionViewModel) dataContext;

            m_Model.AuxillaryConditions.Remove(auvw.Model);
            NotifyOfPropertyChange(() => AuxillaryConditionViewModels);
        }

        public ObservableCollection<AuxillaryConditionViewModel> AuxillaryConditionViewModels
        {
            get { return GetConditionsFromPermission(); }
        }

        private ObservableCollection<AuxillaryConditionViewModel> GetConditionsFromPermission()
        {
            var auxillaryConditionViewModels = new ObservableCollection<AuxillaryConditionViewModel>();

            foreach (var auxillaryCondition in m_Model.AuxillaryConditions)
            {
                if (m_Repository.AuxillaryConditions.Any(ac => ac == auxillaryCondition))
                {
                    var ac2 = m_Repository.AuxillaryConditions.Single(ac => ac == auxillaryCondition);
                    auxillaryConditionViewModels.Add(new AuxillaryConditionViewModel(auxillaryCondition) { IsDeletable = !ac2.ConditionInspections.Any() });
                }
                else
                {
                    auxillaryConditionViewModels.Add(new AuxillaryConditionViewModel(auxillaryCondition) { IsDeletable = true });
                } 
            }

            return auxillaryConditionViewModels;
        }

        public void NotifyCanPermissionAdd()
        {
            NotifyOfPropertyChange(() => CanPermissionAdd);
        }
    }
}
