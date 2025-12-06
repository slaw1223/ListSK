using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Xml.Serialization;

namespace ListSK.Models
{
    public partial class ProductModel : ObservableObject
    {
        [ObservableProperty] private string name;
        [ObservableProperty] private string category;
        [ObservableProperty] private string unit;
        [ObservableProperty] private double amount;
        [ObservableProperty] private bool isBought;
        [ObservableProperty] private bool isOptional;
        [ObservableProperty] private string shop;
    }
}
