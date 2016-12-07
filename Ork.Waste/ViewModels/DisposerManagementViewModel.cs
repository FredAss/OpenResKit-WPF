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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Waste.DomainModelService;
using Ork.Waste.Factories;

namespace Ork.Waste.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class DisposerManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly IAvvWasteTypeProvider m_AvvWasteTypeProvider;
    private readonly BindableCollection<WasteCollectionContainerViewModel> m_Containers = new BindableCollection<WasteCollectionContainerViewModel>();
    private readonly IDisposerViewModelFactory m_DisposerViewModelFactory;
    private readonly BindableCollection<DisposerViewModel> m_Disposers = new BindableCollection<DisposerViewModel>();
    private readonly IWasteRepository m_Repository;
    private readonly ISelectableWasteCollectionViewModelFactory m_SelectableWasteCollectionViewModelFactory;
    private readonly IWasteCollectionContainerViewModelFactory m_WasteCollectionContainerViewModelFactory;
    private readonly IWasteCollectionFinishViewModelFactory m_WasteCollectionFinishViewModelFactory;
    private readonly IWasteCollectionGenerationViewModelFactory m_WasteCollectionGenerationViewModelFactory;
    private readonly BindableCollection<SelectableWasteCollectionViewModel> m_WasteCollections = new BindableCollection<SelectableWasteCollectionViewModel>();
    private Visibility m_DataGridWithDisposerVisible;
    private Visibility m_DataGridWithoutDisposerVisible;
    private IScreen m_EditItem;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private string m_SearchTextContainer;
    private string m_SearchTextDisposer;
    private string m_SearchTextOpenWasteCollections;
    private DisposerViewModel m_SelectedDisposer;

    [ImportingConstructor]
    public DisposerManagementViewModel(IWasteRepository contextRepository, IDisposerViewModelFactory disposerViewModelFactory,
      IWasteCollectionContainerViewModelFactory wasteCollectionContainerViewModelFactory, IWasteCollectionGenerationViewModelFactory wasteCollectionGenerationViewModelFactory,
      ISelectableWasteCollectionViewModelFactory selectableWasteCollectionViewModelFactory, IWasteCollectionFinishViewModelFactory wasteCollectionFinishViewModelFactory,
      IAvvWasteTypeProvider avvWasteTypeProvider)
    {
      m_Repository = contextRepository;
      m_DisposerViewModelFactory = disposerViewModelFactory;
      m_WasteCollectionContainerViewModelFactory = wasteCollectionContainerViewModelFactory;
      m_WasteCollectionGenerationViewModelFactory = wasteCollectionGenerationViewModelFactory;
      m_SelectableWasteCollectionViewModelFactory = selectableWasteCollectionViewModelFactory;
      m_WasteCollectionFinishViewModelFactory = wasteCollectionFinishViewModelFactory;
      m_AvvWasteTypeProvider = avvWasteTypeProvider;
      IsEnabled = m_Repository.HasConnection;
      FlyoutActivated = true;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
      Reload();
    }

    private bool IsSaveEnabled { get; set; }

    public IEnumerable<DisposerViewModel> FilteredDisposers
    {
      get
      {
        if (string.IsNullOrEmpty(SearchTextDisposer))
        {
          return m_Disposers;
        }


        return m_Disposers.Where(d => d.Name.ToLower()
                                       .Contains(SearchTextDisposer.ToLower()));
      }
    }

    public IEnumerable<SelectableWasteCollectionViewModel> WasteCollections
    {
      get { return m_WasteCollections; }
    }


    public string SearchTextOpenWasteCollections
    {
      get { return m_SearchTextOpenWasteCollections; }
      set
      {
        m_SearchTextOpenWasteCollections = value;
        NotifyOfPropertyChange(() => FilteredOpenWasteCollectionViewModels);
      }
    }

    public IEnumerable<SelectableWasteCollectionViewModel> FilteredOpenWasteCollectionViewModels
    {
      get
      {
        if (SelectedDisposer == null)
        {
          return string.IsNullOrEmpty(SearchTextOpenWasteCollections)
            ? OpenWasteCollectionViewModels
            : OpenWasteCollectionViewModels.Where(owcvm => owcvm.ContainerText.ToLower()
                                                                .Contains(SearchTextOpenWasteCollections.ToLower()));
        }

        return string.IsNullOrEmpty(SearchTextOpenWasteCollections)
          ? OpenWasteCollectionViewModels
          : OpenWasteCollectionViewModels.Where(owcvm => owcvm.ContainerText.ToLower()
                                                              .Contains(SearchTextOpenWasteCollections.ToLower()) && owcvm.Model.Disposer == SelectedDisposer.Model);
      }
    }

    public IEnumerable<SelectableWasteCollectionViewModel> OpenWasteCollectionViewModels
    {
      get { return WasteCollections.Where(wc => wc.ActualState == 0); }
    }

    public DisposerViewModel SelectedDisposer
    {
      get { return m_SelectedDisposer; }
      set
      {
        if (value != null)
        {
          DataGridWithoutDisposerVisibile = Visibility.Visible;
          DataGridWithDisposerVisibile = Visibility.Hidden;
        }
        else
        {
          DataGridWithoutDisposerVisibile = Visibility.Hidden;
          DataGridWithDisposerVisibile = Visibility.Visible;
        }

        m_SelectedDisposer = value;
        NotifyOfPropertyChange(() => FilteredContainers);
      }
    }

    public IEnumerable<WasteCollectionContainerViewModel> FilteredContainers
    {
      get
      {
        if (string.IsNullOrEmpty(SearchTextContainer) &&
            SelectedDisposer == null)
        {
          return m_Containers;
        }

        if (SelectedDisposer == null)
        {
          return m_Containers.Where(d => d.Name.ToLower()
                                          .Contains(SearchTextContainer.ToLower()));
        }
        else
        {
          return m_Containers.Where(wccvm => wccvm.Name.ToLower()
                                                  .Contains(SearchTextContainer.ToLower()) && SelectedDisposer.Containers.Any(c => c == wccvm.ContainerViewModel.Model));
        }
      }
    }


    public string SearchTextDisposer
    {
      get { return m_SearchTextDisposer; }
      set
      {
        m_SearchTextDisposer = value;
        NotifyOfPropertyChange(() => FilteredDisposers);
      }
    }

    public string SearchTextContainer
    {
      get { return m_SearchTextContainer; }
      set
      {
        m_SearchTextContainer = value;
        NotifyOfPropertyChange(() => FilteredContainers);
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

    public bool IsExecutionEnabled
    {
      get { return FilteredContainers.Any(fc => fc.IsSelected); }
    }

    public bool IsFinishEnabled
    {
      get { return FilteredOpenWasteCollectionViewModels.Any(fc => fc.IsSelected); }
    }

    public Visibility DataGridWithoutDisposerVisibile
    {
      get { return m_DataGridWithoutDisposerVisible; }
      set
      {
        m_DataGridWithoutDisposerVisible = value;
        NotifyOfPropertyChange(() => DataGridWithoutDisposerVisibile);
      }
    }

    public Visibility DataGridWithDisposerVisibile
    {
      get { return m_DataGridWithDisposerVisible; }
      set
      {
        m_DataGridWithDisposerVisible = value;
        NotifyOfPropertyChange(() => DataGridWithDisposerVisibile);
      }
    }

    public int Index
    {
      get { return 301; }
    }

    public string Title
    {
      get { return TranslationProvider.Translate("TitleWasteCollectionViewModel"); }
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

    public void SaveWasteCollection()
    {
      if (IsSaveEnabled)
      {
        m_Repository.Save();
        IsSaveEnabled = false;
      }
    }

    private void WasteCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (propertyChangedEventArgs.PropertyName == "IsSelected")
      {
        NotifyOfPropertyChange(() => IsFinishEnabled);
      }
    }

    private void LoadWasteCollections()
    {
      m_WasteCollections.Clear();
      //TODO: Ordentliche Auswahlkriterien für WasteCollections
      var wasteCollections = m_Repository.WasteCollections.Select(m_SelectableWasteCollectionViewModelFactory.CreateSelectableWasteCollectionViewModel);
      foreach (var wasteCollection in wasteCollections)
      {
        wasteCollection.PropertyChanged += WasteCollectionOnPropertyChanged;
        m_WasteCollections.Add(wasteCollection);
      }
      NotifyOfPropertyChange(() => WasteCollections);
      NotifyOfPropertyChange(() => FilteredOpenWasteCollectionViewModels);
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConnection;
      if (IsEnabled)
      {
        LoadData();
      }
    }

    private void LoadData()
    {
      LoadDisposersAndWasteContainers();
      LoadWasteCollections();
    }

    private void CreateContainers(Disposer disposer, IEnumerable<WasteContainer> wasteContainers)
    {
      var containers = wasteContainers.Select(m_WasteCollectionContainerViewModelFactory.CreateWasteCollectionContainerViewModel);
      foreach (var container in containers)
      {
        container.PropertyChanged += ContainerOnPropertyChanged;
        container.Disposer = disposer;
        m_Containers.Add(container);
      }
    }

    private void ContainerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (propertyChangedEventArgs.PropertyName == "IsSelected")
      {
        NotifyOfPropertyChange(() => IsExecutionEnabled);
      }
    }

    private void LoadDisposersAndWasteContainers()
    {
      m_Disposers.Clear();
      m_Containers.Clear();

      var disposers = m_Repository.Disposer.Select(m_DisposerViewModelFactory.CreateDisposerViewModel);
      foreach (var disposer in disposers)
      {
        m_Disposers.Add(disposer);
        CreateContainers(disposer.Model, disposer.Containers);
      }
      NotifyOfPropertyChange(() => FilteredDisposers);
      NotifyOfPropertyChange(() => FilteredContainers);
    }

    public void OpenDisposerAddDialog()
    {
      OpenEditor(m_DisposerViewModelFactory.CreateDisposerAddViewModel(new Disposer()));
    }

    private void OpenEditor(object dataContext)
    {
      m_EditItem = (IScreen) dataContext;
      Dialogs.ShowDialog(m_EditItem);
    }

    public void AddDisposer(object dataContext)
    {
      var disposerAddViewModel = ((DisposerAddViewModel) dataContext);
      disposerAddViewModel.Accept();
      m_Repository.Disposer.Add(disposerAddViewModel.Model);
      Save();

      LoadDisposersAndWasteContainers();
      SelectedDisposer = m_Disposers.Last();
    }

    public void EditDisposer(DisposerEditViewModel disposerEditViewModel)
    {
      disposerEditViewModel.Accept();

      LoadDisposersAndWasteContainers();
    }

    public void RemoveDisposer()
    {
      m_Repository.Disposer.Remove(SelectedDisposer.Model);
      CloseEditor();
      NotifyOfPropertyChange(() => FilteredDisposers);
      Save();
    }

    public void CloseEditor()
    {
      m_EditItem.TryClose();
    }

    private void Save()
    {
      m_Repository.Save();
    }

    public void OpenDisposerEditDialog(object dataContext)
    {
      OpenEditor(m_DisposerViewModelFactory.CreateDisposerEditViewModel(((DisposerViewModel) dataContext).Model, RemoveDisposer));
    }

    public void OpenEditor(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount < 2)
      {
        return;
      }
      if (dataContext is DisposerViewModel)
      {
        OpenDisposerEditDialog(SelectedDisposer);
      }
    }

    public void ExecuteCollection()
    {
      var disposers = GetDisposersFromSelectedContainers();
      var wasteCollections = (from disposer in disposers
                              let containers = GetSelectedContainersBelongingToDisposer(disposer)
                              from wasteContainer in containers
                              select GenerateWasteCollection(disposer.Model, wasteContainer)).ToArray();

      OpenEditor(m_WasteCollectionGenerationViewModelFactory.CreateWasteCollectionGenerationViewModel(wasteCollections));
    }


    public void ExecuteGeneration(WasteCollectionGenerationViewModel wasteCollectionGenerationViewModel)
    {
      foreach (var wasteCollectionViewModel in wasteCollectionGenerationViewModel.WasteCollections)
      {
        wasteCollectionViewModel.Model.ScheduledDate = DateTime.Now; //Das Feld ist wohl überflüssig, wird nur initialisiert um Fehler bei beim speichern zu verhindern.
        m_Repository.WasteCollections.Add(wasteCollectionViewModel.Model);
      }
      m_Repository.Save();
      CloseEditor();

      var disposers = GetDisposersFromSelectedContainers();

      foreach (var disposer in disposers)
      {
        var containers = GetSelectedContainersBelongingToDisposer(disposer);
        CreateEmail(disposer, containers);
      }
    }

    private WasteCollection GenerateWasteCollection(Disposer disposer, WasteContainer wasteContainer)
    {
      var wasteCollection = new WasteCollection();
      wasteCollection.Disposer = disposer;
      wasteCollection.Container = wasteContainer;
      wasteCollection.GenerationDate = DateTime.Now;

      return wasteCollection;
    }

    private IEnumerable<WasteContainer> GetSelectedContainersBelongingToDisposer(DisposerViewModel disposer)
    {
      var selectedContainerModels = FilteredContainers.Where(fc => fc.IsSelected)
                                                      .Select(selectedContainer => selectedContainer.ContainerViewModel.Model);
      var containers = disposer.Containers.Where(selectedContainerModels.Contains)
                               .ToArray();
      return containers;
    }

    private IEnumerable<DisposerViewModel> GetDisposersFromSelectedContainers()
    {
      var selectedContainerModels = FilteredContainers.Where(fc => fc.IsSelected)
                                                      .Select(selectedContainer => selectedContainer.ContainerViewModel.Model);
      var disposers = m_Disposers.Where(d => d.Containers.Intersect(selectedContainerModels)
                                              .Any())
                                 .Distinct()
                                 .ToArray();
      return disposers;
    }

    public void ShowAllContainers()
    {
      SelectedDisposer = null;
      NotifyOfPropertyChange(() => FilteredContainers);
    }

    private void CreateEmail(DisposerViewModel disposer, IEnumerable<WasteContainer> containers)
    {
      var stringBuilder = new StringBuilder();

      stringBuilder.Append("Sehr geehrte Damen und Herren,%0D%0A%0D%0A%09");
      stringBuilder.Append("hiermit veranlassen wir die Abholung der folgenden Container:%0D%0A%0D%0A%09");

      foreach (var wasteContainer in containers)
      {
        stringBuilder.Append("Containerbezeichnung: " + wasteContainer.Name + "%0D%0A");
        stringBuilder.Append("%09Containerbarcode: " + wasteContainer.Barcode + "%0D%0A");

        foreach (var wasteType in wasteContainer.WasteTypes)
        {
          var selectedWasteType = m_AvvWasteTypeProvider.AvvWasteTypes.First(wt => wt.Id == wasteType.AvvId);
          stringBuilder.Append("%09Abfallart: " + selectedWasteType.Name + " (AVV-Nummer: " + selectedWasteType.Number + ")%0D%0A");
        }

        stringBuilder.Append("%09Containergröße: " + wasteContainer.Size + "m³%0D%0A%0D%0A");
      }
      stringBuilder.Append("Wir bitten um eine kurze Eingangsbestätigung dieser E-Mail, gern auch telefonisch!%0D%0A%0D%0A");
      stringBuilder.Append("Mit freundlichen Grüßen");

      var disposerMail = disposer.EMail;
      var containerCount = containers.Count();

      var mailto = "mailto:" + disposerMail + "?subject=Abholung%20von%20" + containerCount + "%20Containern&body=" + stringBuilder;
      Process.Start(mailto);
    }

    public void FinishWasteCollections()
    {
      var selectedWasteCollections = WasteCollections.Where(wc => wc.IsSelected);

      OpenEditor(m_WasteCollectionFinishViewModelFactory.CreateWasteCollectionFinishViewModel(selectedWasteCollections));
    }

    public void SaveFinishedWasteCollections()
    {
      m_Repository.Save();
      CloseEditor();
      NotifyOfPropertyChange(() => FilteredOpenWasteCollectionViewModels);
    }
  }
}