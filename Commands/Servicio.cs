using Rocket.API;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lafalafa.AutoBus.Commands
{
    class Servicio : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "bus";

        public string Help => "Add or remove you from bus service";

        public string Syntax => "/bus (toggle: service on/off)";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "bus.servicie" };

        public void Execute(IRocketPlayer caller, string[] command)
        {

            UnturnedPlayer player = (UnturnedPlayer)caller;

            RocketPermissionsGroup group = R.Permissions.GetGroup(AutoBusPlugin.pluginInstance.Configuration.Instance.BusGroupID);
            if (command.Length > 0)
            {

                ChatManager.serverSendMessage(AutoBusPlugin.pluginInstance.Translate("BAD_USE", Syntax).Replace('(', '<').Replace(')', '>'), Color.white, null, player.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                return;
            }

            if (AutoBusPlugin.isInGroup(group, player))
            {
                if (!AutoBusPlugin.Colectivero.Contains(player.CSteamID))
                {

                    AutoBusPlugin.Colectivero.Add(player.CSteamID);
                    ChatManager.serverSendMessage(AutoBusPlugin.pluginInstance.Translate("SERVICE_ON").Replace('(', '<').Replace(')', '>'), Color.white, null, player.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                    return;
                }
                else if (AutoBusPlugin.Colectivero.Contains(player.CSteamID))
                {

                    ChatManager.serverSendMessage(AutoBusPlugin.pluginInstance.Translate("SERVICE_OFF").Replace('(', '<').Replace(')', '>'), Color.white, null, player.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                    AutoBusPlugin.Colectivero.Remove(player.CSteamID);

                    return;
                }
            }
            else
            {
              
                ChatManager.serverSendMessage(AutoBusPlugin.pluginInstance.Translate("NOT_IN_JOB").Replace('(', '<').Replace(')', '>'), Color.white, null, player.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                return;
            }
         
        }    

    }
}
