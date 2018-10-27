using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    /// <summary>
    /// Control window - model part
    /// </summary>
    public class ControlWindowModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ControlWindowModel()
        {
            // Load from configuration
            CommonWindowProperties.ControlWindowPosition = Configuration.ControlWindowPosition;
        }

    }

}
