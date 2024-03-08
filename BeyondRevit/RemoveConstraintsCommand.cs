using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using BeyondRevit.UI;
using BeyondRevit.ViewModels;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using Forms = System.Windows.Forms;
using adskWindows = Autodesk.Windows;
using Autodesk.Revit.UI.Selection;
using System.Collections.ObjectModel;
using BeyondRevit.Models;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Drawing.Printing;
using static System.Drawing.Printing.PrinterSettings;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.ApplicationServices;

namespace BeyondRevit.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RemoveConstraintsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            IList<Reference> references = Utils.GetCurrentSelection(uidoc, null, "Select Elements from which to remove the Constraints");
            using(Transaction transaction = new Transaction(doc, "Remove Constraints"))
            {
                transaction.Start();
                foreach(Reference reference in references)
                {
                    Element element = doc.GetElement(reference);
                    IList<ElementId> constraints = GetElementConstraints(element);
                    foreach(ElementId id in constraints)
                    {
                        doc.Delete(id);
                    }
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }

        public static IList<ElementId> GetElementConstraints(Element element)
        {
            ElementCategoryFilter catFil = new ElementCategoryFilter(BuiltInCategory.OST_Constraints);
            IList<ElementId> constraints = element.GetDependentElements(catFil);
            return constraints;
        }
    }
}

