using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using BeyondRevit.UI;
using Autodesk.Revit.Creation;

namespace BeyondRevit.Commands
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LinkFamilyParameters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document document = uiDocument.Document;

            if (!document.IsFamilyDocument)
            {
                Utils.Show("This command can only run in a Family Document");
                return Result.Succeeded;
            }

            Selection selection = uiDocument.Selection;
            using(Transaction transaction = new Transaction(document, "Link Family Parameters"))
            {
                transaction.Start();
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    
                    Reference referenceElement = Utils.GetCurrentSelectionSingleElement(uiDocument, null, "Select Family Instance which parameter you want to link");
                    Element element = document.GetElement(referenceElement);
                    
                    Dictionary<string, dynamic> parameters = FamilyParameterUtils.GetEditableParametersFromElement(element);
                    GenericDropdownWindow window = new GenericDropdownWindow("Select parameters", "Select Parameter which you want to Link", parameters, Utils.RevitWindow(commandData), true);
                    window.ShowDialog();
                    if (!window.Cancelled)
                    {
                        List<dynamic> selectedParameters = window.SelectedItems;
                        foreach (Parameter parameter in selectedParameters)
                        {
                            FamilyParameterUtils.CreateFamilyParameterByParameter(document, parameter, element.Category);
                        }
                    }
                }
                catch(OperationCanceledException e)
                {
                    return Result.Succeeded;
                }
#pragma warning restore CS0168 // Variable is declared but never used
                transaction.Commit();
            }
            return Result.Succeeded;


        }
    }
    public class FamilyParameterUtils
    {
        public static Dictionary<string, dynamic> GetEditableParametersFromElement(Element element)
        {
            Dictionary<string, dynamic> Result = new Dictionary<string, dynamic>();
            int version = int.Parse(element.Document.Application.VersionNumber);
            foreach (Parameter p in element.Parameters)
            {

                Result.Add(string.Format("{0} (Instance) - {1}", p.Definition.Name, p.Id.ToString()) , p);
            }
            ElementType type = (ElementType)element.Document.GetElement(element.GetTypeId());
            if (type != null)
            {
                foreach (Parameter p in type.Parameters)
                {
                    Result.Add(string.Format("{0} (Type) - {1}", p.Definition.Name, p.Id.ToString()), p);
                }
            }


            return Result;
        }

        public static FamilyParameter CreateFamilyParameterByParameter(Autodesk.Revit.DB.Document familyDocument, Parameter parameter, Category category = null)
        {
            FamilyManager manager = familyDocument.FamilyManager;
            MakeSureThereIsDefaultType(manager);
            bool instance = true;
            if(typeof(ElementType).IsAssignableFrom(parameter.Element.GetType()))
            {
                instance = false;
            }
            FamilyParameter famparameter = GetFamilyParameterByName(familyDocument, parameter);
            if (famparameter == null)
            {
                famparameter = manager.AddParameter(parameter.Definition.Name, parameter.Definition.ParameterGroup, category, instance);
                //if (category == null)
                //{
                //    famparameter = manager.AddParameter(parameter.Definition.Name, parameter.Definition.ParameterGroup, parameter.Definition.ParameterType, instance);

                //}
                //else
                //{
                    
                //}

            }
            StorageType storageType = parameter.StorageType;
            switch (storageType)
            {
                case StorageType.Double:
                    dynamic value = parameter.AsDouble();
                    if (value != null)
                    {
                        manager.Set(famparameter, value);
                    }
                    break;
                case StorageType.Integer:
                    value = parameter.AsInteger();
                    if (value != null)
                    {
                        manager.Set(famparameter, value);
                    }
                    break;
                case StorageType.ElementId:
                    value = parameter.AsElementId();
                    if (value != null)
                    {
                        manager.Set(famparameter, value);
                    }
                    break;
                case StorageType.String:
                    value = parameter.AsString();
                    if (value != null)
                    {
                        manager.Set(famparameter, value);
                    }
                    break;
            }
            manager.AssociateElementParameterToFamilyParameter(parameter, famparameter);

            return null;
        }

        public static FamilyParameter GetFamilyParameterByName(Autodesk.Revit.DB.Document familyDocument, Parameter parameter)
        {
            FamilyParameter result = null;
            FamilyParameterSet parameters = familyDocument.FamilyManager.Parameters;
            foreach(FamilyParameter famParameter in parameters)
            {
                if(famParameter.Definition.Name == parameter.Definition.Name)
                {
                    result = famParameter;
                    break;
                    //if (famParameter.Definition.ParameterType == parameter.Definition.ParameterType &
                    //    famParameter.Definition.ParameterGroup == parameter.Definition.ParameterGroup)
                    //{
                    //    break;
                    //}
                }
            }
            return result;

        }

        public static void MakeSureThereIsDefaultType(FamilyManager manager)
        {
            if(manager.CurrentType == null)
            {
                FamilyTypeSet types = manager.Types;
                if(types.IsEmpty)
                {
                    manager.NewType("Default Type");
                }
                else
                {
                    manager.CurrentType = (FamilyType)types.GetEnumerator().Current;
                }
            }
        }

    }
}
