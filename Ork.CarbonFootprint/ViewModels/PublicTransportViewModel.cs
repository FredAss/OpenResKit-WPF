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
using Ork.CarbonFootprint.DomainModelService;

namespace Ork.CarbonFootprint.ViewModels
{
  public class PublicTransportViewModel : PositionViewModel
  {
    public enum TransportTypeEnum
    {
      Fernzug,
      Regionalzug,
      Metro,
      Linienbus,
      Reisebus,
    }

    public PublicTransportViewModel(CarbonFootprintPosition cfp, Func<string, TagColor> getColorForTag, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, getColorForTag, responsibleSubjects)
    {
      PublicTransportModel = (PublicTransport) cfp;
    }

    public int Kilometrage
    {
      get { return PublicTransportModel.Kilometrage; }
      set
      {
        PublicTransportModel.Kilometrage = value;
        NotifyOfPropertyChange(() => Kilometrage);
      }
    }

    public TransportTypeEnum TransportType
    {
      get { return (TransportTypeEnum) PublicTransportModel.m_TransportType; }
      set
      {
        PublicTransportModel.m_TransportType = (int) value;
        NotifyOfPropertyChange(() => TransportType);
      }
    }

    private PublicTransport PublicTransportModel { get; set; }
  }
}