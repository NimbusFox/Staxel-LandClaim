using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NimbusFox.FoxCore;
using NimbusFox.LandClaim.Classes;
using NimbusFox.LandClaim.Interfaces;
using Plukit.Base;
using Staxel.Logic;

namespace NimbusFox.LandClaim {
    public class LandManager : ILandClaim {
        internal static FxCore FoxCore { get; } = new FxCore("NimbusFox", "LandClaim", "v0.1");
        internal static TempData TempData { get; set; }
        internal static ClaimDataV1 ClaimData { get; set; }
        private const string ClaimFile = "LC.db";

        internal static void Init() {
            TempData = new TempData();

            if (!FoxCore.FileManager.FileExists(ClaimFile)) {
                ClaimData = new ClaimDataV1();
                Flush();
            } else {
                var data = FoxCore.FileManager.ReadFile<object>(ClaimFile);

                ClaimData = JsonConvert.DeserializeObject<ClaimDataV1>(JsonConvert.SerializeObject(data));
            }
        }

        private static void Flush() {
            FoxCore.FileManager.WriteFile(ClaimFile, ClaimData);
        }

        internal static void CheckPlayerMarkers(Entity entity) {
            if (entity != null) {
                if (!TempData.CloneMarkers().ContainsKey(entity)) {
                    TempData.ClaimMarkers.Add(entity, new Positions());
                }
            }
        }

        internal static void _AddPos1(Entity entity, Vector3D position) {
            CheckPlayerMarkers(entity);

            TempData.ClaimMarkers[entity].Start = position;
        }

        public void AddPos1(Entity entity, Vector3D position) {
            _AddPos1(entity, position);
        }

        internal static void _AddPos2(Entity entity, Vector3D position) {
            CheckPlayerMarkers(entity);

            TempData.ClaimMarkers[entity].End = position;
        }

        public void AddPos2(Entity entity, Vector3D position) {
            _AddPos2(entity, position);
        }

        internal static void _Confirm(Entity entity) {
            Flush();
        }
 
        public void Confirm(Entity entity) {
            _Confirm(entity);
        }

        internal static void _Clear(Entity entity) {
            if (TempData.CloneMarkers().Any(x => x.Key == entity)) {
                TempData.ClaimMarkers.Remove(entity);
            }
        }

        public void Clear(Entity entity) {
            _Clear(entity);
        }

        internal static void _Purge(Entity entity) {
            var target = ClaimData.CloneClaimedAreas().FirstOrDefault(x => x.OwnerUid == entity.PlayerEntityLogic.Uid());
            if (target != null) {
                ClaimData.ClaimedAreas.Remove(target);
                Flush();
            }
        }

        public void Purge(Entity entity) {
            _Purge(entity);
        }

        public void ToggleAdmin(Entity entity) { }

        internal static bool _IsInArea(Entity entity) {
            return ClaimData.ClaimedAreas.Any(x => x.Area.IsInside(entity.Physics.Position));
        }

        public bool IsInArea(Entity entity) {
            return _IsInArea(entity);
        }

        internal static ClaimAreaV1 GetArea(Entity entity) {
            return ClaimData.CloneClaimedAreas().FirstOrDefault(x => _IsInArea(entity));
        }
    }
}
