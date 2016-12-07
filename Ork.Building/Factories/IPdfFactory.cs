using System.Collections.Generic;
using Ork.Building.ViewModels;

namespace Ork.Building.Factories
{
    public interface IPdfFactory
    {
        void CreateRoomList(IEnumerable<RoomViewModel> rooms);
    }
}
