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
using Microsoft.Maui.ApplicationModel;

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
            var chosenCategory;
            var chosenShop;
            var chosenUnit;
            if (string.IsNullOrWhiteSpace(Name))
                return;
            if (string.IsNullOrWhiteSpace(Amount))
                return;

            if(string.IsNullOrWhiteSpace(SelectedCategory))
            {
                chosenCategory = "Warzywa";
            }
            else 
            {
                chosenCategory = SelectedCategory;
            }
            if (string.IsNullOrWhiteSpace(Shop))
            {
                chosenShop = "Wszystkie";
            }else
            {
                chosenShop = Shop;
            }
            if (string.IsNullOrWhiteSpace(Unit))
            {
                chosenUnit = "szt";
            }
            else
            {
                chosenUnit = Unit;
            }


            var product = new ProductModel
            {
                Name = Name.Trim(),
                Category = chosenCategory,
                Unit = Unit ?? string.Empty,
                Amount = double.Parse(Amount),
                IsBought = false,
                IsOptional = IsOptional,
                Shop = chosenShop
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    _mainVM.AddProduct(product);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Błąd podczas dodawania produktu: {ex}");
                }
            });

            Name = string.Empty;
            Category = string.Empty;
            Unit = string.Empty;
            Amount = string.Empty;
            IsOptional = false;
            Shop = string.Empty;
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
