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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Services.Client;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Microsoft.Win32;
using Ork.Framework;
using Ork.Map;
using Ork.Map.ViewModels;
using Ork.Meter.Commands;
using Ork.Meter.DomainModelService;
using Ork.Meter.Factories;
using Mouse = System.Windows.Input.Mouse;

namespace Ork.Meter.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class MeterManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly IMapViewModelFactory m_MapViewModelFactory;
    private readonly BindableCollection<MapViewModel> m_Maps = new BindableCollection<MapViewModel>();
    private readonly IMeterViewModelFactory m_MeterViewModelFactory;
    private readonly BindableCollection<MeterViewModel> m_Meters = new BindableCollection<MeterViewModel>();
    private readonly IMeterRepository m_Repository;
    private IScreen m_EditItem;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private bool m_PositionSettingActivated;
    private string m_SearchText;
    private MapViewModel m_SelectedMap;
    private MeterViewModel m_SelectedMeter;

    [ImportingConstructor]
    public MeterManagementViewModel([Import] IMeterRepository contextRepository, [Import] IMapViewModelFactory mapViewModelFactory, [Import] IMeterViewModelFactory meterViewModelFactory)
    {
      m_Repository = contextRepository;
      m_MapViewModelFactory = mapViewModelFactory;
      m_MeterViewModelFactory = meterViewModelFactory;

      AddNewMap = new AddNewMapCommand(this);
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
      Reload();

      FlyoutActivated = true;
    }

    public ICommand AddNewMap { get; set; }

    public IEnumerable<MapItemViewModel> MeterPositionsOnSelectedMap
    {
      get
      {
        if (SelectedMap != null)
        {
          return m_Meters.Where(c => c.Map == SelectedMap.Model)
                         .Select(c => c.MeterPosition);
        }
        return new MapItemViewModel[0];
      }
    }

    public IEnumerable<MeterViewModel> FilteredMeters
    {
      get
      {
        var filteredMeters = SearchInMeterList()
          .ToArray();

        if (SelectedMap == null)
        {
          if (Maps.Count == 0)
          {
            filteredMeters.ForEach(c => c.MapIndicator = 3);
          }
          else
          {
            filteredMeters.ForEach(c => c.MapIndicator = 2);
          }

          return filteredMeters;
        }

        var metersOnMap = filteredMeters.Where(c => c.Map == SelectedMap.Model)
                                        .ToArray();
        metersOnMap.ForEach(c => c.MapIndicator = 1);

        var metersOnOtherMap = filteredMeters.Where(c => c.Map != SelectedMap.Model && c.Map != null)
                                             .ToArray();
        metersOnOtherMap.ForEach(c => c.MapIndicator = 2);

        var metersWithoutMap = filteredMeters.Where(c => c.Map == null)
                                             .ToArray();
        metersWithoutMap.ForEach(c => c.MapIndicator = 3);

        return metersOnMap.Concat(metersWithoutMap)
                          .Concat(metersOnOtherMap);
      }
    }

    public bool FlyoutActivated
    {
      get { return m_FlyoutActivated; }
      set
      {
        if (m_FlyoutActivated == value)
        {
          return;
        }
        m_FlyoutActivated = value;
        NotifyOfPropertyChange(() => FlyoutActivated);
      }
    }

    public override bool IsDirty
    {
      get { return m_EditItem != null && m_EditItem.IsActive; }
      set { base.IsDirty = value; }
    }

    public ObservableCollection<MapViewModel> Maps
    {
      get { return m_Maps; }
    }

    public bool PositionSettingActivated
    {
      get { return m_PositionSettingActivated; }
      set
      {
        if (m_PositionSettingActivated == value)
        {
          return;
        }
        m_PositionSettingActivated = value;
        NotifyOfPropertyChange(() => PositionSettingActivated);
        NotifyOfPropertyChange(() => IsMapAddButtonEnabled);
      }
    }

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredMeters);
      }
    }

    public MeterViewModel SelectedMeter
    {
      get { return m_SelectedMeter; }
      set
      {
        if (m_SelectedMeter == value || PositionSettingActivated)
        {
          return;
        }
        m_SelectedMeter = value;

        SetSelectedMapItem();
        NotifyOfPropertyChange(() => SelectedMeter);
      }
    }


    public MapViewModel SelectedMap
    {
      get { return m_SelectedMap; }
      set
      {
        if (m_SelectedMap == value)
        {
          return;
        }
        m_SelectedMap = value;
        NotifyOfPropertyChange(() => SelectedMap);
        NotifyOfPropertyChange(() => FilteredMeters);
        NotifyOfPropertyChange(() => MeterPositionsOnSelectedMap);
      }
    }

    public int Index
    {
      get { return 100; }
    }

    public bool IsEnabled
    {
      get { return m_IsEnabled; }
      private set
      {
        m_IsEnabled = value;
        NotifyOfPropertyChange(() => IsEnabled);
      }
    }

    public string Title
    {
      get { return TranslationProvider.Translate("Meter"); }
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConnection;
      if (IsEnabled)
      {
        LoadData();
      }
    }

    public void Accept()
    {
      Save();
      CloseEditor();
    }

    public void Cancel()
    {
      CloseEditor();
    }

    public void ActivatePositionSetting()
    {
      Mouse.OverrideCursor = Cursors.Cross;
      CloseEditor();
      FlyoutActivated = false;
      PositionSettingActivated = true;
    }

    public void AddMeter(object dataContext)
    {
      var meterAddViewModel = ((MeterAddViewModel) dataContext);
      m_Repository.Meter.Add(meterAddViewModel.Model);
      NotifyOfPropertyChange(() => FilteredMeters);
      SelectedMeter = m_Meters.Last();
      CloseEditor();
      Save();
    }

    public void AddMap(object dataContext)
    {
      var mapViewModel = ((MapAddViewModel) dataContext);
      m_Repository.Maps.Add(mapViewModel.Model);
      NotifyOfPropertyChange(() => Maps);
      SelectedMap = m_Maps.Last();
      CloseEditor();
      Save();
    }

    public void CancelPositionSetting()
    {
      if (PositionSettingActivated)
      {
        OpenEditor(m_EditItem);
        FlyoutActivated = true;
        PositionSettingActivated = false;
        Mouse.OverrideCursor = null;
        Save();
      }
    }

    public void ChangeSelectedMap(object dataContext)
    {
      var map = ((MeterViewModel) dataContext).Map;
      if (map != null)
      {
        SelectedMap = m_Maps.Single(m => m.Model == map);
      }
    }

    public void CloseEditor()
    {
      m_EditItem.TryClose();
    }

    public void MoveMeter(MapMouseEventArgs e)
    {
      if (PositionSettingActivated)
      {
        var selectedMeter = (MeterEditViewModel) m_EditItem;
        selectedMeter.X = e.Coordinates.X;
        selectedMeter.Y = e.Coordinates.Y;
        if (SelectedMap != null &&
            selectedMeter.Map != SelectedMap.Model)
        {
          selectedMeter.Map = SelectedMap.Model;
          SelectedMeter = m_Meters.Single(m => m.Model == selectedMeter.Model);
        }
        else
        {
          NotifyOfPropertyChange(() => MeterPositionsOnSelectedMap);
        }
      }
    }

    public bool IsMapAddButtonEnabled
    {
      get
      {
        return !PositionSettingActivated;
      }
    }


    public void OpenMeterAddDialog()
    {
      OpenEditor(m_MeterViewModelFactory.CreateAddViewModel(SelectedMap));
    }

    public void OpenMeterEditDialog(object dataContext)
    {
      SelectedMeter = (MeterViewModel) dataContext;
      OpenEditor(m_MeterViewModelFactory.CreateEditViewModel(((MeterViewModel) dataContext).Model, RemoveMeter));
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2)
      {
        if (dataContext is MeterViewModel)
        {
          OpenMeterEditDialog(dataContext);
        }
      }
    }

    public void OpenMapAddDialog()
    {
      var o = new OpenFileDialog
              {
                Filter = "Bilder (.png, .jpg, .gif)|*.png; *.jpg; *.gif"
              };
      var result = o.ShowDialog();
      if (result == true)
      {
        var mapAddViewModel = m_MapViewModelFactory.CreateAddViewModel(o.FileName);
        OpenEditor(mapAddViewModel);
      }
    }

    public void OpenMapEditDialog(object dataContext)
    {
      if (PositionSettingActivated)
      {
        return;
      }
      OpenEditor(m_MapViewModelFactory.CreateEditViewModel(((MapViewModel) dataContext).Model, RemoveMap));
    }

    public void RemoveMeter()
    {
      var meterViewModel = SelectedMeter;

      m_Repository.DeleteObject(meterViewModel.Model.MapPosition);
      m_Repository.Meter.Remove(meterViewModel.Model);

      CloseEditor();
      NotifyOfPropertyChange(() => FilteredMeters);
      Save();
    }

    public void RemoveMap()
    {
      var mapViewModel = SelectedMap;
      foreach (var meter in m_Repository.Meter.Where(c => c.MapPosition.Map == mapViewModel.Model))
      {
        meter.MapPosition.Map = null;
      }
      m_Repository.Maps.Remove(mapViewModel.Model);
      CloseEditor();
      Save();
    }

    public void Save()
    {
      if (m_Repository.Entities.Where(ed => ed.Entity is DomainModelService.Meter || ed.Entity is DomainModelService.Map || ed.Entity is MapPosition)
                      .Any(ed => ed.State != EntityStates.Unchanged) ||
          m_Repository.Links.Where(l => l.Source is DomainModelService.Meter || l.Source is DomainModelService.Map || l.Source is MapPosition)
                      .Any(ed => ed.State != EntityStates.Unchanged))
      {
        m_Repository.Save();
      }
    }

    private void AlterMeterCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<DomainModelService.Meter>())
        {
          CreateMeterViewModel(newItem);
        }
      }
      else
      {
        foreach (var oldItem in e.OldItems.OfType<DomainModelService.Meter>())
        {
          var cvm = m_Meters.Single(c => c.Model == oldItem);
          cvm.PropertyChanged -= MeterPropertyChanged;
          m_Meters.Remove(cvm);
        }
      }

      NotifyOfPropertyChange(() => MeterPositionsOnSelectedMap);
      NotifyOfPropertyChange(() => FilteredMeters);
    }

    private void AlterMapCollection(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (var newItem in e.NewItems.OfType<DomainModelService.Map>())
        {
          CreateMapViewModel(newItem);
        }
      }
      else if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        foreach (var oldItem in e.OldItems.OfType<DomainModelService.Map>())
        {
          m_Maps.Remove(m_Maps.Single(m => m.Model == oldItem));
        }
      }
      SelectedMap = m_Maps.FirstOrDefault();
    }

    private void MeterPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Map")
      {
        NotifyOfPropertyChange(() => MeterPositionsOnSelectedMap);
        NotifyOfPropertyChange(() => FilteredMeters);
      }
    }

    private void CreateMeterViewModel(DomainModelService.Meter newItem)
    {
      var cvm = m_MeterViewModelFactory.CreateFromExisiting(newItem);
      cvm.PropertyChanged += MeterPropertyChanged;
      m_Meters.Add(cvm);
    }

    private void CreateMapViewModel(DomainModelService.Map newItem)
    {
      m_Maps.Add(m_MapViewModelFactory.CreateFromExisting(newItem));
    }

    private void LoadMeter()
    {
      m_Repository.Meter.CollectionChanged += AlterMeterCollection;
      foreach (var meter in m_Repository.Meter)
      {
        CreateMeterViewModel(meter);
      }
      NotifyOfPropertyChange(() => FilteredMeters);
    }

    private void LoadData()
    {
      m_Maps.Clear();
      m_Meters.Clear();
      LoadMeter();
      LoadMaps();
    }

    private void LoadMaps()
    {
      m_Repository.Maps.CollectionChanged += AlterMapCollection;
      foreach (var map in m_Repository.Maps)
      {
        CreateMapViewModel(map);
      }
      SelectedMap = m_Maps.FirstOrDefault();
    }

    private void OpenEditor(object dataContext)
    {
      m_EditItem = (IScreen) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    private IEnumerable<MeterViewModel> SearchInMeterList()
    {
      if (string.IsNullOrEmpty(SearchText))
      {
        return m_Meters;
      }
      var searchText = SearchText.ToLower();
      var searchResult = m_Meters.Where(c => (((c.Number != null) && (c.Number.ToLower()
                                                                       .Contains(searchText))) || ((c.Barcode != null) && (c.Barcode.ToLower()
                                                                                                                            .Contains(searchText)))) || ((c.Map != null) && (c.Map.Name.ToLower()
                                                                                                                                                                              .Contains(searchText))));
      return searchResult;
    }

    private void SetSelectedMapItem()
    {
      if (SelectedMeter == null)
      {
        return;
      }
      foreach (var meterViewModel in m_Meters)
      {
        meterViewModel.MeterPosition.IsSelected = (meterViewModel.Model == SelectedMeter.Model);
      }
    }
  }
}