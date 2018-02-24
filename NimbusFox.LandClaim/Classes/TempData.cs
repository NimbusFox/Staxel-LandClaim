using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NimbusFox.FoxCore.Classes;
using Staxel.Logic;

namespace NimbusFox.LandClaim.Classes {
    public class TempData {
        public Dictionary<Entity, Positions> ClaimMarkers = new Dictionary<Entity, Positions>();

        public Dictionary<Entity, Positions> CloneMarkers() {
            return new Dictionary<Entity, Positions>(ClaimMarkers);
        }
    }
}
