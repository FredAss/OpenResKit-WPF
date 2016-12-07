using System.Collections.Generic;
using Ork.Inventory.ViewModels;

namespace Ork.Inventory.Factories
{
    public interface IPdfFactory
    {
        void CreateInventoryList(IEnumerable<InventoryViewModel> inventories);
    }
}
