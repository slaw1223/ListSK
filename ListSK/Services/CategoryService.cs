using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ListSK.Services
{
    public static class CategoryService
    {
        private const string FileName = "categories.xml";

        public static List<string> LoadCategories()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, FileName);

            if (!File.Exists(path))
                return new List<string> { "Warzywa", "Nabiał", "Mięso", "Napoje" };

            var doc = XDocument.Load(path);

            return doc.Root
                .Elements("Category")
                .Select(c => c.Value)
                .ToList();
        }

        public static void SaveCategories(List<string> categories)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Categories",
                    categories.Select(c => new XElement("Category", c))
                )
            );

            var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
            doc.Save(path);
        }

        public static void AddCategory(string newCat)
        {
            var cats = LoadCategories();

            if (!cats.Contains(newCat))
            {
                cats.Add(newCat);
                SaveCategories(cats);
            }
        }
    }
}
