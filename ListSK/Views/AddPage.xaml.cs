using ListSK.Models;
using ListSK.ViewModels;

namespace ListSK.Views;

public partial class AddPage : ContentPage
{
    public AddPage() : this(App.Services.GetService<AddPageViewModel>())
    {
    }
    public AddPage(AddPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}