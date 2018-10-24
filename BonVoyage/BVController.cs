using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    public class BVController
    {
        #region Public properties

        public Vessel vessel; // Vessel containing BonVoyageModule

        #endregion


        #region Private properties

        private ConfigNode BVModule; // Config node of BonVoyageModule

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="module"></param>
        public BVController(Vessel v, ConfigNode module)
        {
            vessel = v;
            BVModule = module;
        }

    }

}
