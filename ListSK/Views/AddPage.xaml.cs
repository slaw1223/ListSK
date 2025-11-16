using ListSK.Models;

namespace ListSK.Views;

public partial class AddPage : ContentPage
{
    public AddPage() : this(App.Services.GetService<AddPageModel>())
    {
    }
    public AddPage(AddPageModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}