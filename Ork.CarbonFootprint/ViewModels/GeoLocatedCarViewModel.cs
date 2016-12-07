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
using Microsoft.Maps.MapControl.WPF;
using Ork.CarbonFootprint.DomainModelService;

namespace Ork.CarbonFootprint.ViewModels
{
  public class GeoLocatedCarViewModel : FullyQualifiedCarViewModel
  {
    private bool m_MapIsShown;

    public GeoLocatedCarViewModel(GeoLocatedCar cfp, PositionDataAccessor positionDataAccessor, Func<string, TagColor> getColorForTag, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, positionDataAccessor, getColorForTag, responsibleSubjects)
    {
      GeoLocatedCarModel = cfp;
    }

    private GeoLocatedCar GeoLocatedCarModel { get; set; }

    public string StartName
    {
      get { return GeoLocatedCarModel.StartName; }
    }

    public string DestinationName
    {
      get { return GeoLocatedCarModel.DestinationName; }
    }

    public Location StartPoint
    {
      get { return new Location(GeoLocatedCarModel.StartLatitude, GeoLocatedCarModel.StartLongitude); }
    }

    public Location DestinationPoint
    {
      get { return new Location(GeoLocatedCarModel.DestinationLatitude, GeoLocatedCarModel.DestinationLongitude); }
    }

    public Location CenterPoint
    {
      get { return Utils.CalculateMidPoint(GeoLocatedCarModel.StartLatitude, GeoLocatedCarModel.StartLongitude, GeoLocatedCarModel.DestinationLatitude, GeoLocatedCarModel.DestinationLongitude); }
    }
  }
}