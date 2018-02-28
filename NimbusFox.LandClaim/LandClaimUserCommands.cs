using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NimbusFox.FoxCore.Classes;
using Plukit.Base;
using Staxel.Commands;
using Staxel.Server;

namespace NimbusFox.LandClaim {
    public class LandClaimUserCommands : ICommandBuilder {
        public string Execute(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };
            try {
                if (bits.Length > 1) {
                    switch (bits[1].ToLower()) {
                        case "help":
                            return Help(bits, blob, connection, api, out responseParams);
                        case "pos1":
                            return Pos1(bits, blob, connection, api, out responseParams);
                        case "pos2":
                            return Pos2(bits, blob, connection, api, out responseParams);
                        case "confirm":
                            return Confirm(bits, blob, connection, api, out responseParams);
                        case "clear":
                            return Clear(bits, blob, connection, api, out responseParams);
                        case "purge":
                            return Purge(bits, blob, connection, api, out responseParams);
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
            return "mods.nimbusfox.landclaim.command.claim.description";
        }

        public string Kind => "claim";
        public string Usage => "mods.nimbusfox.landclaim.description";
        public bool Public => true;

        private static string Help(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };
            if (bits.Any()) {
                switch (bits[0].ToLower()) {
                    case "pos1":
                        return "mods.nimbusfox.landclaim.command.pos1.description";
                    case "pos2":
                        return "mods.nimbusfox.landclaim.command.pos2.description";
                    case "confirm":
                        return "mods.nimbusfox.landclaim.command.confirm.description";
                    case "clear":
                        return "mods.nimbusfox.landclaim.command.clear.description";
                    case "purge":
                        return "mods.nimbusfox.landclaim.command.purge.description";
                }
            }

            return "mods.nimbusfox.landclaim.command.help.description";
        }

        private static string Pos1(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] {};

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            return LandManager._AddPos1(player, player.Physics.BottomPosition());
        }

        private static string Pos2(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            return LandManager._AddPos2(player, player.Physics.BottomPosition());
        }

        private static string Confirm(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] { };

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            return LandManager._Confirm(player);
        }

        private static string Clear(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] {};

            var player = LandManager.FoxCore.UserManager.GetPlayerEntityByUid(connection.Credentials.Uid);

            return LandManager._Clear(player);
        }

        private static string Purge(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] {};

            return LandManager._Purge(connection.Credentials.Uid);
        }
    }
}
