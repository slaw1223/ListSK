using CommunityToolkit.Mvvm.ComponentModel;
using ListSK.Models;
using ListSK.Services;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace ListSK.ViewModels
{
    public partial class ShopingListViewModel : ObservableObject
    {
        private MainListViewModel _mainVM;

        public ObservableCollection<string> Shops { get; } = new();


        public ObservableCollection<CategoryGroup> GroupedProducts { get; } = new();

        [ObservableProperty]
        private string shop = "Wszystkie";

        public ShopingListViewModel()
        {
            foreach (var s in ShopService.LoadShops() ?? Enumerable.Empty<string>())
                Shops.Add(s);

            if (!Shops.Contains("Wszystkie"))
                Shops.Insert(0, "Wszystkie");
        }

        public void Attach(MainListViewModel vm)
        {
            if (vm == null) return;

            if (_mainVM != null)
            {
                _mainVM.Products.CollectionChanged -= ProductsChanged;
                foreach (var p in _mainVM.Products)
                    p.PropertyChanged -= ProductChanged;
            }

            _mainVM = vm;

            _mainVM.Products.CollectionChanged += ProductsChanged;

            foreach (var p in _mainVM.Products)
                p.PropertyChanged += ProductChanged;

            RefreshGroups();
        }

        private void ProductsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (ProductModel p in e.NewItems)
                    p.PropertyChanged += ProductChanged;

            if (e.OldItems != null)
                foreach (ProductModel p in e.OldItems)
                    p.PropertyChanged -= ProductChanged;

            RefreshGroups();
        }
        private void ProductChanged(object? sender, PropertyChangedEventArgs e)
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

        partial void OnShopChanged(string value) => RefreshGroups();

        private void RefreshGroups()
        {
            if (_mainVM?.Products == null)
            {
                GroupedProducts.Clear();
                return;
            }

            var filtered = _mainVM.Products
                .Where(p => !p.IsBought)
                .Where(p => Shop == "Wszystkie" ||
                            string.Equals(p.Shop, Shop, StringComparison.OrdinalIgnoreCase));

            var groups = filtered
                .GroupBy(p => p.Category ?? "")
                .OrderBy(g => g.Key)
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                GroupedProducts.Clear();

                foreach (var g in groups)
                {
                    var grp = new CategoryGroup(g.Key);
                    foreach (var p in g.OrderBy(p => p.Name))
                        grp.Products.Add(p);

                    GroupedProducts.Add(grp);
                }
            });
        }
    }

}
