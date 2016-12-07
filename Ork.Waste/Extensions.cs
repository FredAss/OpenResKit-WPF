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

namespace Ork.Waste
{
  internal static class Extensions
  {
    public static DayOfWeek GetAsDayOfWeek(this DomainModelService.DayOfWeek dayOfWeek)
    {
      switch (dayOfWeek.WeekDay)
      {
        case 0:
          return DayOfWeek.Sunday;
        case 1:
          return DayOfWeek.Monday;
        case 2:
          return DayOfWeek.Tuesday;
        case 3:
          return DayOfWeek.Wednesday;
        case 4:
          return DayOfWeek.Thursday;
        case 5:
          return DayOfWeek.Friday;
        case 6:
          return DayOfWeek.Saturday;
        default:
          throw new IndexOutOfRangeException();
      }
    }
  }
}