using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Framework;
using Ork.Inventory.DomainModelService;
using Ork.Inventory.Factories;

namespace Ork.Inventory.ViewModels
{
    [Export(typeof (IWorkspace))]
    public class InventoryManagementViewModel : DocumentBase, IWorkspace
    {
        private readonly BindableCollection<InventoryViewModel> m_Inventories =
            new BindableCollection<InventoryViewModel>();

        private readonly BindableCollection<InventoryTypeViewModel> m_InventoryTypes =
            new BindableCollection<InventoryTypeViewModel>();

        private readonly IInventoryViewModelFactory m_InventoryViewModelFactory;
        private readonly IInventoryRepository m_Repository;
        private readonly BindableCollection<RoomViewModel> m_Rooms = new BindableCollection<RoomViewModel>();
        private bool m_CatByRooms = true;
        private bool m_DgvVisible;
        private bool m_DgvVisibleRoom;
        private IScreen m_EditItem;
        private bool m_FlyoutActivated;
        private bool m_IsEnabled = true;
        private string m_SearchText;
        private string m_SearchTextInventories;
        private InventoryTypeViewModel m_SelectedInventoryType;
        private RoomViewModel m_SelectedRoom;
        private IPdfFactory m_PdfFactory;

        [ImportingConstructor]
        public InventoryManagementViewModel([Import] IInventoryRepository contextRepository,
            [Import] IInventoryViewModelFactory inventoryViewModelFactory, [Import] IPdfFactory pdfFactory)
        {
            m_Repository = contextRepository;
            m_InventoryViewModelFactory = inventoryViewModelFactory;
            m_PdfFactory = pdfFactory;

            FlyoutActivated = true;

            Reload();
        }

        protected override void OnViewAttached(object view, object context)
        {
            Reload();
            NotifyOfPropertyChange(()=>CatByRooms);
        }

        public bool CanAdd
        {
            get
            {
                if (SelectedRoom == null && SelectedInventoryType == null)
                {
                    return false;
                }
                return true;
            }
        }

        public bool CatByRooms
        {
            get { return m_CatByRooms; }
            set
            {
                m_CatByRooms = value;
                SelectedRoom = null;
                SelectedInventoryType = null;
                NotifyOfPropertyChange(() => CatByRooms);
            }
        }

        public IEnumerable<InventoryTypeViewModel> FilteredInventoryTypes
        {
            get
            {
                var filteredInventoryTypes = SearchInInventoryTypeList()
                    .ToArray();
                return filteredInventoryTypes;
            }
        }

        //public bool CanAdd
        //{
        //    get
        //    {
        //        if (SelectedBuilding == null)
        //        {
        //            return false;
        //        }
        //        return true;
        //    }
        //}

        public IEnumerable<RoomViewModel> FilteredRooms
        {
            get
            {
                var filteredRooms = SearchInRoomList()
                    .ToArray();
                return filteredRooms;
            }
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

        public IEnumerable<InventoryViewModel> Inventories
        {
            get
            {
                if (SelectedRoom != null)
                {
                    return m_Inventories.Where(ivm => SelectedRoom.Inventories.Contains(ivm.Model)
                                                      &&
                                                      ((!string.IsNullOrEmpty(ivm.Name) &&
                                                        ivm.Name.Contains(SearchTextInventories)) ||
                                                       (!string.IsNullOrEmpty(ivm.Description) &&
                                                        ivm.Description.Contains(SearchTextInventories))))
                        .ToArray();
                }

                if (SelectedInventoryType != null)
                {
                    return m_Inventories.Where(ivm => SelectedInventoryType.Inventories.Contains(ivm.Model)
                                                      &&
                                                      ((!string.IsNullOrEmpty(ivm.Name) &&
                                                        ivm.Name.Contains(SearchTextInventories)) ||
                                                       (!string.IsNullOrEmpty(ivm.Description) &&
                                                        ivm.Description.Contains(SearchTextInventories))))
                        .ToArray();
                }

                return
                    m_Inventories.Where(
                        ivm =>
                            (!string.IsNullOrEmpty(ivm.Name) && ivm.Name.Contains(SearchTextInventories)) ||
                            (!string.IsNullOrEmpty(ivm.Description) && ivm.Description.Contains(SearchTextInventories)))
                        .ToArray();
            }
        }

        public IEnumerable<InventoryTypeViewModel> InventoryTypes
        {
            get { return FilteredInventoryTypes; }
        }

        public IEnumerable<RoomViewModel> Rooms
        {
            get { return FilteredRooms; }
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set
            {
                m_SearchText = value;
                NotifyOfPropertyChange(() => Rooms);
                NotifyOfPropertyChange(() => InventoryTypes);
            }
        }

        public string SearchTextInventories
        {
            get { return m_SearchTextInventories; }
            set
            {
                m_SearchTextInventories = value;
                NotifyOfPropertyChange(() => Inventories);
            }
        }

        public InventoryTypeViewModel SelectedInventoryType
        {
            get { return m_SelectedInventoryType; }
            set
            {
                if (value == null)
                {
                    VisibleRoom = true;
                    VisibleNormal = false;
                }
                else
                {
                    VisibleRoom = false;
                    VisibleNormal = true;
                }

                m_SelectedInventoryType = value;
                NotifyOfPropertyChange(() => SelectedInventoryType);
                NotifyOfPropertyChange(() => CanAdd);
                NotifyOfPropertyChange(() => Inventories);
            }
        }

        public InventoryViewModel SelectedInventory { get; set; }


        public RoomViewModel SelectedRoom
        {
            get { return m_SelectedRoom; }
            set
            {
                if (value == null)
                {
                    VisibleRoom = true;
                    VisibleNormal = false;
                }
                else
                {
                    VisibleRoom = false;
                    VisibleNormal = true;
                }

                m_SelectedRoom = value;
                NotifyOfPropertyChange(() => SelectedRoom);
                NotifyOfPropertyChange(() => CanAdd);
                NotifyOfPropertyChange(() => Inventories);
            }
        }

        public bool VisibleNormal
        {
            get { return m_DgvVisible; }
            set
            {
                m_DgvVisible = value;
                NotifyOfPropertyChange(() => VisibleNormal);
            }
        }

        public bool VisibleRoom
        {
            get { return m_DgvVisibleRoom; }
            set
            {
                m_DgvVisibleRoom = value;
                NotifyOfPropertyChange(() => VisibleRoom);
            }
        }

        public int Index
        {
            get { return 3; }
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
            get { return TranslationProvider.Translate("TitleInventoryManagementViewModel"); }
        }

        public void Accept(object dataContext)
        {
            Save();
            NotifyOfPropertyChange(() => Rooms);
            NotifyOfPropertyChange(() => Inventories);
            NotifyOfPropertyChange(() => InventoryTypes);
            NotifyOfPropertyChange(() => SelectedRoom);
            NotifyOfPropertyChange(() => SelectedInventoryType);
        }

        public void AddInventory(object dataContext)
        {
            var addViewModel = ((InventoryAddViewModel)dataContext);
            CreateInventoryViewModel(addViewModel.Model);
            
            if (addViewModel.Model.Room != null)
            {
                addViewModel.SelectedRoom.Inventories.Add(addViewModel.Model);
            }

            if (addViewModel.Model.InventoryType != null)
            {
                addViewModel.SelectedInventoryType.Inventories.Add(addViewModel.Model);
            }

            Save();

            NotifyOfPropertyChange(() => Inventories);
        }

        public void OpenInventoryAddDialog()
        {
            if (!VisibleRoom)
            {
                OpenEditor(m_InventoryViewModelFactory.CreateInventoryAddViewModel(SelectedRoom, SelectedInventoryType, m_InventoryTypes, m_Rooms));
            } 
        }

        public void CreateInventoryListPdf()
        {
            m_PdfFactory.CreateInventoryList(Inventories);
        }

        public void AddInventoryType(object dataContext)
        {
            var inventoryTypeAddViewModel = (InventoryTypeAddViewModel) dataContext;
            m_Repository.InventoryTypes.Add(inventoryTypeAddViewModel.Model);
            Save();
            SelectedInventoryType = m_InventoryTypes.Last();
        }

        public void Cancel()
        {
            CloseEditor();
        }

        public void DeleteInventory(object dataContext)
        {
            if (dataContext.GetType() == typeof (InventoryViewModel))
            {
                var ivm = (InventoryViewModel) dataContext;
                m_Inventories.Remove(ivm);
                m_Repository.Inventories.Remove(ivm.Model);
                NotifyOfPropertyChange(() => Inventories);
            }
        }

        public void OpenEditor(object dataContext, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                return;
            }
            if (dataContext is InventoryTypeViewModel)
            {
                OpenInventoryTypeEditDialog(dataContext);
            }
            else if (dataContext is InventoryManagementViewModel)
            {
                OpenInventoryEditDialog(SelectedInventory);
            }
        }

        private void OpenInventoryEditDialog(InventoryViewModel inventoryViewModel)
        {
            if (inventoryViewModel != null)
            {
                OpenEditor(m_InventoryViewModelFactory.CreateInventoryEditViewModel(inventoryViewModel, RemoveInventory, m_InventoryTypes, m_Rooms));
            }
        }

        public void OpenInventoryTypeAddDialog()
        {
            OpenEditor(m_InventoryViewModelFactory.CreateInventoryTypeAddViewModel());
        }

        public void RemoveInventoryType()
        {
            var inventoryTypeViewModel = SelectedInventoryType;

            foreach (var inventory in inventoryTypeViewModel.Inventories)
            {
                inventory.InventoryType = null;
            }

            m_Repository.InventoryTypes.Remove(inventoryTypeViewModel.Model);
            Save();

            SelectedInventoryType = null;
        }

        public void RemoveInventory()
        {
            var inventoryViewModel = SelectedInventory;
            m_Inventories.Remove(inventoryViewModel);
            m_Repository.Inventories.Remove(inventoryViewModel.Model);
            Save();

            SelectedInventory = null;
            NotifyOfPropertyChange(() => Inventories);
        }

        //public void SaveInventories()
        //{
        //    m_Repository.Save();
        //}

        public IEnumerable<InventoryTypeViewModel> SearchInInventoryTypeList()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return m_InventoryTypes;
            }
            var searchText = SearchText.ToLower();
            return m_InventoryTypes.Where(c => (c.Name != null) && c.Name.ToLower().Contains(searchText));
        }

        public IEnumerable<RoomViewModel> SearchInRoomList()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return m_Rooms;
            }
            var searchText = SearchText.ToLower();
            return m_Rooms.Where(c => (c.Name != null) && c.Name.ToLower().Contains(searchText));
        }

        public void ShowAllInventories()
        {
            SelectedRoom = null;
            SelectedInventoryType = null;
        }

        private void AlterInventoryTypesCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newItem in e.NewItems.OfType<InventoryType>())
                    {
                        CreateInventoryTypeViewModel(newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var inventoryTypeViewModel in e.OldItems.OfType<InventoryType>()
                        .Select(oldItem => m_InventoryTypes.Single(r => r.Model == oldItem)))
                    {
                        m_InventoryTypes.Remove(inventoryTypeViewModel);
                    }
                    break;
            }
            NotifyOfPropertyChange(() => InventoryTypes);
        }

        private void CloseEditor()
        {
            m_EditItem.TryClose();
            m_EditItem = null;
        }

        private void CreateInventoryTypeViewModel(InventoryType inventoryType)
        {
            var itvm = m_InventoryViewModelFactory.CreateFromExisting(inventoryType);
            m_InventoryTypes.Add(itvm);
        }

        private void CreateInventoryViewModel(DomainModelService.Inventory inventory)
        {
            var ivm = m_InventoryViewModelFactory.CreateFromExisting(inventory);
            m_Inventories.Add(ivm);
        }

        private void CreateRoomViewModel(BuildingRoom room)
        {
            var rvm = m_InventoryViewModelFactory.CreateFromExisting(room);
            m_Rooms.Add(rvm);
        }

        private void LoadData()
        {
            m_Rooms.Clear();
            m_Inventories.Clear();
            m_InventoryTypes.Clear();
            LoadInventories();
            LoadInventoryTypes();
            LoadRooms();
        }

        private void LoadInventories()
        {
            foreach (var inventory in m_Repository.Inventories)
            {
                CreateInventoryViewModel(inventory);
            }
            NotifyOfPropertyChange(() => Inventories);
        }

        private void LoadInventoryTypes()
        {
            m_Repository.InventoryTypes.CollectionChanged += AlterInventoryTypesCollection;
            foreach (var inventoryType in m_Repository.InventoryTypes)
            {
                CreateInventoryTypeViewModel(inventoryType);
            }
            NotifyOfPropertyChange(() => InventoryTypes);
        }

        private void LoadRooms()
        {
            foreach (var room in m_Repository.Rooms)
            {
                CreateRoomViewModel(room);
            }
            NotifyOfPropertyChange(() => Rooms);
        }

        private void OpenEditor(object dataContext)
        {
            m_EditItem = (IScreen) dataContext;
            Dialogs.ShowDialog(m_EditItem);
        }

        private void OpenInventoryTypeEditDialog(object dataContext)
        {
            SelectedInventoryType = (InventoryTypeViewModel) dataContext;
            OpenEditor(m_InventoryViewModelFactory.CreateInventoryTypeEditViewModel(
                (InventoryTypeViewModel) dataContext,
                RemoveInventoryType));
        }

        private void Reload()
        {
            IsEnabled = m_Repository.HasConnection;
            m_Repository.ReloadDataFromServer();
            if (IsEnabled)
            {
                LoadData();
            }
            ShowAllInventories();
        }

        private void Save()
        {
            CloseEditor();
            m_Repository.Save();
        }
    }
}