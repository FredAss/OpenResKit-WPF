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
  public class GeoLocatedPublicTransportViewModel : PublicTransportViewModel
  {
    public GeoLocatedPublicTransportViewModel(GeoLocatedPublicTransport cfp, PositionDataAccessor positionDataAccessor, Func<string, TagColor> getColorForTag,
      IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, getColorForTag, responsibleSubjects)
    {
      GeoLocatedPublicTransportModel = cfp;
    }

    private GeoLocatedPublicTransport GeoLocatedPublicTransportModel { get; set; }

    public string StartName
    {
      get { return GeoLocatedPublicTransportModel.StartName; }
    }

    public string DestinationName
    {
      get { return GeoLocatedPublicTransportModel.DestinationName; }
    }

    public Location StartPoint
    {
      get { return new Location(GeoLocatedPublicTransportModel.StartLatitude, GeoLocatedPublicTransportModel.StartLongitude); }
    }

    public Location DestinationPoint
    {
      get { return new Location(GeoLocatedPublicTransportModel.DestinationLatitude, GeoLocatedPublicTransportModel.DestinationLongitude); }
    }

    public Location CenterPoint
    {
      get
      {
        return Utils.CalculateMidPoint(GeoLocatedPublicTransportModel.StartLatitude, GeoLocatedPublicTransportModel.StartLongitude, GeoLocatedPublicTransportModel.DestinationLatitude,
          GeoLocatedPublicTransportModel.DestinationLongitude);
      }
    }
  }
}