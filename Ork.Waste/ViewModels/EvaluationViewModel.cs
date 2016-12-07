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
using Caliburn.Micro;
using Ork.Framework;
using Ork.Waste.DomainModelService;
using Ork.Waste.Factories;

namespace Ork.Waste.ViewModels
{
  [Export(typeof (IWorkspace))]
  public class EvaluationViewModel : LocalizableScreen, IWorkspace
  {
    private readonly BindableCollection<FillLevelReadingViewModel> m_FillLevelReadings = new BindableCollection<FillLevelReadingViewModel>();
    private readonly IFillLevelReadingViewModelFactory m_ReadingViewModelFactory;
    private readonly IWasteRepository m_Repository;
    private bool m_FlyoutActivated;
    private bool m_IsEnabled;
    private string m_SearchText;
    private object m_SelectedItem;
    private int m_SelectedYear = DateTime.Now.Year;

    [ImportingConstructor]
    public EvaluationViewModel(IContainerViewModelFactory containerViewModelFactory, IFillLevelReadingViewModelFactory readingViewModelFactory, IWasteRepository wasteRepository)
    {
      m_ReadingViewModelFactory = readingViewModelFactory;
      m_Repository = wasteRepository;
      m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
      Reload();

      FlyoutActivated = true;
    }

    public IEnumerable<int> EvaluationYears
    {
      get
      {
        var filteredReadings = FillLevelReadings;
        if (SelectedItem is SeriesViewModel)
        {
          var series = (SeriesViewModel) SelectedItem;
          filteredReadings = FillLevelReadings.Where(flr => flr.RelatedSeriesViewModel.Model == series.Model);
        }
        else if (SelectedItem is ContainerViewModel)
        {
          var container = (ContainerViewModel) SelectedItem;
          filteredReadings = FillLevelReadings.Where(flr => flr.ContainerViewModel.Model == container.Model);
        }
        return filteredReadings.Select(flr => flr.OrderDate.Year)
                               .Distinct()
                               .ToArray();
      }
    }

    public IEnumerable<FillLevelReadingViewModel> FillLevelReadings
    {
      get { return m_FillLevelReadings; }
    }

    public IEnumerable<FillLevelReadingViewModel> SelectedFillLevelReadings
    {
      get
      {
        if (m_SelectedItem == null)
        {
          return m_FillLevelReadings.OrderBy(flrvm => flrvm.DueDateBeginDateTime)
                                    .ToArray();
        }
        if (m_SelectedItem.IsOfType(typeof (ContainerViewModel)))
        {
          return m_FillLevelReadings.Where(flrvm => flrvm.ContainerViewModel.Name == ((ContainerViewModel) SelectedItem).Name && flrvm.DueDateBeginDateTime.Year == SelectedYear)
                                    .ToArray();
        }
        return m_FillLevelReadings.Where(flrvm => flrvm.RelatedSeriesViewModel.Name == ((SeriesViewModel) SelectedItem).Name && flrvm.DueDateBeginDateTime.Year == SelectedYear)
                                  .OrderBy(flrvm => flrvm.DueDateBeginDateTime)
                                  .ToArray();
      }
    }

    public IEnumerable<ContainerViewModel> FilteredContainers
    {
      get
      {
        if (string.IsNullOrEmpty(SearchText))
        {
          return FillLevelReadings.Select(flr => flr.ContainerViewModel)
                                  .Distinct(model => model.Model);
        }

        var searchText = SearchText.ToLower();
        return FillLevelReadings.Select(flr => flr.ContainerViewModel)
                                .Where(cvm => (cvm.Name.ToLower()
                                                  .Contains(searchText)))
                                .Distinct(model => model.Model)
                                .ToArray();
      }
    }

    public IEnumerable<SeriesViewModel> FilteredSeries
    {
      get
      {
        if (string.IsNullOrEmpty(SearchText))
        {
          return FillLevelReadings.Select(flr => flr.RelatedSeriesViewModel)
                                  .Distinct(model => model.Name);
        }

        var searchText = SearchText.ToLower();
        return FillLevelReadings.Select(flr => flr.RelatedSeriesViewModel)
                                .Where(svm => (svm.Name.ToLower()
                                                  .Contains(searchText)))
                                .Distinct(model => model.Model)
                                .ToArray();
      }
    }

    public IEnumerable<object> FilteredItems
    {
      get { return FilteredSeries.Concat<object>(FilteredContainers); }
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
        NotifyOfPropertyChange(() => FilteredItems);
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
        m_SelectedItem = value;
        NotifyOfPropertyChange(() => SelectedItem);
        NotifyOfPropertyChange(() => EvaluationYears);
        NotifyOfPropertyChange(() => SelectedFillLevelReadings);
      }
    }

    public int SelectedYear
    {
      get { return m_SelectedYear; }
      set
      {
        if (m_SelectedYear == value)
        {
          return;
        }
        m_SelectedYear = value;
        NotifyOfPropertyChange(() => SelectedYear);
        NotifyOfPropertyChange(() => SelectedFillLevelReadings);
      }
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
      get { return TranslationProvider.Translate("TitleEvaluationViewModel"); }
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
      m_FillLevelReadings.Clear();
      LoadFillLevelReadings();
    }

    private void LoadFillLevelReadings()
    {
      foreach (var reading in m_Repository.FillLevelReadings.OfType<FillLevelReading>())
      {
        m_FillLevelReadings.Add(m_ReadingViewModelFactory.CreateFromExisting(reading));
      }
      NotifyOfPropertyChange(() => EvaluationYears);
      NotifyOfPropertyChange(() => FilteredItems);
    }
  }
}