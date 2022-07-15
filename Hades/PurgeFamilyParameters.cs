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
    public class PurgeUnusedFamilyParamers : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            if (!doc.IsFamilyDocument)
            {
                Utils.Show("This Command can only Run in Family Documents");
            }
            Dictionary<string, dynamic> unusedParameterData = GetUnusedFamilyParameters(doc);
            List<FamilyParameter> unusedParameters = unusedParameterData["UnusedParameters"];
            if(HadesUtils.Show("Found " + unusedParameters.Count + " Unused Family Parameters. Do you want to Delete Them?", unusedParameterData["WhereUsedParameters"]) == TaskDialogResult.Yes)
            {
                using (Transaction transaction = new Transaction(doc, "Purge Unused Family Parameters"))
                {
                    transaction.Start();

                    foreach (FamilyParameter parameter in unusedParameters)
                    {
                        try
                        {

                            FamilyManager manager = doc.FamilyManager;
                            manager.RemoveParameter(parameter);
                        }
                        catch { }
                    }
                    transaction.Commit();
                }
            }
            return Result.Succeeded;
        }

        public static Dictionary<string, dynamic>  GetUnusedFamilyParameters(Document document)
        {
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>
            {
                {"UnusedParameters", new List<FamilyParameter>() },
                {"WhereUsedParameters", "" }
            };
            string report = "";
            FamilyManager manager = document.FamilyManager;
            FamilyParameterSet parameters = manager.Parameters;
            foreach(FamilyParameter parameter in parameters)
            {
                Dictionary<string, dynamic> isUsedDictionary = IsParameterUsedSomewhere(document, parameter);
                result["WhereUsedParameters"] += isUsedDictionary["WhereUsed"];
                if (!isUsedDictionary["IsUsed"])
                {
                    result["UnusedParameters"].Add(parameter);
                }
            }
            return result;

        }
        public static Dictionary<string, dynamic> IsParameterUsedSomewhere(Document document, FamilyParameter parameter)
        {
            Dictionary<string, dynamic> result = new Dictionary<string, dynamic>();
            //string result = "";
            List<string> formulaParameters = GetAllUsedParametersInFormulas(document.FamilyManager.Parameters);
            if (!parameter.AssociatedParameters.IsEmpty)
            {
                result.Add("IsUsed", true);
                result.Add("WhereUsed", parameter.Definition.Name + " Is used in another Element\n");
                return result;
            }
            else if(formulaParameters.Contains(parameter.Definition.Name))
            {
                result.Add("IsUsed", true);
                result.Add("WhereUsed", parameter.Definition.Name + " is used in a Formula\n");
                return result;
            }
            else if (IsParameterUsedInDimension(document, parameter))
            {
                result.Add("IsUsed", true);
                result.Add("WhereUsed", parameter.Definition.Name + " Is used in a Dimension\n");
                return result;

            }

            result.Add("IsUsed", false);
            result.Add("WhereUsed", parameter.Definition.Name + " Is not used in an Element, Formula, or Dimension\n");
            return result;
        }

        public static string[] ExtractParametersFromFormulaString(FamilyParameter parameter)
        {
            
            string[] result = null;
            string[] forbiddenCharacters = new string[]
            {
                "if(",
                "atan(",
                "acon(",
                "asin(",
                "tan(",
                "cos(",
                "sin(",
                ")",
                "(",
                ",",
                "-",
                "+",
                "*",
                "/",
                ">",
                "<",
                "=",
            };
            string formulaString = parameter.Formula;

            if(formulaString != String.Empty & formulaString != null)
            {
                result = formulaString.Split(forbiddenCharacters, StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i< result.Length; i++)
                {
                    string txt = result[i];
                    result[i] = txt.Trim();
                }
            }
            return result;
        }
        public static List<string> GetAllUsedParametersInFormulas(FamilyParameterSet parameters)
        {
            List<string> formulaParameters = new List<string>();
            foreach(FamilyParameter parameter in parameters)
            {
                string[] formulaParametersInParameter = ExtractParametersFromFormulaString(parameter);
                if (formulaParametersInParameter != null)
                {
                    formulaParameters.AddRange(formulaParametersInParameter);
                }
            }
            return formulaParameters;
        }

        public static bool IsParameterUsedInDimension(Document doc, FamilyParameter parameter)
        {
            bool result = false;
            string msg = "";
            ICollection<Element> dimensions = new FilteredElementCollector(doc).OfClass(typeof(Dimension)).ToElements();
            foreach (Dimension dimension in dimensions)
            {
                try
                {
                    FamilyParameter associatedParameter = dimension.FamilyLabel;
                    if (associatedParameter != null)
                    {
                        if (associatedParameter.Id == parameter.Id)
                        {
                            result = true;
                            break;
                        }

                    }
                }
                catch { }
                
            }
            return result;
        }
    }
}
