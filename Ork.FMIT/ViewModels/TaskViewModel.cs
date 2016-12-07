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
using System.IO;

using Ork.FMIT.DomainModelService;

namespace Ork.FMIT.ViewModels
{
  // This class is used as a wrapper for a Task
  // It contains all properties of the class Task and returns the specified values of the properties
  public class TaskViewModel
  {
    private Task m_Model;

    public TaskViewModel(Task model)
    {
      m_Model = model;
    }

    // Returns the description of the Task
    public string Description
    {
      get
      {
        return m_Model.Description;
      }
    }

    // Returns the DueDate of the Task
    public DateTime DueDate
    {
      get
      {
        return m_Model.DueDate;
      }
    }

    // Returns the ReportDate of the Task
    public DateTime ReportDate
    {
      get
      {
        return m_Model.ReportDate;
      }
    }

    // Returns the Location of the Task
    public string Location
    {
      get
      {
        return m_Model.Location;
      }
    }

    // Returns the IsTaskFixed of the Task
    public bool IsFixed
    {
      get
      {
        return m_Model.IsTaskFixed;
      }
      set
      {
        m_Model.IsTaskFixed = value;

      }
    }

    // Returns the Image of the Task
    public byte[] Image
    {
      get
      {
        if (m_Model.Image != null)
        {
          return m_Model.Image;
        }
        Stream imageStream = File.OpenRead(@".\Resources\missing.png");

        byte[] byteArray;
        using (var br = new BinaryReader(imageStream))
        {
          byteArray = br.ReadBytes((int)imageStream.Length);
          return byteArray;
        }
      }
    }
  }
}