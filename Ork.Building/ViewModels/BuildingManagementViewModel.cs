using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Ork.Building.DomainModelService;
using Ork.Building.Factories;
using Ork.Framework;

namespace Ork.Building.ViewModels
{
    [Export(typeof (IWorkspace))]
    public class BuildingManagementViewModel : DocumentBase, IWorkspace
    {
        private readonly BindableCollection<AddressViewModel> m_Addresses = new BindableCollection<AddressViewModel>();
        private readonly BindableCollection<BuildingViewModel> m_Buildings = new BindableCollection<BuildingViewModel>();
        private readonly IBuildingViewModelFactory m_BuildingViewModelFactory;
        private readonly IExcelExportFactory m_ExcelExportFactory;
        private readonly IPdfFactory m_PdfFactory;
        private readonly IBuildingRepository m_Repository;
        private readonly BindableCollection<RoomViewModel> m_Rooms = new BindableCollection<RoomViewModel>();
        private bool m_DgvVisible;
        private bool m_DgvVisibleBuilding;
        private IScreen m_EditItem;
        private bool m_FlyoutActivated;
        private bool m_IsEnabled = true;
        private string m_SearchText;
        private string m_SearchTextRooms;
        private BuildingViewModel m_SelectedBuilding;

        [ImportingConstructor]
        public BuildingManagementViewModel([Import] IBuildingRepository contextRepository,
            [Import] IBuildingViewModelFactory buildingViewModelFactory, [Import] IPdfFactory pdfFactory,
            [Import] IExcelExportFactory excelExportFactory)
        {
            m_Repository = contextRepository;
            m_BuildingViewModelFactory = buildingViewModelFactory;
            m_Repository.ContextChanged += (s, e) => Application.Current.Dispatcher.Invoke(Reload);
            m_PdfFactory = pdfFactory;
            m_ExcelExportFactory = excelExportFactory;

            Reload();

            FlyoutActivated = true;
        }

        public IEnumerable<BuildingViewModel> Buildings
        {
            get { return FilteredBuildings; }
        }

        public bool CanAdd
        {
            get
            {
                if (SelectedBuilding == null)
                {
                    return false;
                }
                return true;
            }
        }

        public IEnumerable<BuildingViewModel> FilteredBuildings
        {
            get
            {
                var filteredBuildings = SearchInBuildingList()
                    .ToArray();
                return filteredBuildings;
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

        public IEnumerable<RoomViewModel> Rooms
        {
            get
            {
                if (SelectedBuilding != null)
                {
                    return m_Rooms.Where(rvm => SelectedBuilding.Rooms.Contains(rvm.Model)
                                                &&
                                                ((!string.IsNullOrEmpty(rvm.Name) &&
                                                  rvm.Name.Contains(m_SearchTextRooms)) ||
                                                 (!string.IsNullOrEmpty(rvm.Description) &&
                                                  rvm.Description.Contains(m_SearchTextRooms))))
                        .ToArray();
                }

                return
                    m_Rooms.Where(
                        rvm =>
                            (!string.IsNullOrEmpty(rvm.Name) && rvm.Name.Contains(m_SearchTextRooms)) ||
                            (!string.IsNullOrEmpty(rvm.Description) && rvm.Description.Contains(m_SearchTextRooms)))
                        .ToArray();
            }
        }

        public string SearchText
        {
            get { return m_SearchText; }
            set
            {
                m_SearchText = value;
                NotifyOfPropertyChange(() => Buildings);
            }
        }

        public string SearchTextRooms
        {
            get { return m_SearchTextRooms; }
            set
            {
                m_SearchTextRooms = value;
                NotifyOfPropertyChange(() => Rooms);
            }
        }

        public BuildingViewModel SelectedBuilding
        {
            get { return m_SelectedBuilding; }
            set
            {
                if (value == null)
                {
                    VisibleBuilding = true;
                    VisibleNormal = false;
                }
                else
                {
                    VisibleBuilding = false;
                    VisibleNormal = true;
                }

                m_SelectedBuilding = value;
                NotifyOfPropertyChange(() => SelectedBuilding);
                NotifyOfPropertyChange(() => CanAdd);
                NotifyOfPropertyChange(() => Rooms);
            }
        }

        public bool VisibleBuilding
        {
            get { return m_DgvVisibleBuilding; }
            set
            {
                m_DgvVisibleBuilding = value;
                NotifyOfPropertyChange(() => VisibleBuilding);
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

        public int Index
        {
            get { return 2; }
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
            get { return TranslationProvider.Translate("TitleBuildingManagementViewModel"); }
        }

        public void Accept(object dataContext)
        {
            Save();
            NotifyOfPropertyChange(() => Rooms);
            NotifyOfPropertyChange(() => Buildings);
            NotifyOfPropertyChange(() => SelectedBuilding);
        }

        public void AddAddress()
        {
            var address = new Address {Street = "Neu"}; // TODO:: remove default value
            m_Repository.Addresses.Add(address);
            CreateAddressViewModel(address);
        }

        public void AddBuilding(object dataContext)
        {
            var buildingAddViewModel = (BuildingAddViewModel) dataContext;
            m_Repository.Buildings.Add(buildingAddViewModel.Model);
            Save();
            SelectedBuilding = m_Buildings.Last();
        }

        public void AddRoom()
        {
            if (!VisibleBuilding)
            {
                var newRoom = new BuildingRoom {Name = "Neu"}; // TODO:: remove default value
                newRoom.PropertyChanged += NewRoomOnPropertyChanged;
                SelectedBuilding.Rooms.Add(newRoom);
                CreateRoomViewModel(newRoom);
                NotifyOfPropertyChange(() => Buildings);
                NotifyOfPropertyChange(() => Rooms);
            }
        }

        public void Cancel()
        {
            CloseEditor();
        }

        public void CreateRoomListExcel()
        {
            m_ExcelExportFactory.CreateRoomList(Rooms);
        }

        public void CreateRoomListPdf()
        {
            m_PdfFactory.CreateRoomList(Rooms);
        }

        public void DeleteAddress(object dataContext)
        {
            if (dataContext.GetType() == typeof (AddressViewModel))
            {
                var addressViewModel = (AddressViewModel) dataContext;
                m_Addresses.Remove(addressViewModel);
                m_Repository.Addresses.Remove(addressViewModel.Model);
            }
        }

        public void DeleteRoom(object dataContext)
        {
            if (dataContext.GetType() == typeof (RoomViewModel))
            {
                var confirmResult = MessageBox.Show(TranslationProvider.Translate("DeleteRoomMsg"),
                    TranslationProvider.Translate("DeleteRoom"), MessageBoxButton.YesNo);
                if (confirmResult != MessageBoxResult.Yes) return;

                var rvm = (RoomViewModel) dataContext;
                m_Rooms.Remove(rvm);
                m_Repository.Rooms.Remove(rvm.Model);
                NotifyOfPropertyChange(() => Rooms);
            }
        }

        public void OpenBuildingAddDialog()
        {
            OpenEditor(m_BuildingViewModelFactory.CreateBuildingAddViewModel(m_Addresses));
        }

        public void OpenEditor(object dataContext, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
            {
                return;
            }
            if (dataContext is BuildingViewModel)
            {
                OpenBuildingEditDialog(dataContext);
            }
        }

        public void RemoveBuilding()
        {
            var buildingViewModel = SelectedBuilding;

            foreach (var buildingRoom in buildingViewModel.Rooms)
            {
                m_Repository.Rooms.Remove(buildingRoom);
            }

            m_Repository.Buildings.Remove(buildingViewModel.Model);
            Save();

            SelectedBuilding = null;
        }

        public void SaveRooms()
        {
            m_Repository.Save();
        }

        public IEnumerable<BuildingViewModel> SearchInBuildingList()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                return m_Buildings;
            }
            var searchText = SearchText.ToLower();
            return m_Buildings.Where(c => (c.Name != null) && c.Name.ToLower().Contains(searchText));
        }

        public void ShowAllRooms()
        {
            SelectedBuilding = null;
        }

        private void AlterBuildingsCollection(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newItem in e.NewItems.OfType<DomainModelService.Building>())
                    {
                        CreateBuildingViewModel(newItem);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var buildingViewModel in e.OldItems.OfType<DomainModelService.Building>()
                        .Select(oldItem => m_Buildings.Single(r => r.Model == oldItem)))
                    {
                        m_Buildings.Remove(buildingViewModel);
                        foreach (var rvm in m_Rooms.Where(rvm => buildingViewModel.Rooms.Contains(rvm.Model))
                            .ToArray())
                        {
                            m_Rooms.Remove(rvm);
                        }
                    }
                    break;
            }
            NotifyOfPropertyChange(() => Buildings);
        }

        private void CloseEditor()
        {
            m_EditItem.TryClose();
            m_EditItem = null;
        }

        private void CreateAddressViewModel(Address address)
        {
            var avm = m_BuildingViewModelFactory.CreateFromExisting(address);
            m_Addresses.Add(avm);
        }

        private void CreateBuildingViewModel(DomainModelService.Building building)
        {
            var bvm = m_BuildingViewModelFactory.CreateFromExisting(building);
            m_Buildings.Add(bvm);
        }

        private void CreateRoomViewModel(BuildingRoom room)
        {
            var rvm = m_BuildingViewModelFactory.CreateFromExisting(room, m_Buildings);
            m_Rooms.Add(rvm);
        }

        private void LoadAddresses()
        {
            foreach (var address in m_Repository.Addresses)
            {
                CreateAddressViewModel(address);
            }
        }

        private void LoadBuildings()
        {
            m_Repository.Buildings.CollectionChanged += AlterBuildingsCollection;
            foreach (var building in m_Repository.Buildings)
            {
                CreateBuildingViewModel(building);
            }
            NotifyOfPropertyChange(() => Buildings);
        }

        private void LoadData()
        {
            m_Rooms.Clear();
            m_Addresses.Clear();
            m_Buildings.Clear();
            LoadBuildings();
            LoadAddresses();
            LoadRooms();
        }

        private void LoadRooms()
        {
            foreach (var room in m_Repository.Rooms)
            {
                CreateRoomViewModel(room);
            }
            NotifyOfPropertyChange(() => Rooms);
        }

        private void NewRoomOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var room = (BuildingRoom) sender;
            if (m_Rooms.Count(r => r.Name == room.Name) > 1)
            {
                room.Name = room.Name + " (duplicate)";
            }
        }

        private void OpenBuildingEditDialog(object dataContext)
        {
            SelectedBuilding = (BuildingViewModel) dataContext;
            OpenEditor(m_BuildingViewModelFactory.CreateBuildingEditViewModel((BuildingViewModel) dataContext,
                m_Addresses, RemoveBuilding));
        }

        private void OpenEditor(object dataContext)
        {
            m_EditItem = (IScreen) dataContext;
            Dialogs.ShowDialog(m_EditItem);
        }

        private void Reload()
        {
            IsEnabled = m_Repository.HasConnection;
            if (IsEnabled)
            {
                LoadData();
            }
            ShowAllRooms();
        }

        private void Save()
        {
            CloseEditor();
            m_Repository.Save();
        }
    }
}