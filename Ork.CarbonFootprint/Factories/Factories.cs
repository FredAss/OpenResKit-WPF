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
using System.ComponentModel.Composition;
using Ork.CarbonFootprint.DomainModelService;
using Ork.CarbonFootprint.ViewModels;

namespace Ork.CarbonFootprint.Factories
{
  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("Flight")]
  internal class FlightFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    internal FlightFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (Flight);
    }

    public bool CanCreate
    {
      get { return true; }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new FlightViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateFlight(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddYears(1), FlightViewModel.FlightRange.Mittelstrecke), responsibleSubjects);
    }

    private Flight CreateFlight(string name, string description, string tag, string iconId, DateTime start, DateTime finish,
      FlightViewModel.FlightRange flightRange = FlightViewModel.FlightRange.Kurzstrecke, bool radiativeForcing = false)
    {
      return new Flight
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               m_FlightType = (int) flightRange,
               RadiativeForcing = radiativeForcing
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("CarDrive")]
  internal class CarFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    public CarFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (Car);
    }

    public bool CanCreate
    {
      get
      {
        return true;
      }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new CarViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateCar(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddYears(1), 0, 0, 0, CarViewModel.Fuel.Benzin), responsibleSubjects);
    }

    private Car CreateCar(string name, string description, string tag, string iconId, DateTime start, DateTime finish, int kilometrage = 0, double consumption = 0, double carbonProduction = 0,
      CarViewModel.Fuel fuelType = CarViewModel.Fuel.Diesel)
    {
      return new Car
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               Kilometrage = kilometrage,
               CarbonProduction = carbonProduction,
               m_Fuel = (int) fuelType,
               Consumption = consumption
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("AirportBasedFlight")]
  public class AirportBasedFlightFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;
    private readonly PositionDataAccessor m_PositionDataAccessor;

    [ImportingConstructor]
    public AirportBasedFlightFactory(PositionDataAccessor positionDataAccessor, [Import] Func<string, TagColor> getColorForTag)
    {
      m_PositionDataAccessor = positionDataAccessor;
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (AirportBasedFlight);
    }

    public bool CanCreate
    {
      get
      {
        return true;
      }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new AirportBasedFlightViewModel(model, m_PositionDataAccessor, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateAirportBasedFlight(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddYears(1)), responsibleSubjects);
    }

    private AirportBasedFlight CreateAirportBasedFlight(string name, string description, string tag, string iconId, DateTime start, DateTime finish)
    {
      return new AirportBasedFlight
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("QuallifiedCar")]
  internal class FullyQualifiedCarFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;
    private readonly PositionDataAccessor m_PositionDataAccessor;

    [ImportingConstructor]
    public FullyQualifiedCarFactory(PositionDataAccessor positionDataAccessor, [Import] Func<string, TagColor> getColorForTag)
    {
      m_PositionDataAccessor = positionDataAccessor;
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (FullyQualifiedCar);
    }

    public bool CanCreate
    {
      get
      {
        return true;
      }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new FullyQualifiedCarViewModel(model, m_PositionDataAccessor, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateFullyQualifiedCar(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddYears(1), 0, 13), responsibleSubjects);
    }

    private FullyQualifiedCar CreateFullyQualifiedCar(string name, string description, string tag, string iconId, DateTime start, DateTime finish, int kilometrage = 0, int carId = 1,
      double consumption = 0)
    {
      return new FullyQualifiedCar
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               Kilometrage = kilometrage,
               Consumption = consumption,
               CarId = carId
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("GeoLocatedCar")]
  internal class GeoLocatedCarFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;
    private readonly PositionDataAccessor m_PositionDataAccessor;

    [ImportingConstructor]
    public GeoLocatedCarFactory(PositionDataAccessor positionDataAccessor, [Import] Func<string, TagColor> getColorForTag)
    {
      m_PositionDataAccessor = positionDataAccessor;
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (GeoLocatedCar);
    }

    public bool CanCreate
    {
      get
      {
        return false;
      }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new GeoLocatedCarViewModel((GeoLocatedCar) model, m_PositionDataAccessor, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      throw new InvalidOperationException();
    }
  }
  
  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("GeoLocatedPublicTransport")]
  internal class GeoLocatedPublicTransportFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;
    private readonly PositionDataAccessor m_PositionDataAccessor;

    [ImportingConstructor]
    public GeoLocatedPublicTransportFactory(PositionDataAccessor positionDataAccessor, [Import] Func<string, TagColor> getColorForTag)
    {
      m_PositionDataAccessor = positionDataAccessor;
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof(GeoLocatedPublicTransport);
    }

    public bool CanCreate
    {
      get
      {
        return false;
      }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new GeoLocatedPublicTransportViewModel((GeoLocatedPublicTransport) model, m_PositionDataAccessor, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      throw new InvalidOperationException();
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("EnergyConsumption")]
  internal class EnergyConsumptionFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    public EnergyConsumptionFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (EnergyConsumption);
    }

    public bool CanCreate
    {
      get
      {
        return true;
      }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new EnergyConsumptionViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateEnergyConsumption(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddYears(1), 0, EnergySource.Ecostrom), responsibleSubjects);
    }

    private EnergyConsumption CreateEnergyConsumption(string name, string description, string tag, string iconId, DateTime start, DateTime finish, double consumption = 0,
      EnergySource energySource = EnergySource.Strommix, double carbonProduction = 0)
    {
      return new EnergyConsumption
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               Consumption = consumption,
               m_EnergySource = (int) energySource,
               CarbonProduction = carbonProduction
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("EnergyConsumptionPerMachine")]
  internal class MachineEnergyConsumptionFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    public MachineEnergyConsumptionFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (MachineEnergyConsumption);
    }

    public bool CanCreate
    {
      get
      {
        return true;
      }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new MachineEnergyConsumptionViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return CreateFromExisting(CreateMachineEnergyConsumption(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddYears(1), EnergySource.Ecostrom, 0, 0, 0, 0),
        responsibleSubjects);
    }

    private MachineEnergyConsumption CreateMachineEnergyConsumption(string name, string description, string tag, string iconId, DateTime start, DateTime finish,
      EnergySource energySource = EnergySource.Strommix, double hourInStandbyState = 0, double consumptionPerHourForStandby = 0, double hourInProcessingState = 0,
      double consumptionPerHourForProcessing = 0, double carbonProduction = 0)
    {
      return new MachineEnergyConsumption
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               m_EnergySource = (int) energySource,
               HoursInProcessingState = hourInProcessingState,
               HoursInStandbyState = hourInStandbyState,
               ConsumptionPerHourForStandby = consumptionPerHourForStandby,
               ConsumptionPerHourForProcessing = consumptionPerHourForProcessing,
               CarbonProduction = carbonProduction
             };
    }
  }

  [Export(typeof (IPositionViewModelFactory))]
  [PositionMetadata("PublicTransportation")]
  internal class PublicTransportFactory : IPositionViewModelFactory
  {
    private readonly Func<string, TagColor> m_GetColorForTag;

    [ImportingConstructor]
    public PublicTransportFactory([Import] Func<string, TagColor> getColorForTag)
    {
      m_GetColorForTag = getColorForTag;
    }

    public bool CanDecorate(CarbonFootprintPosition model)
    {
      return model.GetType() == typeof (PublicTransport);
    }

    public bool CanCreate
    {
      get
      {
        return true;
      }
    }

    public PositionViewModel CreateFromExisting(CarbonFootprintPosition model, IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return new PublicTransportViewModel(model, m_GetColorForTag, responsibleSubjects);
    }

    public PositionViewModel CreateNew(IEnumerable<ResponsibleSubjectViewModel> responsibleSubjects)
    {
      return
        CreateFromExisting(
          CreatePublicTransport(string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now, DateTime.Now.AddYears(1), 0, PublicTransportViewModel.TransportTypeEnum.Reisebus),
          responsibleSubjects);
    }

    private PublicTransport CreatePublicTransport(string name, string description, string tag, string iconId, DateTime start, DateTime finish, int kilometrage = 0,
      PublicTransportViewModel.TransportTypeEnum transportType = PublicTransportViewModel.TransportTypeEnum.Linienbus)
    {
      return new PublicTransport
             {
               Name = name,
               Description = description,
               Tag = tag,
               IconId = iconId,
               Start = start,
               Finish = finish,
               m_TransportType = (int) transportType,
               Kilometrage = kilometrage
             };
    }
  }
}