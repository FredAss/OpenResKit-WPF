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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Ork.Approval.DomainModelService;
using Ork.Framework;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Ork.Approval.ViewModels
{
    [Export(typeof(IWorkspace))]
    public class EvaluationManagementViewModel : LocalizableScreen, IWorkspace
    {
        private IApprovalRepository m_Repository;
        private BindableCollection<PlantPermissionViewModel> m_PlantPermissionViewModels = new BindableCollection<PlantPermissionViewModel>();
        private ObservableCollection<AuxillaryConditionViewModel> m_AuxillaryConditionViewModels = new ObservableCollection<AuxillaryConditionViewModel>(); 
        private ObservableCollection<ConditionInspectionViewModel> m_ConditionInspectionViewModels = new ObservableCollection<ConditionInspectionViewModel>();
        private ObservableCollection<MeasureViewModel> m_MeasureViewModels = new ObservableCollection<MeasureViewModel>(); 
        private PlantPermissionViewModel m_SelectedPlantPermissionViewModel;
        private PlotModel m_PieModelAuxillaryTotal;
        private PlotModel m_PieModelMeasuresTotal;
        private PlotModel m_LineModelSelectedPermission;
        private string m_SearchText;

            
        [ImportingConstructor]
        public EvaluationManagementViewModel([Import] IApprovalRepository repository)
        {
            m_Repository = repository;
            m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(LoadData);
            LoadData();
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set
            {
                m_SearchText = value;
                NotifyOfPropertyChange(() => FilteredPlantPermissionViewModels);
            }
        }

        public IEnumerable<PlantPermissionViewModel> FilteredPlantPermissionViewModels
        {
            get { return SearchInPlantPermissionViewModels(); }
        }

        private IEnumerable<PlantPermissionViewModel> SearchInPlantPermissionViewModels()
        {
            var plantPermissionViewModelsResult = FilterPlantPermissionViewModelsBySearchText();

            //if (plantViewModelsResult.Any())
            //    SelectedPlantViewModel = plantViewModelsResult.First();

            return plantPermissionViewModelsResult;
        }

        private IEnumerable<PlantPermissionViewModel> FilterPlantPermissionViewModelsBySearchText()
        {
            var searchText = SearchText.ToLower();
            var plantpermissionViewModelsResult = m_PlantPermissionViewModels.Where(pvm =>
                        ((pvm.Plant.Name != null) && (pvm.Plant.Name.ToLower().Contains(searchText))) ||
                        ((pvm.Plant.Number != null) && (pvm.Plant.Number.ToLower().Contains(searchText))));

            return plantpermissionViewModelsResult;
        }

        public string CountOfPermissions
        {
            get
            {
                if (m_SelectedPlantPermissionViewModel != null)
                {
                    return string.Format(TranslationProvider.Translate("TotalPermissionDescription"),
                        m_SelectedPlantPermissionViewModel.Plant.Permissions.Count);
                }

                return string.Format(TranslationProvider.Translate("TotalPermissionDescription"), m_Repository.Permissions.Count);
            }
        }

        public string CountOfAC
        {
            get
            {
                if (m_SelectedPlantPermissionViewModel != null)
                {
                    var count = m_SelectedPlantPermissionViewModel.Permission.AuxillaryConditions.Count;

                    var coll = new Collection<Permission>();
                    coll.Add(m_SelectedPlantPermissionViewModel.Permission);

                    return string.Format(TranslationProvider.Translate("TotalACDescription"), count, GetCorrectACs(coll));
                }


                return string.Format(TranslationProvider.Translate("TotalACDescription"), m_Repository.AuxillaryConditions.Count, GetCorrectACs(m_Repository.Permissions));
            }
        }

        public string CountOfCorrectAC
        {
            get
            {
                if (m_SelectedPlantPermissionViewModel != null)
                    return GetCorrectACs(m_SelectedPlantPermissionViewModel.Plant.Permissions).ToString();

                return GetCorrectACs(m_Repository.Permissions).ToString();
            }
        }

        public string CountOfMeasures
        {
            get
            {
                if (m_SelectedPlantPermissionViewModel != null)
                {
                    var cis =
                        m_SelectedPlantPermissionViewModel.Permission.AuxillaryConditions.SelectMany(
                            ac => ac.ConditionInspections);

                    var measures = cis.SelectMany(ci => ci.Measures);

                    var countOfUndoneMeasures = measures.Count(m => m.Progress != 2);

                    return string.Format(TranslationProvider.Translate("TotalMeasureDescription"), countOfUndoneMeasures);
                }

                return string.Format(TranslationProvider.Translate("TotalMeasureDescription"), m_Repository.Measures.Where(m => m.Progress != 2).Count());
            }    
        }

        private int GetCorrectACs(Collection<Permission> Permissions)
        {
            var countOfLegalAcs = 0;

            foreach (var permission in Permissions)
            {
                var a = permission.AuxillaryConditions.SelectMany(ac => ac.ConditionInspections.Where(ci => ci.EntryDate != new DateTime())).OrderByDescending(ci => ci.EntryDate);

                var acIds = a.Select(ci => ci.AuxillaryConditionId).Distinct();

                foreach (var acId in acIds)
                {
                    var cis = a.Where(ci => ci.AuxillaryConditionId == acId).OrderByDescending(ci => ci.EntryDate);

                    if (cis.First().Status)
                        countOfLegalAcs++;
                }
            }

            return countOfLegalAcs;
        }

        public int Index
        {
            get { return 1; }
        }

        public bool IsEnabled
        {
            get { return true; }
        }

        public string Title
        {
            get { return "Dashboard"; }
        }

        public BindableCollection<PlantPermissionViewModel> PlantPermissionViewModels
        {
            get { return m_PlantPermissionViewModels; }
        }

        public PlantPermissionViewModel SelectedPlantPermissionViewModel
        {
            get { return m_SelectedPlantPermissionViewModel; }
            set
            {
                m_SelectedPlantPermissionViewModel = value;
                NotifyOfPropertyChange(() => SelectedPlantPermissionViewModel);
                NotifyOfPropertyChange(() => ConditionInspectionsViewModels);
                NotifyOfPropertyChange(() => ColumnModelFromPermission);
                NotifyOfPropertyChange(() => IsVisible);
                NotifyOfPropertyChange(() => IsVisibleSelected);
                NotifyOfPropertyChange(() => CountOfPermissions);
                NotifyOfPropertyChange(() => CountOfCorrectAC);
                NotifyOfPropertyChange(() => CountOfAC);
                NotifyOfPropertyChange(() => CountOfMeasures);
            }
        }

        public IEnumerable ConditionInspectionsViewModels
        {
            get
            {
                if (m_SelectedPlantPermissionViewModel == null)
                    return null;

                var a = m_SelectedPlantPermissionViewModel.Permission.AuxillaryConditions.SelectMany(ac => m_ConditionInspectionViewModels.Where(ci => ci.ModelAuxillaryCondition.Model.Id == ac.Id));

                var idList = a.Select(ci => ci.ModelAuxillaryCondition.Model.Id).Distinct();
                
                Collection<ConditionInspectionViewModel> ciList = new Collection<ConditionInspectionViewModel>();

                foreach (var i in idList)
                {
                    var latestCIofCondition =
                        a.Where(ci => ci.ModelAuxillaryCondition.Model.Id == i).OrderByDescending(ci => ci.EntryDate).First();

                    ciList.Add(latestCIofCondition);
                }

                return ciList;
                
            }
        }

        public string IsVisible
        {
            get
            {
                if (m_SelectedPlantPermissionViewModel != null)
                {
                    return "Hidden";
                }
                else
                {
                    return "Visible";
                }
            }
        }

        public string IsVisibleSelected
        {
            get
            {
                if (m_SelectedPlantPermissionViewModel != null)
                {
                    return "Visible";
                }
                else
                {
                    return "Hidden";
                }
            }
        }

        public void ShowTotalEvaluation()
        {
            m_SelectedPlantPermissionViewModel = null;
            NotifyOfPropertyChange(() => SelectedPlantPermissionViewModel);
            NotifyOfPropertyChange(() => IsVisible);
            NotifyOfPropertyChange(() => IsVisibleSelected);

            NotifyOfPropertyChange(() => CountOfPermissions);
            NotifyOfPropertyChange(() => CountOfCorrectAC);
            NotifyOfPropertyChange(() => CountOfAC);
            NotifyOfPropertyChange(() => CountOfMeasures);
        }

        private void LoadData()
        {
            LoadPlantPermissions();
            LoadAuxillaryConditions();
            LoadConditionInspections();
            LoadMeasures();

            NotifyOfPropertyChange(() => FilteredPlantPermissionViewModels);
        }

        private void LoadMeasures()
        {
            if (m_Repository.Measures != null)
            {
                foreach (var measure in m_Repository.Measures)
                {
                    m_MeasureViewModels.Add(new MeasureViewModel(measure));
                }
            }
            
        }

        private void LoadPlantPermissions()
        {
            foreach (var plant in m_Repository.Plants)
            {
                foreach (var permission in m_Repository.Permissions)
                {
                    foreach (var t1 in permission.Plants)
                    {
                        if (plant.Id == t1.Id)
                        {
                            m_PlantPermissionViewModels.Add(CreatePlantPermissionViewModel(plant, permission));
                        }
                    }
                }
            }

            NotifyOfPropertyChange(() => PlantPermissionViewModels);
        }

        private void LoadAuxillaryConditions()
        {
            if (m_Repository.AuxillaryConditions != null)
            {
                foreach (var auxillaryCondition in m_Repository.AuxillaryConditions)
                {
                    m_AuxillaryConditionViewModels.Add(new AuxillaryConditionViewModel(auxillaryCondition));
                }
            }
        }

        private void LoadConditionInspections()
        {
            if (m_Repository.ConditionInspections != null)
            {
                foreach (var conditionInspection in m_Repository.ConditionInspections)
                {
                    var auxillaryCondition =
                        m_AuxillaryConditionViewModels.Single(
                            acvm => acvm.Model.Id == conditionInspection.AuxillaryConditionId);

                    m_ConditionInspectionViewModels.Add(new ConditionInspectionViewModel(conditionInspection, null, auxillaryCondition));
                }
            }
        }

        private PlantPermissionViewModel CreatePlantPermissionViewModel(Plant plant, Permission permission)
        {
            return new PlantPermissionViewModel(plant, permission);
        }

        public PlotModel PieModelAuxillary
        {
            get
            {
                InitializePlot();
                GeneratePlotDataForAuxillary();
                return m_PieModelAuxillaryTotal;
            }
        }

        public PlotModel PieModelMeasures
        {
            get
            {
                InitializePlot();
                GeneratePlotDataForMeasures();
                return m_PieModelMeasuresTotal;
            }
        }

        public PlotModel ColumnModelFromPermission
        {
            get
            {
                CreateLinePlotModel();
                GenerateGraphColumnData();
                return m_LineModelSelectedPermission;
            }
        }

        public PlotModel LineModelSelectedPermission
        {
            get
            {
                CreateLinePlotModel();
                GenerateGraphColumnData();
                return m_LineModelSelectedPermission;
            }
        }

        private void GenerateGraphColumnData()
        {
            if (m_SelectedPlantPermissionViewModel == null)
                return;

            var textForegroundColor = (Color)Application.Current.Resources["TextForegroundColor"];
            var lightControlColor = (Color)Application.Current.Resources["LightControlColor"];

            var countOfAuxillaryConditions = m_SelectedPlantPermissionViewModel.Permission.AuxillaryConditions.Count;

            var civm =
                m_SelectedPlantPermissionViewModel.Permission.AuxillaryConditions.SelectMany(
                    ac => m_ConditionInspectionViewModels.Where(ci => ci.ModelAuxillaryCondition.Model.Id == ac.Id));

            var civmIds = civm.Select(ci => ci.ModelAuxillaryCondition.Model.Id).Distinct();

            var civmTrue = new Collection<ConditionInspectionViewModel>();
            var civmFalse = new Collection<ConditionInspectionViewModel>();

            //var civmTrue = civm.Where(ci => ci.Status);
            //var civmFalse = civm.Where(ci => ci.Status == false);

            foreach (var conditionInspectionViewModel in civmIds)
            {
                var civmOfId = civm.Where(ci => ci.ModelAuxillaryCondition.Model.Id == conditionInspectionViewModel).OrderByDescending(ci => ci.EntryDate).First();

                if(civmOfId.Status)
                    civmTrue.Add(civmOfId);
                else
                    civmFalse.Add(civmOfId);
            }

            var columnTotalCount = new ColumnSeries()
            {
                StrokeThickness = 0,
                FillColor = OxyColors.Yellow,
                IsStacked = true,
                StrokeColor = OxyColors.Yellow
            };

            var columnTrueCount = new ColumnSeries()
            {
                StrokeThickness = 0,
                FillColor = OxyColors.Green,
                IsStacked = true,
                StrokeColor = OxyColors.Green
            };

            var columnFalseCount = new ColumnSeries()
            {
                StrokeThickness = 0,
                FillColor = OxyColors.Red,
                IsStacked = true,
                StrokeColor = OxyColors.Red
            };

            columnTotalCount.Items.Add(new ColumnItem(countOfAuxillaryConditions, 0));
            columnTrueCount.Items.Add(new ColumnItem(civmTrue.Count(), 1));
            columnFalseCount.Items.Add(new ColumnItem(civmFalse.Count(), 2));

            m_LineModelSelectedPermission.Series.Add(columnTotalCount);
            m_LineModelSelectedPermission.Series.Add(columnTrueCount);
            m_LineModelSelectedPermission.Series.Add(columnFalseCount);

        }

       private void InitializePlot()
        {
            m_PieModelAuxillaryTotal = CreatePiePlotModel();
            m_PieModelMeasuresTotal = CreatePiePlotModel();   
        }

        private void CreateLinePlotModel()
        {
            var textForegroundColor = (Color)Application.Current.Resources["TextForegroundColor"];
            var lightControlColor = (Color)Application.Current.Resources["LightControlColor"];
            var workspaceBackgroundColor = (Color)Application.Current.Resources["WorkspaceBackgroundColor"];

                  

            m_LineModelSelectedPermission = new PlotModel
            {
                PlotAreaBorderColor = OxyColor.Parse(textForegroundColor.ToString()),
                Title = TranslationProvider.Translate("LatestPermission"),
                //LegendTitle = "Permission",
                //LegendOrientation = LegendOrientation.Horizontal,
                //LegendPlacement = LegendPlacement.Outside,
                //LegendPosition = LegendPosition.TopRight,
                //LegendBackground = OxyColor.Parse(workspaceBackgroundColor.ToString()),
                //LegendBorder = OxyColor.Parse(textForegroundColor.ToString()),
                TextColor = OxyColor.Parse(textForegroundColor.ToString())
            };

            var categoryAxis = new CategoryAxis(TranslationProvider.Translate("Status"), new string[]{TranslationProvider.Translate("TotalAC"), TranslationProvider.Translate("TrueAC"), TranslationProvider.Translate("FalseAC")})
            {
                TicklineColor = OxyColor.Parse(textForegroundColor.ToString()),
                IsZoomEnabled = false,
                IsPanEnabled = false
            };


            m_LineModelSelectedPermission.Axes.Add(categoryAxis);

            var valueAxis = new LinearAxis(AxisPosition.Left, 0)
                      {
                        MajorGridlineColor = OxyColor.Parse(lightControlColor.ToString()),
                        TicklineColor = OxyColor.Parse(lightControlColor.ToString()),
                        TextColor = OxyColor.Parse(textForegroundColor.ToString()),
                        MajorGridlineStyle = LineStyle.Solid,
                        MinorGridlineStyle = LineStyle.Dot,
                        Title = TranslationProvider.Translate("Count"),
                        IsZoomEnabled = false,
                        IsPanEnabled = false
                      };
      m_LineModelSelectedPermission.Axes.Add(valueAxis);

        }

        private PlotModel CreatePiePlotModel()
        {
            var textForegroundColor = (Color)Application.Current.Resources["TextForegroundColor"];
            var lightControlColor = (Color)Application.Current.Resources["LightControlColor"];
            var workspaceBackgroundColor = (Color)Application.Current.Resources["WorkspaceBackgroundColor"];

            var piePlotModel = new PlotModel
            {
                PlotAreaBorderColor = OxyColor.Parse(textForegroundColor.ToString()),
                TextColor = OxyColor.Parse(textForegroundColor.ToString()),
                TitleHorizontalAlignment = TitleHorizontalAlignment.CenteredWithinPlotArea
            };
            return piePlotModel;
        }

        private void GeneratePlotDataForAuxillary()
        {
            /*if (SelectedPlantPermissionViewModel == null)
            {
                return;
            }*/

            m_PieModelAuxillaryTotal.Series.Clear();
            
            var civm = m_ConditionInspectionViewModels.Where(ci => ci.EntryDate != null);

            Collection<ConditionInspectionViewModel> civmLast = new Collection<ConditionInspectionViewModel>();

            foreach (var conditionInspectionViewModel in civm)
            {
                var safgsd =
                    civm.Where(
                        ci =>
                            ci.ModelAuxillaryCondition.Model.Id ==
                            conditionInspectionViewModel.ModelAuxillaryCondition.Model.Id);

                civmLast.Add(safgsd.OrderByDescending(ci => ci.EntryDate).First());
            }

            var civmLastDistinct = civmLast.Distinct();

            int civmCorrectCount = civmLastDistinct.Count(ci => ci.Status);
            int civmIncorrectCount = civmLastDistinct.Count(ci => ci.Status == false);

            Int32[] status = new Int32[]{civmCorrectCount, civmIncorrectCount};
            string[] statusMessage = new string[]{TranslationProvider.Translate("TrueAC"), TranslationProvider.Translate("FalseAC")};

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                OutsideLabelFormat = "{1}: {0}",
                InsideLabelFormat = string.Empty,
                Title = TranslationProvider.Translate("TotalAuxillaryCondition"),
                
                
            };

            for (int i = 0; i < status.Length; i++)
            {
                pieSeries.Slices.Add(new PieSlice(statusMessage[i], (int) status[i]));
            }

            m_PieModelAuxillaryTotal.Series.Add(pieSeries);
        }

        private void GeneratePlotDataForMeasures()
        {
            m_PieModelMeasuresTotal.Series.Clear();
            int mvmDoneCount = m_MeasureViewModels.Count(mvm => mvm.Progress == 2);
            int mvmInProgressCount = m_MeasureViewModels.Count(mvm => mvm.Progress == 1);
            int mvmUndoneCount = m_MeasureViewModels.Count(mvm => mvm.Progress == 0);

            Int32[] status = new Int32[] {mvmDoneCount, mvmInProgressCount, mvmUndoneCount};
            string[] statusMessage = new string[]{"Erfüllt", "Laufend", "Angelegt"};

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                OutsideLabelFormat = "{1}: {0}",
                InsideLabelFormat = string.Empty,
                Title = TranslationProvider.Translate("TotalMeasures")
            };

            for (int i = 0; i < status.Length; i++)
            {
                pieSeries.Slices.Add(new PieSlice(statusMessage[i], (int)status[i]));
            }

            m_PieModelMeasuresTotal.Series.Add(pieSeries);

        }
    }
}
