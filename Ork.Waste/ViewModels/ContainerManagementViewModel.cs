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
using Ork.Waste.Commands;
using Ork.Waste.DomainModelService;
using Ork.Waste.Factories;
using Mouse = System.Windows.Input.Mouse;

namespace Ork.Waste.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class ContainerManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly BindableCollection<ContainerViewModel> m_Container = new BindableCollection<ContainerViewModel>();
    private readonly IContainerViewModelFactory m_ContainerViewModelFactory;
    private readonly IMapViewModelFactory m_MapViewModelFactory;
    private readonly BindableCollection<MapViewModel> m_Maps = new BindableCollection<MapViewModel>();
    private readonly IWasteRepository m_Repository;
    private IScreen m_EditItem;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private bool m_PositionSettingActivated;
    private string m_SearchText;
    private ContainerViewModel m_SelectedContainer;
    private MapViewModel m_SelectedMap;

    [ImportingConstructor]
    public ContainerManagementViewModel(IWasteRepository contextRepository, IMapViewModelFactory mapViewModelFactory, IContainerViewModelFactory containerViewModelFactory)
    {
      m_Repository = contextRepository;
      m_MapViewModelFactory = mapViewModelFactory;
      m_ContainerViewModelFactory = containerViewModelFactory;

      AddNewMap = new AddNewMapCommand(this);
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
      Reload();

      FlyoutActivated = true;
    }

    public string FilterContainers
    {
      get { return TranslationProvider.Translate("FilterContainers"); }
    }

    public ICommand AddNewMap { get; set; }

    public IEnumerable<MapItemViewModel> ContainerPositionsOnSelectedMap
    {
      get
      {
        if (SelectedMap != null)
        {
          return m_Container.Where(c => c.Map == SelectedMap.Model)
                            .Select(c => c.ContainerPosition);
        }
        return new MapItemViewModel[0];
      }
    }

    public IEnumerable<ContainerViewModel> FilteredContainers
    {
      get
      {
        var filteredContainers = SearchInContainerList()
          .ToArray();

        if (SelectedMap == null)
        {
          if (m_Maps.Count == 0)
          {
            filteredContainers.ForEach(c => c.MapIndicator = 3);
          }
          else
          {
            filteredContainers.ForEach(c => c.MapIndicator = 2);
          }

          return filteredContainers;
        }

        var containersOnMap = filteredContainers.Where(c => c.Map == SelectedMap.Model)
                                                .ToArray();
        containersOnMap.ForEach(c => c.MapIndicator = 1);

        var containersOnOtherMap = filteredContainers.Where(c => c.Map != SelectedMap.Model && c.Map != null)
                                                     .ToArray();
        containersOnOtherMap.ForEach(c => c.MapIndicator = 2);

        var containersWithoutMap = filteredContainers.Where(c => c.Map == null)
                                                     .ToArray();
        containersWithoutMap.ForEach(c => c.MapIndicator = 3);

        return containersOnMap.Concat(containersWithoutMap)
                              .Concat(containersOnOtherMap);
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

    public IEnumerable<MapViewModel> Maps
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
        NotifyOfPropertyChange(() => FilteredContainers);
      }
    }

    public ContainerViewModel SelectedContainer
    {
      get { return m_SelectedContainer; }
      set
      {
        if (m_SelectedContainer == value || PositionSettingActivated)
        {
          return;
        }
        m_SelectedContainer = value;

        SetSelectedMapItem();
        NotifyOfPropertyChange(() => SelectedContainer);
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
        NotifyOfPropertyChange(() => FilteredContainers);
        NotifyOfPropertyChange(() => ContainerPositionsOnSelectedMap);
      }
    }

    public bool IsMapAddButtonEnabled
    {
      get { return !PositionSettingActivated; }
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
      get { return TranslationProvider.Translate("TitleContainerManagementViewModel"); }
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConnection;
      if (IsEnabled)
      {
        LoadData();
      }
    }

    public void ActivatePositionSetting()
    {
      Mouse.OverrideCursor = Cursors.Cross;
      CloseEditor();
      FlyoutActivated = false;
      PositionSettingActivated = true;
    }

    public void AddContainer(object dataContext)
    {
      var containerAddViewModel = ((ContainerAddViewModel) dataContext);
      m_Repository.Container.Add(containerAddViewModel.Model);
      CloseEditor();
      Save();
      //SelectedContainer = m_Container.LastOrDefault();
    }

    public void AddMap(object dataContext)
    {
      var mapViewModel = ((MapAddViewModel) dataContext);
      m_Repository.Maps.Add(mapViewModel.Model);
      NotifyOfPropertyChange(() => Maps);
      CloseEditor();
      Save();
      SelectedMap = m_Maps.LastOrDefault();
    }

    public void CancelPositionSetting()
    {
      if (PositionSettingActivated)
      {
        OpenEditor(m_EditItem);
        FlyoutActivated = true;
        PositionSettingActivated = false;
        Mouse.OverrideCursor = null;
      }
    }

    public void ChangeSelectedMap(object dataContext)
    {
      var map = ((ContainerViewModel) dataContext).Map;
      if (map != null)
      {
        SelectedMap = m_Maps.Single(m => m.Model == map);
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

    private void CloseEditor()
    {
      m_EditItem.TryClose();
    }

    public void MoveContainer(MapMouseEventArgs e)
    {
      if (PositionSettingActivated)
      {
        var selectedContainer = (ContainerEditViewModel) m_EditItem;
        selectedContainer.X = e.Coordinates.X;
        selectedContainer.Y = e.Coordinates.Y;
        if (SelectedMap != null &&
            selectedContainer.Map != SelectedMap.Model)
        {
          selectedContainer.Map = SelectedMap.Model;
          SelectedContainer = m_Container.Single(c => c.Model == selectedContainer.Model);
        }
        else
        {
          NotifyOfPropertyChange(() => ContainerPositionsOnSelectedMap);
        }
      }
    }

    public void OpenContainerAddDialog()
    {
      OpenEditor(m_ContainerViewModelFactory.CreateAddViewModel(SelectedMap));
    }

    public void OpenContainerEditDialog(object dataContext)
    {
      OpenEditor(m_ContainerViewModelFactory.CreateEditViewModel(((ContainerViewModel) dataContext).Model, RemoveContainer));
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2)
      {
        if (dataContext is ContainerViewModel)
        {
          OpenContainerEditDialog(dataContext);
        }
      }
    }

    public void OpenMapAddDialog()
    {
      var o = new OpenFileDialog();
      o.Filter = "Bilder (.png, .jpg, .gif)|*.png; *.jpg; *.gif";
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

    public void RemoveContainer()
    {
      var containerViewModel = SelectedContainer;

      //var client = new DeleteEvaluationService.DeleteEvaluationServiceClient("BasicHttpBinding_DeleteEvaluationService",
      //                                                          "http://localhost:7000/DeleteEvaluationService");
      //client.CanDelete(containerViewModel.Model.GetType().ToString(), containerViewModel.Model.Id);

      m_Repository.DeleteObject(containerViewModel.Model.MapPosition);
      m_Repository.Container.Remove(containerViewModel.Model);
      CloseEditor();
      Save();
    }

    public void RemoveMap()
    {
      var mapViewModel = SelectedMap;
      foreach (var container in m_Repository.Container.Where(c => c.MapPosition.Map == mapViewModel.Model))
      {
        container.MapPosition.Map = null;
      }
      m_Repository.Maps.Remove(mapViewModel.Model);
      CloseEditor();
      Save();
    }

    private void Save()
    {
      if (m_Repository.Entities.Where(ed => ed.Entity is WasteContainer || ed.Entity is DomainModelService.Map || ed.Entity is MapPosition || ed.Entity is WasteType)
                      .Any(ed => ed.State != EntityStates.Unchanged) ||
          m_Repository.Links.Where(l => l.Source is WasteContainer || l.Source is DomainModelService.Map || l.Source is MapPosition || l.Source is WasteType)
                      .Any(ed => ed.State != EntityStates.Unchanged))
      {
        m_Repository.Save();
      }
    }

    private void ContainerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Map")
      {
        NotifyOfPropertyChange(() => ContainerPositionsOnSelectedMap);
        NotifyOfPropertyChange(() => FilteredContainers);
      }
    }

    private void CreateContainerViewModel(WasteContainer newItem)
    {
      var cvm = m_ContainerViewModelFactory.CreateFromExisiting(newItem);
      cvm.PropertyChanged += ContainerPropertyChanged;
      m_Container.Add(cvm);
    }

    private void CreateMapViewModel(DomainModelService.Map newItem)
    {
      m_Maps.Add(m_MapViewModelFactory.CreateFromExisting(newItem));
    }

    private void LoadContainer()
    {
      foreach (var wasteContainer in m_Repository.Container)
      {
        CreateContainerViewModel(wasteContainer);
      }
      NotifyOfPropertyChange(() => FilteredContainers);
    }

    private void LoadData()
    {
      m_Container.Clear();
      m_Maps.Clear();

      LoadContainer();
      LoadMaps();
    }

    private void LoadMaps()
    {
      foreach (var map in m_Repository.Maps)
      {
        CreateMapViewModel(map);
      }
      NotifyOfPropertyChange(() => Maps);
      SelectedMap = m_Maps.FirstOrDefault();
    }

    private void OpenEditor(object dataContext)
    {
      m_EditItem = (IScreen) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    private IEnumerable<ContainerViewModel> SearchInContainerList()
    {
      if (string.IsNullOrEmpty(SearchText))
      {
        return m_Container;
      }
      var searchText = SearchText.ToLower();
      var searchResult = m_Container.Where(c => (((c.Name != null) && (c.Name.ToLower()
                                                                        .Contains(searchText))) || ((c.Barcode != null) && (c.Barcode.ToLower()
                                                                                                                             .Contains(searchText)))) ||
                                                ((c.SelectedAvvWasteTypes != null) && (c.SelectedAvvWasteTypes.Contains(c.SelectedAvvWasteTypes.FirstOrDefault(wt => wt.Number.ToString()
                                                                                                                                                                       .Contains(searchText))))) ||
                                                ((c.Map != null) && (c.Map.Name.ToLower()
                                                                      .Contains(searchText))));
      return searchResult;
    }

    private void SetSelectedMapItem()
    {
      if (SelectedContainer == null)
      {
        return;
      }
      foreach (var containerViewModel in m_Container)
      {
        containerViewModel.ContainerPosition.IsSelected = containerViewModel.Model == SelectedContainer.Model;
      }
    }
  }
}