using Plukit.Base;
using Staxel.Logic;

namespace NimbusFox.LandClaim.Interfaces {
    public interface ILandClaim {
        void AddPos1(Entity entity, Vector3D position);
        void AddPos2(Entity entity, Vector3D position);
        void Confirm(Entity entity);
        void Clear(Entity entity);
        void Purge(Entity entity);
        void ToggleAdmin(Entity entity);
        bool IsInArea(Entity entity);
    }
}