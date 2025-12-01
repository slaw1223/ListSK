using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListSK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListSK.Models
{
    public partial class CategoryGroup : ObservableObject
    {
        public string Name { get; }

        public ObservableCollection<ProductModel> Products { get; } = new();

        public int ProductCount => Products.Count;

        [ObservableProperty]
        private bool isExpanded;

        public CategoryGroup(string name)
        {
            Name = name;
            Products.CollectionChanged += Products_CollectionChanged;
        }

        private void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ProductCount));
        }

        [RelayCommand]
        private void Toggle()
        {
            IsExpanded = !IsExpanded;
        }
    }
}