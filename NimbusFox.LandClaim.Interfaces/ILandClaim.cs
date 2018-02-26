using Plukit.Base;
using Staxel.Logic;

namespace NimbusFox.LandClaim.Interfaces {
    public interface ILandClaim {
        void AddGuest(string ownerUid, string guestUid);
        void RemoveGuest(string ownerUid, string guestUid);
    }
}