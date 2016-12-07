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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;
using Ork.Approval.Factories;
using Ork.Framework;
using DayOfWeek = System.DayOfWeek;


namespace Ork.Approval.ViewModels
{
    public class SeriesAddViewModel : Screen
    {
        protected Series m_Model;
        private ObservableCollection<SelectedAuxillaryConditionViewModel> m_SelectableAuxillaryConditionViewModels = new ObservableCollection<SelectedAuxillaryConditionViewModel>();
        private IApprovalRepository m_Repository;
        private IResponsibleSubjectViewModelFactory m_ResponsibleSubjectViewModelFactory;
        protected ObservableCollection<ResponsibleSubjectViewModel> m_ResponsibleSubjectViewModels = new ObservableCollection<ResponsibleSubjectViewModel>();
        private ITaskGenerator m_TaskGenerator;
        private ResponsibleSubjectViewModel m_SelectedResponsibleSubject;
        private IList<SelectableWeekdayViewModel> m_WeekDayList;
        private int m_sicb;
        private string m_SearchTextAuxillaryCondition;
        private string m_SearchTextResponsibleSubject;



        public SeriesAddViewModel(Series model, IApprovalRepository repository, IPlantManagementViewModelFactory plantManagementViewModelFactory, IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory, ITaskGenerator taskGenerator)
        {
            DisplayName = TranslationProvider.Translate("InspectionAdd");
            m_Model = model;
            m_Repository = repository;
            m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(LoadData);
            m_ResponsibleSubjectViewModelFactory = responsibleSubjectViewModelFactory;
            m_TaskGenerator = taskGenerator;

            LoadData();
        }

        public string Name
        {
            get { return m_Model.Name; }
            set
            {
                m_Model.Name = value; 
                NotifyOfPropertyChange(() => GenerateEnabled);
            }
        }

        public DateTime BeginDate
        {
            get { return DueDateBegin; }
            set
            {
                DueDateBegin = new DateTime(value.Year, value.Month, value.Day, DueDateBegin.Hour, DueDateBegin.Minute, DueDateBegin.Second);
                NotifyOfPropertyChange(() => DueDateBegin);
                NotifyOfPropertyChange(() => DueDateEnd);
            }
        }

        public Color Color
        {
            get
            {
                var col = new Color();
                col.A = 255;
                col.R = m_Model.SeriesColor.R;
                col.G = m_Model.SeriesColor.G;
                col.B = m_Model.SeriesColor.B;
                return col;
            }
            set
            {
                m_Model.SeriesColor.R = value.R;
                m_Model.SeriesColor.G = value.G;
                m_Model.SeriesColor.B = value.B;
            }
        }

        public DateTime BeginTime
        {
            get { return DueDateBegin; }
            set { DueDateBegin = new DateTime(DueDateBegin.Year, DueDateBegin.Month, DueDateBegin.Day, value.Hour, value.Minute, value.Second); }
        }

        public DateTime EndDate
        {
            get { return DueDateEnd; }
            set
            {
                DueDateEnd = new DateTime(value.Year, value.Month, value.Day, DueDateBegin.Hour, DueDateBegin.Minute, DueDateBegin.Second);
                NotifyOfPropertyChange(() => DueDateBegin);
                NotifyOfPropertyChange(() => DueDateEnd);
            }
        }

        public DateTime EndTime
        {
            get { return DueDateEnd; }
            set { DueDateEnd = new DateTime(DueDateBegin.Year, DueDateBegin.Month, DueDateBegin.Day, value.Hour, value.Minute, value.Second); }
        }

        public bool IsAllDay
        {
            get { return m_Model.IsAllDay; }
            set
            {
                m_Model.IsAllDay = value;
                NotifyOfPropertyChange(() => IsAllDay);
            }
        }

        public DateTime DueDateBegin
        {
            get { return m_Model.Begin; }
            set
            {
                var range = m_Model.End.Subtract(m_Model.Begin);
                m_Model.Begin = value;
                m_Model.End = m_Model.Begin.Add(range);
                NotifyOfPropertyChange(() => DueDateBegin);
                NotifyOfPropertyChange(() => SeriesBeginDate);
                NotifyOfPropertyChange(() => RepeatUntilDate);
                NotifyOfPropertyChange(() => EndDate);
                NotifyOfPropertyChange(() => EndTime);
                NotifyOfPropertyChange(() => BeginDate);
                NotifyOfPropertyChange(() => BeginTime);
                NotifyOfPropertyChange(() => DueDateEnd);
                NotifyOfPropertyChange(() => GenerateEnabled);
                GetRepeatUntilDateValue();
            }
        }

        public DateTime DueDateEnd
        {
            get { return m_Model.End; }
            set
            {
                m_Model.End = value;
                if (m_Model.Begin >= m_Model.End)
                {
                    m_Model.Begin = m_Model.End.Subtract(new TimeSpan(0, 30, 0));
                }
                NotifyOfPropertyChange(() => EndDate);
                NotifyOfPropertyChange(() => EndTime);
                NotifyOfPropertyChange(() => BeginDate);
                NotifyOfPropertyChange(() => BeginTime);
                NotifyOfPropertyChange(() => DueDateEnd);
                NotifyOfPropertyChange(() => DueDateBegin);
                NotifyOfPropertyChange(() => GenerateEnabled);
            }
        }

        public virtual bool IsEditable
        {
            get { return true; }
        }

        private void GetRepeatUntilDateValue()
        {
            switch (Cycle)
            {
                case 0:
                    RepeatUntilDate = DueDateBegin.AddDays(28);
                    break;
                case 1:
                    RepeatUntilDate = DueDateBegin.AddWeeks(12);
                    break;
                case 2:
                    RepeatUntilDate = DueDateBegin.AddMonths(12);
                    break;
                case 3:
                    RepeatUntilDate = DueDateBegin.AddYears(5);
                    break;
                default:
                    RepeatUntilDate = DueDateBegin;
                    break;
            }
        }

        public int[] Days
        {
            get
            {
                return new[]
               {
                 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30
               };
            }
        }

        public int RecurrenceInterval
        {
            get { return m_Model.RecurrenceInterval; }
            set
            {
                m_Model.RecurrenceInterval = value;
                NotifyOfPropertyChange(() => RecurrenceInterval);
            }
        }

        public int Cycle
        {
            get { return m_Model.Cycle; }
            set
            {
                m_Model.Cycle = value;
                NotifyOfPropertyChange(() => Cycle);
                NotifyOfPropertyChange(() => CycleDescription);
                if (value == 1 &&
                    !WeekDayList.Any(wd => wd.IsSelected))
                {
                    PreSelectCurrentWeekDay();
                }
                GetRepeatUntilDateValue();
            }
        }

        public void PreSelectCurrentWeekDay()
        {
            var currentWeekDay = DateTime.Now.DayOfWeek;
            foreach (var swvm in WeekDayList)
            {
                swvm.IsSelected = swvm.DayOfWeek == currentWeekDay;
            }
        }

        public void GenerateReadings()
        {
            if (Repeat && Cycle == 1)
            {
                foreach (var swvm in WeekDayList.Where(wd => wd.IsSelected))
                {
                    var dayOfWeek = new DomainModelService.DayOfWeek();
                    dayOfWeek.WeekDay = (int)swvm.DayOfWeek;
                    m_Model.WeekDays.Add(dayOfWeek);
                }
            }

            var selectedConditionInspections = new ObservableCollection<ConditionInspection>();

            foreach (var scvm in m_SelectableAuxillaryConditionViewModels.Where(scvm => scvm.IsSelected))
            {
                var ciModel = new ConditionInspection();
                ciModel.AuxillaryConditionId = scvm.Model.Id;
                selectedConditionInspections.Add(ciModel);

                //SaveInAuxillaryCondition(ciModel, scvm.Model);
            }

             var result = m_TaskGenerator.GenerateInspections(selectedConditionInspections, SelectedResponsibleSubject.Model, m_Model, WeekDayList.Where(wd => wd.IsSelected)
                                                                                                                                                  .Select(wd => wd.DayOfWeek)
                                                                                                                                                  .ToArray());

             foreach (var reading in result)
             {
                 foreach (var conditionInspection in reading.ConditionInspections)
                 {
                     m_Repository.ConditionInspections.Add(conditionInspection);
                     
                 }

                 m_Repository.Inspections.Add(reading);
             }
            
            m_Repository.Save();
            TryClose();
        }

        //private void SaveInAuxillaryCondition(ConditionInspection condtionInspection, AuxillaryCondition auxillaryCondition)
        //{
        //    var auxillaryConditionToSave = m_Repository.AuxillaryConditions.Single(ac => ac == auxillaryCondition);
        //    auxillaryConditionToSave.ConditionInspections.Add(condtionInspection);
        //}

        public IEnumerable<SelectableWeekdayViewModel> WeekDayList
        {
            get
            {
                if (m_WeekDayList == null)
                {
                    m_WeekDayList = new List<SelectableWeekdayViewModel>();
                    foreach (var weekDay in new[]
                                  {
                                    DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
                                  })
                    {
                        var swvm = new SelectableWeekdayViewModel(weekDay, false);
                        swvm.PropertyChanged += WeekDaySelectionChanged;
                        m_WeekDayList.Add(swvm);
                    }
                }
                return m_WeekDayList;
            }
        }

        private void WeekDaySelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                NotifyOfPropertyChange(() => GenerateEnabled);
            }
        }

        public string CycleDescription
        {
            get
            {
                switch ((int)Cycle)
                {
                    case 0:
                        return TranslationProvider.Translate("Days");
                        break;
                    case 1:
                        return TranslationProvider.Translate("Weeks");
                        break;
                    case 2:
                        return TranslationProvider.Translate("Months");
                        break;
                    default:
                        return TranslationProvider.Translate("Years");
                        break;
                }
            }
        }

        public bool IsWeekdayRecurrence
        {
            get { return m_Model.IsWeekdayRecurrence; }
            set
            {
                m_Model.IsWeekdayRecurrence = value;
                NotifyOfPropertyChange(() => IsWeekdayRecurrence);
            }
        }

        public bool EndsWithDate
        {
            get { return m_Model.EndsWithDate; }
            set
            {
                m_Model.EndsWithDate = value;
                NotifyOfPropertyChange(() => EndsWithDate);
            }
        }

        public int NumberOfRecurrences
        {
            get { return m_Model.NumberOfRecurrences; }
            set
            {
                if (m_Model.NumberOfRecurrences == value)
                {
                    return;
                }
                m_Model.NumberOfRecurrences = value;
                NotifyOfPropertyChange(() => NumberOfRecurrences);
            }
        }

        public bool Repeat
        {
            get { return m_Model.Repeat; }
            set
            {
                if (m_Model.Repeat == value)
                {
                    return;
                }
                m_Model.Repeat = value;
                NotifyOfPropertyChange(() => Repeat);
            }
        }

        public DateTime RepeatUntilDate
        {
            get { return m_Model.RepeatUntilDate; }
            set
            {
                m_Model.RepeatUntilDate = value;
                NotifyOfPropertyChange(() => RepeatUntilDate);
            }
        }

        public bool GenerateEnabled
        {
            get
            {
                if (Name == null)
                    return false;

                return (!Name.Equals("") && SelectedResponsibleSubject != null && FilteredSelectedAuxillaryConditionViewModels.Any(c => c.IsSelected) && (DueDateEnd > DueDateBegin) && (WeekDayList.Any(wd => wd.IsSelected) || Cycle != 1));
            }
        }

        public string SeriesBeginDate
        {
            get { return m_Model.Begin.ToString("ddddd, d MMMM yyyy"); }
        }

        public string SearchTextAuxillaryCondition
        {
            get { return m_SearchTextAuxillaryCondition; }
            set
            {
                if(m_SearchTextAuxillaryCondition == value)
                    return;

                m_SearchTextAuxillaryCondition = value;
                NotifyOfPropertyChange(() => FilteredSelectedAuxillaryConditionViewModels);
            }
        }

        public string SearchTextResponsibleSubject
        {
            get { return m_SearchTextResponsibleSubject; }
            set
            {
                if(m_SearchTextResponsibleSubject == value)
                    return;

                m_SearchTextResponsibleSubject = value;
                NotifyOfPropertyChange(() => FilteredResponsibleSubjectViewModels);
            }
        }

        public virtual ResponsibleSubjectViewModel SelectedResponsibleSubject
        {
            get { return m_SelectedResponsibleSubject; }
            set
            {
                if (m_SelectedResponsibleSubject == value)
                {
                    return;
                }
                m_SelectedResponsibleSubject = value;
                NotifyOfPropertyChange(() => SelectedResponsibleSubject);
                NotifyOfPropertyChange(() => GenerateEnabled);
            }
        }

        public ObservableCollection<SelectedAuxillaryConditionViewModel> FilteredSelectedAuxillaryConditionViewModels
        {
            get { return FilterSelectedAuxillaryConditionViewModelsBySearchText(); }
        }

        public IEnumerable<object> FilteredResponsibleSubjectViewModels
        {
            get { return FilterResponsibleSubjectViewModelsBySearchText(); }
        }

        public ObservableCollection<SelectedAuxillaryConditionViewModel> FilterSelectedAuxillaryConditionViewModelsBySearchText()
        {
            if (string.IsNullOrEmpty(m_SearchTextAuxillaryCondition))
                return new ObservableCollection<SelectedAuxillaryConditionViewModel>(m_SelectableAuxillaryConditionViewModels.Where(sacvm => sacvm.InEffect));

            var searchText = m_SearchTextAuxillaryCondition.ToLower();

            var searchResult = m_SelectableAuxillaryConditionViewModels.Where(sacvm => sacvm.Condition.ToLower().Contains(searchText) && (sacvm.InEffect));
            return new ObservableCollection<SelectedAuxillaryConditionViewModel>(searchResult);
        }

        private IEnumerable<object> FilterResponsibleSubjectViewModelsBySearchText()
        {
            if (string.IsNullOrEmpty(m_SearchTextResponsibleSubject))
                return m_ResponsibleSubjectViewModels;

            var searchText = m_SearchTextResponsibleSubject.ToLower();

            var searchResultEmplees =
                m_ResponsibleSubjectViewModels.OfType<EmployeeViewModel>()
                    .Where(
                        evm =>
                            (((evm.FirstName != null) && (evm.FirstName.ToLower().Contains(searchText))) ||
                             ((evm.LastName != null) && (evm.LastName.ToLower().Contains(searchText))) ||
                             ((evm.Number != null) && (evm.Number.ToLower().Contains(searchText)))));

            var searchResultGroups =
                m_ResponsibleSubjectViewModels.OfType<GroupViewModel>()
                    .Where(gvm => (((gvm.Name != null) && (gvm.Name.ToLower().Contains(searchText)))));

            return searchResultEmplees.Concat<object>(searchResultGroups);
        }

        private void LoadData()
        {
            LoadSelectedAuxillaryConditionViewModels();
            LoadResponsibleSubjectViewModels();
        }

        private void LoadSelectedAuxillaryConditionViewModels()
        {
            foreach (var auxillaryCondition in m_Repository.AuxillaryConditions)
            {
                m_SelectableAuxillaryConditionViewModels.Add(new SelectedAuxillaryConditionViewModel(auxillaryCondition));
            }
        }

        private void LoadResponsibleSubjectViewModels()
        {
            foreach (var responsibleSubject in m_Repository.ResponsibleSubjects)
            {
                if(responsibleSubject.IsOfType<Employee>())
                    m_ResponsibleSubjectViewModels.Add(m_ResponsibleSubjectViewModelFactory.CreateEmployeeViewModel((Employee)responsibleSubject));
                else
                    m_ResponsibleSubjectViewModels.Add(m_ResponsibleSubjectViewModelFactory.CreateGroupViewModel((EmployeeGroup)responsibleSubject));
            }
        }

        public void NotifyCanInspectionAdd()
        {
            NotifyOfPropertyChange(() => GenerateEnabled);
        }
    }
}
