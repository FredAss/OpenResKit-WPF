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
using System.ComponentModel.Composition;
using System.Linq;
using DateTimeGenerator;
using Ork.Approval.DomainModelService;
using DayOfWeek = System.DayOfWeek;


namespace Ork.Approval
{
    [Export(typeof(ITaskGenerator))]
    internal class TaskGenerator : ITaskGenerator
    {
        public IEnumerable<DateTime> GenerateDailyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences)
        {
            return endsWithDate
                ? begin.GenerateDailyRecurrence(recurrenceInterval, repeatUntilDate)
                : begin.GenerateDailyRecurrence(recurrenceInterval, numberOfRecurrences);
        }

        public IEnumerable<DateTime> GenerateMonthlyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences, bool isWeekdayRecurrence)
        {
            if (isWeekdayRecurrence)
            {
                return endsWithDate
                  ? begin.GenerateMonthlyRecurrenceWithWeekDaySpecification(recurrenceInterval, repeatUntilDate)
                  : begin.GenerateMonthlyRecurrenceWithWeekDaySpecification(recurrenceInterval, numberOfRecurrences);
            }
            else
            {
                return endsWithDate
                  ? begin.GenerateMonthlyRecurrenceWithCalendarDaySpecification(recurrenceInterval, repeatUntilDate)
                  : begin.GenerateMonthlyRecurrenceWithCalendarDaySpecification(recurrenceInterval, numberOfRecurrences);
            }
        }

        public IEnumerable<DateTime> GenerateWeeklyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences, DayOfWeek[] weekDays)
        {
            return endsWithDate
                ? begin.GenerateWeeklyRecurrence(recurrenceInterval, weekDays, repeatUntilDate )
                : begin.GenerateWeeklyRecurrence(recurrenceInterval, weekDays, numberOfRecurrences);
        }

        public IEnumerable<DateTime> GenerateYearlyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences)
        {
            return endsWithDate
                ? begin.GenerateYearlyRecurrence(recurrenceInterval, repeatUntilDate)
                : begin.GenerateYearlyRecurrence(recurrenceInterval, numberOfRecurrences);
        }

        public IEnumerable<Approval_Inspection> GenerateInspections(TaskGeneratorConfig taskGeneratorConfig)
        {
            if (!taskGeneratorConfig.Repeat)
            {
                return new[]
               {
                 CreateInspection(taskGeneratorConfig.Begin, taskGeneratorConfig.End, taskGeneratorConfig.IsAllDay, taskGeneratorConfig.ConditionInspections, taskGeneratorConfig.ResponsibleSubject,
                   taskGeneratorConfig.Series)
               };
            }

            IEnumerable<DateTime> dates;
            switch (taskGeneratorConfig.Cycle)
            {
                case 0:
                    {
                        dates = GenerateDailyRecurrence(taskGeneratorConfig.Begin, taskGeneratorConfig.RecurrenceInterval, taskGeneratorConfig.EndsWithDate, taskGeneratorConfig.RepeatUntilDate,
                          taskGeneratorConfig.NumberOfRecurrences);
                        break;
                    }
                case 1:
                    {
                        dates = GenerateWeeklyRecurrence(taskGeneratorConfig.Begin, taskGeneratorConfig.RecurrenceInterval, taskGeneratorConfig.EndsWithDate, taskGeneratorConfig.RepeatUntilDate,
                          taskGeneratorConfig.NumberOfRecurrences, taskGeneratorConfig.WeekDays);
                        break;
                    }
                case 2:
                    {
                        dates = GenerateMonthlyRecurrence(taskGeneratorConfig.Begin, taskGeneratorConfig.RecurrenceInterval, taskGeneratorConfig.EndsWithDate, taskGeneratorConfig.RepeatUntilDate,
                          taskGeneratorConfig.NumberOfRecurrences, taskGeneratorConfig.IsWeekdayRecurrence);
                        break;
                    }
                case 3:
                    {
                        dates = GenerateYearlyRecurrence(taskGeneratorConfig.Begin, taskGeneratorConfig.RecurrenceInterval, taskGeneratorConfig.EndsWithDate, taskGeneratorConfig.RepeatUntilDate,
                          taskGeneratorConfig.NumberOfRecurrences);
                        break;
                    }
                default:
                    throw new InvalidOperationException();
            }

            var range = taskGeneratorConfig.End.Subtract(taskGeneratorConfig.Begin);
            return
              dates.Select(date => CreateInspection(date, date.Add(range), taskGeneratorConfig.IsAllDay, taskGeneratorConfig.ConditionInspections, taskGeneratorConfig.ResponsibleSubject, taskGeneratorConfig.Series));
        }

        public IEnumerable<Approval_Inspection> GenerateInspections(ObservableCollection<ConditionInspection> conditionInspections, ResponsibleSubject responsibleSubject, Series series, DayOfWeek[] weekDays)
        {
            var taskGeneratorConfig = new TaskGeneratorConfig();
            taskGeneratorConfig.ConditionInspections = conditionInspections;

            taskGeneratorConfig.Series = series;
            taskGeneratorConfig.ResponsibleSubject = responsibleSubject;
            taskGeneratorConfig.WeekDays = weekDays;

            taskGeneratorConfig.Begin = series.Begin;
            taskGeneratorConfig.End = series.End;
            taskGeneratorConfig.Repeat = series.Repeat;
            taskGeneratorConfig.RecurrenceInterval = series.RecurrenceInterval;
            taskGeneratorConfig.EndsWithDate = series.EndsWithDate;
            taskGeneratorConfig.RepeatUntilDate = series.RepeatUntilDate;
            taskGeneratorConfig.NumberOfRecurrences = series.NumberOfRecurrences;
            taskGeneratorConfig.IsWeekdayRecurrence = series.IsWeekdayRecurrence;
            taskGeneratorConfig.IsAllDay = series.IsAllDay;
            taskGeneratorConfig.Cycle = series.Cycle;

            return GenerateInspections(taskGeneratorConfig);
        }

        private Approval_Inspection CreateInspection(DateTime begin, DateTime end, bool isAllDay, ObservableCollection<ConditionInspection> conditionInspections, ResponsibleSubject responsibleSubject, Series relatedSeries)
        {
            Approval_Inspection result = new Approval_Inspection();

            foreach (var conditionInspection in conditionInspections)
            {
                var ci = new ConditionInspection();
                ci.EntryDate = DateTime.Now;
                ci.AuxillaryConditionId = conditionInspection.AuxillaryConditionId;
                result.ConditionInspections.Add(ci);
            }

            result.DueDate = new Appointment
            {
                Begin = begin,
                End = end,
                IsAllDay = isAllDay
            };

            result.Progress = 0.0f;
            result.RelatedSeries = relatedSeries;
            result.AppointmentResponsibleSubject = responsibleSubject;

            return result;
        }
    }
}
