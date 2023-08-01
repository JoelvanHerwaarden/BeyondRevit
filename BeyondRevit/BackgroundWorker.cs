using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI.Selection;

namespace BeyondRevit.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ActivateDynamicBackground : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document document = uiDoc.Document;
            uiApp.ViewActivated += BackgroundUtils.View_Switched; 
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DeActivateDynamicBackground : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document document = uiDoc.Document;
            uiApp.ViewActivated -= BackgroundUtils.View_Switched;
            return Result.Succeeded;
        }
    }

    public class BackgroundUtils
    {
        public static void View_Switched(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            Color grayColor = new Color(195, 195, 195);
            Color whiteColor = new Color(255, 255, 255);
            Utils.Show("Did Something");
            ViewDisplayBackground grayBackground = ViewDisplayBackground.CreateGradient(grayColor, grayColor, grayColor);
            ViewDisplayBackground whiteBackground = ViewDisplayBackground.CreateGradient(whiteColor, whiteColor, whiteColor);
            e.PreviousActiveView.SetBackground(whiteBackground);
            e.CurrentActiveView.SetBackground(grayBackground);
            e.Document.Regenerate();
        }
    }
    
}
