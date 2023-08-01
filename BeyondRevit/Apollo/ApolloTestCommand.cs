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
using Autodesk.Revit.DB.Macros;
using BeyondRevit.Apollo.UI;

namespace BeyondRevit.Commands
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ApolloTestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ChatWindow window = new ChatWindow();
            window.Owner = Utils.RevitWindow(commandData);
            window.ShowDialog();
            return Result.Succeeded;
        }

    }

}