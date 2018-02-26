using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plukit.Base;

namespace NimbusFox.LandClaim.Classes {
    public class Positions {
        public Vector3D Start { get; set; }
        public Vector3D End { get; set; }
        public Guid RegionGuid { get; set; } = Guid.Empty;
    }
}
