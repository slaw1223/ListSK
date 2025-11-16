using ListSK.ViewModels;

namespace ListSK.Views;

public partial class MainListView : ContentPage
{
    public MainListView() : this(App.Services.GetService<MainListViewModel>())
    {
    }
    public MainListView(MainListViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }
}