using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using BeyondRevit.UI;
using BeyondRevit.ViewModels;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using Forms = System.Windows.Forms;
using adskWindows = Autodesk.Windows;
using Autodesk.Revit.UI.Selection;
using System.Collections.ObjectModel;
using BeyondRevit.Models;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Drawing.Printing;
using static System.Drawing.Printing.PrinterSettings;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.ApplicationServices;
namespace BeyondRevit
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CleanCollaborationCache : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            int filesDeleted = 0;
            long cleanedSpace = 0;
            string version = commandData.Application.Application.VersionNumber;
            string cacheUrl = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                    string.Format(@"Autodesk\Revit\Autodesk Revit {0}\CollaborationCache", version));

            string[] filesToDelete = Directory.GetFiles(cacheUrl, "*", SearchOption.AllDirectories);
            foreach(string file in filesToDelete)
            {
                try
                {
                    FileInfo info = new FileInfo(file);
                    long size = info.Length;
                    File.Delete(file);
                    filesDeleted += 1;
                    cleanedSpace += size / 1000000;
                }
                catch { }
            }


            string[] foldersToDelete = Directory.GetDirectories(cacheUrl); ;
            foreach (string folder in foldersToDelete)
            {
                try
                {
                    Directory.Delete(folder);
                }
                catch { }
            }
            Utils.Show(string.Format("Deleted {0} Files\nTotal size of the files {1} MB", filesDeleted, cleanedSpace));
            return Result.Succeeded;
        }
    }

    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CleanTempFolder : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            int filesDeleted = 0;
            long cleanedSpace = 0;
            string cacheUrl = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),"Temp");

            string[] filesToDelete = Directory.GetFiles(cacheUrl, "*", SearchOption.AllDirectories);
            foreach (string file in filesToDelete)
            {
                try
                {
                    FileInfo info = new FileInfo(file);
                    long size = info.Length;
                    File.Delete(file);
                    filesDeleted += 1;
                    cleanedSpace += size / 1000000;
                }
                catch { }
            }

            string[] foldersToDelete = Directory.GetDirectories(cacheUrl); ;
            foreach (string folder in foldersToDelete)
            {
                try
                {
                    Directory.Delete(folder);
                }
                catch { }
            }
            Utils.Show(string.Format("Deleted {0} Files\nTotal size of the files {1} MB", filesDeleted, cleanedSpace));
            return Result.Succeeded;
        }
    }
}
