using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plukit.Base;
using Staxel.Items;
using Staxel.Logic;
using Staxel.Modding;
using Staxel.Tiles;

namespace NimbusFox.LandClaim {
    class LandClaimHook : IModHookV2 {
        private static long LastTick;
        public void Dispose() {
            LandManager.TempData.ClaimMarkers.Clear();
        }

        public void GameContextInitializeInit() {
            LandManager.Init();
        }
        public void GameContextInitializeBefore() { }
        public void GameContextInitializeAfter() { }
        public void GameContextDeinitialize() { }
        public void GameContextReloadBefore() { }
        public void GameContextReloadAfter() { }

        public void UniverseUpdateBefore(Universe universe, Timestep step) {
            LandManager.FoxCore.ParticleManager.DrawParticles();
            LandManager.FoxCore.EntityParticleManager.DrawParticles();
            LandManager.FoxCore.EntityFollowParticleManager.DrawParticles();

            if (LastTick <= DateTime.Now.Ticks) {
                foreach (var marker in LandManager.TempData.CloneMarkers()) {
                    if (!universe.TryGetEntity(marker.Key.Id, out _)) {
                        LandManager._Clear(marker.Key);
                    }
                }

                foreach (var expire in new Dictionary<Guid, DateTime>(LandManager.Expires)) {
                    if (expire.Value.Ticks <= DateTime.Now.Ticks) {
                        LandManager.FoxCore.ParticleManager.Remove(expire.Key);
                        LandManager.Expires.Remove(expire.Key);
                    }
                }

                LastTick = DateTime.Now.Ticks + 100;
            }
        }

        public void UniverseUpdateAfter() { }

        private static bool CanModify(Entity entity, Vector3I location) {
            if (entity.Logic != null) {
                if (entity.PlayerEntityLogic != null) {
                    if (entity.PlayerEntityLogic.IsAdmin()) {
                        return true;
                    }

                    foreach (var area in LandManager.ClaimData.CloneClaimedAreas()) {
                        if (area.Area.IsInside(location)) {
                            if (area.OwnerUid != entity.PlayerEntityLogic.Uid()) {
                                if (!new List<string>(area.Guests).Any(x => x == entity.PlayerEntityLogic.Uid())) {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool CanPlaceTile(Entity entity, Vector3I location, Tile tile, TileAccessFlags accessFlags) {
            return entity == null || CanModify(entity, location);
        }

        public bool CanReplaceTile(Entity entity, Vector3I location, Tile tile, TileAccessFlags accessFlags) {
            return entity == null || CanModify(entity, location);
        }

        public bool CanRemoveTile(Entity entity, Vector3I location, TileAccessFlags accessFlags) {
            return entity == null || CanModify(entity, location);
        }

        public void ClientContextInitializeInit() { }
        public void ClientContextInitializeBefore() { }
        public void ClientContextInitializeAfter() { }
        public void ClientContextDeinitialize() { }
        public void ClientContextReloadBefore() { }
        public void ClientContextReloadAfter() { }
        public void CleanupOldSession() { }
    }
}
