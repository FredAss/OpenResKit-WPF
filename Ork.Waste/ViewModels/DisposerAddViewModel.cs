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

using System.Collections.Generic;
using System.Data.Services.Client;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ork.Framework;
using Ork.Framework.Utilities;
using Ork.Waste.DomainModelService;
using Ork.Waste.Factories;

namespace Ork.Waste.ViewModels
{
  public class DisposerAddViewModel : LocalizableScreen
  {
    private readonly List<SelectableContainerViewModel> m_ContainerList;
    private readonly IContainerViewModelFactory m_ContainerViewModelFactory;
    private readonly Disposer m_Model;
    private readonly IWasteRepository m_WasteRepository;
    private string m_ContainerSearchText = string.Empty;

    public DisposerAddViewModel(Disposer disposer, IWasteRepository repository, IContainerViewModelFactory containerViewModelFactory)
    {
      m_Model = disposer;
      m_WasteRepository = repository;
      m_ContainerViewModelFactory = containerViewModelFactory;
      m_ContainerList = new List<SelectableContainerViewModel>();
      DisplayName = TranslationProvider.Translate("TitleDisposerAddViewModel");

      LoadSelectedContainers();
    }

    public Disposer Model
    {
      get { return m_Model; }
    }

    public IEnumerable<SelectableContainerViewModel> SelectableContainerViewModels
    {
      get { return m_ContainerList; }
    }

    public string ContainerSearchText
    {
      get { return m_ContainerSearchText; }
      set
      {
        m_ContainerSearchText = value;
        NotifyOfPropertyChange(() => FilteredSelectableContainerViewModels);
      }
    }

    public IEnumerable<SelectableContainerViewModel> FilteredSelectableContainerViewModels
    {
      get
      {
        if (string.IsNullOrEmpty(ContainerSearchText))
        {
          return SelectableContainerViewModels;
        }

        return SelectableContainerViewModels.Where(scvm => scvm.ContainerName.ToLower()
                                                               .Contains(ContainerSearchText.ToLower()));
      }
    }

    public string Name
    {
      get { return m_Model.Name; }
      set
      {
        m_Model.Name = value;
        NotifyOfPropertyChange(() => Name);
        NotifyOfPropertyChange(() => IsSaveAllowed);
      }
    }

    public string Number
    {
      get { return m_Model.Number; }
      set
      {
        m_Model.Number = value;
        NotifyOfPropertyChange(() => Number);
      }
    }


    public string Street
    {
      get { return m_Model.Street; }
      set
      {
        m_Model.Street = value;
        NotifyOfPropertyChange(() => Street);
      }
    }


    public string ZipCode
    {
      get { return m_Model.ZipCode; }
      set
      {
        m_Model.ZipCode = value;
        NotifyOfPropertyChange(() => ZipCode);
      }
    }


    public string EMail
    {
      get { return m_Model.EMail; }
      set
      {
        m_Model.EMail = value;
        NotifyOfPropertyChange(() => EMail);
        NotifyOfPropertyChange(() => IsSaveAllowed);
      }
    }

    public string AdditionalInformation
    {
      get { return m_Model.AdditionalInformation; }
      set
      {
        m_Model.AdditionalInformation = value;
        NotifyOfPropertyChange(() => AdditionalInformation);
      }
    }

    public string City
    {
      get { return m_Model.City; }
      set
      {
        m_Model.City = value;
        NotifyOfPropertyChange(() => City);
      }
    }

    public IEnumerable<WasteContainer> AllContainers
    {
      get { return m_WasteRepository.Container; }
    }

    public DataServiceCollection<Document> Documents
    {
      get { return m_Model.Documents; }
      set
      {
        m_Model.Documents = value;
        NotifyOfPropertyChange(() => Documents);
      }
    }

    public bool IsSaveAllowed
    {
      get { return !string.IsNullOrEmpty(m_Model.Name) && !string.IsNullOrEmpty(m_Model.EMail); }
      private set { }
    }

    private void LoadSelectedContainers()
    {
      if (m_Model.Containers != null)
      {
        foreach (var container in AllContainers)
        {
          m_ContainerList.Add(new SelectableContainerViewModel(m_ContainerViewModelFactory.CreateFromExisiting(container), m_Model.Containers.Any(c => c == container)));
        }
      }
    }

    public void Accept()
    {
      foreach (var selectableContainerViewModel in SelectableContainerViewModels)
      {
        if (selectableContainerViewModel.IsSelected &&
            !Model.Containers.Contains(selectableContainerViewModel.ContainerViewModel.Model))
        {
          Model.Containers.Add(selectableContainerViewModel.ContainerViewModel.Model);
        }
        else if (!selectableContainerViewModel.IsSelected &&
                 Model.Containers.Contains(selectableContainerViewModel.ContainerViewModel.Model))
        {
          var link = m_WasteRepository.Context.Links.SingleOrDefault(l => l.Source == Model && l.Target == selectableContainerViewModel.ContainerViewModel.Model);
          if (link != null)
          {
            m_WasteRepository.Context.DeleteLink(Model, "Containers", selectableContainerViewModel.ContainerViewModel.Model);
          }
        }
      }

      m_WasteRepository.Save();

      TryClose();
    }

    public void AddDocument()
    {
      var filename = string.Empty;
      var binarySourceDocument = FileHelpers.GetByeArrayFromUserSelectedFile(string.Empty, out filename);

      if (binarySourceDocument != null)
      {
        m_Model.Documents.Add(new Document()
                              {
                                DocumentSource = new DocumentSource()
                                                 {
                                                   BinarySource = binarySourceDocument
                                                 },
                                Name = filename
                              });

        NotifyOfPropertyChange(() => Documents);
      }
    }

    public void OpenDocument(Document context)
    {
      File.WriteAllBytes(Path.GetTempPath() + context.Name, context.DocumentSource.BinarySource);

      Process.Start(Path.GetTempPath() + context.Name);
    }


    public void DeleteDocument(Document context)
    {
      m_Model.Documents.Remove(context);
      NotifyOfPropertyChange(() => Documents);
    }


    public void Cancel()
    {
      CloseEditor();
    }

    private void CloseEditor()
    {
      TryClose();
    }
  }
}