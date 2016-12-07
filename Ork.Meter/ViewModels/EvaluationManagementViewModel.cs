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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Meter.DomainModelService;
using Ork.Meter.Factories;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Ork.Meter.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class EvaluationManagementViewModel : DocumentBase, IWorkspace
  {
    private readonly BindableCollection<MeterReadingViewModel> m_MeterReadings = new BindableCollection<MeterReadingViewModel>();
    private readonly BindableCollection<MeterViewModel> m_Meters = new BindableCollection<MeterViewModel>();
    private readonly IMeterReadingViewModelFactory m_ReadingViewModelFactory;
    private readonly IMeterRepository m_Repository;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private PlotModel m_Plot;
    private string m_SearchText;
    private MeterViewModel m_SelectedMeterViewModel;
    private int m_SelectedYear = DateTime.Now.Year;

    [ImportingConstructor]
    public EvaluationManagementViewModel(IMeterViewModelFactory meterViewModelFactory, IMeterReadingViewModelFactory readingViewModelFactory, IMeterRepository meterRepository)
    {
      m_ReadingViewModelFactory = readingViewModelFactory;
      m_Repository = meterRepository;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
      Reload();

      FlyoutActivated = true;
    }

    public IEnumerable<int> EvaluationYears
    {
      get
      {
        var selectedReadings = GetSelectedReadings()
          .ToArray();
        if (selectedReadings.Any())
        {
          return selectedReadings.Select(flr => flr.OrderDate.Year)
                                 .Distinct();
        }
        return m_MeterReadings.Select(mr => mr.OrderDate.Year)
                              .Distinct();
      }
    }

    public IEnumerable<MeterReadingViewModel> MeterReadings
    {
      get { return m_MeterReadings; }
    }

    public MeterViewModel SelectedMeterViewModel
    {
      get { return m_SelectedMeterViewModel; }
      set
      {
        m_SelectedMeterViewModel = value;

        if (m_SelectedMeterViewModel != null)
        {
          m_SelectedMeterViewModel.IsChecked = !m_SelectedMeterViewModel.IsChecked;
          GetSelectedMeterReadings();
        }
        NotifyOfPropertyChange(() => SelectedMeterViewModel);
      }
    }

    public IEnumerable<MeterReadingViewModel> SelectedMetersReadings
    {
      get
      {
        var selectedMeters = GetSelectedMeters();

        return !selectedMeters.Any()
          ? FilterReadingsBySelectedYear(m_MeterReadings)
            .ToArray()
          : GetSelectedReadingsFilteredByYear()
            .ToArray();
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

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        if (m_SearchText == value)
        {
          return;
        }
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredMeters);
      }
    }


    public int SelectedYear
    {
      get
      {
        if (EvaluationYears.Contains(m_SelectedYear))
        {
          return m_SelectedYear;
        }
        m_SelectedYear = EvaluationYears.FirstOrDefault();
        return m_SelectedYear;
      }
      set
      {
        if (m_SelectedYear == value)
        {
          return;
        }
        m_SelectedYear = value;
        NotifyOfPropertyChange(() => SelectedYear);
        NotifyOfPropertyChange(() => SelectedMetersReadings);
        GenerateGraphData();
      }
    }

    public PlotModel PlotModel
    {
      get
      {
        //todo: increase performance by setting plot only once
        InitializePlot();
        GenerateGraphData();
        return m_Plot;
      }
    }

    public IEnumerable<MeterViewModel> FilteredMeters
    {
      get { return SearchInMeterList(); }
    }

    public int Index
    {
      get { return 300; }
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
      get { return TranslationProvider.Translate("TitleEvaluationManagementViewModel"); }
    }

    private IEnumerable<MeterReadingViewModel> GetSelectedReadingsFilteredByYear()
    {
      return FilterReadingsBySelectedYear(GetSelectedReadings())
        .ToArray();
    }

    private IEnumerable<MeterReadingViewModel> GetSelectedReadings()
    {
      var selectedMeterReadings = new List<MeterReadingViewModel>();

      foreach (var readingsOfMeter in GetSelectedMeters()
        .Select(meter => m_MeterReadings.Where(mr => mr.MeterViewModel.Model == meter.Model)))
      {
        selectedMeterReadings.AddRange(readingsOfMeter);
      }
      return selectedMeterReadings.ToArray();
    }

    private IEnumerable<MeterViewModel> GetSelectedMeters()
    {
      return m_Meters.Where(mvm => mvm.IsChecked)
                     .ToArray();
    }

    private IEnumerable<MeterReadingViewModel> FilterReadingsBySelectedYear(IEnumerable<MeterReadingViewModel> meterReadings)
    {
      return meterReadings.Where(m => m.DueDateBeginDateTime.Year == SelectedYear)
                          .ToArray();
    }

    public void GetSelectedMeterReadings()
    {
      NotifyOfPropertyChange(() => SelectedYear);
      NotifyOfPropertyChange(() => EvaluationYears);
      NotifyOfPropertyChange(() => SelectedMetersReadings);
      GenerateGraphData();
    }


    private void GenerateGraphData()
    {
      m_Plot.Series.Clear();

      var readingsPerMeter = SelectedMetersReadings.Where(m => m.DueDateBeginDateTime.Year == SelectedYear)
                                                   .GroupBy(m => m.MeterName);

      var counter = 0;
      foreach (var reading in readingsPerMeter)
      {
        var lineSerie = new LineSeries
                        {
                          StrokeThickness = 2,
                          MarkerSize = 3,
                          MarkerStroke = m_Plot.DefaultColors[counter % m_Plot.DefaultColors.Count],
                          MarkerType = MarkerType.Plus,
                          CanTrackerInterpolatePoints = false,
                          Title = string.Format(TranslationProvider.Translate("Meter") + " {0}", reading.Key),
                          Smooth = false,
                        };

        reading.ForEach(d => lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(d.DueDateBeginDateTime), d.Value)));
        m_Plot.Series.Add(lineSerie);
        counter++;
      }
      m_Plot.InvalidatePlot(true);
    }

    private void InitializePlot()
    {
      var textForegroundColor = (Color) Application.Current.Resources["TextForegroundColor"];
      var lightControlColor = (Color) Application.Current.Resources["LightControlColor"];
      var workspaceBackgroundColor = (Color) Application.Current.Resources["WorkspaceBackgroundColor"];

      m_Plot = new PlotModel
               {
                 PlotAreaBorderColor = OxyColor.Parse(textForegroundColor.ToString()),
                 LegendTitle = TranslationProvider.Translate(Assembly.GetExecutingAssembly(), "Reading"),
                 LegendOrientation = LegendOrientation.Horizontal,
                 LegendPlacement = LegendPlacement.Outside,
                 LegendPosition = LegendPosition.TopRight,
                 LegendBackground = OxyColor.Parse(workspaceBackgroundColor.ToString()),
                 LegendBorder = OxyColor.Parse(textForegroundColor.ToString()),
                 TextColor = OxyColor.Parse(textForegroundColor.ToString())
               };

      var dateAxis = new DateTimeAxis(AxisPosition.Bottom, TranslationProvider.Translate("ReadingAt"), TranslationProvider.Translate("DateFormat"))
                     {
                       MajorGridlineColor = OxyColor.Parse(lightControlColor.ToString()),
                       TicklineColor = OxyColor.Parse(lightControlColor.ToString()),
                       TitleColor = OxyColor.Parse(textForegroundColor.ToString()),
                       TextColor = OxyColor.Parse(textForegroundColor.ToString()),
                       MajorGridlineStyle = LineStyle.Solid,
                       MinorGridlineStyle = LineStyle.Dot,
                       IntervalLength = 80,
                       IsZoomEnabled = false,
                       IsPanEnabled = false
                     };
      m_Plot.Axes.Add(dateAxis);

      var valueAxis = new LinearAxis(AxisPosition.Left, 0)
                      {
                        MajorGridlineColor = OxyColor.Parse(lightControlColor.ToString()),
                        TicklineColor = OxyColor.Parse(lightControlColor.ToString()),
                        TextColor = OxyColor.Parse(textForegroundColor.ToString()),
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = TranslationProvider.Translate("ValueOfReading"),
                        IsZoomEnabled = false,
                        IsPanEnabled = false
                      };
      m_Plot.Axes.Add(valueAxis);
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
      m_MeterReadings.Clear();
      m_Meters.Clear();
      LoadMeterReadings();
      LoadMeters();
    }

    private void LoadMeterReadings()
    {
      foreach (var reading in m_Repository.MeterReadings.OfType<MeterReading>())
      {
        m_MeterReadings.Add(m_ReadingViewModelFactory.CreateFromExisting(reading));
      }
      NotifyOfPropertyChange(() => EvaluationYears);
      NotifyOfPropertyChange(() => SelectedMetersReadings);
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
                                                                                                                            .Contains(searchText)))));
      return searchResult;
    }

    private void LoadMeters()
    {
      foreach (var result in m_Repository.Meter)
      {
        m_Meters.Add(new MeterViewModel(result));
      }
      NotifyOfPropertyChange(() => FilteredMeters);
    }
  }
}