using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace ListSK.Views;

public partial class ShopingListView : ContentPage
{
    public ShopingListView()
    {
        InitializeComponent();

        var mainVm = Application.Current?.MainPage?.BindingContext as ListSK.ViewModels.MainListViewModel;
        var vm = new ListSK.ViewModels.ShopingListViewModel(mainVm);

        this.BindingContext = vm;

        try
        {
            if (mainVm == null)
            {
                var maybeMain = Shell.Current?.CurrentPage?.BindingContext as ListSK.ViewModels.MainListViewModel;
                if (maybeMain != null)
                    vm.Attach(maybeMain);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ShopingListView attach attempt error: {ex}");
        }
    }
}