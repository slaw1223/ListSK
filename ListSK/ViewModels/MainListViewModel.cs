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
using System.Xml.Serialization;

namespace ListSK.ViewModels
{
    public partial class MainListViewModel : ObservableObject
    {
        private const string FileName = "products.xml";

        [ObservableProperty]
        private ObservableCollection<ProductModel> products = new();

        public MainListViewModel()
        {
            Products.CollectionChanged += Products_CollectionChanged;
            LoadProducts();
        }

        public void AddProduct(ProductModel product)
        {
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

        private void Products_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProductModel item in e.NewItems)
                {
                    item.PropertyChanged += Product_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (ProductModel item in e.OldItems)
                {
                    item.PropertyChanged -= Product_PropertyChanged;
                }
            }
        }

        private void Product_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductModel.IsBought))
            {
                var sorted = Products
                    .OrderBy(p => p.IsBought)
                    .ToList();

                Products.Clear();

                foreach (var p in sorted)
                    Products.Add(p);
                SaveProducts();
            }
            else
            {
                SaveProducts();
            }
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
                                new XElement("Amount", p.Amount ?? string.Empty),
                                new XElement("IsBought", p.IsBought),
                                new XElement("IsOptional", p.IsOptional)
                            )
                        )
                    )
                );
                var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
                doc.Save(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd zapisy: {ex}");
            }
        }

        public void LoadProducts()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
                if (!File.Exists(path))
                {
                    return;
                }
                var doc = XDocument.Load(path);

                Products.Clear();

                foreach (var element in doc.Root?.Elements().Where(e => e.Element("Name") != null) ?? Enumerable.Empty<XElement>())
                {
                    var product = new ProductModel
                    {
                        Name = element.Element("Name")?.Value ?? string.Empty,
                        Category = element.Element("Category")?.Value ?? string.Empty,
                        Unit = element.Element("Unit")?.Value ?? string.Empty,
                        Amount = element.Element("Amount")?.Value ?? string.Empty,
                        IsBought = bool.TryParse(element.Element("IsBought")?.Value, out var b) && b,
                        IsOptional = bool.TryParse(element.Element("IsOptional")?.Value, out var o) && o
                    };
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd ładowania: {ex}");
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
                                new XElement("Amount", p.Amount ?? string.Empty),
                                new XElement("IsBought", p.IsBought),
                                new XElement("IsOptional", p.IsOptional)
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
#if WINDOWS
            try
            {
                var psi = new ProcessStartInfo("notepad.exe", $"\"{path}\"")
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
            }
#else
                    await Launcher.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
#endif
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

                if (result == null)
                    return;

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
                        Amount = e.Element("Amount")?.Value ?? string.Empty,
                        IsBought = bool.TryParse(e.Element("IsBought")?.Value, out var b) && b,
                        IsOptional = bool.TryParse(e.Element("IsOptional")?.Value, out var o) && o
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
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Błąd importu", ex.Message, "OK");
            }
        }

    }
}