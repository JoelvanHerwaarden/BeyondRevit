using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using adWin = Autodesk.Windows;
using Autodesk.Revit.DB;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Windows;
using BeyondRevit.Commands;
using System.Windows.Interop;

namespace BeyondRevit
{
    public class BRApplication : IExternalApplication
    {
        private static RibbonPanel SheetPanel { get; set; }
        private static RibbonPanel HadesPanel { get; set; }
        private static RibbonPanel SelectionPanel { get; set; }
        private static RibbonPanel JoinCutUtilsPanel { get; set; }
        private static RibbonPanel DimensionsPanel { get; set; }
        private static RibbonPanel SyncPanel { get; set; }
        private static RibbonPanel QuickCommandPanel { get; set; }
        public static PushButton CommandLineButton { get; set; }
        public static PushButton OrganizeViewsButton { get; set; }
        public static PushButton AutoSyncButton { get; set; }
        public static PushButton PauzeAutoSyncButton { get; set; }
        public static PushButton SyncSettingsButton { get; set; }
        public static PulldownButtonData HadesPulldownButton { get; set; }

        public Result OnShutdown(UIControlledApplication application)
        {
            SynchronisationConfig.Current.SaveConfigSettings();
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            Window revitWindow = HwndSource.FromHwnd(application.MainWindowHandle).RootVisual as Window;
            BeyondRevit.UI.SplashScreen splashScreen = new BeyondRevit.UI.SplashScreen(revitWindow);

            //MIPOverride.TrainingWheelsProtocol(application);

            splashScreen.ShowSplash();

            SetupRibbonPanels(application);
            CommandLineButton = CreateRibbonButton("Command Line", "CommandLine", "BeyondRevit.Commands.CommandLine", QuickCommandPanel, "commandLine_32.bmp");

            OrganizeViewsButton = CreateRibbonButton("Organize Views", "OrganizeViews", "BeyondRevit.Commands.OrganizeViewsOnSheet", SheetPanel, "organizeViews_32.bmp");

            string[] titles = new List<string>() { "Select All Instances in View", "Select All Instances on the same Sheet", "Select All Instances in the Project", "Select All Types in View", "Select All Types on the same Sheet", "Select All Types in the Project" }.ToArray();
            string[] buttonNames = new List<string>() { "Instances In View", "Instances on Sheet", "Instances In Project", "Types In View", "Types on Sheet", "Types In Project" }.ToArray();
            string[] commands = new List<string>() 
            {
                "BeyondRevit.Commands.SelectAllInstancesInCurrentView",
                "BeyondRevit.Commands.SelectAllInstancesInCurrentSheet",
                "BeyondRevit.Commands.SelectAllInstancesInProject",
                "BeyondRevit.Commands.SelectAllTypesInCurrentView",
                "BeyondRevit.Commands.SelectAllTypesInCurrentSheet",
                "BeyondRevit.Commands.SelectAllTypesInProject"}.ToArray();
            string[] images = new List<string>() { "selectInstancesInView_32.bmp", "selectInstancesInView_32.bmp" , "selectInstancesInView_32.bmp", "selectInstancesInView_32.bmp" , "selectInstancesInView_32.bmp", "selectInstancesInView_32.bmp" }.ToArray();
            string[] tooltips = new List<string>()
            {
                "Select all Family Instances in the current View. You can Select multiple FamilyInstances.",
                "Select all Family Instances which are displayed on the same Sheet as the current view. \nYou can Select multiple FamilyInstances.",
                "Select all Family Instances in the whole Project. You can Select multiple FamilyInstances.",
                "Select all Types related to the FamilyInstances you are selecting in the current View. You can Select multiple FamilyInstances.",
                "Select all Types related to the FamilyInstances you are selecting which are displayed on the same Sheet as the current view. \nYou can Select multiple FamilyInstances.",
                "Select all Types related to the FamilyInstances you are selecting in the whole Project. You can Select multiple FamilyInstances.",
            }.ToArray();
            PulldownButtonData selectPullDown = new PulldownButtonData("SelectPullDown", "Select");
            CreateStackedRibbonButton(selectPullDown, titles, buttonNames, commands, "selectInstancesInView_32.bmp", SelectionPanel, images, tooltips);
            

            AutoSyncButton = CreateRibbonButton("Auto Sync", "StartSync", "BeyondRevit.Commands.SyncSubscribeIdleEvent", SyncPanel, "startSync_32.bmp");
            PauzeAutoSyncButton = CreateRibbonButton("Auto Sync", "PauzeSync", "BeyondRevit.Commands.SyncUnSubscribeIdleEvent", SyncPanel, "pauseSync_32.bmp");
            SyncSettingsButton = CreateRibbonButton("Sync Settings", "SyncSettings", "BeyondRevit.Commands.AutoSyncSettings", SyncPanel, "syncSettings_32.bmp");
            AutoSyncButton.Visible = false;
            SynchronisationConfig config = new SynchronisationConfig();
            SynchronisationConfig.Current = config;
            application.Idling += BeyondRevitSynchronizerUtils.AutomaticSync;

            PulldownButtonData joinCutPullDown = new PulldownButtonData("joinCutPullDown", "Join/Cut");
            CreateStackedRibbonButton(joinCutPullDown, new List<string>() { "Join Multiple Elements", "Cut Multiple Elements", "Allow Wall/Beam Joins", "Disallow Wall/Beam Joins" }.ToArray(),
                new List<string>() { "JoinMultipleElements", "CutMultipleElements", "AllowWallBeamJoins", "DisallowWallBeamJoins" }.ToArray(),
                new List<string>() { "BeyondRevit.Commands.JoinMultipleElements", "BeyondRevit.Commands.CutMultipleElements", "BeyondRevit.Commands.AllowJoins", "BeyondRevit.Commands.DisallowJoins" }.ToArray() ,
                "selectInstancesInView_32.bmp", 
                JoinCutUtilsPanel);

            PulldownButtonData dimensionPullDown = new PulldownButtonData("DimensionsPullDown", "Dimensions");
            CreateStackedRibbonButton(dimensionPullDown,
                new List<string>() { "Create Total Dimension", "Duplicate Dimension with Offset", "Move Small Dimensions", "Remove Dimension Reference", "Split Dimensions", "Merge Dimensions", "Align First Dimension Distance", "Auto Dimension" }.ToArray(),
                new List<string>() { "CreateTotalDimension","DuplicateDimension", "MoveSmallDimensions", "RemoveDimensionSegment", "SplitDimension","MergeDimensions", "AlignFirstDimension", "AutoDimension" }.ToArray(),
                new List<string>() { "BeyondRevit.Commands.CreateTotalDimension", "BeyondRevit.Commands.DuplicateDimension", "BeyondRevit.Commands.MoveDimensionEnds", "BeyondRevit.Commands.RemoveDimensionSegment",  "BeyondRevit.Commands.SplitDimensionLine", "BeyondRevit.Commands.MergeDimensions", "BeyondRevit.Commands.AlignFirstDimensionbyDistance", "BeyondRevit.Commands.AutoDimension" }.ToArray(),
                "dimensions_32.bmp",
                DimensionsPanel, 
                new List<string>() { "dimensionTotal_32.bmp", "dimensionDuplicate_32.bmp", "smalldimensions_32.bmp", "dimensionRemoveReference_32.bmp", "dimensionSplit_32.bmp", "dimensionMerge_32.bmp", "alignDimensions_32.bmp", "autoDimension_32.bmp" }.ToArray());

            PushButtonData AlignElevationTags = CreateQuickButton("Align ElevationTags", "AlignElevationTags", "BeyondRevit.Commands.AlignElevations");
            PushButtonData RemoveDimensionSegment = CreateQuickButton("Remove Dimension Segment", "RemoveDimensionSegment", "BeyondRevit.Commands.RemoveDimensionSegment");
            PushButtonData UnhideTemp = CreateQuickButton("Add Items to Temporary Hide", "UnhideTemp", "BeyondRevit.Commands.UnTempHide");
            QuickCommandPanel.AddStackedItems(AlignElevationTags, RemoveDimensionSegment, UnhideTemp);
            QuickCommandPanel.AddSeparator();


            PushButtonData HideCrop = CreateQuickButton("Hide Crop Regions from Viewports", "HideCrop", "BeyondRevit.Commands.HideCropRegion");
            PushButtonData ShowCrop = CreateQuickButton("Show Crop Regions from Viewports", "ShowCrop", "BeyondRevit.Commands.ShowCropRegion");
            PushButtonData OpenViewport = CreateQuickButton("Open Views from Viewports", "OpenViewport", "BeyondRevit.Commands.OpenViewportView");
            QuickCommandPanel.AddStackedItems(HideCrop, ShowCrop, OpenViewport);
            QuickCommandPanel.AddSeparator();

            PushButtonData FlipHand = CreateQuickButton("⇄ Flip FamilyInstance Horizontally", "FlipHand", "BeyondRevit.Commands.FlipHand");
            PushButtonData FlipFace = CreateQuickButton("⇅ Flip FamilyInstance Vertically", "FlipFace", "BeyondRevit.Commands.FlipFace");
            PushButtonData RotateInstance = CreateQuickButton("↻ Rotate FamilyInstance 180", "RotateFamilyInstance", "BeyondRevit.Commands.RotateInstance");
            QuickCommandPanel.AddStackedItems(FlipHand, FlipFace, RotateInstance);
            QuickCommandPanel.AddSeparator();

            PushButtonData MakeHalftone = CreateQuickButton("Make Elements Halftone in View", "HalfTone", "BeyondRevit.Commands.MakeHalftone");
            PushButtonData RemoveOverrides = CreateQuickButton("Remove Element Overrides in View", "RemoveOverrides", "BeyondRevit.Commands.RemoveOverrides");
            QuickCommandPanel.AddStackedItems(MakeHalftone, RemoveOverrides);

            CreateRibbonButton("PhaseBack", "Previous Phase", "BeyondRevit.Commands.GoToPreviousPhase", QuickCommandPanel, "previous_32.bmp");
            CreateRibbonButton("PhaseForward", "Next Phase", "BeyondRevit.Commands.GoToNextPhase", QuickCommandPanel, "next_32.bmp");

            HadesPulldownButton = new PulldownButtonData("HadesPullDown", "Hades");
            CreateStackedRibbonButton(HadesPulldownButton,
                new List<string>() { "Purge Import Lines styles", "Purge View Filters", "Purge View Template", "Purge Unplaced Views", "Purge Worksets" }.ToArray(),
                new List<string>() { "PurgeLinesStyles", "PurgeViewFilters", "PurgeViewTemplates", "PurgeViews", "PurgeWorksets"}.ToArray(),
                new List<string>() { "BeyondRevit.Hades.PurgeImportedLineStyles", "BeyondRevit.Hades.PurgeViewFilters", "BeyondRevit.Hades.PurgeViewTemplates", "BeyondRevit.Hades.PurgeViewsNotOnSheet", "BeyondRevit.Hades.PurgeWorksets" }.ToArray(),
                "hades_32.bmp",
                HadesPanel,
                new List<string>() { "hades_32.bmp", "hades_32.bmp", "hades_32.bmp", "hades_32.bmp", "hades_32.bmp" }.ToArray());

            return Result.Succeeded;
        }


        private void SetupRibbonPanels(UIControlledApplication application)
        {
            application.CreateRibbonTab("Beyond Revit");
            SheetPanel = application.CreateRibbonPanel("Beyond Revit", "Sheet");
            SelectionPanel = application.CreateRibbonPanel("Beyond Revit", "Selection");
            SyncPanel = application.CreateRibbonPanel("Beyond Revit", "Synchronization");
            JoinCutUtilsPanel = application.CreateRibbonPanel("Beyond Revit", "Join/Cut Commands");
            DimensionsPanel = application.CreateRibbonPanel("Beyond Revit", "Dimensions");
            QuickCommandPanel = application.CreateRibbonPanel("Beyond Revit", "Quick Commands");
            HadesPanel = application.CreateRibbonPanel("Beyond Revit", "Hades");
        }

        private PushButton CreateRibbonButton(string Title, string buttonName, string Command, RibbonPanel panel, string imageName = null, string toolTip = null)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string folderPath = Path.GetDirectoryName(assemblyPath);
            PushButtonData pushButtonData = new PushButtonData(
                buttonName,
                Title,
                assemblyPath,
                Command);

            PushButton pushButton = panel.AddItem(pushButtonData) as PushButton;
            if (imageName != null)
            {
                string pictureUri = string.Format(Path.Combine(folderPath, "Resources", imageName));
                BitmapImage bitmap = new BitmapImage(new Uri(pictureUri));
                pushButton.LargeImage = bitmap;
            }

            if (toolTip != null)
            {
                pushButton.ToolTip = toolTip;
            }
            return pushButton;
        }
        private PulldownButton CreateStackedRibbonButton(PulldownButtonData pullDownData, string[] Titles, string[] buttonNames, string[] Commands, string RibbonPanelImage, RibbonPanel panel, string[] imageNames = null, string[] toolTips = null)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string folderPath = Path.GetDirectoryName(assemblyPath);


            pullDownData.LargeImage =  new BitmapImage(new Uri(string.Format(Path.Combine(folderPath, "Resources", RibbonPanelImage))));
            PulldownButton pullDown = panel.AddItem(pullDownData) as PulldownButton;

            for(int i = 0; i < Titles.Count(); i++)
            {
                PushButtonData pushButton = new PushButtonData(buttonNames[i],Titles[i], assemblyPath, Commands[i]);
                if (imageNames != null)
                {
                    string imageName = imageNames[i];
                    string pictureUri = string.Format(Path.Combine(folderPath, "Resources", imageName));
                    BitmapImage bitmap = new BitmapImage(new Uri(pictureUri));
                    pushButton.LargeImage = bitmap;
                } 
                if (toolTips != null)
                {
                    string toolTip = toolTips[i];
                    pushButton.ToolTip = toolTip;
                }
                pullDown.AddPushButton(pushButton);
            }
            return pullDown;
        }

        private PushButtonData CreateQuickButton(string Title, string buttonName, string Command, string toolTip = null)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData pushButtonData = new PushButtonData(
                buttonName,
                Title,
                assemblyPath,
                Command);


            if (toolTip != null)
            {
                pushButtonData.ToolTip = toolTip;
            }
            return pushButtonData;
        }
    }
}
