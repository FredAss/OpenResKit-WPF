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
using System.ComponentModel.Composition;
using System.Data.Services.Client;
using System.Net;

using Caliburn.Micro;
using Ork.FMIT.DomainModelService;
using Ork.Setting;

namespace Ork.FMIT
{
  // This class implements PropertyChangedBase and ITaskRepository
  // This class is exported as ITaskRepository to be found by the DefaultViewModel
  [Export(typeof (ITaskRepository))]
  public class TaskRepository: PropertyChangedBase,ITaskRepository
  {
    private readonly Func<DomainModelContext> m_CreateMethod;
    private DomainModelContext m_Context;
    public event EventHandler SaveCompleted;

    // Imports the CreateContext() from ContextFactory to create the context later in Initialize()
    [ImportingConstructor]
    public TaskRepository([Import] Func<DomainModelContext> createMethod)
    {
      m_CreateMethod = createMethod;
      Initialize();
    }

    // Contains the context of FMIT
    public DomainModelContext Context { get; set; }
    // Contains all Tasks
    public DataServiceCollection<Task> Tasks { get; private set; }

    /// <summary>
    /// The Context is created here. The Context also contains the connection-string
    /// </summary>
    private void Initialize()
    {
      // Context is created with the methde CreateContext from ContextFactory, which returns a DomainModelContext
      m_Context = m_CreateMethod();
      LoadTasks();
    }

    /// <summary>
    /// Loads all Tasks stored in the Hub
    /// </summary>
    public void LoadTasks()
    {
      Tasks = new DataServiceCollection<Task>(m_Context);
      // The query to filter the Tasks (this can be used to make much more complex queries)
      var query = m_Context.Tasks;
     
      // The query is passed to the Load()-Function of Tasks (DataServiceCollection) to load the filtered Tasks
      try
      {
        Tasks.Load(query);
      }
      catch (WebException e)
      {
        return;
      }
    }

    /// <summary>
    /// The Context is saved in an async process
    /// </summary>
    public void Save()
    {
      var result = m_Context.BeginSaveChanges(SaveChangesOptions.Batch, r =>
      {
        var dm = (DomainModelContext)r.AsyncState;
        dm.EndSaveChanges(r);
        RaiseEvent(SaveCompleted);
      }, m_Context);
    }

    private void RaiseEvent(EventHandler eventHandler)
    {
      if (eventHandler != null)
      {
        eventHandler(this, new EventArgs());
      }
    }
  }

  public interface ITaskRepository
  {
    // Contains the structure of the domain
    DomainModelContext Context { get; set; }
    // Contains an amount of Tasks, which are designed after the Task-description in the domain
    DataServiceCollection<Task> Tasks { get; }
    // Methode to save the repository
    void Save();
  }
}