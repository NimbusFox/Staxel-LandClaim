using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NimbusFox.FoxCore;
using NimbusFox.FoxCore.Classes;
using NimbusFox.LandClaim.Classes;
using NimbusFox.LandClaim.Enums;
using NimbusFox.LandClaim.Interfaces;
using Plukit.Base;
using Staxel;
using Staxel.Logic;

namespace NimbusFox.LandClaim {
    public class LandManager : ILandClaim {
        internal static FxCore FoxCore { get; private set; }
        internal static TempData TempData { get; set; }
        internal static ClaimDataV1 ClaimData { get; set; }
        internal static LandClaimSettings Settings { get; set; }
        private const string ClaimFile = "LC.db";
        private const string SettingsFile = "LC.config";
        internal static readonly Dictionary<Guid, DateTime> Expires = new Dictionary<Guid, DateTime>();

        internal static void Init() {
            FoxCore = new FxCore("NimbusFox", "LandClaim", "v0.1");
            TempData = new TempData();
            var needFlush = false;

            if (!FoxCore.FileManager.FileExists(ClaimFile)) {
                ClaimData = new ClaimDataV1();
                needFlush = true;
            } else {
                ClaimData = FoxCore.FileManager.ReadFile<ClaimDataV1>(ClaimFile);
            }

            if (!FoxCore.FileManager.FileExists(SettingsFile)) {
                Settings = new LandClaimSettings();
                needFlush = true;
            } else {
                Settings = FoxCore.FileManager.ReadFile<LandClaimSettings>(SettingsFile, true);
            }

            if (needFlush) {
                Flush();
            }
        }

        internal static void Flush() {
            FoxCore.FileManager.WriteFile(ClaimFile, ClaimData);
            FoxCore.FileManager.WriteFile(SettingsFile, Settings, true);
        }

        internal static void CheckPlayerMarkers(Entity entity) {
            if (entity != null) {
                if (!TempData.CloneMarkers().ContainsKey(entity)) {
                    TempData.ClaimMarkers.Add(entity, new Positions());
                }
            }
        }

        internal static string _AddPos1(Entity entity, Vector3D position) {
            CheckPlayerMarkers(entity);

            TempData.ClaimMarkers[entity].Start = position;

            if (!IsInsideClaim(TempData.ClaimMarkers[entity])) {
                if (_CreateClaimRegion(entity) && Settings.ChargeForClaiming) {
                    return "mods.nimbusfox.landclaim.success.pos1.price";
                }
                return "mods.nimbusfox.landclaim.success.pos1";
            }

            TempData.ClaimMarkers[entity].Start = default(Vector3D);
            return "mods.nimbusfox.landclaim.error.landtaken";
        }

        internal static string _AddPos2(Entity entity, Vector3D position) {
            CheckPlayerMarkers(entity);

            TempData.ClaimMarkers[entity].End = position;

            if (!IsInsideClaim(TempData.ClaimMarkers[entity])) {
                if (_CreateClaimRegion(entity) && Settings.ChargeForClaiming) {
                    return "mods.nimbusfox.landclaim.success.pos2.price";
                }
                return "mods.nimbusfox.landclaim.success.pos2";
            }

            TempData.ClaimMarkers[entity].End = default(Vector3D);
            return "mods.nimbusfox.landclaim.error.landtaken";
        }

        private static bool IsInsideClaim(Positions positions) {
            var start = positions.Start == default(Vector3D) ? positions.End : positions.Start;
            var end = positions.End == default(Vector3D) ? positions.Start : positions.End;

            var range = new VectorCubeI(start.From3Dto3I(), end.From3Dto3I());

            for (var x = range.X.Start; x <= range.X.End; x++) {
                for (var z = range.Z.Start; z <= range.Z.End; z++) {
                    foreach (var claimRange in ClaimData.CloneClaimedAreas()) {
                        if (claimRange.Area.IsInside(new Vector3I(x, 0, z))) {
                            Expires.Add(
                                FoxCore.ParticleManager.Add(
                                    new Vector3I(claimRange.Area.X.Start, range.Y.Start, claimRange.Area.Z.Start),
                                    new Vector3I(claimRange.Area.X.End, range.Y.Start + 10, claimRange.Area.Z.End), "mods.nimbusfox.landclaim.particles.taken"), DateTime.Now.AddSeconds(5));
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static bool _CreateClaimRegion(Entity entity) {
            var data = TempData.ClaimMarkers[entity];

            if (data.Start != default(Vector3D) && data.End != default(Vector3D)) {
                if (data.RegionGuid != Guid.Empty) {
                    FoxCore.ParticleManager.Remove(data.RegionGuid);
                }
                data.RegionGuid = FoxCore.ParticleManager.Add(data.Start, data.End, "mods.nimbusfox.landclaim.particles.region");
                return true;
            }

            return false;
        }

        internal static string _Confirm(Entity entity, out object[] responseParams, bool admin = false) {
            responseParams = new object[] {};
            CheckPlayerMarkers(entity);

            var target = TempData.ClaimMarkers[entity];

            if (target.Start == default(Vector3D) || target.End == default(Vector3D)) {
                return "mods.nimbusfox.landclaim.error.nopos";
            }

            var petalCostSquare = new VectorSquareI(target.Start.From3Dto3I(), target.End.From3Dto3I());

            if (petalCostSquare.GetTileCount() * Settings.CostPerTile >
                entity.PlayerEntityLogic.Inventory().GetMoney() && Settings.ChargeForClaiming) {
                return "mods.nimbusfox.landlcaim.error.notenoughpetals";
            }

            if (petalCostSquare.GetTileCount() > Settings.MaxTiles && !admin) {
                responseParams = new object[] {Settings.MaxTiles};
                return "mods.nimbusfox.landclaim.error.toomanytiles";
            }

            if (IsInsideClaim(target)) {
                return "mods.nimbusfox.landclaim.error.landtaken";
            }

            if (_HasClaim(entity.PlayerEntityLogic.Uid()) && !admin) {
                return "mods.nimbusfox.landclaim.error.hasclaim";
            }

            var newArea = new ClaimAreaV1 {
                OwnerUid = entity.PlayerEntityLogic.Uid(),
                OwnerName = entity.PlayerEntityLogic.DisplayName(),
                Area = new VectorSquareI(target.Start.From3Dto3I(), target.End.From3Dto3I()),
                IsAdminArea = admin
            };

            ClaimData.ClaimedAreas.Add(newArea);

            FoxCore.ParticleManager.Remove(target.RegionGuid);

            Expires.Add(FoxCore.ParticleManager.Add(target.Start, new Vector3D(target.End.X, target.End.Y + 10, target.End.Z), "mods.nimbusfox.landclaim.particles.claimed"), DateTime.Now.AddSeconds(3));

            Flush();

            return "mods.nimbusfox.landclaim.success.claim";
        }

        internal static string _Clear(Entity entity) {
            if (TempData.CloneMarkers().Any(x => x.Key == entity)) {
                var target = TempData.CloneMarkers().First(x => x.Key == entity);
                var positions = target.Value;
                if (positions.RegionGuid != Guid.Empty) {
                    FoxCore.ParticleManager.Remove(positions.RegionGuid);
                }
                TempData.ClaimMarkers.Remove(entity);
            }

            return "mods.nimbusfox.landclaim.success.clear";
        }

        internal static bool _HasClaim(string uid) {
            return ClaimData.CloneClaimedAreas().Any(x => x.OwnerUid == uid && !x.IsAdminArea);
        }

        internal static string _Purge(string ownerUid) {
            var target = ClaimData.CloneClaimedAreas().FirstOrDefault(x => x.OwnerUid == ownerUid && !x.IsAdminArea);
            if (target != null) {
                ClaimData.ClaimedAreas.Remove(target);
                Flush();
            }

            return "mods.nimbusfox.landclaim.success.purge";
        }

        internal static bool _IsInArea(Entity entity) {
            return ClaimData.ClaimedAreas.Any(x => x.Area.IsInside(entity.Physics.BottomPosition()));
        }

        internal static ClaimAreaV1 GetArea(Entity entity) {
            return ClaimData.CloneClaimedAreas().FirstOrDefault(x => x.Area.IsInside(entity.Physics.BottomPosition()));
        }

        internal static void _AddGuest(string ownerUid, string guestUid) {
            var area = ClaimData.CloneClaimedAreas().FirstOrDefault(x => x.OwnerUid == ownerUid && !x.IsAdminArea);
            if (area != null) {
                if (!area.Guests.Contains(guestUid)) {
                    area.Guests.Add(guestUid);
                    Flush();
                }
            }
        }

        internal static void _RemoveGuest(string ownerUid, string guestUid) {
            var area = ClaimData.CloneClaimedAreas().FirstOrDefault(x => x.OwnerUid == ownerUid && !x.IsAdminArea);
            if (area != null) {
                if (area.Guests.Contains(guestUid)) {
                    area.Guests.Remove(guestUid);
                    Flush();
                }
            }
        }

        public void AddGuest(string ownerUid, string guestUid) {
            _AddGuest(ownerUid, guestUid);
        }

        public void RemoveGuest(string ownerUid, string guestUid) {
            _RemoveGuest(ownerUid, guestUid);
        }

        internal static bool _Remove(ClaimAreaV1 area) {
            if (ClaimData.CloneClaimedAreas().Contains(area)) {
                ClaimData.ClaimedAreas.Remove(area);
                Flush();
                return true;
            }
            return false;
        }

        internal static int _ShowRange(Vector3I start, int distance) {
            var regions = new List<ClaimAreaV1>();

            var collection = ClaimData.CloneClaimedAreas();

            for (var x = start.X - distance; x <= start.X + distance; x++) {
                for (var z = start.Z - distance; z <= start.Z + distance; z++) {
                    var current = collection.Where(v => v.Area.IsInside(new Vector3I(x, start.Y, z)));
                    foreach (var region in current) {
                        if (!regions.Contains(region)) {
                            regions.Add(region);
                        }
                    }
                }
            }

            foreach (var region in regions) {
                Expires.Add(FoxCore.ParticleManager.Add(new Vector3D(region.Area.X.Start, start.Y, region.Area.Z.Start), new Vector3D(region.Area.X.End, start.Y + 10, region.Area.Z.End), "mods.nimbusfox.landclaim.particles.claimed"), DateTime.Now.AddSeconds(5));
            }

            return regions.Count;
        }

        internal static AdminState _ToggleAdmin(Entity entity) {
            if (!_IsInArea(entity)) {
                return AdminState.Fail;
            }

            var area = GetArea(entity);

            if (area.IsAdminArea) {
                area.IsAdminArea = false;
                Flush();
                return AdminState.User;
            }

            area.IsAdminArea = true;
            Flush();
            return AdminState.Admin;
        }
    }
}
