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

using System.Linq;
using System.Windows;
using System.Windows.Input;
using Ork.Framework;
using Ork.Waste.AVVCatalog;
using Ork.Waste.DomainModelService;

namespace Ork.Waste.ViewModels
{
  public class ContainerAddViewModel : ContainerViewModel
  {
    private readonly WasteContainer m_Model;
    private AvvWasteType m_SelectedAvvWasteType;

    public ContainerAddViewModel(WasteContainer model, IAvvWasteTypeProvider avvWasteTypeProvider)
      : base(model, avvWasteTypeProvider)
    {
      DisplayName = TranslationProvider.Translate("TitleContainerAddViewModel");
      m_Model = model;
    }

    public new DomainModelService.Map Map
    {
      get { return base.Map; }
      set
      {
        m_Model.MapPosition.Map = value;
        NotifyOfPropertyChange(() => ContainerPosition);
      }
    }

    public AvvWasteType SelectedAvvWasteType
    {
      get { return m_SelectedAvvWasteType; }
      set
      {
        m_SelectedAvvWasteType = value;
        NotifyOfPropertyChange(() => SelectedAvvWasteType);
      }
    }

    public double X
    {
      get { return m_Model.MapPosition.XPosition; }
      set
      {
        m_Model.MapPosition.XPosition = value;
        ContainerPosition.Location = new Point(value, Y);
        NotifyOfPropertyChange(() => ContainerPosition);
      }
    }

    public double Y
    {
      get { return m_Model.MapPosition.YPosition; }
      set
      {
        m_Model.MapPosition.YPosition = value;
        ContainerPosition.Location = new Point(X, value);
        NotifyOfPropertyChange(() => ContainerPosition);
      }
    }

    public void AddAvvWasteTypeOnEnterButton(KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        AddAvvWasteType();
      }
    }

    public void AddAvvWasteTypeOnMouseClick()
    {
      AddAvvWasteType();
    }

    public void RemoveAvvWasteType(object dataContext)
    {
      var avvWasteType = (AvvWasteType) dataContext;
      SelectedAvvWasteTypes.Remove(avvWasteType);
      m_Model.WasteTypes.Remove(Enumerable.Single(m_Model.WasteTypes, wt => wt.AvvId == avvWasteType.Id));
    }

    private void AddAvvWasteType()
    {
      if (!SelectedAvvWasteTypes.Contains(SelectedAvvWasteType) &&
          SelectedAvvWasteType != null)
      {
        SelectedAvvWasteTypes.Add(SelectedAvvWasteType);
        m_Model.WasteTypes.Add(new WasteType
                               {
                                 AvvId = SelectedAvvWasteType.Id
                               });
        SelectedAvvWasteType = null;
      }
    }
  }
}