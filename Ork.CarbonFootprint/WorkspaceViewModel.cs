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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Ork.CarbonFootprint.Factories;
using Ork.CarbonFootprint.ViewModels;
using Ork.Framework;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Ork.CarbonFootprint
{
  [Export(typeof (IWorkspace))]
  public class WorkspaceViewModel : DocumentWorkspace<CarbonFootprintViewModel>
  {
    private readonly ICarbonFootprintViewModelFactory m_CarbonFootprintViewModelFactory;
    private readonly ContextRepository m_Repository;
    private readonly TagColorProvider m_TagProvider;
    private CategoryAxis m_CategoryAxis;
    private PlotModel m_ChartModel;
    private bool m_FlyoutActivated;
    private IEnumerable<ResponsibleSubjectViewModel> m_ResponsibleSubjects;
    private String m_SearchText;


    [ImportingConstructor]
    public WorkspaceViewModel([Import] ICarbonFootprintViewModelFactory carbonFootprintViewModelFactory, ContextRepository repository, TagColorProvider tagProvider, IDialogManager dialogs)
    {
      m_CarbonFootprintViewModelFactory = carbonFootprintViewModelFactory;
      m_Repository = repository;

      m_TagProvider = tagProvider;
      Dialogs = dialogs;

      m_TagProvider.ColorsUpdated += (s, e) => NotifyOfPropertyChange(() => ChartModel);
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);

      LoadData();
      FlyoutActivated = true;
    }

    private void Reload()
    {
      Items.Clear();
      LoadData();
      NotifyOfPropertyChange(() => ChartModel);
      NotifyOfPropertyChange(() => FilteredFootprints);
    }

    public override int Index
    {
      get { return 5; }
    }

    public override string Title
    {
      get { return "Carbon Footprints"; }
    }

    public string SearchText
    {
      get { return m_SearchText; }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredFootprints);
      }
    }

    public IEnumerable<CarbonFootprintViewModel> FilteredFootprints
    {
      get
      {
        return SearchInFootprintList()
          .ToArray();
      }
    }

    public IEnumerable<TagColor> TagColors
    {
      get { return m_TagProvider.TagColors; }
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

    public PlotModel ChartModel
    {
      get
      {
        if (m_ChartModel == null)
        {
          InitializeChart();
        }
        GenerateChartData();
        return m_ChartModel;
      }
    }

    public void EditFootprint(object context)
    {
      var carbonFootprint = context as CarbonFootprintViewModel;

      if (carbonFootprint == null)
      {
        return;
      }
      Edit(carbonFootprint);
    }

    public void EditFootprint(object dataContext, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2)
      {
        EditFootprint(dataContext);
      }
    }

    public void New()
    {
      var vm = m_CarbonFootprintViewModelFactory.CreateNew(string.Empty, string.Empty, m_ResponsibleSubjects);
      m_Repository.CarbonFootprints.Add(vm.Model);
      m_Repository.Save();
      Edit(vm);
      NotifyOfPropertyChange(() => FilteredFootprints);
    }

    public void TagColorButton()
    {
      Dialogs.ShowDialog(new TagColorPickerViewModel(m_TagProvider));
    }

    public void Remove(object item)
    {
      var carbonFootprint = item as CarbonFootprintViewModel;
      if (carbonFootprint == null)
      {
        return;
      }

      carbonFootprint.IsSelected = false;
      m_Repository.CarbonFootprints.Remove(carbonFootprint.Model);
      Items.Remove(carbonFootprint);

      m_Repository.Save();
      NotifyOfPropertyChange(() => FilteredFootprints);
    }


    private void LoadData()
    {
      IsEnabled = m_Repository.HasConnection;
      if (!IsEnabled)
      {
        return;
      }

      if (State == DocumentWorkspaceState.Detail)
      {
        DeactivateItem(ActiveItem, true);
      }
      LoadResponsibleSubjects();
      LoadCarbonFootprints();
    }

    private void LoadResponsibleSubjects()
    {
      m_ResponsibleSubjects = m_Repository.ResponsibleSubjects.Select(rs => new ResponsibleSubjectViewModel(rs))
                                          .ToArray();
    }

    private void LoadCarbonFootprints()
    {
      Items.AddRange(m_Repository.CarbonFootprints.Select(cf => m_CarbonFootprintViewModelFactory.CreateFromExisting(cf, m_ResponsibleSubjects))
                                 .ToArray());
      foreach (var carbonFootprintViewModel in Items)
      {
        carbonFootprintViewModel.IsSelectedChanged += (s, e) => NotifyOfPropertyChange(() => ChartModel);
      }
    }

    private IEnumerable<CarbonFootprintViewModel> SearchInFootprintList()
    {
      if (string.IsNullOrEmpty(SearchText))
      {
        return Items;
      }

      var searchText = SearchText.ToLower();
      var searchResult = Items.Where(cfp => (cfp.Name != null) && (cfp.Name.ToLower()
                                                                      .Contains(searchText)));

      return searchResult;
    }

    private void GenerateChartData()
    {
      m_ChartModel.Series.Clear();
      m_CategoryAxis.Labels.Clear();

      var selectedCarbonFootprints = Items.Where(cfvm => cfvm.IsSelected)
                                          .Select(cfvm => cfvm.Model)
                                          .ToArray();

      var allTags = selectedCarbonFootprints.SelectMany(cf => cf.Positions)
                                            .Select(cfp => cfp.Tag)
                                            .Distinct();

      foreach (var columnSeries in allTags.Select(tag => new ColumnSeries
                                                         {
                                                           StrokeThickness = 0,
                                                           Title = tag,
                                                           IsStacked = false,
                                                           FillColor = m_TagProvider.GetColorForTag(tag)
                                                                                    .OxyColor
                                                         }))
      {
        m_ChartModel.Series.Add(columnSeries);
      }

      for (var i = 0; i < selectedCarbonFootprints.Length; i++)
      {
        var carbonFootprint = selectedCarbonFootprints[i];
        m_CategoryAxis.Labels.Add(carbonFootprint.Name);

        var sumByTag = from cfp in carbonFootprint.Positions
                       group cfp by cfp.Tag
                       into tagged
                       select new
                              {
                                ColumnSeries = (ColumnSeries) m_ChartModel.Series.Single(s => s.Title == tagged.Key),
                                CulumnItem = new ColumnItem(tagged.Sum(t => t.Calculation) / 1000, i)
                              };

        foreach (var sum in sumByTag)
        {
          sum.ColumnSeries.Items.Add(sum.CulumnItem);
        }
      }
      m_ChartModel.InvalidatePlot(true);
    }

    private void InitializeChart()
    {
      var textForegroundColor = OxyColor.Parse(((Color) Application.Current.Resources["TextForegroundColor"]).ToString());
      var lightControlColor = OxyColor.Parse(((Color) Application.Current.Resources["LightControlColor"]).ToString());

      m_ChartModel = new PlotModel
                     {
                       Title = "Carbon Footprints",
                       PlotAreaBorderColor = textForegroundColor,
                       TextColor = textForegroundColor,
                       IsLegendVisible = true,
                       LegendOrientation = LegendOrientation.Horizontal,
                       LegendPlacement = LegendPlacement.Outside,
                       LegendPosition = LegendPosition.BottomLeft,
                       PlotAreaBorderThickness = new OxyThickness(1)
                     };

      var valueAxis = new LinearAxis(AxisPosition.Left, 0)
                      {
                        ShowMinorTicks = true,
                        MinorGridlineStyle = LineStyle.Dot,
                        MajorGridlineStyle = LineStyle.Dot,
                        MajorGridlineColor = lightControlColor,
                        MinorGridlineColor = lightControlColor,
                        Title = "kg CO₂",
                        TicklineColor = textForegroundColor,
                        IsZoomEnabled = false,
                        IsPanEnabled = false
                      };

      m_ChartModel.Axes.Add(valueAxis);

      m_CategoryAxis = new CategoryAxis
                       {
                         TicklineColor = textForegroundColor,
                         IsZoomEnabled = false,
                         IsPanEnabled = false,
                         MinorStep = 1
                       };

      m_ChartModel.Axes.Add(m_CategoryAxis);
    }
  }
}