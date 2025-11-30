using ListSK.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace ListSK.Views;

public partial class ShopingListView : ContentPage
{
    public ShopingListView(MainListViewModel mainVM)
    {
        InitializeComponent();
        var vm = new ShopingListViewModel();
        vm.Attach(mainVM);
        BindingContext = vm;
    }
}
