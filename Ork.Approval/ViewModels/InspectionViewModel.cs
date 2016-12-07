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
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;
using Ork.Calendar.Models;
using Ork.Framework;

namespace Ork.Approval.ViewModels
{
    public class InspectionViewModel : Screen
    {
        private Approval_Inspection m_Model;
        private SeriesViewModel m_RelatedSeriesViewModel;
        private CalendarEntry m_CalendarEntry;
        private ResponsibleSubject m_ResponsibleSubject;
        private int m_DateIndicator;
        private Brush m_SeriesColor;
        private IEnumerable<Plant> m_Plants; 

        public InspectionViewModel(Approval_Inspection model, SeriesViewModel seriesViewModel, IEnumerable<Plant> plants)
        {
            m_Model = model;
            m_RelatedSeriesViewModel = seriesViewModel;
            m_ResponsibleSubject = model.AppointmentResponsibleSubject;
            m_Model.PropertyChanged += OnModelPropertyChanged;
            m_Model.RelatedSeries.SeriesColor.PropertyChanged += OnSeriesColorPropertyChanged;
            m_Model.RelatedSeries.PropertyChanged += OnRelatedSeriesPropertyChanged;
            m_Model.DueDate.PropertyChanged += OnDueDatePropertyChanged;
            var color = Color.FromRgb(m_Model.RelatedSeries.SeriesColor.R, m_Model.RelatedSeries.SeriesColor.G, m_Model.RelatedSeries.SeriesColor.B);
            SeriesColor = new SolidColorBrush(color);
            m_CalendarEntry = new CalendarEntry(m_Model.DueDate.Begin, m_Model.DueDate.End, null, color, m_Model.DueDate.IsAllDay, this);
            m_Plants = plants;
        }

        public bool AllConditionInspectionsCorrect
        {
            get { return m_Model.ConditionInspections.All(ci => ci.Status); }
        }

        public ResponsibleSubject AppointmentResponsibleSubject
        {
            get { return m_ResponsibleSubject; }
            set
            {
                if (m_ResponsibleSubject == value)
                {
                    return;
                }
                m_ResponsibleSubject = value;
                NotifyOfPropertyChange(() => AppointmentResponsibleSubject);
                NotifyOfPropertyChange(() => ResponsibleSubject);
            }
        }

        public string ResponsibleSubject
        {
            get
            {
                if (m_ResponsibleSubject is Employee)
                {
                    var employee = (Employee)m_ResponsibleSubject;
                    return employee.FirstName + " " + employee.LastName;
                }
                if (m_ResponsibleSubject is EmployeeGroup)
                {
                    var employeeGroup = (EmployeeGroup)m_ResponsibleSubject;
                    return employeeGroup.Name;
                }
                return TranslationProvider.Translate("NotAvailable");
            }
        }

        public Approval_Inspection Inspection
        {
            get { return m_Model; }
        }

        public CalendarEntry CalendarEntry
        {
            get { return m_CalendarEntry; }
        }

        public Brush SeriesColor
        {
            get { return m_SeriesColor; }
            set
            {
                if (m_SeriesColor == value)
                {
                    return;
                }
                m_SeriesColor = value;
                NotifyOfPropertyChange(() => SeriesColor);
            }
        }

        public DateTime OrderDate
        {
            get { return m_Model.DueDate.Begin; }
        }

        public SeriesViewModel RelatedSeriesViewModel
        {
            get { return m_RelatedSeriesViewModel; }
        }

        public int DateIndicator
        {
            get { return m_DateIndicator; }
            set
            {
                if (value.Equals(m_DateIndicator))
                {
                    return;
                }
                m_DateIndicator = value;
                NotifyOfPropertyChange(() => DateIndicator);
            }
        }

        public bool IsAllDay
        {
            get { return m_Model.DueDate.IsAllDay; }
        }

        public Series RelatedSeries
        {
            get { return m_Model.RelatedSeries; }
        }

        private void OnSeriesColorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var color = Color.FromRgb(m_Model.RelatedSeries.SeriesColor.R, m_Model.RelatedSeries.SeriesColor.G, m_Model.RelatedSeries.SeriesColor.B);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            SeriesColor = brush;
            m_CalendarEntry.Color = SeriesColor;
        }

        public string RelatedSeriesName
        {
            get { return m_Model.RelatedSeries.Name; }
        }

        public string DueDate
        {
            get { return m_Model.DueDate.Begin.ToString(TranslationProvider.Translate("DateTimeForShortPlanning")); }
        }

        public string DueDateShort
        {
            get { return m_Model.DueDate.Begin.ToString("ddd, ") + m_Model.DueDate.Begin.ToShortDateString(); }
        }

        public string DueDateTimeRange
        {
            get { return string.Format("{0} - {1}", m_Model.DueDate.Begin.ToString("H:mm"), m_Model.DueDate.End.ToString("H:mm")); }
        }

        private void OnDueDatePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => CalendarEntry);
            NotifyOfPropertyChange(() => DueDate);
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(e.PropertyName);
            if (e.PropertyName == "RelatedSeries")
            {
                NotifyOfPropertyChange(() => RelatedSeriesName);
            }
        }

        private void OnRelatedSeriesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                NotifyOfPropertyChange(() => RelatedSeriesName);
            }
        }

        public string ConditionCount
        {
            get { return m_Model.ConditionInspections.Count + " " + TranslationProvider.Translate("AuxillaryCondition"); }
        }

        public string ConcernedPlants
        {
            get
            {
                string a = "";
                foreach (var plant in m_Plants)
                {
                    a += plant.Name + " (" + plant.Number + ")\n";
                }

                return a;
            }
        }

    }
}
