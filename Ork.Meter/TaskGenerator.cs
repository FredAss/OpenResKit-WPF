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
using System.ComponentModel.Composition;
using System.Linq;
using DateTimeGenerator;
using Ork.Meter.DomainModelService;
using DayOfWeek = System.DayOfWeek;

namespace Ork.Meter
{
  [Export(typeof (ITaskGenerator))]
  internal class TaskGenerator : ITaskGenerator
  {
    public IEnumerable<MeterReading> GenerateMeterReadings(TaskGeneratorConfig taskGeneratorConfig)
    {
      if (!taskGeneratorConfig.Repeat)
      {
        return new[]
               {
                 CreateFillLevelReading(taskGeneratorConfig.Begin, taskGeneratorConfig.End, taskGeneratorConfig.IsAllDay, taskGeneratorConfig.Meter, taskGeneratorConfig.ResponsibleSubject,
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
        dates.Select(date => CreateFillLevelReading(date, date.Add(range), taskGeneratorConfig.IsAllDay, taskGeneratorConfig.Meter, taskGeneratorConfig.ResponsibleSubject, taskGeneratorConfig.Series));
    }

    public IEnumerable<MeterReading> GenerateMeterReadings(DomainModelService.Meter meter, ResponsibleSubject responsibleSubject, Series series, DayOfWeek[] weekDays)
    {
      var taskGeneratorConfig = new TaskGeneratorConfig();
      taskGeneratorConfig.Meter = meter;
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

      return GenerateMeterReadings(taskGeneratorConfig);
    }

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
        ? begin.GenerateWeeklyRecurrence(recurrenceInterval, weekDays, repeatUntilDate)
        : begin.GenerateWeeklyRecurrence(recurrenceInterval, weekDays, numberOfRecurrences);
    }

    public IEnumerable<DateTime> GenerateYearlyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences)
    {
      return endsWithDate
        ? begin.GenerateYearlyRecurrence(recurrenceInterval, repeatUntilDate)
        : begin.GenerateYearlyRecurrence(recurrenceInterval, numberOfRecurrences);
    }

    private MeterReading CreateFillLevelReading(DateTime begin, DateTime end, bool isAllDay, DomainModelService.Meter meter, ResponsibleSubject responsibleSubject, Series relatedSeries)
    {
      return new MeterReading
             {
               DueDate = new Appointment
                         {
                           Begin = begin,
                           End = end,
                           IsAllDay = isAllDay
                         },
               RelatedSeries = relatedSeries,
               AppointmentResponsibleSubject = responsibleSubject,
               ReadingMeter = meter
             };
    }
  }
}