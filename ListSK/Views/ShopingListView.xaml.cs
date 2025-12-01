using ListSK.ViewModels;
using Microsoft.Maui.Controls;

namespace ListSK.Views;

public partial class ShopingListView : ContentPage
{
    public ShopingListView(MainListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}