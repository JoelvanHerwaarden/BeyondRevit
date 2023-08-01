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

namespace BeyondRevit.Commands
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ApolloCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;
            
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            MacroManager manager = MacroManager.GetMacroManager(app);
            
            MacroModule apolloModule = WakeupApollo(manager);
            CleanUp(apolloModule);
            Macro apolloMacro = apolloModule.AddMacro("Apollo", "Test Code", Code());
            try
            {
                apolloMacro.Execute();
            }
            catch(Exception e)
            {
                Utils.Show(e.Message);
            }
            return Result.Succeeded;
        }

        public string Code()
        {
            string code =  File.ReadAllText(@"C:\Users\907335\Desktop\ApolloExample.txt");
            return code;
        }

        private MacroModule WakeupApollo(MacroManager manager)
        {
            MacroModule result = null;
            IEnumerator<MacroModule> modules = manager.GetEnumerator();
            while (modules.MoveNext())
            {
                MacroModule current = modules.Current;
                if(current.Name == "Apollo")
                {
                    result = current;
                    break;
                }
            }
            return result;
        }

        private bool CleanUp(MacroModule apollo)
        {
            Macro apolloCommand = null;
            IEnumerator<Macro> macros =  apollo.GetEnumerator();
            while (macros.MoveNext())
            {
                Macro current = macros.Current;
                if(current.Name == "ApolloCommand")
                {
                    apolloCommand = current;
                }
            }
            if (apolloCommand != null)
            {
                apollo.RemoveMacro(apolloCommand);
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}