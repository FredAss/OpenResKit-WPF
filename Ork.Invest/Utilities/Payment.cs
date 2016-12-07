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

namespace Ork.Invest.Utilities
{
  public struct Payment
  {
    public int Year { get; set; }
    public double JährlicherRückfluss { get; set; }
    public double Zins { get; set; }
    public double Tilgung { get; set; }
    public double Überschüsse { get; set; }
    public double Abzahlungsbetrag { get; set; }
    public double Kapitalwert { get; set; }
  }
}