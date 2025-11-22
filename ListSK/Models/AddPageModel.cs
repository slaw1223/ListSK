using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListSK.ViewModels;

namespace ListSK.Models
{
    public partial class AddPageModel : ObservableObject
    {
        private readonly MainListViewModel _mainVM;

        public AddPageModel(MainListViewModel mainVM)
        {
            _mainVM = mainVM;
        }
        
        [ObservableProperty] private string name;
        [ObservableProperty] private string category;
        [ObservableProperty] private string unit;
        [ObservableProperty] private string amount;
        [ObservableProperty] private bool isOptional;

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
                IsOptional = IsOptional
            };

            _mainVM.AddProduct(product);

            Name = "";
            Category = "";
            Unit = "";
            Amount = "";
            IsOptional = false;
        }
    }
}
