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
using Ork.Approval.DomainModelService;
using DayOfWeek = System.DayOfWeek;


namespace Ork.Approval
{
    public interface ITaskGenerator
    {
        IEnumerable<DateTime> GenerateDailyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences);
        IEnumerable<DateTime> GenerateMonthlyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences, bool isWeekdayRecurrence);
        IEnumerable<DateTime> GenerateWeeklyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences, DayOfWeek[] weekDays);
        IEnumerable<DateTime> GenerateYearlyRecurrence(DateTime begin, int recurrenceInterval, bool endsWithDate, DateTime repeatUntilDate, int numberOfRecurrences);
        IEnumerable<Approval_Inspection> GenerateInspections(TaskGeneratorConfig taskGeneratorConfig);
        IEnumerable<Approval_Inspection> GenerateInspections(ObservableCollection<ConditionInspection> conditionInspections, ResponsibleSubject responsibleSubject, Series series, DayOfWeek[] weekDays);
    }
}
