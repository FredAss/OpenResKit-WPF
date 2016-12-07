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

using Caliburn.Micro;
using Ork.FMIT.DomainModelService;
using Ork.Framework;

namespace Ork.FMIT.ViewModels
{
  // To create a new Workspace, the class has to implement Screen and IWorkspace
  // Also the class has to export itself to be found later by the ShellViewModel
  [Export(typeof (IWorkspace))]
  public class DefaultViewModel: Screen, IWorkspace
  {
    private ITaskRepository m_Repository;
    private TaskViewModel m_SelectedItem;
    private bool m_SelectedItemIsFixed;
    private bool m_FlyoutActivated = true;
    private string m_SearchText;

    private readonly ObservableCollection<TaskViewModel> m_Tasks = new ObservableCollection<TaskViewModel>();
    
    // The Constructor of this class imports the ITaskRepository-Constructor and receives the repository 
    [ImportingConstructor]
    public DefaultViewModel([Import] ITaskRepository repository)
    {
      m_Repository = repository;
      Load();
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

    // Contains a List with Tasks, which are wrapped in TaskViewModels
    public IEnumerable<TaskViewModel> Tasks
    {
      get
      {
        return m_Tasks;
      }
    }

    // Contains the selected Item (TaskViewModel), which was selected in the List of the Flyout 
    public TaskViewModel SelectedItem
    {
      get
      {
        return m_SelectedItem;
      }
      set
      {
        m_SelectedItem = value;
        NotifyOfPropertyChange(() => SelectedItem);
      }
    }

    /// <summary>
    /// Loads all Tasks from the repository and stores them in a List with TaskViewModels
    /// </summary>
    public void Load()
    {
      // This checks if the repository is filled with values
      if (m_Repository.Tasks == null)
      {
        return;
      }

      // Creates for every Task in the repository a TaskViewModel with the particular Task
      foreach (var task in m_Repository.Tasks)
      {
        var tvm = new TaskViewModel(task);
        m_Tasks.Add(tvm);
      }

      if (!m_Repository.Tasks.Any())
      {
        return;
      }

      // Here is checked if there is any Task where the property isFixed is false, the first Task will then be selected
      // Otherwise the first Task with isFixed = true is selected
      if ((Tasks.Any(x => !x.IsFixed)))
        SelectedItem = Tasks.FirstOrDefault(x => !x.IsFixed);
      else
        SelectedItem = Tasks.FirstOrDefault(x => x.IsFixed);
    }

    // The Property returns an IEnumerable with the TaskViewModels from SearchInTaskList()
    public IEnumerable<TaskViewModel> FilteredTasks
    {
      get
      {
        var filteredTasks = SearchInTaskList();
        return filteredTasks;
      }
    }

    /// <summary>
    /// In this methode the properties Loaction and Description of the Tasks are checked, if they contains the value of SearchText.
    /// The return is an IEnumerable, which contains all TaskViewModels, where the search-result was positive
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TaskViewModel> SearchInTaskList()
    {
      if (string.IsNullOrEmpty(SearchText))
      {
        return m_Tasks;
      }
      var searchText = SearchText.ToLower();
      var searchResult = m_Tasks.Where(m => (((m.Location != null) && (m.Location.ToLower().Contains(searchText))))
                                             || ((m.Description != null) && (m.Description.ToLower().Contains(searchText)))) ;
      return searchResult;
    }

    // Contains the searchtext, which is entered in the TextBox above the List
    public string SearchText
    {
      get 
      { 
        return m_SearchText; 
      }
      set
      {
        m_SearchText = value;
        NotifyOfPropertyChange(() => FilteredTasks);
      }
    }

    /// <summary>
    /// Saves the repository, clears the List and reloads it
    /// </summary>
    public void Save()
    {
      m_Repository.Save();
      m_Tasks.Clear();
      Load();
    }

    // The following four properties are imported from the IWorkspace-Interface  
    // Here is the index of the workspace defined, so you can identify them explicity
    public int Index
    {
      get
      {
        return 100;
      }
    }

    public bool IsEnabled
    {
      get
      {
        return true;
      }
    }

    public string Status
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    // Defines the name of the Workspace. This value is shown later in the tabs above the workspace-shell
    public string Title
    {
      get
      {
        return "Meldungen";
      }
    }
  }
}

