using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ListSK.Models;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ListSK.Services;

namespace ListSK.ViewModels
{
    public partial class MainListViewModel : ObservableObject
    {
        private const string FileName = "products.xml";

        [ObservableProperty]
        private ObservableCollection<ProductModel> products = new();

        [ObservableProperty]
        private ObservableCollection<CategoryGroup> categoryGroups = new();

        public ObservableCollection<string> Shops { get; } = new();

        public ObservableCollection<CategoryGroup> GroupedProducts { get; } = new();

        [ObservableProperty]
        private string shop;

        public MainListViewModel()
        {
            var categories = CategoryService.LoadCategories();
            foreach (var category in categories)
            {
                CategoryGroups.Add(new CategoryGroup(category));
            }
            var shops = ShopService.LoadShops();
            foreach (var s in shops)
            {
                Shops.Add(s);
            }
            shop = Shops.FirstOrDefault();

            Products.CollectionChanged += Products_CollectionChanged;
            LoadProducts();
            RefreshGroupedProducts();
        }

        public void AddProduct(ProductModel product)
        {
            if (product == null) return;
            Products.Add(product);
            SaveProducts();
        }

        [RelayCommand]
        private void RemoveProduct(ProductModel product)
        {
            Products.Remove(product);
            SaveProducts();
        }

        [RelayCommand]
        public void ClearList()
        {
            Products.Clear();
            SaveProducts();
        }
        [RelayCommand]
        public void Increment(ProductModel product)
        {
            product.Amount += 1;
        }

        [RelayCommand]
        public void Decrement(ProductModel product)
        {
            product.Amount += -1;
        }

        [RelayCommand]
        private void RemoveCategory(CategoryGroup category)
        {
            if (category == null) return;

            foreach (var product in category.ToList())
            {
                Products.Remove(product);
            }

            CategoryGroups.Remove(category);

            var categories = CategoryService.LoadCategories();
            if (categories.Contains(category.Name))
            {
                categories.Remove(category.Name);
                CategoryService.SaveCategories(categories);
            }
            SaveProducts();
        }

        private void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (ProductModel item in e.NewItems)
                {
                    item.PropertyChanged -= Product_PropertyChanged;
                    item.PropertyChanged += Product_PropertyChanged;

                    var group = GetOrCreateGroup(item.Category);
                    if (!group.Contains(item))
                        group.Add(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (ProductModel item in e.OldItems)
                {
                    item.PropertyChanged -= Product_PropertyChanged;
                    foreach (var g in CategoryGroups)
                        if (g.Contains(item))
                            g.Remove(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                {
                    foreach (ProductModel item in e.OldItems)
                    {
                        item.PropertyChanged -= Product_PropertyChanged;
                        foreach (var g in CategoryGroups)
                            if (g.Contains(item))
                                g.Remove(item);
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (ProductModel item in e.NewItems)
                    {
                        item.PropertyChanged -= Product_PropertyChanged;
                        item.PropertyChanged += Product_PropertyChanged;
                        var group = GetOrCreateGroup(item.Category);
                        if (!group.Contains(item))
                            group.Add(item);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var g in CategoryGroups)
                    g.Clear();
                foreach (var item in Products)
                {
                    item.PropertyChanged -= Product_PropertyChanged;
                    item.PropertyChanged += Product_PropertyChanged;
                    var group = GetOrCreateGroup(item.Category);
                    if (!group.Contains(item))
                        group.Add(item);
                }
            }

            RefreshGroupedProducts();
        }

        private void Product_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not ProductModel p) return;

            if (e.PropertyName == nameof(ProductModel.Category))
            {
                foreach (var g in CategoryGroups)
                    if (g.Contains(p))
                        g.Remove(p);

                var group = GetOrCreateGroup(p.Category);
                if (!group.Contains(p))
                    group.Add(p);

                SaveProducts();
                RefreshGroupedProducts();
                return;
            }

            if (e.PropertyName == nameof(ProductModel.IsBought))
            {
                try
                {
                    var sorted = Products.OrderBy(x => x.IsBought).ThenBy(x => x.Name).ToList();
                    var newIndex = sorted.IndexOf(p);
                    var oldIndex = Products.IndexOf(p);

                    if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
                    {
                        Products.CollectionChanged -= Products_CollectionChanged;
                        try
                        {
                            Products.Move(oldIndex, newIndex);
                        }
                        finally
                        {
                            Products.CollectionChanged += Products_CollectionChanged;
                        }
                    }

                    var group = CategoryGroups.FirstOrDefault(g => string.Equals(g.Name, p.Category, StringComparison.OrdinalIgnoreCase));
                    if (group != null && group.Contains(p))
                    {
                        var sortedGroup = group.OrderBy(x => x.IsBought).ThenBy(x => x.Name).ToList();
                        var newGroupIndex = sortedGroup.IndexOf(p);
                        var oldGroupIndex = group.IndexOf(p);
                        if (oldGroupIndex >= 0 && newGroupIndex >= 0 && oldGroupIndex != newGroupIndex)
                        {
                            group.Move(oldGroupIndex, newGroupIndex);
                        }
                    }
                    SaveProducts();
                    RefreshGroupedProducts();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Błąd przy przestawianiu elementu: {ex}");
                }
                return;
            }

            SaveProducts();
            RefreshGroupedProducts();
        }

        private CategoryGroup GetOrCreateGroup(string category)
        {
            var name = category ?? string.Empty;
            var group = CategoryGroups.FirstOrDefault(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                group = new CategoryGroup(name);
                CategoryGroups.Add(group);

                var categories = CategoryService.LoadCategories();
                if (!categories.Contains(name))
                {
                    categories.Add(name);
                    CategoryService.SaveCategories(categories);
                }
            }
            return group;
        }

        public void SaveProducts()
        {
            try
            {
                var doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Products",
                        Products.Select(p =>
                            new XElement("Product",
                                new XElement("Name", p.Name ?? string.Empty),
                                new XElement("Category", p.Category ?? string.Empty),
                                new XElement("Unit", p.Unit ?? string.Empty),
                                new XElement("Amount", p.Amount),
                                new XElement("IsBought", p.IsBought),
                                new XElement("IsOptional", p.IsOptional),
                                new XElement("Shop", p.Shop ?? string.Empty)
                            )
                        )
                    )
                );
                var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
                doc.Save(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd zapisu: {ex}");
            }
        }

        public void LoadProducts()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
                if (!File.Exists(path)) return;

                var doc = XDocument.Load(path);

                Products.Clear();

                foreach (var element in doc.Root?.Elements().Where(e => e.Element("Name") != null) ?? Enumerable.Empty<XElement>())
                {
                    var product = new ProductModel
                    {
                        Name = element.Element("Name")?.Value ?? string.Empty,
                        Category = element.Element("Category")?.Value ?? string.Empty,
                        Unit = element.Element("Unit")?.Value ?? string.Empty,
                        Amount = double.Parse(element.Element("Amount")?.Value ?? string.Empty),
                        IsBought = bool.TryParse(element.Element("IsBought")?.Value, out var b) && b,
                        IsOptional = bool.TryParse(element.Element("IsOptional")?.Value, out var o) && o,
                        Shop = element.Element("Shop")?.Value ?? string.Empty
                    };
                    Products.Add(product);
                }
                RefreshGroupedProducts();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd ładowania: {ex}");
            }
        }

        [RelayCommand]
        public async Task ExportAsync()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "lista_zakupow.xml");
                var doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Products",
                        Products.Select(p =>
                            new XElement("Product",
                                new XElement("Name", p.Name ?? string.Empty),
                                new XElement("Category", p.Category ?? string.Empty),
                                new XElement("Unit", p.Unit ?? string.Empty),
                                new XElement("Amount", p.Amount),
                                new XElement("IsBought", p.IsBought),
                                new XElement("IsOptional", p.IsOptional),
                                new XElement("Shop", p.Shop ?? string.Empty)
                            )
                        )
                    )
                );

                doc.Save(path);
                var open = await Shell.Current.DisplayAlert(
                    "Zapisano",
                    $"Plik zapisano w:\n{path}",
                    "Otwórz",
                    "OK");

                if (open)
                {
                    try
                    {
                        await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
                    }
                    catch (Exception ex)
                    {
                        await Shell.Current.DisplayAlert("Błąd", $"Nie można otworzyć pliku: {ex.Message}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Błąd eksportu", ex.Message, "OK");
            }
        }

        [RelayCommand]
        public async Task ImportAsync()
        {
            var customFileTypes = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/xml", "text/xml" } },
                    { DevicePlatform.iOS, new[] { "public.xml" } },
                    { DevicePlatform.WinUI, new[] { ".xml" } },
                }
            );

            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Wybierz plik XML",
                    FileTypes = customFileTypes
                });

                if (result == null) return;

                using var stream = await result.OpenReadAsync();
                var doc = XDocument.Load(stream);

                if (doc.Root == null)
                {
                    await Shell.Current.DisplayAlert("Błąd", "Nieprawidłowy plik XML (brak korzenia).", "OK");
                    return;
                }

                var nodes = doc.Root.Elements("Product").ToList();

                var items = nodes
                    .Select(e => new ProductModel
                    {
                        Name = e.Element("Name")?.Value ?? string.Empty,
                        Category = e.Element("Category")?.Value ?? string.Empty,
                        Unit = e.Element("Unit")?.Value ?? string.Empty,
                        Amount = double.Parse(e.Element("Amount")?.Value ?? string.Empty),
                        IsBought = bool.TryParse(e.Element("IsBought")?.Value, out var b) && b,
                        IsOptional = bool.TryParse(e.Element("IsOptional")?.Value, out var o) && o,
                        Shop = e.Element("Shop")?.Value ?? string.Empty
                    })
                    .ToList();

                if (!items.Any())
                {
                    await Shell.Current.DisplayAlert("OK", "Nie znaleziono elementów do zaimportowania.", "OK");
                    return;
                }

                int addedCount = 0;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var prev = Products.Count;
                    foreach (var p in items)
                        Products.Add(p);
                    SaveProducts();
                    addedCount = Products.Count - prev;
                });

                await Shell.Current.DisplayAlert("OK", $"Dodano {addedCount} elementów. Razem: {Products.Count}.", "OK");
                RefreshGroupedProducts();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Błąd importu", ex.Message, "OK");
            }
        }

        partial void OnShopChanged(string value)
        {
            RefreshGroupedProducts();
        }

        private void RefreshGroupedProducts()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IEnumerable<ProductModel> filtered;
                if (string.IsNullOrWhiteSpace(Shop) || string.Equals(Shop, "Wszystkie"))
                    filtered = Products.Where(p => !p.IsBought);
                else
                    filtered = Products.Where(p => string.Equals(p.Shop, Shop) && !p.IsBought);

                var grouped = filtered
                    .GroupBy(p => p.Category ?? string.Empty)
                    .OrderBy(g => g.Key)
                    .ToList();

                var wantedGroupNames = new HashSet<string>(grouped.Select(g => g.Key));
                for (int i = GroupedProducts.Count - 1; i >= 0; i--)
                {
                    if (!wantedGroupNames.Contains(GroupedProducts[i].Name))
                        GroupedProducts.RemoveAt(i);
                }
                for (int targetGroupIndex = 0; targetGroupIndex < grouped.Count; targetGroupIndex++)
                {
                    var g = grouped[targetGroupIndex];
                    var groupName = g.Key;

                    var existingGroup = GroupedProducts.FirstOrDefault(x => string.Equals(x.Name, groupName, StringComparison.OrdinalIgnoreCase));
                    if (existingGroup == null)
                    {
                        existingGroup = new CategoryGroup(groupName);
                        GroupedProducts.Insert(targetGroupIndex, existingGroup);
                    }
                    else
                    {
                        var currentIndex = GroupedProducts.IndexOf(existingGroup);
                        if (currentIndex != targetGroupIndex)
                            GroupedProducts.Move(currentIndex, targetGroupIndex);
                    }

                    var desiredItems = g.OrderBy(x => x.Category).ThenBy(x => x.Name).ToList();

                    for (int i = existingGroup.Count - 1; i >= 0; i--)
                    {
                        if (!desiredItems.Contains(existingGroup[i]))
                            existingGroup.RemoveAt(i);
                    }

                    for (int i = 0; i < desiredItems.Count; i++)
                    {
                        var item = desiredItems[i];
                        var curIdx = existingGroup.IndexOf(item);
                        if (curIdx == -1)
                        {
                                existingGroup.Insert(i, item);
                            
                        }
                        else if (curIdx != i)
                        {
                            existingGroup.Move(curIdx, i);
                        }
                    }
                    existingGroup.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(existingGroup.ProductCount)));
                }
            });
        }
    }
}
