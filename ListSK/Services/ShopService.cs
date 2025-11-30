using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ListSK.Services
{
    public static class ShopService
    {
        private const string FileName ="shops.xml";
        public static List<string> LoadShops()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, FileName);

            if (!File.Exists(path))
                return new List<string> {"Wszystkie", "Biedronka", "Lidl", "Selgros", "Żabka" };

            var doc = XDocument.Load(path);

            return doc.Root
                .Elements("Shop")
                .Select(c => c.Value)
                .ToList();
        }

        public static void SaveShops(List<string> shops)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Shops",
                    shops.Select(c => new XElement("Shop", c))
                )
            );

            var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
            doc.Save(path);
        }

        public static void AddShop(string newShop)
        {
            var shops = LoadShops();

            if (!shops.Contains(newShop))
            {
                shops.Add(newShop);
                SaveShops(shops);
            }
        }
    }
}
