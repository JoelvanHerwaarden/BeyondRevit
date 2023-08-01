using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.Hades
{
    public interface IHadesCommand
    {
        void Command(ExternalCommandData commandData, bool prompt = true);
    }
}
