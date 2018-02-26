using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NimbusFox.LandClaim.Classes {
    public class LandClaimSettings {
        public int MaxTiles { get; set; }
        public bool ChargeForClaiming { get; set; }
        public int CostPerTile { get; set; }

        public LandClaimSettings() {
            MaxTiles = 100;
            ChargeForClaiming = false;
            CostPerTile = 10;
        }
    }
}
