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
using System.Linq;
using Ork.CarbonFootprint.DomainModelService;
using Ork.CarbonFootprint.PositionDataProvider;

namespace Ork.CarbonFootprint.ViewModels
{
  public class AirportBasedFlightViewModel : PositionViewModel
  {
    private readonly AirportBasedFlight m_Model;
    private readonly PositionDataAccessor m_PositionDataAccessor;
    private IEnumerable<GlobalAirport> m_GlobalAirports;
    private GlobalAirport m_SelectedAirport;
    private AirportEntryViewModel m_SelectedPosition;

    public AirportBasedFlightViewModel(CarbonFootprintPosition cfp, PositionDataAccessor positionDataAccessor, Func<string, TagColor> getColorForTag,
      IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
      : base(cfp, getColorForTag, responsibleSubjects)
    {
      m_Model = (AirportBasedFlight) cfp;

      m_PositionDataAccessor = positionDataAccessor;
      m_PositionDataAccessor.Loaded += AirportDataLoaded;

      if (m_PositionDataAccessor.AirportData != null)
      {
        GlobalAirports = m_PositionDataAccessor.AirportData;
      }
    }

    public IEnumerable<AirportEntryViewModel> AirportPositions
    {
      private set { return; }
      get { return GetAirportEntryViewModels(); }
    }
    
    public IEnumerable<GlobalAirport> GlobalAirports
    {
      get { return m_GlobalAirports; }
      set
      {
        if (m_GlobalAirports == value)
        {
          return;
        }
        m_GlobalAirports = value;
        NotifyOfPropertyChange(() => GlobalAirports);
      }
    }

    public GlobalAirport SelectedAirport
    {
      get { return m_SelectedAirport; }
      set
      {
        if (m_SelectedAirport == value)
        {
          return;
        }
        m_SelectedAirport = value;
        NotifyOfPropertyChange(() => SelectedAirport);
      }
    }

    public AirportEntryViewModel SelectedPosition
    {
      get { return m_SelectedPosition; }
      set
      {
        if (m_SelectedPosition == value)
        {
          return;
        }
        m_SelectedPosition = value;
        NotifyOfPropertyChange(() => SelectedPosition);
      }
    }

    public void AddAirportToList()
    {
      if (SelectedAirport == null)
      {
        return;
      }

      var orderValue = 1;
      if (m_Model.Airports.Count > 0)
      {
        orderValue = m_Model.Airports.Max(a => a.Order) + 1;
      }

      var airport = new AirportPosition();
      airport.Order = orderValue;
      airport.AirportID = SelectedAirport.id;

      m_Model.Airports.Add(airport);
      NotifyOfPropertyChange(() => AirportPositions);
    }

    public void DeleteAirportFromList()
    {
      if (SelectedPosition == null)
      {
        return;
      }

      m_Model.Airports.Remove(SelectedPosition.Model);
      NotifyOfPropertyChange(() => AirportPositions);
    }

    private void AirportDataLoaded(object sender, EventArgs e)
    {
      GlobalAirports = m_PositionDataAccessor.AirportData;
    }

    private bool FilterItems(string search, object data)
    {
      if (data is GlobalAirport)
      {
        if (((GlobalAirport) data).name.ToLower()
                                  .Contains(search.ToLower()) ||
            ((GlobalAirport) data).iata_code.ToLower()
                                  .Contains(search.ToLower()))
        {
          return true;
        }
      }

      return false;
    }

    private IEnumerable<AirportEntryViewModel> GetAirportEntryViewModels()
    {
      foreach (var position in m_Model.Airports)
      {
        yield return new AirportEntryViewModel(position, GlobalAirports.Single(air => air.id == position.AirportID));
      }
    }
  }
}