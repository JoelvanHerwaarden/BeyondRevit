using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit
{
    public class DrawCurveUtils
    {
        public static ICollection<ElementId> DetailLinesBeforeDrawing { get; set; }
        public static ICollection<ElementId> DetailLinesAfterDrawing { get; set; }
        public static void PromptUserToDrawCurveCommandBinding_BeforeExecuted(object sender, BeforeExecutedEventArgs e)
        {
            Document document = e.ActiveDocument;
            DetailLinesBeforeDrawing = CollectAllLinesInView(document);
            Utils.Show("There are " + DetailLinesBeforeDrawing.Count.ToString());
        }

        public static void PromptUserToDrawCurveCommandBinding_AfterExecuted(object sender, IdlingEventArgs e)
        {
            UIApplication uiApp = (UIApplication)sender;
            DetailLinesAfterDrawing =  CollectAllLinesInView(uiApp.ActiveUIDocument.Document) ;
            if (DetailLinesAfterDrawing.Count > DetailLinesBeforeDrawing.Count)
            {
                Utils.Show("You are done Drawing Curves");
                uiApp.Idling -= PromptUserToDrawCurveCommandBinding_AfterExecuted;
            }
        }

        public static ICollection<ElementId> CollectAllLinesInView(Document doc)
        {
            ICollection<ElementId> curves = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Lines).ToElementIds();
            ICollection<ElementId> result = new List<ElementId>();
            foreach (ElementId id in curves)
            {
                Element curve = doc.GetElement(id);
                if (curve.GetType().ToString().Contains("Detail"))
                {
                    result.Add(id);
                }
            }
            return result;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DrawCurve : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            UIApplication uiApp = commandData.Application;



            AddInCommandBinding drawDetailCurveCommand = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.DetailLine));
            drawDetailCurveCommand.BeforeExecuted += DrawCurveUtils.PromptUserToDrawCurveCommandBinding_BeforeExecuted;
            uiApp.Idling += DrawCurveUtils.PromptUserToDrawCurveCommandBinding_AfterExecuted;
            uiApp.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.DetailLine));
            return Result.Succeeded;
        }

    }
}
