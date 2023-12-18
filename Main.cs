using Life;
using Life.UI;
using System;
using UnityEngine;
using MyMenu.Entities;
using Life.Network;
using Life.AreaSystem;
using UIPanelManager;
using Life.VehicleSystem;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

namespace MyJumper
{
    public class Main : Plugin
    {
        public Main(IGameAPI api):base(api)
        {

        }

        public override void OnPluginInit()
        {
            base.OnPluginInit();

            new SChatCommand("/val", "Permet d'aller sur un terrain", "/val", (player, arg) =>
            {
                player.setup.NetworkisVal = true;
            }).Register();

            new SChatCommand("/tpa", "Permet d'aller sur un terrain", "/tpa areaId", (player, arg) =>
            {
                if (player.IsAdmin)
                {
                    if (arg[0] != null && uint.TryParse(arg[0], out uint areaId))
                    {
                        LifeArea area = Nova.a.GetAreaById(areaId);
                        if (area != null)
                        {
                            Vector3 spawn = area.instance.spawn;
                            player.setup.TargetSetPosition(new Vector3(spawn.x, spawn.y, spawn.z));
                        } else PanelManager.Notification(player, "Erreur", "Aucun terrain ne semble correspondre à votre identifiant.", NotificationManager.Type.Error);
                    } else PanelManager.Notification(player, "Erreur", "Vous devez indiquer l'identifiant du terrain en paramètre. (exemple: /tp 1)", NotificationManager.Type.Error);
                }  else PanelManager.Notification(player, "Erreur", "Vous n'avez pas les permissions d'accéder à cette commande.", NotificationManager.Type.Error);
            }).Register();

            new SChatCommand("/tpv", "Permet d'aller sur un véhicule", "/tpv plateId", (player, arg) =>
            {
                if (player.IsAdmin)
                {
                    if (arg[0] != null)
                    {
                        LifeVehicle vehicle = Nova.v.GetVehicle(arg[0].ToUpper());
                        if (vehicle != null)
                        {
                            if (!vehicle.isStowed)
                            {
                                player.setup.TargetSetPosition(new Vector3(vehicle.x, vehicle.y + 3, vehicle.z));
                            }
                            else PanelManager.Notification(player, "Information", "Ce véhicule est dans le garage virtuel (stowed).", NotificationManager.Type.Info);
                        }
                        else PanelManager.Notification(player, "Erreur", "Aucun véhicule ne semble correspondre à cette plaque.", NotificationManager.Type.Error);
                    }
                    else PanelManager.Notification(player, "Erreur", "Vous devez indiquer la plaque du véhicule en paramètre. (exemple: /tpv RB-364-EP)", NotificationManager.Type.Error);
                }
                else PanelManager.Notification(player, "Erreur", "Vous n'avez pas les permissions d'accéder à cette commande.", NotificationManager.Type.Error);
            }).Register();

            //MyMenu
            try
            {
                Section section = new Section(Section.GetSourceName(), Section.GetSourceName(), "v1.0.0", "Aarnow");
                Action<UIPanel> action = ui => Open(section.GetPlayer(ui));
                section.OnlyAdmin = true;
                section.Line = new UITabLine(section.Title, action);
                section.Insert(true);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }

            Debug.Log($"Plugin \"MyJumper\" initialisé avec succès.");
        }

        public void Open(Player player)
        {
            UIPanel panel = new UIPanel("MyJumper", UIPanel.PanelType.Tab).SetTitle($"MyJumper");
         
            panel.AddTabLine("Téléportation à un terrain", ui => PanelManager.NextPanel(player, ui, () => SetAreaId(player)));
            panel.AddTabLine("Téléportation à un véhicule", ui => PanelManager.NextPanel(player, ui, () => SetVehiclePlate(player)));
            panel.AddTabLine("Regarder un joueur", ui => PanelManager.NextPanel(player, ui, () => WatchPlayer(player)));
            //panel.AddTabLine("Téléportation à une société", ui => Debug.Log("tp biz"));


            panel.AddButton("Sélectionner", ui => ui.SelectTab());
            panel.AddButton("Fermer", ui => PanelManager.Quit(ui, player));

            player.ShowPanelUI(panel);
        }

        public void SetAreaId(Player player)
        {
            UIPanel panel = new UIPanel("MyJumper", UIPanel.PanelType.Input).SetTitle($"Téléportation à un terrain");

            panel.inputPlaceholder = "Identifiant du terrain";

            panel.AddButton("Sélectionner", ui =>
            {
                if(ui.inputText.Length > 0 && uint.TryParse(ui.inputText, out uint areaId))
                {
                    LifeArea area = Nova.a.GetAreaById(areaId);
                    if (area != null)
                    {
                        Vector3 spawn = area.instance.spawn;
                        player.setup.TargetSetPosition(new Vector3(spawn.x, spawn.y, spawn.z));
                        PanelManager.NextPanel(player, ui, () => Open(player));
                    } else PanelManager.Notification(player, "Erreur", "Aucun terrain ne semble correspondre à votre identifiant.", NotificationManager.Type.Error);
                } else PanelManager.Notification(player, "Erreur", "Vous devez indiquer l'identifiant du terrain.", NotificationManager.Type.Error);
            });
            panel.AddButton("Fermer", ui => PanelManager.Quit(ui, player));

            player.ShowPanelUI(panel);
        }

        public void SetVehiclePlate(Player player)
        {
            UIPanel panel = new UIPanel("MyJumper", UIPanel.PanelType.Input).SetTitle($"Téléportation à un véhicule");

            panel.inputPlaceholder = "Plaque du véhicule";

            panel.AddButton("Sélectionner", ui =>
            {
                if (ui.inputText.Length > 0)
                {
                    LifeVehicle vehicle = Nova.v.GetVehicle(ui.inputText.ToUpper());
                    if (vehicle != null)
                    {
                        if(!vehicle.isStowed)
                        {
                            player.setup.TargetSetPosition(new Vector3(vehicle.x, vehicle.y + 3, vehicle.z));
                            PanelManager.NextPanel(player, ui, () => Open(player));
                        }
                        else PanelManager.Notification(player, "Information", "Ce véhicule est dans le garage virtuel (stowed).", NotificationManager.Type.Info);
                    }
                    else PanelManager.Notification(player, "Erreur", "Aucun véhicule ne semble correspondre à cette plaque.", NotificationManager.Type.Error);
                }
                else PanelManager.Notification(player, "Erreur", "Vous devez indiquer la plaque du véhicule en paramètre. (exemple: /tpv RB-364-EP)", NotificationManager.Type.Error);
            });
            panel.AddButton("Fermer", ui => PanelManager.Quit(ui, player));

            player.ShowPanelUI(panel);
        }

        public void WatchPlayer(Player player, Player target = null, int indexNextPlayer = 0)
        {
            List<Player> allPlayers = Nova.server.GetAllInGamePlayers().Where(p => p.netId != player.netId && !p.setup.isAdminService).ToList();

            UIPanel panel = new UIPanel("MyJumper", UIPanel.PanelType.Tab).SetTitle($"Téléportation sur un joueur");

            if (target == null && allPlayers.Count != 0) target = allPlayers.First();          

            panel.AddTabLine($"{(allPlayers.Count != 0 ? player.GetFullName() : "Aucun joueur en jeu")}", ui => ui.selectedTab=0);

            if(allPlayers.Count != 0)
            {
                if (!player.setup.isFlying) player.setup.NetworkisFlying = true;
                if (!player.setup.isVanished) player.setup.NetworkisVanished = true;

                //coroutine

                Player currentPlayerTargeted = Nova.server.GetAllInGamePlayers().Where(p => p.netId == target.netId).FirstOrDefault();
                player.setup.TargetSetPosition(new Vector3(currentPlayerTargeted.setup.transform.position.x, currentPlayerTargeted.setup.transform.position.y, currentPlayerTargeted.setup.transform.position.z));
                Coroutine followCoroutine = player.setup.StartCoroutine(FollowTarget(player, currentPlayerTargeted));

                panel.AddButton("Précédent", ui =>
                {
                    if (followCoroutine != null) player.setup.StopCoroutine(followCoroutine);
                    allPlayers = Nova.server.GetAllInGamePlayers().Where(p => p.netId != player.netId).ToList();
                    indexNextPlayer = (indexNextPlayer - 1 + allPlayers.Count) % allPlayers.Count;
                    Player nextPlayer = allPlayers[indexNextPlayer];
                    PanelManager.NextPanel(player, ui, () => WatchPlayer(player, nextPlayer, indexNextPlayer));
                });
                panel.AddButton("Suivant", ui =>
                {
                    if (followCoroutine != null) player.setup.StopCoroutine(followCoroutine);
                    allPlayers = Nova.server.GetAllInGamePlayers().Where(p => p.netId != player.netId).ToList();
                    indexNextPlayer = (indexNextPlayer + 1) % allPlayers.Count;
                    Player nextPlayer = allPlayers[indexNextPlayer];
                    PanelManager.NextPanel(player, ui, () => WatchPlayer(player, nextPlayer, indexNextPlayer));
                });
                panel.AddButton("Stop", ui =>
                {
                    if (followCoroutine != null) player.setup.StopCoroutine(followCoroutine);
                    PanelManager.Notification(player, "Arrêt Myjumper", "Vous avez cesser de suivre votre cible", NotificationManager.Type.Warning);
                });
                panel.AddButton("Fermer", ui =>
                {
                    if (followCoroutine != null) player.setup.StopCoroutine(followCoroutine);
                    PanelManager.Quit(ui, player);
                });
            } else
            {
                panel.AddButton("Fermer", ui => PanelManager.Quit(ui, player));
            }
            

            player.ShowPanelUI(panel);
        }

        IEnumerator FollowTarget(Player player, Player target)
        {
            while (true)
            {
                if (!target.isInGame) yield return null;
                Vector3 position;
                if (target.setup.driver.NetworkcurrentVehicle != 0)
                {
                    position = new Vector3(target.setup.driver.vehicle.transform.localPosition.x, target.setup.driver.vehicle.transform.position.y + 3, target.setup.driver.vehicle.transform.position.z);
                } else position = new Vector3(target.setup.transform.position.x, target.setup.transform.position.y + 4, target.setup.transform.position.z);
                
                player.setup.TargetSetPosition(Vector3.Lerp(player.setup.transform.position, position, 100f * Time.deltaTime));  

                yield return null;
            }
        }
    }
}