using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeyondRevit.UI;
using System.Reflection;

namespace BeyondRevit.Hades
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SearchAndDestroy : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Categories categories = doc.Settings.Categories;
            Dictionary<string, dynamic> searchableElements = new Dictionary<string, dynamic>();
            IEnumerable<Type> types = ReflectiveEnumerator.GetElementsWhichInheritedFrom();
            foreach (Type type in types)
            {
                searchableElements.Add(type.Name + " (Type)", type);
            }
            foreach(Category category in categories)
            {
                searchableElements.Add(category.Name + " (Category)", category);
            }
            searchableElements = Utils.SortDictionary(searchableElements);
            GenericDropdownWindow chooseCategories = new GenericDropdownWindow("Select Objects", "Select Types/Categories in which you want to Delete Elements", searchableElements, Utils.RevitWindow(commandData), true);
            chooseCategories.ShowDialog();
            if (chooseCategories.Cancelled)
            {
                return Result.Cancelled;
            }
            else
            {
                Dictionary<string, dynamic> purgableElements = new Dictionary<string, dynamic>();
                foreach (var obj in chooseCategories.SelectedItems)
                {
                    if(obj.GetType().ToString() == "Autodesk.Revit.DB.Category")
                    {
                        IList<Element> categoryElements = new FilteredElementCollector(doc).OfCategoryId(obj.Id).ToElements();
                        foreach (Element categoryElement in categoryElements)
                        {
                            string key = string.Format("{0} = {1} ({2})", categoryElement.Name, categoryElement.Id, obj.Name);
                            if (!purgableElements.ContainsKey(key))
                            {
                                purgableElements.Add(key, categoryElement);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            IList<Element> typeElements = new FilteredElementCollector(doc).OfClass(obj).ToElements();
                            foreach (Element typeElement in typeElements)
                            {
                                string key = string.Format("{0} = {1} ({2})", typeElement.Name, typeElement.Id, obj.Name);
                                if (!purgableElements.ContainsKey(key))
                                {
                                    purgableElements.Add(key, typeElement);
                                }
                            }
                        }
                        catch { }
                        
                    }
                    
                }
                purgableElements = Utils.SortDictionary(purgableElements);
                GenericDropdownWindow chooseElements = new GenericDropdownWindow("Select Elements To Delete", "Select All Elements you want to Purge from the Project", purgableElements, Utils.RevitWindow(commandData), true);
                chooseElements.ShowDialog();
                if (chooseElements.Cancelled)
                {
                    return Result.Cancelled;
                }
                else
                {
                    TaskDialogResult result = HadesUtils.Show("You have Selected " + chooseElements.SelectedItems.Count.ToString() + " Elements\nDo you want to delete them?");
                    string msg = "Errors:\n\n";
                    if (result == TaskDialogResult.Yes)
                    {
                        using (Transaction t = new Transaction(doc, "Purging Selected Elements"))
                        {
                            t.Start();
                            foreach (Element element in chooseElements.SelectedItems)
                            {
                                try
                                {

                                    doc.Delete(element.Id);
                                }
                                catch (Exception e)
                                {
                                    msg = e.Message + "\n";
                                }
                            }
                            t.Commit();
                        }
                    }
                    if(msg != "Errors:\n\n")
                    {
                        Utils.Show(msg);
                    }
                }
                return Result.Succeeded;
            }
        }

        public static class ReflectiveEnumerator
        {
            public static IEnumerable<Type> GetElementsWhichInheritedFrom()
            {
                Assembly assembly = GetAssemblyByName("RevitAPI");
                IEnumerable<Type> types = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Element)));
                return types;
            }
            public static Assembly GetAssemblyByName(string name)
            {
                return AppDomain.CurrentDomain.GetAssemblies().
                       SingleOrDefault(assembly => assembly.GetName().Name == name);
            }
        }
    }
}
