using CommunityToolkit.Mvvm.ComponentModel;
using ListSK.Models;
using ListSK.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListSK.ViewModels
{
    class ShopingListViewModel
    {
        private readonly MainListViewModel _mainVM;
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<string> Shops { get; set; }
        [ObservableProperty]
        private ObservableCollection<ProductModel> products = new();
        public ShopingListViewModel(MainListViewModel mainVM)
        {
            _mainVM = mainVM;
            Shops = new ObservableCollection<string>(ShopService.LoadShops());
            Categories = new ObservableCollection<string>(CategoryService.LoadCategories());
        }
    }
}
