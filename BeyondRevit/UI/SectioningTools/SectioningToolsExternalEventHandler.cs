using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BeyondRevit.UI.SectioningTools
{
    public class SectioningToolsExternalEventHandler : IExternalEventHandler
    {
        // The value of the latest request made by the modeless form 
        private SectioningRequest m_request = new SectioningRequest();

        /// <summary>
        /// A public property to access the current request value
        /// </summary>
        public SectioningRequest Request
        {
            get { return m_request; }
        }
        public string GetName()
        {
            return "SectioningToolsExternalEventHandler";
        }
        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;
            switch (Request.Take())
            {
                ////Create Sections
                //case SectioningRequest.RequestId.CreateSections:
                ////Code

                ////Modify Views
                //case SectioningRequest.RequestId.ModifyCropViews:
                ////Code

                ////Place on Sheet
                //case SectioningRequest.RequestId.PlaceViewsOnSheet:
                ////Code

                
            }
        }
        public class SectioningRequest
        {
            public enum RequestId : int
            {
                CreateSections = 0,
                ModifyCropViews = 1,
                PlaceViewsOnSheet = 2,
            }
            // Storing the value as a plain Int makes using the interlocking mechanism simpler
            private int m_request = (int)RequestId.CreateSections;

            /// <summary>
            ///   Take - The Idling handler calls this to obtain the latest request. 
            /// </summary>
            /// <remarks>
            ///   This is not a getter! It takes the request and replaces it
            ///   with 'None' to indicate that the request has been "passed on".
            /// </remarks>
            /// 
            public RequestId Take()
            {
                return (RequestId)Interlocked.Exchange(ref m_request, (int)RequestId.CreateSections);
            }

            /// <summary>
            ///   Make - The Dialog calls this when the user presses a command button there. 
            /// </summary>
            /// <remarks>
            ///   It replaces any older request previously made.
            /// </remarks>
            /// 
            public void Make(RequestId request)
            {
                Interlocked.Exchange(ref m_request, (int)request);
            }
        }
    }
}
