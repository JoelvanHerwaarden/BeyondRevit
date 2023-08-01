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
    public class GoToNextPhase : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document document = uiDoc.Document;
            using (Transaction transaction = new Transaction(document, "Next Phase"))
            {
                transaction.Start();
                Phase nextPhase = PhasingUtils.GetNextPhase(document.ActiveView);
                if (nextPhase != null)
                {
                    document.ActiveView.get_Parameter(BuiltInParameter.VIEW_PHASE).Set(nextPhase.Id);
                    transaction.Commit();
                }
                else
                {
                    transaction.RollBack();
                }
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GoToPreviousPhase : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiApp.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document document = uiDoc.Document;
            using (Transaction transaction = new Transaction(document, "Previous Phase"))
            {
                transaction.Start();
                Phase previousPhase = PhasingUtils.GetPreviousPhase(document.ActiveView);
                if (previousPhase != null)
                {
                    document.ActiveView.get_Parameter(BuiltInParameter.VIEW_PHASE).Set(previousPhase.Id);
                    transaction.Commit();
                }
                else
                {
                    transaction.RollBack();
                }
            }
            return Result.Succeeded;
        }
    }

    public class PhasingUtils
    {
        public static Phase GetNextPhase(View view)
        {
            Phase nextPhase = null;
            ElementId currentPhaseId = view.get_Parameter(BuiltInParameter.VIEW_PHASE).AsElementId();
            Phase phase = (Phase)view.Document.GetElement(currentPhaseId);
            int phaseNumber = phase.get_Parameter(BuiltInParameter.PHASE_SEQUENCE_NUMBER).AsInteger() + 1;
            Dictionary<string, Phase> phases = GetAllPhases(view.Document);
            if (phases.ContainsKey(phaseNumber.ToString()))
            {
                nextPhase = phases[phaseNumber.ToString()];
            }
            else
            {
                Utils.Show("This is the last Phase in the Project.\nThere are no more Phases left");
            }
            return nextPhase;
        }
        public static Phase GetPreviousPhase(View view)
        {
            Phase previousPhase = null;
            ElementId currentPhaseId = view.get_Parameter(BuiltInParameter.VIEW_PHASE).AsElementId();
            Phase phase = (Phase)view.Document.GetElement(currentPhaseId);
            int phaseNumber = phase.get_Parameter(BuiltInParameter.PHASE_SEQUENCE_NUMBER).AsInteger()-1;
            Dictionary<string, Phase> phases = GetAllPhases(view.Document);
            if (phases.ContainsKey(phaseNumber.ToString()))
            {
                previousPhase = phases[phaseNumber.ToString()];
            }
            else
            {
                Utils.Show("This is the first Phase in the Project.\nThere are no more Phases before this one");
            }
            return previousPhase;
        }

        public static Dictionary<string, Phase> GetAllPhases(Document document)
        {
            Dictionary<string, Phase> phases = new Dictionary<string, Phase>();
            ICollection<ElementId> elementIds = new FilteredElementCollector(document).OfClass(typeof(Phase)).ToElementIds();
            foreach (ElementId elementId in elementIds)
            {
                Phase phase = (Phase)document.GetElement(elementId);
                string key = phase.get_Parameter(BuiltInParameter.PHASE_SEQUENCE_NUMBER).AsInteger().ToString();
                phases.Add(key, phase);
            }
            return phases;
        }
    }

}
