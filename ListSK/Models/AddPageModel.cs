using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListSK.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ListSK.Services;
using System.Threading.Tasks;

namespace ListSK.Models
{
    public partial class AddPageModel : ObservableObject
    {
        private readonly MainListViewModel _mainVM;
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<string> Shops { get; set; }

        [ObservableProperty] private string name;
        [ObservableProperty] private string category;
        [ObservableProperty] private string unit;
        [ObservableProperty] private string amount;
        [ObservableProperty] private bool isOptional;
        [ObservableProperty] private string selectedCategory;
        [ObservableProperty] private string newCategory;
        [ObservableProperty] private string shop;
        [ObservableProperty] private string newShop;

        public AddPageModel(MainListViewModel mainVM)
        {
            _mainVM = mainVM;
            Categories = new ObservableCollection<string>(CategoryService.LoadCategories());
            Shops = new ObservableCollection<string>(ShopService.LoadShops());
        }

        [RelayCommand]
        private void AddProduct()
        {
            var product = new ProductModel
            {
                Name = Name,
                Category = Category,
                Unit = Unit,
                Amount = Amount,
                IsBought = false,
                IsOptional = IsOptional,
                Shop = Shop
            };

            _mainVM.AddProduct(product);

            Name = "";
            Category = "";
            Unit = "";
            Amount = "";
            IsOptional = false;
            Shop = "";
        }

        [RelayCommand]
        private void AddNewCategory()
        {
            if (!string.IsNullOrWhiteSpace(NewCategory))
            {
                CategoryService.AddCategory(NewCategory);

                Categories.Add(NewCategory);
                SelectedCategory = NewCategory;
                NewCategory = string.Empty;
            }
        }
        [RelayCommand]
        private void AddNewShop()
        {
            if (!string.IsNullOrWhiteSpace(NewShop))
            {
                ShopService.AddShop(NewShop);
                Shops.Add(NewShop);
                Shop = NewShop;
                NewShop = string.Empty;
            }
        }
    }
}
