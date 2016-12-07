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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Waste.Factories;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Ork.Waste.ViewModels
{
  [Export(typeof (IWorkspace))]
  internal class BillingViewModel : DocumentBase, IWorkspace
  {
    private readonly IContainerViewModelFactory m_ContainerViewModelFactory;
    private readonly IDisposerViewModelFactory m_DisposerViewModelFactory;
    private readonly BindableCollection<DisposerViewModel> m_Disposers = new BindableCollection<DisposerViewModel>();
    private readonly IWasteRepository m_Repository;
    private readonly ISelectableWasteCollectionViewModelFactory m_SelectableWasteCollectionViewModelFactory;
    private readonly BindableCollection<SelectableWasteCollectionViewModel> m_WasteCollections = new BindableCollection<SelectableWasteCollectionViewModel>();
    private Visibility m_DataGridWithDisposerVisibility;
    private Visibility m_DataGridWithoutDisposerVisibility;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private PlotModel m_PlotFinancial;
    private PlotModel m_PlotWeight;
    private string m_SearchTextClosedWasteCollections = string.Empty;
    private string m_SearchTextDisposer;
    private object m_SelectedItem;

    [ImportingConstructor]
    public BillingViewModel(IWasteRepository contextRepository, IDisposerViewModelFactory disposerViewModelFactory, ISelectableWasteCollectionViewModelFactory selectableWasteCollectionViewModelFactory,
      IContainerViewModelFactory containerViewModelFactory)
    {
      m_Repository = contextRepository;
      m_DisposerViewModelFactory = disposerViewModelFactory;
      m_SelectableWasteCollectionViewModelFactory = selectableWasteCollectionViewModelFactory;
      m_ContainerViewModelFactory = containerViewModelFactory;
      IsEnabled = m_Repository.HasConnection;
      FlyoutActivated = true;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);

      Reload();
    }

    public SelectableWasteCollectionViewModel SelectedWasteCollection
    {
      set
      {
        if (value == null)
        {
          return;
        }
        value.IsSelected = !value.IsSelected;
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

    public string SearchTextDisposer
    {
      get { return m_SearchTextDisposer; }
      set
      {
        m_SearchTextDisposer = value;
        NotifyOfPropertyChange(() => FilteredDisposers);
      }
    }


    public string SearchTextClosedWasteCollections
    {
      get { return m_SearchTextClosedWasteCollections; }
      set
      {
        m_SearchTextClosedWasteCollections = value;
        NotifyOfPropertyChange(() => SearchTextClosedWasteCollections);
        NotifyOfPropertyChange(() => FilteredClosedWasteCollectionViewModels);
      }
    }

    public Visibility DataGridWithoutDisposerVisibility
    {
      get { return m_DataGridWithoutDisposerVisibility; }
      set
      {
        m_DataGridWithoutDisposerVisibility = value;
        NotifyOfPropertyChange(() => DataGridWithoutDisposerVisibility);
      }
    }

    public IEnumerable<DisposerViewModel> FilteredDisposers
    {
      get
      {
        if (string.IsNullOrEmpty(SearchTextDisposer))
        {
          return m_Disposers;
        }


        return m_Disposers.Where(d => d.Name.ToLower()
                                       .Contains(SearchTextDisposer.ToLower()))
                          .ToArray();
      }
    }


    public IEnumerable<SelectableWasteCollectionViewModel> ClosedWasteCollectionViewModels
    {
      // Todo create isCompleted Field in WasteCollection
      get { return m_WasteCollections.Where(wc => wc.ActualPrice != 0 || wc.ActualState != 0).ToArray(); }
    }

    public IEnumerable<SelectableWasteCollectionViewModel> FilteredClosedWasteCollectionViewModels
    {
      get
      {
        var wasteCollectionsForSelectedItem = ClosedWasteCollectionViewModels;

        if (SelectedItem is DisposerViewModel)
        {
          wasteCollectionsForSelectedItem = ClosedWasteCollectionViewModels.Where(cwcvm => cwcvm.Model.Disposer == ((DisposerViewModel) SelectedItem).Model);
        }

        else if (SelectedItem is ContainerViewModel)
        {
          wasteCollectionsForSelectedItem = ClosedWasteCollectionViewModels.Where(cwcvm => cwcvm.Model.Container == ((ContainerViewModel) SelectedItem).Model);
        }

        var searchText = SearchTextClosedWasteCollections.ToLower();

        return string.IsNullOrEmpty(SearchTextClosedWasteCollections)
          ? wasteCollectionsForSelectedItem.ToArray()
          : wasteCollectionsForSelectedItem.Where(cwcvm => cwcvm.ContainerText.ToLower()
                                                                .Contains(searchText) || cwcvm.Disposer.Name.ToLower()
                                                                                              .Contains(searchText))
                                           .ToArray();
      }
    }

    public IEnumerable<SelectableWasteCollectionViewModel> SelectedWasteCollectionViewModels
    {
      get
      {
        return m_WasteCollections.Where(wc => wc.IsSelected)
                                 .ToArray();
      }
    }

    public PlotModel PlotModelFinancial
    {
      get
      {
        //todo: optimize initialization
        InitializeFinancialPlot();

        GeneratePlotColumnFor(m_PlotFinancial, "Price");

        m_PlotFinancial.InvalidatePlot(true);
        return m_PlotFinancial;
      }
    }

    public PlotModel PlotModelWeight
    {
      get
      {
        //todo: optimize initialization
        InitializeWeightPlot();

        GeneratePlotColumnFor(m_PlotWeight, "State");

        m_PlotWeight.InvalidatePlot(true);
        return m_PlotWeight;
      }
    }

    public Visibility DataGridWithDisposerVisibility
    {
      get { return m_DataGridWithDisposerVisibility; }
      set
      {
        m_DataGridWithDisposerVisibility = value;
        NotifyOfPropertyChange(() => DataGridWithDisposerVisibility);
      }
    }


    public IEnumerable<object> FilteredItems
    {
      get
      {
        return FilteredDisposers.SelectMany(disposer => disposer.Containers)
                                .Distinct()
                                .Select(m_ContainerViewModelFactory.CreateFromExisiting)
                                .Concat<object>(FilteredDisposers)
                                .ToArray();
      }
    }

    public object SelectedItem
    {
      get { return m_SelectedItem; }
      set
      {
        if (m_SelectedItem == value)
        {
          return;
        }

        DeSelectAllWasteCollections();

        if (value is DisposerViewModel)
        {
          DataGridWithoutDisposerVisibility = Visibility.Visible;
          DataGridWithDisposerVisibility = Visibility.Collapsed;
        }
        else
        {
          DataGridWithoutDisposerVisibility = Visibility.Collapsed;
          DataGridWithDisposerVisibility = Visibility.Visible;
        }

        m_SelectedItem = value;
        NotifyOfPropertyChange(() => SelectedItem);
        NotifyOfPropertyChange(() => FilteredClosedWasteCollectionViewModels);
      }
    }

    public int Index
    {
      get { return 400; }
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
      get { return TranslationProvider.Translate("TitleBillingViewModel"); }
    }

    private void DeSelectAllWasteCollections()
    {
      foreach (var wc in m_WasteCollections)
      {
        wc.IsSelected = false;
      }
      NotifyOfPropertyChange(() => FilteredClosedWasteCollectionViewModels);
    }

    private void WasteCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
    {
      if (propertyChangedEventArgs.PropertyName == "IsSelected")
      {
        NotifyOfPropertyChange(() => PlotModelFinancial);
        NotifyOfPropertyChange(() => PlotModelWeight);
      }
    }

    private void LoadDisposers()
    {
      m_Disposers.Clear();
      var disposers = m_Repository.Disposer.Select(m_DisposerViewModelFactory.CreateDisposerViewModel);
      foreach (var disposer in disposers)
      {
        m_Disposers.Add(disposer);
      }
      NotifyOfPropertyChange(() => FilteredItems);
    }

    private void Reload()
    {
      IsEnabled = m_Repository.HasConnection;
      if (IsEnabled)
      {
        LoadData();
        InitializeFinancialPlot();
        InitializeWeightPlot();
      }
    }

    private void LoadData()
    {
      LoadDisposers();
      LoadWasteCollections();
    }

    private void LoadWasteCollections()
    {
      m_WasteCollections.Clear();
      var wasteCollections = m_Repository.WasteCollections.Select(m_SelectableWasteCollectionViewModelFactory.CreateSelectableWasteCollectionViewModel);
      foreach (var wasteCollection in wasteCollections)
      {
        m_WasteCollections.Add(wasteCollection);
        wasteCollection.PropertyChanged += WasteCollectionOnPropertyChanged;
      }
      NotifyOfPropertyChange(() => FilteredClosedWasteCollectionViewModels);
    }

    public void ShowAllWasteCollections()
    {
      SelectedItem = null;
      SearchTextClosedWasteCollections = "";
    }

    private void InitializeFinancialPlot()
    {
      m_PlotFinancial = SetupPlotModel("ValueCurrency");

      var categoryAxis = CreateCategoryAxis();
      m_PlotFinancial.Axes.Add(categoryAxis);

      var valueAxis = CreateValueAxis();
      m_PlotFinancial.Axes.Add(valueAxis);
    }

    private LinearAxis CreateValueAxis()
    {
      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];
      var lightControlColor = (Color) Application.Current.Resources["LightControlColor"];

      var valueAxis = new LinearAxis(AxisPosition.Left)
                      {
                        ShowMinorTicks = true,
                        MinorGridlineStyle = LineStyle.Dot,
                        MajorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = OxyColor.Parse(lightControlColor.ToString()),
                        MinorGridlineColor = OxyColor.Parse(lightControlColor.ToString()),
                        TicklineColor = OxyColor.Parse(textForegroundColor.ToString()),
                        IsZoomEnabled = false,
                        IsPanEnabled = false,
                        ExtraGridlines = new double[]
                                         {
                                           0
                                         },
                        ExtraGridlineThickness = 1,
                        ExtraGridlineColor = OxyColor.Parse(lightControlColor.ToString()),
                        ExtraGridlineStyle = LineStyle.Solid
                      };
      return valueAxis;
    }

    private PlotModel SetupPlotModel(string title)
    {
      var plotModel = new PlotModel();
      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];

      plotModel.Title = TranslationProvider.Translate(title);
      plotModel.TextColor = OxyColor.Parse(textForegroundColor.ToString());
      plotModel.PlotAreaBorderColor = OxyColor.Parse(textForegroundColor.ToString());
      plotModel.PlotAreaBorderThickness = new OxyThickness(1);

      return plotModel;
    }

    private void InitializeWeightPlot()
    {
      m_PlotWeight = SetupPlotModel("ValueWeight");

      var categoryAxis = CreateCategoryAxis();
      m_PlotWeight.Axes.Add(categoryAxis);

      var valueAxis = CreateValueAxis();
      m_PlotWeight.Axes.Add(valueAxis);
    }

    private CategoryAxis CreateCategoryAxis()
    {
      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];

      var readings = SelectedWasteCollectionViewModels.Select(swcvm => swcvm.ContainerText + "\n" + swcvm.ScheduledDate)
                                                      .ToArray();

      var numberOfReadings = readings.Count();

      CategoryAxis categoryAxis;

      if (numberOfReadings > 3)
      {
        categoryAxis = new CategoryAxis(string.Empty, new string[numberOfReadings]);
      }
      else
      {
        categoryAxis = new CategoryAxis(string.Empty, readings);
      }

      categoryAxis.TicklineColor = OxyColor.Parse(textForegroundColor.ToString());
      categoryAxis.IsZoomEnabled = false;
      categoryAxis.IsPanEnabled = false;
      return categoryAxis;
    }

    private void GeneratePlotColumnFor(PlotModel plot, string property)
    {
      var selectedWasteCollectionViewModels = SelectedWasteCollectionViewModels.ToArray();

      var columnActual = new ColumnSeries
                         {
                           StrokeThickness = 0,
                           FillColor = OxyColors.Purple,
                           IsStacked = false,
                           StrokeColor = OxyColors.Purple,
                           ItemsSource = selectedWasteCollectionViewModels,
                           ValueField = "Actual" + property
                         };

      var columnDesired = new ColumnSeries
                          {
                            StrokeThickness = 0,
                            FillColor = OxyColors.Yellow,
                            IsStacked = false,
                            StrokeColor = OxyColors.Yellow,
                            ItemsSource = selectedWasteCollectionViewModels,
                            ValueField = "Desired" + property
                          };

      plot.Series.Add(columnDesired);
      plot.Series.Add(columnActual);
    }
  }
}