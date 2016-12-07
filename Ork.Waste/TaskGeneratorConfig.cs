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
using Ork.Waste.DomainModelService;
using DayOfWeek = System.DayOfWeek;

namespace Ork.Waste
{
  public class TaskGeneratorConfig
  {
    public DateTime Begin { get; set; }
    public DateTime End { get; set; }
    public DateTime RepeatUntilDate { get; set; }

    public bool EndsWithDate { get; set; }
    public bool IsAllDay { get; set; }
    public bool IsWeekdayRecurrence { get; set; }
    public bool Repeat { get; set; }

    public int Cycle { get; set; }
    public int NumberOfRecurrences { get; set; }
    public int RecurrenceInterval { get; set; }

    public WasteContainer Container { get; set; }
    public ResponsibleSubject ResponsibleSubject { get; set; }
    public Series Series { get; set; }
    public DayOfWeek[] WeekDays { get; set; }
  }
}