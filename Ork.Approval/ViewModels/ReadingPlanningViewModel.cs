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
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Data.Services.Client;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;
using Ork.Approval.Factories;
using Ork.Calendar.Models;
using Ork.Framework;
using DayOfWeek = System.DayOfWeek;

namespace Ork.Approval.ViewModels
{
    [Export(typeof(IWorkspace))]
    internal class ReadingPlanningViewModel : DocumentBase, IWorkspace
    {
        private IApprovalRepository m_Repository;
        private IReadingPlanningViewModelFactory m_ReadingPlanningViewModelFactory;
        private IInspectionViewModelFactory m_InspectionViewModelFactory;
        private IScreen m_EditItem;
        private BindableCollection<InspectionViewModel> m_InspectionViewModels = new BindableCollection<InspectionViewModel>();
        private InspectionViewModel m_SelectedInspectionViewModel;
        private string m_SearchText;
        private bool m_FlyoutActivated;


        [ImportingConstructor]
        public ReadingPlanningViewModel([Import] IApprovalRepository repository, [Import] IReadingPlanningViewModelFactory readingPlanningViewModelFactory, IInspectionViewModelFactory inspectionViewModelFactory)
        {
            m_Repository = repository;
            m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(LoadData);
            m_ReadingPlanningViewModelFactory = readingPlanningViewModelFactory;
            m_InspectionViewModelFactory = inspectionViewModelFactory;

            LoadData();

            FlyoutActivated = true;
        }

        public int Index
        {
            get { return 3; }
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public string Title
        {
            get { return TranslationProvider.Translate("Planning"); }
        }

        public bool FlyoutActivated
        {
            get { return m_FlyoutActivated; }
            set
            {
                if (m_FlyoutActivated == value)
                {
                    return;
                }
                m_FlyoutActivated = value;
                NotifyOfPropertyChange(() => FlyoutActivated);
            }
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set
            {
                if(m_SearchText == value)
                    return;

                m_SearchText = value;
                NotifyOfPropertyChange(() => FilterInspectionViewModels);
            }
        }

        public bool IsEnabledIfResponsibleSubjectExists
        {
            get
            {
                if(m_Repository.ResponsibleSubjects != null)
                    return m_Repository.ResponsibleSubjects.Any();

                return false;
            }
        }

        public IEnumerable<InspectionViewModel> FilterInspectionViewModels
        {
            get { return FilterInspectionViewModelsBySearchText(); }
        }

        private IEnumerable<InspectionViewModel> FilterInspectionViewModelsBySearchText()
        {
            if (string.IsNullOrEmpty(m_SearchText))
                return m_InspectionViewModels;

            var searchText = m_SearchText.ToLower();

            var searchResult =
                m_InspectionViewModels.Where(
                    ivm => ((ivm.RelatedSeriesName != null && ivm.RelatedSeriesName.ToLower().Contains(searchText))));

            return searchResult;
        }

        public BindableCollection<InspectionViewModel> InspectionViewModels
        {
            get { return m_InspectionViewModels; }
        }

        public InspectionViewModel SelectedInspectionViewModel
        {
            get { return m_SelectedInspectionViewModel; }
            set
            {
                if(m_SelectedInspectionViewModel == value)
                    return;

                m_SelectedInspectionViewModel = value;
                NotifyOfPropertyChange(() => SelectedInspectionViewModel);
            }
        }

        public IEnumerable<CalendarEntry> CalendarEntries
        {
            get { return m_InspectionViewModels.Select(ivm => ivm.CalendarEntry); }
        }

        public void Accept()
        {
            Save();
            Cancel();
        }

        private void Save()
        {
            if (m_Repository.Entities.Where(ed => ed.Entity is Approval_Inspection || ed.Entity is Series || ed.Entity is Appointment || ed.Entity is DayOfWeek || ed.Entity is SeriesColor)
                            .Any(ed => ed.State != EntityStates.Unchanged) ||
                m_Repository.Links.Where(l => l.Source is Approval_Inspection || l.Source is Series || l.Source is Appointment || l.Source is DayOfWeek || l.Source is SeriesColor)
                            .Any(ed => ed.State != EntityStates.Unchanged))
            {
                m_Repository.Save();
            }
        }

        private void Reload()
        {
            m_InspectionViewModels = null;
            LoadData();
        }

        private void LoadData()
        {
            LoadInspections();
        }

        private void LoadInspections()
        {
            if (m_Repository.Inspections != null)
            {
                m_Repository.Inspections.CollectionChanged += AlterInspectionCollection;

                foreach (var inspection in m_Repository.Inspections.OfType<Approval_Inspection>())
                {
                    m_InspectionViewModels.Add(CreateInspectionViewModel(inspection));
                }
            }
        }

        private InspectionViewModel CreateInspectionViewModel(Approval_Inspection inspection)
        {
            var ivm = m_InspectionViewModelFactory.CreateInspectionViewModel(inspection);

            var now = DateTime.Now;
            var dueDate = ivm.Inspection.DueDate.Begin;

            if (dueDate.Date >= now.GetFirstDayOfWeek() && dueDate.Date <= now.GetLastDayOfWeek() &&
                dueDate >= now)
            {
                //diese Woche
                ivm.DateIndicator = 0;
            }
            else if (dueDate < now)
            {
                if (dueDate.Date == now.Date &&
                    ivm.Inspection.DueDate.IsAllDay)
                {
                    //ganztägig und heute = diese Woche (duedate-zeit ignorieren)
                    ivm.DateIndicator = 0;
                }
                else
                {
                    //abgelaufen
                    ivm.DateIndicator = 1;
                }
            }
            else
            {
                //alle anderen
                ivm.DateIndicator = 2;
            }

            return ivm;
        }

        private void OpenEditor(object dataContext)
        {
            m_EditItem = (IScreen) dataContext;
            Dialogs.ShowDialog(m_EditItem);
        }

        public void OpenSeriesAddDialog()
        {
            OpenEditor(m_ReadingPlanningViewModelFactory.CreateSeriesAddViewModel());
        }

        public void OpenSeriesEditDialog(object dataContext, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if(dataContext is InspectionViewModel)
                    OpenEditor(m_ReadingPlanningViewModelFactory.CreateSeriesEditViewModel((InspectionViewModel)dataContext, m_InspectionViewModels));
            }
        }

        public void OpenSeriesEditDialog(object dataContext)
        {
            if (dataContext is InspectionViewModel)
                OpenEditor(m_ReadingPlanningViewModelFactory.CreateSeriesEditViewModel((InspectionViewModel)dataContext, m_InspectionViewModels));
        }

        public void Cancel()
        {
            m_EditItem.TryClose();
        }

        private void AlterInspectionCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems.OfType<Approval_Inspection>())
                {
                    m_InspectionViewModels.Add(CreateInspectionViewModel(newItem));
                }
            }
            else
            {
                foreach (var oldItem in e.OldItems.OfType<Approval_Inspection>())
                {
                    var inspectionViewModel = m_InspectionViewModels.Single(i => i.Inspection == oldItem);
                    m_InspectionViewModels.Remove(inspectionViewModel);
                }
            }

            NotifyOfPropertyChange(() => FilterInspectionViewModels);
            NotifyOfPropertyChange(() => CalendarEntries);
        }
    }
}
