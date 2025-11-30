using CommunityToolkit.Mvvm.ComponentModel;
using ListSK.Models;
using ListSK.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;
using System.Diagnostics;

namespace ListSK.ViewModels
{
    public partial class ShopingListViewModel : ObservableObject
    {
        private MainListViewModel _mainVM;

        public ObservableCollection<string> Categories { get; } = new();
        public ObservableCollection<string> Shops { get; } = new();

        public ObservableCollection<CategoryGroup> GroupedProducts { get; } = new();

        [ObservableProperty]
        private string shop = string.Empty;

        public ShopingListViewModel(MainListViewModel mainVM = null)
        {
            _mainVM = mainVM;

            try { foreach (var s in ShopService.LoadShops() ?? Enumerable.Empty<string>()) Shops.Add(s); }
            catch (Exception ex) { Debug.WriteLine($"ShopService.LoadShops error: {ex}"); }

            try { foreach (var c in CategoryService.LoadCategories() ?? Enumerable.Empty<string>()) Categories.Add(c); }
            catch (Exception ex) { Debug.WriteLine($"CategoryService.LoadCategories error: {ex}"); }

            if (_mainVM != null)
                AttachToMainVm(_mainVM);

            RefreshGroups();
        }

        public void Attach(MainListViewModel mainVm)
        {
            if (mainVm == null || ReferenceEquals(mainVm, _mainVM)) return;
            _mainVM = mainVm;
            AttachToMainVm(_mainVM);
            RefreshGroups();
        }

        private void AttachToMainVm(MainListViewModel vm)
        {
            if (vm?.Products == null) return;

            vm.Products.CollectionChanged -= Products_CollectionChanged;
            vm.Products.CollectionChanged += Products_CollectionChanged;

            foreach (var p in vm.Products)
            {
                if (p != null)
                {
                    p.PropertyChanged -= Product_PropertyChanged;
                    p.PropertyChanged += Product_PropertyChanged;
                }
            }
        }

        private void Products_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductModel p in e.NewItems)
                    if (p != null)
                        p.PropertyChanged += Product_PropertyChanged;
            }
            if (e.OldItems != null)
            {
                foreach (ProductModel p in e.OldItems)
                    if (p != null)
                        p.PropertyChanged -= Product_PropertyChanged;
            }
            RefreshGroups();
        }

        private void Product_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not ProductModel) return;

            if (e.PropertyName == nameof(ProductModel.IsBought) ||
                e.PropertyName == nameof(ProductModel.Category) ||
                e.PropertyName == nameof(ProductModel.Name) ||
                e.PropertyName == nameof(ProductModel.Shop))
            {
                RefreshGroups();
            }
        }
        partial void OnShopChanged(string value)
        {
            RefreshGroups();
        }

        private void RefreshGroups()
        {
            try
            {
                if (_mainVM?.Products == null)
                {
                    MainThread.BeginInvokeOnMainThread(() => GroupedProducts.Clear());
                    return;
                }

                var items = _mainVM.Products
                    .Where(p => p != null)
                    .Where(p => !p.IsBought)
                    .Where(p => string.IsNullOrEmpty(Shop) || string.Equals(p.Shop ?? string.Empty, Shop, StringComparison.OrdinalIgnoreCase));

                var groups = items
                    .GroupBy(p => p.Category ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    GroupedProducts.Clear();
                    foreach (var g in groups)
                    {
                        var group = new CategoryGroup(g.Key);
                        foreach (var p in g.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
                            group.Products.Add(p);
                        GroupedProducts.Add(group);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RefreshGroups error: {ex}");
                MainThread.BeginInvokeOnMainThread(() => GroupedProducts.Clear());
            }
        }
    }
}