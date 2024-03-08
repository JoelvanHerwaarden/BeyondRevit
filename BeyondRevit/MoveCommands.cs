using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI.Selection;
using System.Xml.Linq;

namespace BeyondRevit.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RelativeMoveCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            IList<Reference> elementsToMove = Utils.GetCurrentSelection(uiDoc, null, "Select Elements to Move");

            return Result.Succeeded;
            
        }

        //private Dictionary<string, dynamic> GetElementsRelativeDirections(Element element)
        //{
        //    string type = element.GetType().ToString();
        //    Dictionary<string, dynamic> Directions = new Dictionary<string, dynamic>();
        //    //Options of Elements:
        //    //FamilyInstances Point Based
        //    if (type.Contains("FamilyInstance"))
        //    {

        //    }
        //    else if (type.Contains("Wall"))
        //    {
                 
        //    }

        //    //FamilyInstances Line Based
        //    //Walls
        //    //Floors -> SpanDirection?

        //}
    }
}