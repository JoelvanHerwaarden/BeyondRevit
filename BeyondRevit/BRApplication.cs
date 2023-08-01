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
using adskWindows = Autodesk.Windows;

namespace BeyondRevit
{
    public class BRApplication : IExternalApplication
    {
        private static string BeyondRevitVersion = "2.0";
        private static RibbonPanel BeyondRevitPanel { get; set; }
        private static RibbonPanel SheetPanel { get; set; }
        private static RibbonPanel HadesPanel { get; set; }
        private static RibbonPanel FactsPanel { get; set; }
        private static RibbonPanel SelectionPanel { get; set; }
        private static RibbonPanel DimensionsPanel { get; set; }
        private static RibbonPanel SectionboxWorkplanes { get; set; }
        private static RibbonPanel SyncPanel { get; set; }
        private static RibbonPanel ModifyPanel { get; set; }
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

            #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            splashScreen.ShowSplash();
            #pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            
            BeyondRevitVariables.RevitVersion = double.Parse(application.ControlledApplication.VersionNumber);
            SetupRibbonPanels(application);

            #region BeyondRevitPanel
            PushButton gotoBeyondRevit = CreateRibbonButton("Beyond Revit", "GoToBeyondRevit", "BeyondRevit.GoToBeyondRevit", BeyondRevitPanel, null, "Activate the Beyond Revit Tab.\nYou can assign a shortkey to this command and on executing the short key, Revit will jump to Beyon Revit");
            #endregion
            #region SelectionPanel
            string[] titles = new List<string>() 
            { 
                "Select All Instances in View", 
                "Select All Instances on the same Sheet", 
                "Select All Instances in the Project", 
                "Select All Types in View", 
                "Select All Types on the same Sheet", 
                "Select All Types in the Project" ,
                "Select All Associated Parts",
                "Select All Sibling Parts"
            }.ToArray();
            string[] buttonNames = new List<string>() 
            { 
                "Instances In View", 
                "Instances on Sheet", 
                "Instances In Project", 
                "Types In View", 
                "Types on Sheet",
                "Types In Project",
                "Associated Parts",
                "Select Sibling Partd"
            }.ToArray();
            string[] commands = new List<string>() 
            {
                "BeyondRevit.Commands.SelectAllInstancesInCurrentView",
                "BeyondRevit.Commands.SelectAllInstancesInCurrentSheet",
                "BeyondRevit.Commands.SelectAllInstancesInProject",
                "BeyondRevit.Commands.SelectAllTypesInCurrentView",
                "BeyondRevit.Commands.SelectAllTypesInCurrentSheet",
                "BeyondRevit.Commands.SelectAllTypesInProject",
                "BeyondRevit.Commands.SelectAllAssociatedParts",
                "BeyondRevit.Commands.SelectAllSiblingParts",
            }.ToArray();
            string[] images = new List<string>() 
            {
                "SelectAllInstancesInView32.bmp",
                "SelectAllInstancesOnSheet32.bmp",
                "SelectAllInstancesInProject32.bmp",
                "SelectAllTypesInView32.bmp",
                "SelectAllTypesOnSheet32.bmp",
                "SelectAllTypesInProject32.bmp",
                "SelectAllPartsInProject32.bmp",
                "SelectAllPartsInProject32.bmp"
            }.ToArray();
            string[] tooltips = new List<string>()
            {
                "Select all Family Instances in the current View. You can Select multiple FamilyInstances.",
                "Select all Family Instances which are displayed on the same Sheet as the current view. \nYou can Select multiple FamilyInstances.",
                "Select all Family Instances in the whole Project. You can Select multiple FamilyInstances.",
                "Select all Types related to the FamilyInstances you are selecting in the current View. You can Select multiple FamilyInstances.",
                "Select all Types related to the FamilyInstances you are selecting which are displayed on the same Sheet as the current view. \nYou can Select multiple FamilyInstances.",
                "Select all Types related to the FamilyInstances you are selecting in the whole Project. You can Select multiple FamilyInstances.",
                "Select all Parts which roots go back to the selected elements.",
                "Select all Parts which share the same source elements"
            }.ToArray();
            PulldownButtonData selectPullDown = new PulldownButtonData("SelectPullDown", "Select");
            CreateStackedRibbonButton(selectPullDown, titles, buttonNames, commands, "SelectAllInstancesInView32.bmp", SelectionPanel, images, tooltips);
            #endregion
            #region SynchronizationPanel
            AutoSyncButton = CreateRibbonButton("Auto Sync", "StartSync", "BeyondRevit.Commands.SyncSubscribeIdleEvent", SyncPanel, "startSync_32.bmp");
            PauzeAutoSyncButton = CreateRibbonButton("Auto Sync", "PauzeSync", "BeyondRevit.Commands.SyncUnSubscribeIdleEvent", SyncPanel, "pauseSync_32.bmp");
            SyncSettingsButton = CreateRibbonButton("Sync Settings", "SyncSettings", "BeyondRevit.Commands.AutoSyncSettings", SyncPanel, "syncSettings_32.bmp");
            AutoSyncButton.Visible = false;
            SynchronisationConfig config = new SynchronisationConfig();
            SynchronisationConfig.Current = config;
            application.Idling += BeyondRevitSynchronizerUtils.AutomaticSync;
            #endregion
            #region ModifyPanel

            PulldownButtonData joinCutPullDown = new PulldownButtonData("joinCutPullDown", "Join/Cut");
            
            CreateStackedRibbonButton(joinCutPullDown,
                new List<string>() {
                    "Join Multiple Elements",
                    "Cut Multiple Elements",
                    "Allow Wall/Beam Joins",
                    "Disallow Wall/Beam Joins" }.ToArray(),
                new List<string>() { "JoinMultipleElements",
                    "CutMultipleElements",
                    "AllowWallBeamJoins",
                    "DisallowWallBeamJoins" }.ToArray(),
                new List<string>() {
                    "BeyondRevit.Commands.JoinMultipleElements",
                    "BeyondRevit.Commands.CutMultipleElements",
                    "BeyondRevit.Commands.AllowJoins",
                    "BeyondRevit.Commands.DisallowJoins" }.ToArray(),
                "JoinCut32.bmp",
                ModifyPanel);

            ModifyPanel.AddSeparator();

            PulldownButtonData MoveElementsDropdown = new PulldownButtonData("MoveElementsButton", "Move/Align");
            CreateStackedRibbonButton(MoveElementsDropdown,
                new List<string>() {
                    "Center - Move Elements",
                    "Center - Align Elements",
                    "Divide between points - Pick Last",
                    "Divide between points - Pick Second",
                    "Squeeze between points - Pick Last"}.ToArray(),
                new List<string>() { 
                    "CenterElements",
                    "CenterElementsPerpendicular",
                    "RedistributeElementsPickLast",
                    "RedistributeElementsPickSecond",
                    "RedistributeElementsWithoutFirstLastPickLast" }.ToArray(),
                new List<string>() {
                    "BeyondRevit.CenterElementCommands.CenterBetweenPoints",
                    "BeyondRevit.CenterElementCommands.CenterBetweenPointsPerpendicular",
                    "BeyondRevit.CenterElementCommands.RedistributeElementsPickLast",
                    "BeyondRevit.CenterElementCommands.RedistributeElementsPickSecond",
                    "BeyondRevit.CenterElementCommands.RedistributeElementsWithoutEnds"}.ToArray(),
                    "MoveAlign32.bmp",
                ModifyPanel,
                null,
                new List<string>() {
                    "Move the Selected elements in the center of 2 picked Points",
                    "Align the Selected elements in the center of 2 picked Points",
                    "Divide Elements according a lineair distribution between 2 points.\nPick the First Position an the Last Position",
                    "Divide Elements according a lineair distribution between 2 points.\nPick the First Position an the Second Position",
                    "Divide Elements according a lineair distribution squeezed between 2 points.\nPick the First Position an the Last Position"}.ToArray()
                );

            ModifyPanel.AddSeparator();
            PushButtonData AlignOffsetButton = CreateQuickButton("Align Offset", "AlignPlus", "BeyondRevit.CenterElementCommands.AlignWithOffset", "Align Elements with a certain offset", "AlignOffset16.bmp"); ;
            PushButtonData MoveOffsetButton = CreateQuickButton("Move Offset", "MovePlus", "BeyondRevit.CenterElementCommands.MoveWithOffset", "Move Elements to a location with a certain offset", "MoveOffset16.bmp");
            
            TextBoxData AlignPlusDistanceTextbox = new TextBoxData("AlignPlusDistanceTextbox");
            IList<RibbonItem> items = ModifyPanel.AddStackedItems(AlignOffsetButton, MoveOffsetButton, AlignPlusDistanceTextbox);

            #endregion

            #region QuickCommands

            CommandLineButton = CreateRibbonButton("Command Line", "CommandLine", "BeyondRevit.Commands.CommandLine", QuickCommandPanel, "commandLine_32.bmp");
            PushButton ManageTabsButton = CreateRibbonButton("Manage Tabs", "ManageTabs", "BeyondRevit.Commands.ManageAddinPanels", QuickCommandPanel, "ManageTabs_32.bmp");
            ModifyPanel.AddSeparator();

            PushButtonData AlignElevationTags = CreateQuickButton("Align Elevations", "AlignElevationTags", "BeyondRevit.Commands.AlignElevations", "Align Elevations to one side", "AlignElevations16.bmp");
            PushButtonData AddItemsToTemporaryIsolation = CreateQuickButton("Add to Isolation", "UnhideTemp", "BeyondRevit.Commands.AddItemsToTemporaryIsolation", "Add Elements to the current Isolation mode", "AddElementsTempIsolate16.bmp");
            PushButtonData StackTagHeads = CreateQuickButton("Stack Tags", "StackTagHeads", "BeyondRevit.Commands.StackTagHeads", "Places the Text of the Tags on top off each other", "StackTagHeads16.bmp");
            QuickCommandPanel.AddStackedItems(AlignElevationTags, AddItemsToTemporaryIsolation, StackTagHeads);
            QuickCommandPanel.AddSeparator();


            PushButtonData FlipHand = CreateQuickButton("Flip Horizontal", "FlipHand", "BeyondRevit.Commands.FlipHand", "Flips a Family Horizontal if the Family has the Horizontal Flipping Control added", "FlipHorizontal16.bmp");
            PushButtonData FlipFace = CreateQuickButton("Flip Vertical", "FlipFace", "BeyondRevit.Commands.FlipFace", "Flips a Family Vertical if the Family has the Vertical Flipping Control added", "FlipVertical16.bmp");
            PushButtonData RotateInstance = CreateQuickButton("Flip 180", "RotateFamilyInstance", "BeyondRevit.Commands.RotateInstance", "Rotates a Family 180 degrees if the Family has the 180 Rotating Control added", "Rotate18016.bmp");
            QuickCommandPanel.AddStackedItems(FlipHand, FlipFace, RotateInstance);
            QuickCommandPanel.AddSeparator();

            PushButtonData MakeHalftone = CreateQuickButton("Make Halftone", "HalfTone", "BeyondRevit.Commands.MakeHalftone", "Overrides the Selected Elements in the current view with an Halftone override", "MakeElementHalftone16.bmp");
            PushButtonData RemoveOverrides = CreateQuickButton("Remove Overrides", "RemoveOverrides", "BeyondRevit.Commands.RemoveOverrides", "Removes Element Overrides in the Current View", "RemoveElementOverrides16.bmp");
            PushButtonData LinkFamilyParameter = CreateQuickButton("Link Nested Family Parameters", "NestedFamilyParameters", "BeyondRevit.Commands.LinkFamilyParameters", "Automatically associate multiple nested Family parameters.\nIf a parameter with the same name already exists it will be associated.\nIf the parameter name does not yet exist it will create it and associate it.", "linkFamilyParameters16.bmp");
            QuickCommandPanel.AddStackedItems(MakeHalftone, RemoveOverrides, LinkFamilyParameter);


            CreateRibbonButton("Phase\nBack", "Previous", "BeyondRevit.Commands.GoToPreviousPhase", QuickCommandPanel, "previous_32.bmp");
            CreateRibbonButton("Phase\nForward", "Next", "BeyondRevit.Commands.GoToNextPhase", QuickCommandPanel, "next_32.bmp");

            #endregion
            #region SectionBoxWorkplanes
            PulldownButtonData workplanes = new PulldownButtonData("Workplanes", "Workplanes");
            CreateStackedRibbonButton(workplanes,
                new List<string>() {
                    "Sectionbox Top",
                    "Sectionbox Bottom",
                    "Sectionbox Front",
                    "Sectionbox Back",
                    "Sectionbox Left",
                    "Sectionbox Right"}.ToArray(),
                new List<string>() {
                    "SectionboxTop",
                    "SectionboxBottom",
                    "SectionboxFront",
                    "SectionboxBack",
                    "SectionboxLeft",
                    "SectionboxRight"}.ToArray(),
                new List<string>() {
                    "BeyondRevit.WorkplaneCommands.WorkplaneOnTopOfSectionBox",
                    "BeyondRevit.WorkplaneCommands.WorkplaneOnBottomOfSectionBox",
                    "BeyondRevit.WorkplaneCommands.WorkplaneOnFrontOfSectionBox",
                    "BeyondRevit.WorkplaneCommands.WorkplaneOnBackOfSectionBox",
                    "BeyondRevit.WorkplaneCommands.WorkplaneOnLeftOfSectionBox",
                    "BeyondRevit.WorkplaneCommands.WorkplaneOnRightOfSectionBox" }.ToArray(),
                "SectionboxFront32.bmp",
                SectionboxWorkplanes, new List<string>() {
                    "SectionboxTop32.bmp",
                    "SectionboxBottom32.bmp",
                    "SectionboxFront32.bmp",
                    "SectionboxBack32.bmp",
                    "SectionboxLeft32.bmp",
                    "SectionboxRight32.bmp",
                }.ToArray());
            #endregion
            #region Dimension
            PulldownButtonData dimensionPullDown = new PulldownButtonData("DimensionsPullDown", "Dimensions");
            CreateStackedRibbonButton(dimensionPullDown,
                new List<string>() {
                    "Create Total Dimension",
                    "Duplicate Dimension with Offset",
                    "Copy Dimension Overrides",
                    "Move Small Dimensions",
                    "Remove Dimension Reference",
                    "Split Dimensions",
                    "Merge Dimensions",
                    "Isolate Dimension Hosts",
                    "Align First Dimension Distance",
                    "Auto Dimension" }.ToArray(),
                new List<string>() {
                    "CreateTotalDimension",
                    "DuplicateDimension",
                    "CopyDimensionOverrides",
                    "MoveSmallDimensions",
                    "RemoveDimensionSegment",
                    "SplitDimension",
                    "MergeDimensions",
                    "IsolateDimensionHosts",
                    "AlignFirstDimension",
                    "AutoDimension" }.ToArray(),
                new List<string>() {
                    "BeyondRevit.Commands.CreateTotalDimension",
                    "BeyondRevit.Commands.DuplicateDimension",
                    "BeyondRevit.Commands.CopyDimensionOverrides",
                    "BeyondRevit.Commands.MoveDimensionEnds",
                    "BeyondRevit.Commands.RemoveDimensionSegment",
                    "BeyondRevit.Commands.SplitDimensionLine",
                    "BeyondRevit.Commands.MergeDimensions",
                    "BeyondRevit.Commands.IsolateDimensionHosts",
                    "BeyondRevit.Commands.AlignFirstDimensionbyDistance",
                    "BeyondRevit.Commands.AutoDimension"}.ToArray(),
                "dimensions_32.bmp",
                DimensionsPanel, new List<string>() {
                    "dimensionTotal_32.bmp",
                    "dimensionDuplicate_32.bmp",
                    "dimensionCopyOverrides_32.bmp",
                    "smalldimensions_32.bmp",
                    "dimensionRemoveReference_32.bmp",
                    "dimensionSplit_32.bmp",
                    "dimensionMerge_32.bmp",
                    "dimensions_32.bmp",
                    "alignDimensions_32.bmp",
                    "autoDimension_32.bmp" }.ToArray());
            #endregion
            #region SheetPanel
            OrganizeViewsButton = CreateRibbonButton("Organize Views", "OrganizeViews", "BeyondRevit.Commands.OrganizeViewsOnSheet", SheetPanel, "organizeViews_32.bmp");

            


            PulldownButtonData viewportPulldown = new PulldownButtonData("ViewportPulldown", "Viewports");
            CreateStackedRibbonButton(viewportPulldown,
                new List<string>() {
                    "Open Views from Viewports",
                    "Move/Copy Views to another Sheet",
                    "Show Viewport Crop Region",
                    "Hide Viewport Crop Region",
                    "Center Viewports Vertically",
                    "Center Viewports Horizontally",
                    "Distribute Viewports Vertically",
                    "Distribute Viewports Vertically"}.ToArray(),
                new List<string>() {
                    "OpenViewsFromViewport",
                    "MoveViewportsToSheet",
                    "ShowViewportCropRegion",
                    "HideViewportCropRegion",
                    "CenterViewportsVertically",
                    "CenterViewportsHorizontally",
                    "DistributeViewportsVertically",
                    "DistributeViewportsHorizontally" }.ToArray(),
                new List<string>() {
                    "BeyondRevit.ViewportCommands.OpenViewportView",
                    "BeyondRevit.ViewportCommands.MoveViewportsToAnotherSheet",
                    "BeyondRevit.ViewportCommands.ShowCropRegion",
                    "BeyondRevit.ViewportCommands.HideCropRegion",
                    "BeyondRevit.ViewportCommands.AlignVerticalTop",
                    "BeyondRevit.ViewportCommands.AlignHorizontalLeft",
                    "BeyondRevit.ViewportCommands.DistributeVertical",
                    "BeyondRevit.ViewportCommands.DistributeHorizontal" }.ToArray(),
                "ViewportHideCropRegion.bmp",
                SheetPanel,
                new List<string>() {
                    "ViewportOpenViews32.bmp",
                    "MoveViewports32.bmp",
                    "ViewportShowCropRegion32.bmp",
                    "ViewportHideCropRegion32.bmp",
                    "ViewportAlignVerticalCenter32.bmp",
                    "ViewportAlignHorizontalCenter32.bmp",
                    "ViewportDistributeVertical32.bmp",
                    "ViewportDistributeHorizontal32.bmp"}.ToArray());
            #endregion
            #region Hades
            HadesPulldownButton = new PulldownButtonData("HadesPullDown", "Hades");
            CreateStackedRibbonButton(HadesPulldownButton,
                new List<string>() { 
                    "Purge Import Lines styles", 
                    "Purge View Filters", 
                    "Purge View Templates", 
                    "Purge Scope Boxes",
                    "Purge Materials",
                    "Purge Unplaced Views",
                    "Purge Unplaced Legends and Schedules",
                    "Purge Unplaced Views by Selection",
                    "Purge Current Sheet",
                    "Purge Worksets" ,
                    "Purge Elements in Current Workset",
                    "Purge Unused Family Parameters",
                    "Search and Destroy"}.ToArray(),
                new List<string>() { 
                    "PurgeLinesStyles", 
                    "PurgeViewFilters",
                    "PurgeViewTemplates",
                    "PurgeScopeBoxes",
                    "PurgeMaterials",
                    "PurgeViews",
                    "PurgeLegendsSchedules",
                    "PurgeViewsSelection",
                    "PurgeSheet",
                    "PurgeWorksets",
                    "PurgeCurrentWorkset",
                    "PurgeUnusedFamilyParameters",
                    "SearchAndDestroy"
                }.ToArray(),
                new List<string>() { 
                    "BeyondRevit.Hades.PurgeImportedLineStyles", 
                    "BeyondRevit.Hades.PurgeViewFilters",
                    "BeyondRevit.Hades.PurgeViewTemplates",
                    "BeyondRevit.Hades.PurgeScopeboxes",
                    "BeyondRevit.Hades.PurgeViewsNotOnSheet",
                    "BeyondRevit.Hades.PurgeMaterials",
                    "BeyondRevit.Hades.PurgeLegendsAndSchedulesNotOnSheet",
                    "BeyondRevit.Hades.PurgeViewsNotOnSheetBySelection",
                    "BeyondRevit.Hades.PurgeCurrentSheet",
                    "BeyondRevit.Hades.PurgeWorksets",
                    "BeyondRevit.Hades.PurgeCurrentWorkset",
                    "BeyondRevit.Hades.PurgeFamilyParameters",
                    "BeyondRevit.Hades.SearchAndDestroy"
                }.ToArray(),
                "hades_32.bmp",
                HadesPanel,
                new List<string>() { 
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp",
                    "hades_32.bmp" 
                }.ToArray());
            #endregion
            #region Facts
            PushButtonData ShowFactButton = CreateQuickButton("Show Awesome Fact", "Fact", "BeyondRevit.Facts.ShowFact", "Shows you the Fact from the Beyond Revit Splash Window", "funfact_16.bmp");
            PushButtonData IWantToKnowMoreButton = CreateQuickButton("I Want to Know More", "IWTKM", "BeyondRevit.Facts.IWantToKnowMore", "Shows you Google Results about the Fact from the Beyond Revit Splash Window", "funfact_16.bmp");
            PushButtonData NextFactButton = CreateQuickButton("Show the Next Awesome Faxt", "NextFact", "BeyondRevit.Facts.NextFact", "Shows the Next Awesome Fact", "funfact_16.bmp");
            FactsPanel.AddStackedItems(ShowFactButton, IWantToKnowMoreButton, NextFactButton);
            #endregion
            PostProcessing();
            ManageAddinTabs();
            TrainingWheelsProtocol.SubscribeTrainingWheelsProtocol(application);
            Utils.ShowInfoBalloon("Training Wheels Protocol Activated");

            return Result.Succeeded;
        }

        private static void PostProcessing()
        {
            adskWindows.RibbonTextBox textBox = FindUIElement("Modify", "AlignPlusDistanceTextbox") as adskWindows.RibbonTextBox;
            if(textBox != null)
            {
                textBox.Width = 75;
                textBox.ShowImage = false;
                textBox.Prompt = "Align offset";
                textBox.SelectTextOnFocus = true;
            }
            PimpRibbon();

        }
        private void SetupRibbonPanels(UIControlledApplication application)
        {
            application.CreateRibbonTab("Beyond Revit");
            BeyondRevitPanel = application.CreateRibbonPanel("Beyond Revit", "Beyond Revit "+BeyondRevitVersion);
            SyncPanel = application.CreateRibbonPanel("Beyond Revit", "Synchronization");
            SelectionPanel = application.CreateRibbonPanel("Beyond Revit", "Selection");
            ModifyPanel = application.CreateRibbonPanel("Beyond Revit", "Modify");
            QuickCommandPanel = application.CreateRibbonPanel("Beyond Revit", "Quick Commands");
            SectionboxWorkplanes = application.CreateRibbonPanel("Beyond Revit", "Workplanes");
            DimensionsPanel = application.CreateRibbonPanel("Beyond Revit", "Dimensions");
            SheetPanel = application.CreateRibbonPanel("Beyond Revit", "Sheet");
            HadesPanel = application.CreateRibbonPanel("Beyond Revit", "Hades");
            FactsPanel = application.CreateRibbonPanel("Beyond Revit", "Facts");
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

        private PushButtonData CreateQuickButton(string Title, string buttonName, string Command, string toolTip = null, string imageName = null)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string folderPath = Path.GetDirectoryName(assemblyPath);
            PushButtonData pushButtonData = new PushButtonData(
                buttonName,
                Title,
                assemblyPath,
                Command);



            if (imageName != null)
            {
                string pictureUri = string.Format(Path.Combine(folderPath, "Resources", imageName));
                BitmapImage bitmap = new BitmapImage(new Uri(pictureUri));
                pushButtonData.LargeImage = bitmap;
                pushButtonData.Image = bitmap;
            }

            if (toolTip != null)
            {
                pushButtonData.ToolTip = toolTip;
            }
            return pushButtonData;
        }

        private static adskWindows.RibbonTab FindBeyondRevitTab()
        {
            adskWindows.RibbonTab result = null;
            adskWindows.RibbonTabCollection tabs = adskWindows.ComponentManager.Ribbon.Tabs;
            foreach (adskWindows.RibbonTab tab in tabs)
            {
                if (tab.Name == "Beyond Revit")
                {
                    result = tab;

                    break;
                }
            }
            return result;
        }
        private static adskWindows.RibbonItem FindUIElement(string panelName, string uiElementName)
        {
            string id = string.Format("CustomCtrl_%CustomCtrl_%Beyond Revit%{0}%{1}", panelName, uiElementName);
            
            adskWindows.RibbonItem result = null;
            adskWindows.RibbonTab beyondRevitRibbon = FindBeyondRevitTab();
            result = beyondRevitRibbon.FindItem(id, true);
            return result;
        }

        private static void TextBox_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            adskWindows.RibbonTextBox textBox = sender as adskWindows.RibbonTextBox;
            if(!double.TryParse(textBox.TextValue, out double distance))
            {
                Utils.Show("Input must be a Number");
            }
            else
            {

            }
        }

        private static void SetLargeImage(PushButtonData button,  string imageName)
        {
            if (imageName != null)
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string folderPath = Path.GetDirectoryName(assemblyPath);
                string pictureUri = string.Format(Path.Combine(folderPath, "Resources", imageName));
                BitmapImage bitmap = new BitmapImage(new Uri(pictureUri));
                button.LargeImage = bitmap;
            }
        }

        private static void PimpRibbon()
        {
            LinearGradientBrush gradientBrush
      = new LinearGradientBrush();

            gradientBrush.StartPoint
              = new System.Windows.Point(0, 0);

            gradientBrush.EndPoint
              = new System.Windows.Point(0, 1);

            gradientBrush.GradientStops.Add(
              new GradientStop(Colors.White, 0.0));
            System.Windows.Media.Color endcolor = new System.Windows.Media.Color();
            endcolor.R = 188;
            endcolor.G = 209;
            endcolor.B = 182;
            gradientBrush.GradientStops.Add(
              new GradientStop(endcolor, 1));

            adskWindows.RibbonControl ribbon = adskWindows.ComponentManager.Ribbon;
            adskWindows.RibbonTab tab = FindBeyondRevitTab();
            if(tab != null)
            {
                foreach (adskWindows.RibbonPanel panel in tab.Panels)
                {
                    panel.CustomPanelBackground = gradientBrush;
                    panel.CustomPanelTitleBarBackground = new SolidColorBrush(new System.Windows.Media.Color() { R = 214, G = 183, B = 171 });
                }
            }
            else
            {
                TaskDialog.Show("Beyond Revit", "Ribbon Tab Is Null");
            }
            
        }

        private static void ManageAddinTabs()
        {
            if (File.Exists(BeyondRevit.UI.ManageTabsWindow.SaveFileLocation))
            {
                string json = File.ReadAllText(BeyondRevit.UI.ManageTabsWindow.SaveFileLocation);
                Dictionary<string, bool> settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, bool>>(json);
                adskWindows.RibbonTabCollection tabs = adskWindows.ComponentManager.Ribbon.Tabs;
                List<adskWindows.RibbonTab> Tabs = new List<adskWindows.RibbonTab>();
                foreach (adskWindows.RibbonTab tab in tabs)
                {
                    if (tab.Name != null)
                    {
                        if (settings.Keys.Contains(tab.Name))
                        {
                            tab.IsVisible = settings[tab.Name];
                        }
                    }
                }
            }
        }
    }

    
}
