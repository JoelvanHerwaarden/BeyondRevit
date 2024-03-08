using Autodesk.Revit.UI;
using BeyondRevit.UI.SectioningTools;
using Synchro;
using System.Windows;


namespace BeyondRevit.UI
{
    public partial class SectioningToolsWindow : Window
    {
        public SectioningToolsWindow(SectioningToolsViewModel viewmodel)
        {
            this.InitializeComponent();
            this.DataContext = viewmodel;
            CreateSectionsAlongCurveControl createSectionsAlongCurveControl = (CreateSectionsAlongCurveControl)this.FindName("SectionAlongCurve");
            createSectionsAlongCurveControl.DataContext = viewmodel;
        }


    }
}
