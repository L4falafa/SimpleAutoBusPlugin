using Rocket.API.Collections;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using fr34kyn01535.Uconomy;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.API;

namespace Lafalafa.AutoBus
{
    public class AutoBusPlugin : RocketPlugin<AutoBusConfiguration>
    {



        #region load
        protected override void Load()
        {
            pluginInstance = this;
            Colectivero = new List<CSteamID>();
            Earnings = new Dictionary<CSteamID, int>();
            Balance = new Dictionary<CSteamID, decimal>();
            AutoBusNameMessage = "(color=blue)[(/color)(color=grey)BusJob(/color)(color=blue)](/color):";


            Logger.Log($"{Assembly.GetName().Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
            Logger.Log($"Plugin By: Lafalafa", ConsoleColor.Yellow);
            VehicleManager.onEnterVehicleRequested += onEnterVehicleRequeseted;
            VehicleManager.onExitVehicleRequested += onExitVechicleRequeseted;
            VehicleManager.onSwapSeatRequested += onSwapSeat;


        }
        #endregion



        private void onSwapSeat(Player player, InteractableVehicle vehicle, ref bool shouldAllow, byte fromSeatIndex, ref byte toSeatIndex)
        {


            if (vehicle.asset.id != Configuration.Instance.BusID) { return; }

            shouldAllow = false;
        }

        private void onExitVechicleRequeseted(Player player, InteractableVehicle vehicle, ref bool shouldAllow, ref Vector3 pendingLocation, ref float pendingYaw)
        {

            if (vehicle.asset.id == Configuration.Instance.BusID)
            {
                RocketPermissionsGroup group = R.Permissions.GetGroup(AutoBusPlugin.pluginInstance.Configuration.Instance.BusGroupID);
                UnturnedPlayer uPlayer = UnturnedPlayer.FromPlayer(player);

                if (isInGroup(group, uPlayer) && (GetDriver(vehicle, uPlayer) == uPlayer))
                {
                    int asiento = vehicle.passengers.Length;
                    while (asiento >= 0)
                    {
                        SteamPlayer steamPlayer = vehicle.passengers[asiento].player;
                        if (steamPlayer != null)
                        {
                            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
                            ChatManager.serverSendMessage(String.Format(pluginInstance.Translate("OUT_DRIVER", unturnedPlayer.CharacterName).Replace('(', '<').Replace(')', '>')), Color.white, null, steamPlayer, EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);

                        }

                    }

                    vehicle.forceRemoveAllPlayers();
                    return;
                }



            }
            shouldAllow = true;
        }



        private void onEnterVehicleRequeseted(Player pl, InteractableVehicle vehicle, ref bool shouldAllow)
        {


            if (vehicle.asset.id == Configuration.Instance.BusID)
            {
                UnturnedPlayer player = UnturnedPlayer.FromPlayer(pl);


                if ((GetDriver(vehicle, player) == player) && (!Colectivero.Contains(player.CSteamID)))
                {

                    ChatManager.serverSendMessage(String.Format(pluginInstance.Translate("ARENT_DRIVER").Replace('(', '<').Replace(')', '>')), Color.white, null, player.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                    shouldAllow = false;
                    return;

                }
                else if (Colectivero.Contains(player.CSteamID))
                {
                    Earnings.Add(player.CSteamID, 0);
                    return;
                }


                UnturnedPlayer driver = GetDriver(vehicle, player);

                var cob = (UInt32)pluginInstance.Configuration.Instance.Payment;


                if (pluginInstance.Configuration.Instance.UseXP)
                {
                    if (player.Experience > cob)
                    {
                        player.Experience = player.Experience - (cob * 2);
                        driver.Experience = driver.Experience + cob;
                    }
                    else {
                        ChatManager.serverSendMessage(String.Format(pluginInstance.Translate("NOT_ENOUGH_MONEY", cob).Replace('(', '<').Replace(')', '>')), Color.white, null, player.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                        shouldAllow = false;
                    }


                }
                else
                {

                    try
                    {

                        ExecuteDependencyCode("Uconomy", (IRocketPlugin plugin) =>
                        {


                            Uconomy Uconomy = (Uconomy)plugin;

                            if ((uint)Uconomy.Database.GetBalance(player.CSteamID.m_SteamID.ToString()) < cob)
                            {
                                AutoBusPlugin.Balance.Add(player.CSteamID, Uconomy.Database.GetBalance(player.CSteamID.m_SteamID.ToString()));


                            }
                            else
                            {
                                Uconomy.Database.IncreaseBalance(player.CSteamID.m_SteamID.ToString(), (cob * -1));
                                Uconomy.Database.IncreaseBalance(driver.CSteamID.m_SteamID.ToString(), (cob));

                            }

                        });
                        if (AutoBusPlugin.Balance[player.CSteamID] < cob)
                        {
                            ChatManager.serverSendMessage(String.Format(pluginInstance.Translate("NOT_ENOUGH_MONEY", cob).Replace('(', '<').Replace(')', '>')), Color.white, null, player.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                            AutoBusPlugin.Balance.Remove(player.CSteamID);
                            shouldAllow = false;
                            return;
                        }
                    }
                    catch (Exception e)
                    {

                        Logger.Log($"error at try execute a trasaction{e.Message}");

                    }
                    Earnings[player.CSteamID] += Configuration.Instance.Payment;
                    EffectManager.sendUIEffect(5463, 5464, true, pluginInstance.Translate("UI_WON"), Earnings[player.CSteamID].ToString(), Configuration.Instance.Payment.ToString());
                    ChatManager.serverSendMessage(String.Format(pluginInstance.Translate("PAY_TO_DRIVER", driver.CharacterName, cob).Replace('(', '<').Replace(')', '>')), Color.white, null, player.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                    ChatManager.serverSendMessage(String.Format(pluginInstance.Translate("WON_DRIVER", cob).Replace('(', '<').Replace(')', '>')), Color.white, null, driver.SteamPlayer(), EChatMode.WELCOME, AutoBusPlugin.pluginInstance.Configuration.Instance.ImageUrl, true);
                    shouldAllow = true;
                    return;

                }
                shouldAllow = true;
                return;






            }
        }

        #region private methods
        public static bool isInGroup(RocketPermissionsGroup group, UnturnedPlayer player)
        {

            if (group.Members.Contains(player.CSteamID.m_SteamID.ToString()))
            {
                return true;
            }
            else
            {

                return false;
            }

        }
        private UnturnedPlayer GetDriver(InteractableVehicle vehicle, UnturnedPlayer cliente)
        {

            int count = Colectivero.Count;


            foreach (CSteamID posibleDriver in Colectivero)
            {


                if (vehicle.checkDriver(posibleDriver))
                {

                    return UnturnedPlayer.FromCSteamID(posibleDriver);
                }

            }
            return cliente;

        }
        #endregion

        public static AutoBusPlugin pluginInstance;
        public static List<CSteamID> Colectivero { get; set; }

        public static Dictionary<CSteamID, decimal> Balance { get; set; }
        public static string AutoBusNameMessage { get; private set; }
        public static Dictionary<CSteamID, int> Earnings {get; set;}

        #region transaltion and unload
        public override TranslationList DefaultTranslations => new TranslationList()
        {
            {"OUT_DRIVER", AutoBusNameMessage+" You get expelled because {0} get out of the vehicle"},
            {"NOT_ENOUGH_MONEY",AutoBusNameMessage+" You don´t have enough money to start the travel, you need {0}"},
            {"ARENT_DRIVER",AutoBusNameMessage+" There aren´t a driver in this vehicle"},
            {"WON_DRIVER", AutoBusNameMessage+" For a new passager you won the amount of ${0}"},
            {"PAY_TO_DRIVER",AutoBusNameMessage+" Yoy paied to {0} a totall of ${1} "},
            {"NOT_IN_JOB",AutoBusNameMessage+" You don´t are in the bus job"},
            {"UI_WON","Won:"},
            {"BAD_USE",AutoBusNameMessage+" Bad use of command. Use: {0}"},
            {"SERVICE_OFF",AutoBusNameMessage+" You now don`t are more in service"},
            {"SERVICE_ON",AutoBusNameMessage+" You started the service as a bus driver"},
        };

        protected override void Unload()
        {
            Logger.Log($"{Assembly.GetName().Name} has been unloaded!", ConsoleColor.Yellow);
            VehicleManager.onEnterVehicleRequested -= onEnterVehicleRequeseted;
            VehicleManager.onExitVehicleRequested -= onExitVechicleRequeseted;
            VehicleManager.onSwapSeatRequested -= onSwapSeat;
        }

        #endregion
    }
}
