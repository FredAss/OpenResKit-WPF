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
using System.Windows.Input;
using Ork.Meter.ViewModels;

namespace Ork.Meter.Commands
{
  public class AddNewMapCommand : ICommand
  {
    private readonly MeterManagementViewModel m_meterViewModel;

    public AddNewMapCommand(MeterManagementViewModel meter)
    {
      m_meterViewModel = meter;
    }

    public event EventHandler CanExecuteChanged
    {
      add { }
      remove { }
    }

    public bool CanExecute(object parameter)
    {
      return true;
    }

    public void Execute(object parameter)
    {
      m_meterViewModel.OpenMapAddDialog();
    }
  }
}