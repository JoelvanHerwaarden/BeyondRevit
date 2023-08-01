using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ParameterChecker : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ElementId selectedElementId = Utils.GetCurrentSelectionSingleElement(uidoc, null, "Select element").ElementId;

            Element selectedElement = doc.GetElement(selectedElementId);
            Element selectedElementType = doc.GetElement(selectedElement.GetTypeId());

            Dictionary<string, dynamic> parameters = new Dictionary<string, dynamic>();
            foreach(Parameter p in selectedElement.Parameters)
            {
                string key = string.Format("{0} - {1}", p.Definition.Name, p.Id);
                if (!parameters.ContainsKey(key))
                {
                    parameters.Add(key, p.Definition.Name);
                }
            }
            foreach (Parameter p in selectedElementType.Parameters)
            {
                string key = string.Format("{0} - {1}", p.Definition.Name, p.Id);
                if (!parameters.ContainsKey(key))
                {
                    parameters.Add(key, p.Definition.Name);
                }
            }
            Utils.SortDictionary(parameters);
            BeyondRevit.UI.GenericDropdownWindow dropdown = new UI.GenericDropdownWindow("Select Parameters", "Select the Parameter to Check", parameters, Utils.RevitWindow(commandData), false);
            dropdown.ShowDialog();
            bool typeParameter = false;
            string selectedParameter = dropdown.SelectedItems.First();
            if (selectedElement.LookupParameter(dropdown.SelectedItems.First()) == null)
            {
                typeParameter = true;
            }
            if (!dropdown.Cancelled)
            {
                IList<ElementId> elementsToIsolate = new List<ElementId>();
                foreach(Element element in new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType().ToElements())
                {
                    Element elementToEvaluate = element;
                    if (typeParameter)
                    {
                        elementToEvaluate = doc.GetElement(element.GetTypeId());
                    }
                    if(elementToEvaluate != null)
                    {
                        Parameter p = elementToEvaluate.LookupParameter(selectedParameter);
                        if (p != null)
                        {
                            string value = p.AsString();
                            if (value == null | value == "")
                            {
                                elementsToIsolate.Add(element.Id);
                            }
                        }

                    }
                    
                }
                using(Transaction transaction = new Transaction(doc, "Isolate Elements with Empty Parameters"))
                {
                    transaction.Start();
                    doc.ActiveView.IsolateElementsTemporary(elementsToIsolate);
                    transaction.Commit();
                }
            }
            return Result.Succeeded;
        }
    }
}
