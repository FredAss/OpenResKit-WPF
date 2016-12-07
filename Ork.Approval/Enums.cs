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

namespace Ork.Approval
{
    public enum KindOfPermission
    {
        Whg = 0,
        Bimschg = 1,
        Bbodschg = 2
    }

    public enum InEffect
    {
        All = 0,
        InEffect = 1,
        OutEffect = 2
    }

    public enum Priority
    {
        PriorityLow = 0,
        PriorityMedium = 1,
        PriorityHigh = 2
    }

    public enum Progress
    {
        ProgressAdded = 0,
        ProgressRunning = 1,
        ProgressClosed = 2
    }
}
