using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeyondRevit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RenameFilters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            IList<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).ToElements();
            using(Transaction transaction = new Transaction(doc, "Rename Filters"))
            {
                transaction.Start();
                foreach(ParameterFilterElement filter in filters)
                {
                    Dictionary<string, List<string>> parameterNames = GetParameterValuesForFilter(filter);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
        public static List<string> GetParameterNamesForFilter(ParameterFilterElement filter)
        {
            List<string> parameterNames = new List<string>();
            foreach(ElementId id in filter.GetElementFilterParameters())
            {
                parameterNames.Add(filter.Document.GetElement(id).Name);
            }
            return parameterNames;
        }
        public static Dictionary<string, List<string>> GetParameterValuesForFilter(ParameterFilterElement filter)
        {
            Document doc = filter.Document;
            Dictionary<string, List<string>> FilterProperties = new Dictionary<string, List<string>>()
            {
                {"Parameters", new List<string>() },
                {"Rules", new List<string>() },
                {"Values", new List<string>() }
            };
            dynamic innerFilter = filter.GetElementFilter();
            foreach(ElementParameterFilter parameterFilter in innerFilter.GetFilters())
            {
                IList<FilterRule> rules = parameterFilter.GetRules();
                foreach(dynamic rule in rules)
                {
                    string parameterName = doc.GetElement(rule.GetRuleParameter()).Name;
                    string parameterRule = rule.GetType().ToString().Replace("Filter", "").Replace("String", "").Replace("Numeric", "").Replace("Integer", "");
                    string parameterValue = "";
                    try
                    {
                        parameterValue = rule.RuleValue;
                    }
                    catch { }
                    FilterProperties["Parameters"].Add(parameterName);
                    FilterProperties["Rules"].Add(parameterValue);
                    FilterProperties["Values"].Add(parameterRule);
                }
            }
            return FilterProperties;
        }
    }
}
