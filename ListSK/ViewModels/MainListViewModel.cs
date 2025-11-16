using ListSK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ListSK.ViewModels
{
    public partial class MainListViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<ProductModel> products = new();

        public void AddProduct(ProductModel product)
        {
            Products.Add(product);
        }

        [RelayCommand]
        private void RemoveProduct(ProductModel product)
        {
            Products.Remove(product);
        }
    }
}