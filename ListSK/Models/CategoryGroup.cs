using CommunityToolkit.Mvvm.Input;
using ListSK.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace ListSK.Models
{
    // Grupa teraz jest kolekcją produktów — to natywne i bezpieczne dla CollectionView.IsGrouped="True"
    public partial class CategoryGroup : ObservableCollection<ProductModel>, INotifyPropertyChanged
    {
        public string Name { get; }

        public int ProductCount => Count;

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        public CategoryGroup(string name)
        {
            Name = name;
            CollectionChanged += Products_CollectionChanged;
        }

        private void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(ProductCount)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

        [RelayCommand]
        private void Toggle()
        {
            IsExpanded = !IsExpanded;
        }
    }
}