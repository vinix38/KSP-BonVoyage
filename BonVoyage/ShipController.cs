using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    /// <summary>
    /// Ship controller. Child of BVController
    /// </summary>
    public class ShipController : BVController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v"></param>
        /// <param name="module"></param>
        public ShipController(Vessel v, ConfigNode module) : base(v, module)
        {
            throw new Exception("Ship controller not implemented");
        }


        /// <summary>
        /// Get controller type
        /// </summary>
        /// <returns></returns>
        public override int GetControllerType()
        {
            return 1;
        }

    }
}
