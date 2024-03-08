using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeyondRevit.Commands
{
    public class SynchronisationConfig
    {
        public static SynchronisationConfig Current;
        public int SaveInterval { get; set; }
        public int SyncInterval { get; set; }
        public int ReloadLatestInterval { get; set; }
        public DateTime NextSave { get; set; }
        public DateTime NextSync { get; set; }
        public DateTime NextReload { get; set; }
        public static string SaveFileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BeyondRevit\\SynchronizationConfig.json");
        public SynchronisationConfig()
        {
            if(File.Exists(SaveFileLocation))
            {
                try
                {
                    string json = File.ReadAllText(SaveFileLocation);
                    JToken config = JsonConvert.DeserializeObject<JToken>(json);
                    this.SaveInterval = config["SaveInterval"].Value<int>();
                    this.SyncInterval = config["SyncInterval"].Value<int>();
                    this.ReloadLatestInterval = config["ReloadLatestInterval"].Value<int>();
                    this.UpdateSettings();
                    Current = this;
                }
                catch(Exception e)
                {
                    SetupDefaultConfig();
                    BeyondRevit.Utils.Show("Could not Setup Synchronization Configuration!\n"+e.Message);
                }
            }
            else
            {
                SetupDefaultConfig();
                Current = this;
            }
        }

        private void SetupDefaultConfig()
        {
            this.SaveInterval = 30;
            this.SyncInterval = 90;
            this.ReloadLatestInterval = 120;
            this.NextReload = DateTime.Now.AddMinutes(this.ReloadLatestInterval);
            this.NextSave = DateTime.Now.AddMinutes(this.SaveInterval);
            this.NextSync = DateTime.Now.AddMinutes(this.SyncInterval);
        }
        public void UpdateSettings()
        {
            DateTime current = DateTime.Now;
            this.NextSave = current.AddMinutes(this.SaveInterval);
            this.NextReload = current.AddMinutes(this.ReloadLatestInterval);
            this.NextSync = current.AddMinutes(this.SyncInterval);
        }

        public void SaveConfigSettings()
        {
            string Folderpath = Path.GetDirectoryName(SaveFileLocation);
            if (!Directory.Exists(Folderpath))
            {
                Directory.CreateDirectory(Folderpath);
            }
            string jsonString = JsonConvert.SerializeObject(Current);
            File.WriteAllText(SaveFileLocation, jsonString);
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SyncSubscribeIdleEvent : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            SynchronisationConfig config = new SynchronisationConfig();
            commandData.Application.Idling += BeyondRevitSynchronizerUtils.AutomaticSync;
            BRApplication.AutoSyncButton.Visible = false;
            BRApplication.PauzeAutoSyncButton.Visible = true;
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SyncUnSubscribeIdleEvent : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            SynchronisationConfig config = SynchronisationConfig.Current;
            commandData.Application.Idling -= BeyondRevitSynchronizerUtils.AutomaticSync;
            BRApplication.AutoSyncButton.Visible = true;
            BRApplication.PauzeAutoSyncButton.Visible = false;
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SyncReportStatus : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            SynchronisationConfig config = SynchronisationConfig.Current;
            if (config != null)
            {
                Utils.Show(string.Format("Next Save at {0}\nNext Sync at {1}\nNext Reload at {2}\n", config.NextSave.ToString(), config.NextSync.ToString(), config.NextReload.ToString()));
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AutoSyncSettings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            BeyondRevit.UI.SyncPropertiesWindow settings = new BeyondRevit.UI.SyncPropertiesWindow();
            settings.ShowDialog();
            return Result.Succeeded;
        }
    }



    public class BeyondRevitSynchronizerUtils
    {
        public static void AutomaticSync(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            UIApplication app = (UIApplication)sender;
            if (NeedsSaving())
            {
                SaveAllDocuments(app);
            }
            if (NeedsSyncing())
            {
                SyncAllDocuments(app);
            }
            if (NeedsReloading())
            {
                ReloadAllDocuments(app);
            }
        }
        public static bool NeedsSaving()
        {
            bool result = false;
            DateTime current = DateTime.Now;
            if (current > SynchronisationConfig.Current.NextSave)
            {
                result = true;
                SynchronisationConfig.Current.NextSave = DateTime.Now.AddMinutes(SynchronisationConfig.Current.SaveInterval);
            }
            return result;
        }
        public static bool NeedsSyncing()
        {
            bool result = false;
            DateTime current = DateTime.Now;
            if (current > SynchronisationConfig.Current.NextSync)
            {
                result = true;
                SynchronisationConfig.Current.NextSync = DateTime.Now.AddMinutes(SynchronisationConfig.Current.SyncInterval);
            }
            return result;
        }
        public static bool NeedsReloading()
        {
            bool result = false;
            DateTime current = DateTime.Now;
            if (current > SynchronisationConfig.Current.NextReload)
            {
                result = true;
                SynchronisationConfig.Current.NextReload = DateTime.Now.AddMinutes(SynchronisationConfig.Current.ReloadLatestInterval);
            }
            return result;
        }
        public static void SaveAllDocuments(UIApplication app)
        {
            DocumentSet documents = app.Application.Documents;
            
            SaveOptions saveOptions = new SaveOptions();
            foreach (Document doc in documents)
            {
                if (doc.PathName != "" && !doc.IsLinked)
                {
                    doc.Save(saveOptions);
                }
            }
        }

        public static void SyncAllDocuments(UIApplication app)
        {
            DocumentSet documents = app.Application.Documents;
            TransactWithCentralOptions transOptions = new TransactWithCentralOptions();
            SynchronizeWithCentralOptions options = new SynchronizeWithCentralOptions()
            {
                SaveLocalAfter = true
            };
            foreach (Document doc in documents)
            {
                if (doc.IsWorkshared && !doc.IsLinked)
                {
                    doc.SynchronizeWithCentral(transOptions, options);
                }
            }
        }

        public static void ReloadAllDocuments(UIApplication app)
        {
            DocumentSet documents = app.Application.Documents;
            ReloadLatestOptions options = new ReloadLatestOptions();
            foreach (Document doc in documents)
            {
                if (doc.IsWorkshared && !doc.HasAllChangesFromCentral() && !doc.IsLinked)
                {
                    doc.ReloadLatest(options);
                }
            }
        }

    }
}