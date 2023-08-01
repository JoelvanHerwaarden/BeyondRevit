using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BeyondRevit.UI;

namespace BeyondRevit.Facts
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowFact : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            SplashScreen splashScreen = new SplashScreen(Utils.RevitWindow(commandData), RandomFactGenerator.RandomFact);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            splashScreen.ShowSplash();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class NextFact : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            SplashScreen splashScreen = new SplashScreen(Utils.RevitWindow(commandData));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            splashScreen.ShowSplash();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class IWantToKnowMore : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            RandomFactGenerator.IWantToKnowMore();
            return Result.Succeeded;
        }

    }
    public  class RandomFactGenerator
    {

        public static string RandomFact { get; set; }
        public static string SearchText { get; set; }
        public static string DownloadRandomFact()
        {
            WebClient client = new WebClient();
            string webcontent = client.DownloadString("https://fungenerators.com/random/facts/");
            int index = webcontent.IndexOf("wow fadeInUp animated\"  data");
            string sub = webcontent.Substring(index);
            int startIndex = sub.IndexOf(">") + 1;
            int endIndex = sub.IndexOf("<");
            RandomFact = sub.Substring(startIndex, endIndex - startIndex);
            sub=sub.Substring(endIndex);

            startIndex = sub.IndexOf("(") + 1;
            endIndex = sub.IndexOf(")");
            SearchText = sub.Substring(startIndex, endIndex - startIndex);
            client.Dispose();

            return RandomFact;
        }

        public static void ShowFact()
        {
            Utils.ShowInfoBalloon(RandomFact);
        }
        public static void IWantToKnowMore()
        {
            string webRequest = string.Format("https://www.google.com/search?q={0}", SearchText);
            Process.Start(webRequest);
        }


    }
}
