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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing.Imaging;
using System.Windows.Input;
using Ork.Discard.DomainModelService;
using Ork.Discard.Factories;
using Ork.Framework;
using Ork.Framework.Utilities;

namespace Ork.Discard.ViewModels
{
  public class ProductionItemAddViewModel : DocumentBase
  {
    private readonly DiscardViewModelFactory m_DiscardViewModelFactory;
    private readonly IEnumerable m_ItemCategories;
    private readonly ProductionItem m_Model;
    //private readonly Customer m_Customer;
    private readonly IDiscardRepository m_repository;

    [ImportingConstructor]
    public ProductionItemAddViewModel(ProductionItem model)
    {
      m_Model = model;
      m_ItemCategories = Enum.GetValues(typeof (ItemCategory));
      DisplayName = TranslationProvider.Translate("TitleProductionItemAddViewModel");
    }

    public ObservableCollection<InspectionAttribute> InspectionAttributes
    {
      get { return m_Model.InspectionAttributes; }
    }

    public ProductionItem Model
    {
      get { return m_Model; }
    }


    public string ItemName
    {
      get { return m_Model.ItemName; }
      set
      {
        m_Model.ItemName = value;
        NotifyOfPropertyChange(() => ItemName);
        NotifyOfPropertyChange(() => CanProductionItemAdd);
      }
    }

    public string ItemNumber
    {
      get { return m_Model.ItemNumber; }
      set
      {
        m_Model.ItemNumber = value;
        NotifyOfPropertyChange(() => ItemNumber);
        NotifyOfPropertyChange(() => CanProductionItemAdd);
      }
    }

    public int ItemCategory
    {
      get { return m_Model.ItemCategory; }
      set
      {
        m_Model.ItemCategory = value;
        NotifyOfPropertyChange(() => ItemCategory);
      }
    }

    public IEnumerable ItemCategories
    {
      get { return m_ItemCategories; }
    }

    public string ItemDescription
    {
      get { return m_Model.ItemDescription; }
      set
      {
        m_Model.ItemDescription = value;
        NotifyOfPropertyChange(() => ItemDescription);
      }
    }

    public string Material
    {
      get { return m_Model.Material; }
      set
      {
        m_Model.Material = value;
        NotifyOfPropertyChange(() => Material);
      }
    }

    public string ToolNumber
    {
      get { return m_Model.ToolNumber; }
      set
      {
        m_Model.ToolNumber = value;
        NotifyOfPropertyChange(() => ToolNumber);
      }
    }

    public string ChangeIndex
    {
      get { return m_Model.ChangeIndex; }
      set
      {
        m_Model.ChangeIndex = value;
        NotifyOfPropertyChange(() => ChangeIndex);
      }
    }

    public string ReferenceNumber1
    {
      get { return m_Model.ReferenceNumber1; }
      set
      {
        m_Model.ReferenceNumber1 = value;
        NotifyOfPropertyChange(() => ReferenceNumber1);
      }
    }

    public string ReferenceNumber2
    {
      get { return m_Model.ReferenceNumber2; }
      set
      {
        m_Model.ReferenceNumber2 = value;
        NotifyOfPropertyChange(() => ReferenceNumber2);
      }
    }

    public Customer Customer
    {
      get { return m_Model.Customer; }
      set
      {
        m_Model.Customer = value;
        NotifyOfPropertyChange(() => Customer);
      }
    }

    public string InspectionAttribute { get; set; }

    public bool CanProductionItemAdd
    {
      get { return !(m_Model.ItemName == null | m_Model.ItemName == "" | m_Model.ItemNumber == null | m_Model.ItemNumber == ""); }
    }


    public void AddInspectionAttribute(string input)
    {
      if (!input.Equals(""))
      {
        var ia = new InspectionAttribute()
                 {
                   Description = input
                 };
        m_Model.InspectionAttributes.Add(ia);
        InspectionAttribute = string.Empty;
        NotifyOfPropertyChange(() => InspectionAttributes);
        NotifyOfPropertyChange(() => InspectionAttribute);
      }
    }

    public void SetPictureToInspectionAtribute(InspectionAttribute dataContext)
    {
      var buffer = FileHelpers.GetByeArrayFromUserSelectedFile("Image Files |*.jpeg;*.png;*.jpg;*.gif");

      if (buffer == null)
      {
        return;
      }

      dataContext.DiscardImageSource = new DiscardImageSource()
                                       {
                                         BinarySource = ImageHelpers.ResizeImage(buffer, 1134, 765, ImageFormat.Jpeg)
                                       };
    }


    public void AddInspectionAttribute(KeyEventArgs e, string input)
    {
      if (e.Key == Key.Return)
      {
        AddInspectionAttribute(input);
      }
    }

    public void DeleteInspectionAttribute(object dataContext)
    {
      m_Model.InspectionAttributes.Remove((InspectionAttribute) dataContext);
      NotifyOfPropertyChange(() => InspectionAttributes);
    }
  }
}