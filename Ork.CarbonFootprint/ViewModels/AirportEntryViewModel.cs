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

using Ork.CarbonFootprint.DomainModelService;
using Ork.CarbonFootprint.PositionDataProvider;

namespace Ork.CarbonFootprint.ViewModels
{
  public class AirportEntryViewModel
  {
    private readonly GlobalAirport m_Airport;
    private readonly AirportPosition m_Model;

    public AirportEntryViewModel(AirportPosition airportPosition, GlobalAirport globalAirport)
    {
      m_Model = airportPosition;
      m_Airport = globalAirport;
    }

    public AirportPosition Model
    {
      get { return m_Model; }
    }

    public int Id
    {
      get { return m_Model.Id; }
    }

    public string Name
    {
      get { return m_Airport.name; }
    }

    public int AirportId
    {
      get { return m_Model.AirportID; }
    }

    public int Order
    {
      get { return m_Model.Order; }
    }

    public string IataCode
    {
      get { return m_Airport.iata_code; }
    }
  }
}