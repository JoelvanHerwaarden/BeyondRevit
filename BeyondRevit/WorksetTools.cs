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
    public class CopyWorksetsFromLinkedInstance : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            Reference selectedLink = selection.PickObject(ObjectType.Element,
                new Utils.TypeSelectionFilter(doc,
                new List<Type>()
                {
                    typeof(RevitLinkInstance)
                }, true), "Select Linked Revit Model");

            RevitLinkInstance instance = (RevitLinkInstance)doc.GetElement(selectedLink);
            Document linkDoc = instance.GetLinkDocument();

            IEnumerable<string> currentWorksetNames = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets().Select(ws => ws.Name);
            IList<Workset> linkedWorksets = new FilteredWorksetCollector(linkDoc).OfKind(WorksetKind.UserWorkset).ToWorksets().Where(ws => !currentWorksetNames.Contains(ws.Name)).ToList();

            Dictionary<string, dynamic> map = new Dictionary<string, dynamic>();
            foreach (Workset workset in linkedWorksets)
            {
                map.Add(workset.Name, workset);
            }
            GenericDropdownWindow window = new GenericDropdownWindow("Select Worksets", "Select Workset from the linked model to import", map, Utils.RevitWindow(commandData), true);
            window.ShowDialog();
            if (!window.Cancelled)
            {
                List<dynamic> worksets = window.SelectedItems;
                using (Transaction transaction = new Transaction(doc, "Import Workset"))
                {
                    transaction.Start();
                    foreach (Workset workset in worksets)
                    {
                        Workset.Create(doc, workset.Name);
                    }
                    transaction.Commit();
                }
            }

            return Result.Succeeded;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class IsolateCurrentWorksetInView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Workset currentWorkset = WorksetUtils.GetActiveWorkset(doc);
            View view = doc.ActiveView;
            using(Transaction t = new Transaction(doc, "Isolate Current Workset"))
            {
                t.Start();
                ICollection<WorksetId> collector = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksetIds();
                foreach(WorksetId ws in collector)
                {
                    view.SetWorksetVisibility(ws, WorksetVisibility.Hidden);
                }
                view.SetWorksetVisibility(currentWorkset.Id, WorksetVisibility.Visible);
                t.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenShownWorksetsForView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Workset currentWorkset = WorksetUtils.GetActiveWorkset(doc);
            Dictionary<string, dynamic> worksetsDictionary = new Dictionary<string, dynamic>();
            View view = doc.ActiveView;
            using (Transaction t = new Transaction(doc, "Isolate Current Workset"))
            {
                t.Start();
                ICollection<WorksetId> collector = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksetIds();
                foreach (WorksetId ws in collector)
                {
                    WorksetVisibility visibility = view.GetWorksetVisibility(ws);
                    if(visibility == WorksetVisibility.Visible) 
                    {

                    }
                }
                view.SetWorksetVisibility(currentWorkset.Id, WorksetVisibility.Visible);
                t.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class IsolateMultipleWorksetsInCurrentView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Workset currentWorkset = WorksetUtils.GetActiveWorkset(doc);
            View view = doc.ActiveView;


            ICollection<WorksetId> collector = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksetIds();
            WorksetTable table = doc.GetWorksetTable();
            Dictionary<string, dynamic> map = new Dictionary<string, dynamic>();
            foreach(WorksetId id in collector)
            {
                Workset ws = table.GetWorkset(id);
                map.Add(ws.Name, id);
            }
            GenericDropdownWindow window = new GenericDropdownWindow("Isolate Worksets", "Select worksets to isolate in view", map, Utils.RevitWindow(commandData), true);
            window.ShowDialog();
            if (window.Cancelled)
            {
                return Result.Cancelled;
            }
            using (Transaction t = new Transaction(doc, "Isolate Current Workset"))
            {
                t.Start();
                foreach (WorksetId ws in collector)
                {
                    view.SetWorksetVisibility(ws, WorksetVisibility.Hidden);
                }
                foreach (WorksetId ws in window.SelectedItems)
                {
                    view.SetWorksetVisibility(ws, WorksetVisibility.Visible);
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }

    public static class WorksetUtils
    {
        public static Workset GetActiveWorkset(Document doc)
        {
            if (doc.IsWorkshared)
            {
                WorksetId activeWorksetId = doc.GetWorksetTable().GetActiveWorksetId();
                return doc.GetWorksetTable().GetWorkset(activeWorksetId);
            }
            return null;
        }
    }

}