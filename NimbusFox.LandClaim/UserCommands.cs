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
    public class UserCommands : ICommandBuilder {
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
                    }
                }
            } catch (Exception ex) {
                //TeamManager.StoreException(ex, new { input = bits });
                responseParams = new object[3];
                responseParams[0] = "TeamFarming";
                responseParams[1] = "TeamFarming";
                responseParams[2] = "TeamFarming";
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

                }
            }

            return "mods.nimbusfox.landclaim.command.help.description";
        }

        private static string Pos1(string[] bits, Blob blob, ClientServerConnection connection, ICommandsApi api,
            out object[] responseParams) {
            responseParams = new object[] {};

            var player = LandManager.FoxCore.WorldManager.GetPlayerEntityByUid(connection.Credentials.Uid);



            return "";
        }
    }
}
