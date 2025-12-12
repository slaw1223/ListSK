using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListSK.Models;
using ListSK.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ListSK.ViewModels
{
    public partial class AddPageViewModel : ObservableObject
    {
        private readonly MainListViewModel _mainVM;

        public ObservableCollection<string> Categories { get; }
        public ObservableCollection<string> Shops { get; }

        [ObservableProperty] private string name;
        [ObservableProperty] private string category;
        [ObservableProperty] private string unit;
        [ObservableProperty] private string amount;
        [ObservableProperty] private bool isOptional;
        [ObservableProperty] private string newCategory;
        [ObservableProperty] private string shop;
        [ObservableProperty] private string newShop;

        public AddPageViewModel(MainListViewModel mainVM)
        {
            _mainVM = mainVM;
            Categories = new ObservableCollection<string>(CategoryService.LoadCategories());
            Shops = new ObservableCollection<string>(ShopService.LoadShops());
        }

        [RelayCommand]
        private async Task AddProduct()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Błąd", "Pole 'Nazwa' nie może być puste.", "OK");
                return;
            }

            if (!double.TryParse(Amount, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var amountVal))
            {
                await Shell.Current.DisplayAlert("Błąd", "Pole 'Ilość' musi być liczbą.", "OK");
                return;
            }
            else
            {
                if (double.Parse(Amount) < 0)
                {
                    await Shell.Current.DisplayAlert("Błąd", "Pole 'Ilość' musi być większe lubrówne 0.", "OK");
                    return;
                }
            }

                var chosenCategory = string.IsNullOrWhiteSpace(Category) ? string.Empty : Category;
            var chosenShop = string.IsNullOrWhiteSpace(Shop) ? "Wszystkie" : Shop;
            var chosenUnit = string.IsNullOrWhiteSpace(Unit) ? "szt" : Unit;

            var product = new ProductModel
            {
                Name = Name.Trim(),
                Category = chosenCategory,
                Unit = chosenUnit,
                Amount = amountVal,
                IsBought = false,
                IsOptional = IsOptional,
                Shop = chosenShop
            };

            try
            {
                MainThread.BeginInvokeOnMainThread(() => _mainVM.AddProduct(product));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas dodawania produktu: {ex}");
            }

            Name = string.Empty;
            Category = string.Empty;
            Unit = string.Empty;
            Amount = string.Empty;
            IsOptional = false;
            Shop = string.Empty;
        }

        [RelayCommand]
        private async Task AddNewCategory()
        {
            if (!string.IsNullOrWhiteSpace(NewCategory))
            {
                CategoryService.AddCategory(NewCategory);

                Categories.Add(NewCategory);

                if (!_mainVM.CategoryGroups.Any(g => string.Equals(g.Name, NewCategory, StringComparison.OrdinalIgnoreCase)))
                {
                    _mainVM.CategoryGroups.Add(new CategoryGroup(NewCategory));
                }

                Category = NewCategory;
                NewCategory = string.Empty;
            }
            else
            {
                await Shell.Current.DisplayAlert("Błąd", "Nazwa kategorii nie może być pusta.", "OK");
            }
        }

        [RelayCommand]
        private async Task AddNewShop()
        {
            if (!string.IsNullOrWhiteSpace(NewShop))
            {
                ShopService.AddShop(NewShop);
                Shops.Add(NewShop);

                if (!_mainVM.Shops.Contains(NewShop))
                    _mainVM.Shops.Add(NewShop);

                Shop = NewShop;
                NewShop = string.Empty;
            }
            else
            {
                await Shell.Current.DisplayAlert("Błąd", "Nazwa sklepu nie może być pusta.", "OK");
            }
        }
    }
}
