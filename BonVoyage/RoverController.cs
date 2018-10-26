using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    /// <summary>
    /// Rover controller. Child of BVController
    /// </summary>
    public class RoverController : BVController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="module"></param>
        public RoverController(Vessel v, ConfigNode module) : base (v, module)
        {
        }


        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        public override int GetControllerType()
        {
            return 0;
        }

    }

}
