using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NimbusFox.FoxCore.Classes;

namespace NimbusFox.LandClaim.Classes {
    [Serializable]
    public class ClaimAreaV1 {
        public string OwnerUid { get; set; }
        public string OwnerName { get; set; }
        public VectorSquareI Area { get; set; }
        public List<string> Guests { get; set; }
        public bool IsAdminArea { get; set; }

        public ClaimAreaV1() {
            Guests = new List<string>();
        }
    }
}
