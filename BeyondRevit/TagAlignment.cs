using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TagAlignmentTool;

namespace BeyondRevit
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BirdToolsAlignTags : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Utils.Show(TagAlignmentTool.BirdTagAlignmentTool.entt.ToString());
            BirdTagAlignmentTool.entt = 1;

            TagAlign command = new TagAlign();
            command.Execute(commandData, ref message, elements);
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BirdToolsAlignTagsModless : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Utils.Show(TagAlignmentTool.BirdTagAlignmentTool.entt.ToString());
            BirdTagAlignmentTool.entt = 1;

            TATMod command = new TATMod();
            //command.Execute(commandData, ref message, elements);
            UIApplication app = commandData.Application;
            RevitCommandId revitCommandId = RevitCommandId.LookupCommandId("TATMOD");
            app.PostCommand(revitCommandId);
            return Result.Succeeded;
        }
    }
}
