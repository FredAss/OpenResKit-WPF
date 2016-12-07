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

using System.Collections.Generic;
using System.Linq;
using Ork.Approval.DomainModelService;
using Ork.Approval.Factories;
using Ork.Framework;

namespace Ork.Approval.ViewModels
{
    public class SeriesEditViewModel : SeriesAddViewModel
    {
        private readonly IEnumerable<Approval_Inspection> m_AllInspections;
        private readonly IApprovalRepository m_Repository;
        private readonly Approval_Inspection m_SelectedInspection;

        public SeriesEditViewModel(Series model, IApprovalRepository repository, IPlantManagementViewModelFactory plantManagementViewModelFactory, IResponsibleSubjectViewModelFactory responsibleSubjectViewModelFactory, ITaskGenerator taskGenerator, Approval_Inspection selectedInspection, IEnumerable<Approval_Inspection> allInspections) : base(model, repository, plantManagementViewModelFactory, responsibleSubjectViewModelFactory, taskGenerator)
        {
            m_Repository = repository;
            m_SelectedInspection = selectedInspection;
            m_AllInspections = allInspections;

            DisplayName = TranslationProvider.Translate("InspectionEdit");

            SelectResponsibleSubject();
            SelectWeekDays();
            SelectAuxillaryConditions();
        }

        public virtual bool IsEditable
        {
            get { return false; }
        }

        public override ResponsibleSubjectViewModel SelectedResponsibleSubject
        {
            get { return m_ResponsibleSubjectViewModels.Single(rs => rs.Model == m_SelectedInspection.AppointmentResponsibleSubject); }
            set
            {
                if (m_SelectedInspection.AppointmentResponsibleSubject == value.Model)
                {
                    return;
                }
                var editMeterReadingResponsibleSubject = m_AllInspections.Where(flr => flr.RelatedSeries == m_SelectedInspection.RelatedSeries);
                foreach (var currentMeterReading in editMeterReadingResponsibleSubject)
                {
                    currentMeterReading.AppointmentResponsibleSubject = value.Model;
                }
            }
        }

        private void SelectResponsibleSubject()
        {
            SelectedResponsibleSubject = m_ResponsibleSubjectViewModels.Single(rs => rs.Model == m_SelectedInspection.AppointmentResponsibleSubject);
        }

        private void SelectWeekDays()
        {
            if (!Repeat ||
                Cycle != 1)
            {
                return;
            }
            foreach (var selectableWeekdayViewModel in m_Model.WeekDays.SelectMany(weekDay => WeekDayList.Where(selectableWeekdayViewModel => selectableWeekdayViewModel.DayOfWeek == weekDay.GetAsDayOfWeek())))
            {
                selectableWeekdayViewModel.IsSelected = true;
            }
        }

       private void SelectAuxillaryConditions()
        {
            var selectedAuxillaryConditions =
                m_SelectedInspection.ConditionInspections.SelectMany(
                    ci => m_Repository.AuxillaryConditions.Where(ac => ac.Id == ci.AuxillaryConditionId));

            foreach (var filteredSelectedAuxillaryConditionViewModel in FilteredSelectedAuxillaryConditionViewModels)
            {
                foreach (var selectedAuxillaryCondition in selectedAuxillaryConditions)
                {
                    if (selectedAuxillaryCondition.Id == filteredSelectedAuxillaryConditionViewModel.Model.Id)
                        filteredSelectedAuxillaryConditionViewModel.IsSelected = true;
                }
            }
        }

        public void DeleteFollowingInspections()
        {
            //Alle nachfolgenden Termine finden
            Approval_Inspection[] followingInspections;
            if (m_SelectedInspection.DueDate.IsAllDay)
            {
                followingInspections = m_AllInspections.Where(flr => flr.DueDate.Begin.Date >= m_SelectedInspection.DueDate.Begin.Date)
                                                   .ToArray();
            }
            else
            {
                followingInspections = m_AllInspections.Where(flr => flr.DueDate.Begin >= m_SelectedInspection.DueDate.Begin)
                                                   .ToArray();
            }

            //Serie ggf. löschen
            if (!m_AllInspections.Except(followingInspections)
                                .Any())
            {
                m_Repository.DeleteObject(m_Model);
            }
            else
            {
                //Ende der alten Serie anpassen
                if (EndsWithDate)
                {
                    m_Model.RepeatUntilDate = m_SelectedInspection.DueDate.Begin;
                }
                else
                {
                    m_Model.NumberOfRecurrences = m_Model.NumberOfRecurrences - followingInspections.Count();
                }
            }

            //Alle folgenden Termine löschen
            foreach (var followingReading in followingInspections)
            {
                DeleteReadingFromRepository(followingReading);
            }
            m_Repository.Save();
            TryClose();
        }

        public void DeleteReading()
        {
            DeleteReadingFromRepository(m_SelectedInspection);

            //Serie ggf. löschen
            if (!m_AllInspections.Any())
            {
                m_Repository.DeleteObject(m_Model);
            }
            m_Repository.Save();
            TryClose();
        }

        public void DeleteSeries()
        {
            foreach (var meterReading in m_AllInspections.ToArray())
            {
                DeleteReadingFromRepository(meterReading);
            }

            //Serie löschen
            m_Repository.DeleteObject(m_Model);
            m_Repository.Save();
            TryClose();
        }

        private void DeleteReadingFromRepository(Approval_Inspection meterReading)
        {
            m_Repository.DeleteObject(meterReading.DueDate);
            if (m_SelectedInspection.EntryDate != null)
            {
                m_Repository.DeleteObject(m_SelectedInspection.EntryDate);
            }
            m_Repository.Inspections.Remove(meterReading);
        }
    }
}
