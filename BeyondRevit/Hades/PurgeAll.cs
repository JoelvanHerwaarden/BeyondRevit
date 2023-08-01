using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.Hades
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PurgeAll : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Purge Import Line Styles
            new PurgeImportedLineStyles().Command(commandData, false);

            //Purge Views not on Sheet
            new PurgeViewsNotOnSheet().Command(commandData, false);
            new PurgeLegendsAndSchedulesNotOnSheet().Command(commandData, false);

            //Purge Scope Boxes
            new PurgeScopeboxes().Command(commandData, false);

            //Purge View Filters
            new PurgeViewFilters().Command(commandData, false);

            //Purge View Templates
            new PurgeViewTemplates().Command(commandData, false);

			//Purge Normal
			Purge(commandData);

			//Purge Materials
			new PurgeMaterials().Command(commandData, false);

			Utils.Show("Purge Completed");
			return Result.Succeeded;
        }

		public void Purge(ExternalCommandData commandData)
		{
			//Note this won't purge materials

			UIDocument uidoc = commandData.Application.ActiveUIDocument;
			Document doc = uidoc.Document;

			string desiredRule = "Project contains unused families and types";

			//access the Performance adviser
			PerformanceAdviser perfAdviser = PerformanceAdviser.GetPerformanceAdviser();

			//create a list with all the rules
			IList<PerformanceAdviserRuleId> allRulesList = perfAdviser.GetAllRuleIds();

			//create an empty list to save the rules that we want to run 
			//(in the purge case just one rule)
			IList<PerformanceAdviserRuleId> rulesToExecute = new List<PerformanceAdviserRuleId>();

			//Iterate through each
			foreach (PerformanceAdviserRuleId r in allRulesList)
			{
				if (perfAdviser.GetRuleName(r).Equals(desiredRule))
				{
					rulesToExecute.Add(r);
				}
			}
			for (int i = 0; i < 5; i++)
			{
				//execute the rules and get the results
				IList<FailureMessage> failureMessages = perfAdviser.ExecuteRules(doc, rulesToExecute);

				//Check if there are results
				if (failureMessages.Count() == 0) return;

				ICollection<ElementId> failingElementsIds = failureMessages[0].GetFailingElements();

				using (Transaction t = new Transaction(doc, "Purge"))
				{
					t.Start();
					foreach (ElementId eid in failingElementsIds)
					{
						try
						{
							doc.Delete(eid);
						}
						catch
						{

						}
					}

					t.Commit();
				}
			}
		}
	}
}
