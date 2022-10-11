# BeyondRevit

A whole toolkit with features which I was missing In Revit. 

**[IMPORTANT: Use 7Zip to open the ZIP File. It will Unblock all the DLL's and Addins for you.](https://www.7-zip.org/)**

## Synchronization

- **Auto Sync**
   Automatically Synchronizes your project every so many minutes

- **Sync Settings**
   Setup in which intervals you want to Save, Sync and Reload the Latest model from the cloud.

## Selection
Everything works with multiple FamilyInstances Selected. Just like the Select all Instances command in Revit 2022. It only took the Revit devs a little while to figure this out. This function also works for Revit 2019 and the rest.

- **Select all instances in the View**
   Select all other Family Instances in the current View

- **Select all instances on the Same Sheet**
   Select all other Family Instances which are also displayed in other Viewports on the same Sheet

- **Select all instances in the Project**
   Select all other Family Instances which are also displayed in the whole project.

- **Select all types in the View**
   Select all the Types in the Family from which you selected the Family Instance in the current view.
   E.g. you have a Beam family with HEA100, HEA120, HEA140. You select the HEA100 and the other will also be selected because they share the same Family

- **Select all instances on the Same Sheet**
   Select all the Types in the Family from which you selected the Family Instance which are also displayed in other Viewports on the same Sheet

- **Select all instances in the Project**
   Select all the Types in the Family from which you selected the Family Instance in the whole project.

- **Select all Associated Parts**
   Select an element and it will select all Associated Parts in that Element.

- **Select all Sibling parts**
   Select a Part and it will Select all the other parts from its Source Element.

## Join/Cut
- **Join Multiple Elements**
   Joins Multiple elements with Multiple Elements

- **Cut Multiple Elements**
   Cuts Multiple elements with Multiple Elements

- **Allow Wall/Beam Joints**
   Sets all Selected Walls and Beam to "Allow Join"

- **Disallow Wall/Beam Joints**
   Sets all Selected Walls and Beam to "Disallow Join"

## Move/Align
- **Center - Move Elements**
   Moves Elements in between 2 picked points.

- **Center - AlignElements**
   Aligns Elements in between 2 picked points

- **Redistribute Elements 1**
   Redivides Selected elements with an equal spacing between 2 points where the First and Last elements will be placed on these points.

- **Redistribute Elements 2**
   Redivides Selected elements with an equal spacing between 2 points where the First and Last elements will not be placed on these points, but on the next points

## Quick Commands
- **Command Line**
   Lets you Type search for all Builtin Action in Revit

- **Align Elevations**
   Aligns Selected Elevations on 1 line

- **Add to Isolation**
   Lets you easily add Temporary Hidden Elements to the Temporary Isolation. No more: Aaah I forgot to Isolate that other element as well, Time to show everything again and select all the Elements AGAIN...

- **Stack Tags**
   Stacks Tags on top of each other like a multi leader tag.

- **Flip Horizontal**
   If a FamilyInstance has the Flip Horizontal Control in it, you can toggle the control with this

- **Flip Vertical**
   If a FamilyInstance has the Flip Vertical Control in it, you can toggle the control with this

- **Flip 180**
   If a FamilyInstance has the Rotate Control in it, you can toggle the control with this

- **Make Halftone**
   Sets the Element overrides in View to Halftone

- **Remove Overrides**
   Removes the Element overrides in View for multiple selected elements

- **Link Nested Family Parameters**
   Works only in Families. Select a Nested Family and this will let you select the Parameters of that Family. It will then Create those parameter, if they are not there yet, and Associate them with the Nest Family's parameters. No more clicking!

- **Phase Back**
   Transports you to the Past 
![meme](https://i0.wp.com/waethnicstudies.com/wp-content/uploads/2021/06/Capture.png?fit=500%2C282&ssl=1)

- **Phase Forward**
   Transports you to the Future

## Workplanes
- **Sectionbox Top, Bottom, Front, Back, Left, Right**
   Creates a Workplane on said side of a Section box. Very handy for those nice looking 3D Details with dimensions.

## Dimensions
- **Total Dimension**
   Creates a Total Dimension line of a Selected Multi Segment Dimension and places it either above or below the original

- **Duplicate Dimension**
   Duplicates a Dimension and places it either above or below the original

- **Copy Dimension Overrides**
   Lets to Select an Dimension overrides and when you click near another dimension (segment), it will override that one with the same settings. 
   "This works really fast" - Random Colleague Dude

- **Move Small Dimensions**
   Lets you place small dimensions which are at the end of the Dimension to the Side of the Dimension

- **Remove Dimension Reference**
   Select a Dimension and pick a point near a Dimension reference and it will be removed. No more tapping trying to get rid of the annoying 0 in your dimension line.

- **Split Dimension at Reference**
   Splits a Dimension in 2 seperate Dimenions at a given Reference

- **Merge Dimensions**
   Merges 2 Dimensions in one dimension

- **Isolate Dimension Hosts**
   Isolates the Dimension and all the Hosts with which it is connected. Good for when you don't know what you just annotated.

## Sheet
- **Organize Views**
   Lets you Edit multiple Viewport properties on a sheet at ones. No more "This Detail number is already in use"... 

Almost there.... another 17 commands to go....

- **Open Viewports**
   Opens the View connected to the Select Viewports.

- **Move Viewports**
   Moves viewport from one sheet to another

- **Show Viewport Crop regions**
   Select multiple viewports and Show the Crop region for all of them.

- **Hide Viewport Crop regions**
   Select multiple viewports and Hide the Crop region for all of them.

- **Center Viewports Vertically**
   Aligns all the Viewports in vertical direction

- **Center Viewports Horizontally**
   Aligns all the Viewports in horizontal direction

- **Redistrubute Viewport Vertically**
   Redivide the Viewports evenly in vertical direction

- **Redistrubute Viewport Horizontally**
   Redivide the Viewports evenly in horizontal direction

## Hades
- **Purge Import Line Styles**
   Purges all the Imported Line Styles

- **Purge Unused View Filters**
   Purges all unused View Filters 

- **Purge Unused View Templates**
   Purges all unused View Templates

- **Purge Unplaced Views**
   Purges all Views which are not placed on a Sheet

- **Purge Current Sheet**
   Deletes all the Views which are placed on the Current sheet and deletes the Sheets

- **Purge Worksets**
   Purges unused Worksets

- **Purge Current Workset**
   Deletes all the Elements which are placed on the Current Workset and deletes the Workset itself

- **Purge Unused Family Parameters**
   Only Works in a Family Document
   Purges all the Unused Family Parameters when in a Family Document

- **Search and Destroy**
   This let's you delete almost everything in Revit. First Select a Category or a Type and then Select all the items in that Category or Type that you want to delete. 
   e.g. You want to Select All Materials with Concrete in their name. First Select Material and then search for Concrete. Select all Materials in the dialog and delete them. 

   e.g. When you want to clean up your company template and there are some things in there that you don't know how to delete.
   Just memorize the name, Select All Categories and Type in Search and Destroy and Search for that name. It will be there and you will be able to delete it.

   YES WE TRIED. We selected all the Categories and All the Type and Deleted everything. Revit Crashes... as always. Don't do THAT!




