using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AddItemsToTemporaryIsolation : IExternalCommand
    {
        public Dictionary<string, OverrideGraphicSettings> OGOverrides { get; set; }
        internal sealed class IsolationFilter : ISelectionFilter
        {
            public IList<ElementId> ElementsVisibleInIsolation { get; set; }
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;


                if (!this.ElementsVisibleInIsolation.Contains(elem.Id))
                {
                    return true;
                }

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }

        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;
            View currentView = document.ActiveView;

            using (Transaction transaction = new Transaction(document, "Add Elements to Isolation"))
            {
                transaction.Start();

                //Get Elements currently Visible
                IList<ElementId> visibleElements = new List<ElementId>();
                if (currentView.IsTemporaryHideIsolateActive())
                {
                    visibleElements = new FilteredElementCollector(document, currentView.Id).WhereElementIsNotElementType().ToElementIds().ToList<ElementId>();
                }
                else
                {
                    return Result.Succeeded;
                }

                //Override Isolation Elements
                OverrideIsolatedElements(currentView, visibleElements);

                //Deactivate the Isolation Mode
                TemporaryViewModes modes = currentView.TemporaryViewModes;
                modes.DeactivateAllModes();

                //Select Elements to Add to the Isolation
                IsolationFilter filter = new IsolationFilter();
                filter.ElementsVisibleInIsolation = visibleElements;
                IList<Reference> references = selection.PickObjects(ObjectType.Element, filter, "Select Elements to Add to the Temporary Isolation");

                //Remove Overrides
                RemoveOverridesIsolatedElements(currentView);

                //Add Selected Elements to the Isolation Elements
                foreach (Reference r in references)
                {
                    visibleElements.Add(r.ElementId);
                }

                //Isolate them
                currentView.IsolateElementsTemporary(visibleElements);
                transaction.Commit();
            }

            uidoc.RefreshActiveView();
            return Result.Succeeded;
        }

        private void OverrideIsolatedElements(View ActiveView, IList<ElementId> elementIds)
        {
            this.OGOverrides = new Dictionary<string, OverrideGraphicSettings>();
            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
            overrideGraphicSettings.SetSurfaceForegroundPatternColor(new Autodesk.Revit.DB.Color(128, 255, 0));
            overrideGraphicSettings.SetSurfaceTransparency(40);
            ElementId pattern = GetFillPatternByName(ActiveView.Document);
            if (pattern != null)
            {
                overrideGraphicSettings.SetSurfaceForegroundPatternId(pattern);
            }
            foreach (ElementId element in elementIds)
            {
                OverrideGraphicSettings overrides = ActiveView.GetElementOverrides(element);
                OGOverrides.Add(element.ToString(), overrides);
                ActiveView.SetElementOverrides(element, overrideGraphicSettings);
            }
        }

        private static ElementId GetFillPatternByName(Document document)
        {
            ElementId result = null;
            IList<Element> patterns = new FilteredElementCollector(document).OfClass(typeof(FillPatternElement)).ToElements();
            foreach (Element pat in patterns)
            {
                FillPatternElement p = (FillPatternElement)pat;
                if (p.GetFillPattern().IsSolidFill)
                {

                    result = pat.Id;
                }
            }
            return result;
        }
        private void RemoveOverridesIsolatedElements(View activeView)
        {
            foreach (string key in this.OGOverrides.Keys)
            {
                OverrideGraphicSettings settings = OGOverrides[key];
                activeView.SetElementOverrides(new ElementId(int.Parse(key)), settings);
            }
        }
    }
    
}
