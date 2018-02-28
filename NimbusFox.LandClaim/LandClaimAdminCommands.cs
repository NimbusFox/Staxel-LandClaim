using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NimbusFox.FoxCore;
using NimbusFox.LandClaim.Enums;
using Plukit.Base;
using Staxel.Commands;
using Staxel.Server;

namespace NimbusFox.LandClaim {
    public class LandClaimAdminCommands : ICommandBuilder {
        public string Execute(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };
            try {
                if (bits.Length > 1) {
                    switch (bits[1].ToLower()) {
                        case "help":
                            return Help(bits.Skip(2).ToArray(), blob, connection, api, out responseParams);
                        case "releaseclaim":
                            return ReleaseClaim(bits.Skip(2).ToArray(), blob, connection, api, out responseParams);
                        case "adminclaim":
                            return AdminClaim(bits.Skip(2).ToArray(), blob, connection, api, out responseParams);
                        case "showclaims":
                            return ShowClaims(bits.Skip(2).ToArray(), blob, connection, api, out responseParams);
                        case "toggleadmin":
                            return ToggleAdmin(bits.Skip(2).ToArray(), blob, connection, api, out responseParams);
                        case "details":
                            return Details(bits.Skip(2).ToArray(), blob, connection, api, out responseParams);
                        case "settings":
                            return Settings(bits.Skip(2).ToArray(), blob, connection, api, out responseParams);
                    }
                }
            } catch (Exception ex) {
                LandManager.FoxCore.ExceptionManager.HandleException(ex,
                    new Dictionary<string, object>
                        {{"input", bits}, {"LC.db", LandManager.ClaimData}, {"LC.config", LandManager.Settings}});
                responseParams = new object[3];
                responseParams[0] = "LandClaim";
                responseParams[1] = "LandClaim";
                responseParams[2] = "LandClaim";
                return "mods.nimbusfox.exception.message";
            }
            return "mods.nimbusfox.landclaim.admin.command.lcadmin.description";
        }

        public string Kind => "lcadmin";
        public string Usage => "mods.nimbusfox.landclaim.admin.description";
        public bool Public => false;

        private static string Help(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };
            if (bits.Any()) {
                switch (bits[0].ToLower()) {
                    case "releaseclaim":
                        return "mods.nimbusfox.landclaim.admin.command.releaseclaim.description";
                    case "adminclaim":
                        return "mods.nimbusfox.landclaim.admin.command.adminclaim.description";
                    case "showclaim":
                        return "mods.nimbusfox.landclaim.admin.command.showclaims.description";
                    case "toggleclaim":
                        return "mods.nimbusfox.landclaim.admin.command.toggleadmin.description";
                    case "details":
                        return "mods.nimbusfox.landclaim.admin.command.details.description";
                    case "settings":
                        return "mods.nimbusfox.landclaim.admin.command.settings.description";
                }
            }

            return "mods.nimbusfox.landclaim.admin.command.help.description";
        }

        private static string ReleaseClaim(string[] bits, Blob blob, ClientServerConnection connection,
            ICommandsApi api, out object[] responseParams) {
            responseParams = new object[] { };

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            if (LandManager._IsInArea(player)) {
                var area = LandManager.GetArea(player);

                var result = LandManager._Remove(area);

                if (result) {
                    return "mods.nimbusfox.landclaim.success.releaseclaim";
                }
            }

            return "mods.nimbusfox.landclaim.error.notinclaim";
        }

        private static string AdminClaim(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            return LandManager._Confirm(player, out _, true);
        }

        private static string ShowClaims(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            var range = 10;

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            if (bits.Any()) {
                int.TryParse(bits[0], out range);
            }

            responseParams = new object[] { LandManager._ShowRange(player.Physics.Position.From3Dto3I(), range) };

            return "mods.nimbusfox.landclaim.success.showclaims";
        }

        private static string ToggleAdmin(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            var result = LandManager._ToggleAdmin(player);

            switch (result) {
                case AdminState.Fail:
                    return "mods.nimbusfox.landclaim.error.notinclaim";
                case AdminState.User:
                    return "mods.nimbusfox.landclaim.success.toggleadmin.user";
            }

            return "mods.nimbusfox.landclaim.success.toggleadmin.admin";
        }

        private static string Details(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            if (LandManager._IsInArea(player)) {
                var area = LandManager.GetArea(player);

                api.MessagePlayer(player.PlayerEntityLogic.Uid(), "mods.nimbusfox.landclaim.message.details.1",
                    new object[] {
                        area.OwnerName
                    });

                api.MessagePlayer(player.PlayerEntityLogic.Uid(), "mods.nimbusfox.landclaim.message.details.2",
                    new object[] {
                        area.Area.GetTileCount()
                    });

                api.MessagePlayer(player.PlayerEntityLogic.Uid(),
                    area.IsAdminArea ? "mods.nimbusfox.landclaim.message.details.3.adminzone"
                        : "mods.nimbusfox.landclaim.message.details.3.userzone", new object[] { });

                return "";
            }

            return "mods.nimbusfox.landclaim.error.notinclaim";
        }

        private static string Settings(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };

            var items = new Dictionary<int, string>();

            var needflush = false;

            if (bits.Any()) {
                if (bits.Length >= 2) {
                    int parseInt;
                    bool parseBool;
                    switch (bits[0].ToLower()) {
                        case "maxtiles":
                            if (int.TryParse(bits[1], out parseInt)) {
                                LandManager.Settings.MaxTiles = parseInt;
                                items.Add(3, "mods.nimbusfox.landclaim.message.settings.3.update");
                                needflush = true;
                                goto notinvalid;
                            }

                            break;
                        case "costpertile":
                            if (int.TryParse(bits[1], out parseInt)) {
                                LandManager.Settings.CostPerTile = parseInt;
                                items.Add(2, "mods.nimbusfox.landclaim.message.settings.2.update");
                                needflush = true;
                                goto notinvalid;
                            }

                            break;
                        case "chargeforclaiming":
                            if (bool.TryParse(bits[1], out parseBool)) {
                                LandManager.Settings.ChargeForClaiming = parseBool;
                                items.Add(1, "mods.nimbusfox.landclaim.message.settings.1.update");
                                needflush = true;
                                goto notinvalid;
                            }

                            break;
                    }

                    return "mods.nimbusfox.landclaim.error.invalidsetting";
                }
                return "mods.nimbusfox.landclaim.error.invalidsetting";
            }
            notinvalid:
            if (!items.ContainsKey(1)) {
                items.Add(1, "mods.nimbusfox.landclaim.message.settings.1");
            }

            if (!items.ContainsKey(2)) {
                items.Add(2, "mods.nimbusfox.landclaim.message.settings.2");
            }

            if (!items.ContainsKey(3)) {
                items.Add(3, "mods.nimbusfox.landclaim.message.settings.3");
            }

            if (needflush) {
                LandManager.Flush();
            }

            api.MessagePlayer(connection.Credentials.Uid, items[1],
                new object[] {LandManager.Settings.ChargeForClaiming.ToString()});

            api.MessagePlayer(connection.Credentials.Uid, items[2], new object[] {LandManager.Settings.MaxTiles});

            api.MessagePlayer(connection.Credentials.Uid, items[3], new object[] {LandManager.Settings.CostPerTile});

            return "";
        }
    }
}
