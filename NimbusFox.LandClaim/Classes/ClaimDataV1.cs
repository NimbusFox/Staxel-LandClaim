using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NimbusFox.FoxCore.Classes;
using Staxel.Logic;

namespace NimbusFox.LandClaim.Classes {
    [Serializable]
    public class ClaimDataV1 {
        public List<ClaimAreaV1> ClaimedAreas { get; set; }

        public ClaimDataV1() {
            ClaimedAreas = new List<ClaimAreaV1>();
        }

        public List<ClaimAreaV1> CloneClaimedAreas() {
            return new List<ClaimAreaV1>(ClaimedAreas);
        }
    }
}
