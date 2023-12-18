using Life;
using Life.AreaSystem;
using Life.Network;
using Life.UI;
using Life.VehicleSystem;
using System.Collections.Generic;
using System.Linq;
using UIPanelManager;
using UnityEngine;

namespace MyJumper
{
    abstract class AdminPanels
    {
        public static void Open(Player player)
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

        public static void SetAreaId(Player player)
        {
            UIPanel panel = new UIPanel("MyJumper", UIPanel.PanelType.Input).SetTitle($"Téléportation à un terrain");

            panel.inputPlaceholder = "Identifiant du terrain";

            panel.AddButton("Sélectionner", ui =>
            {
                if (ui.inputText.Length > 0 && uint.TryParse(ui.inputText, out uint areaId))
                {
                    LifeArea area = Nova.a.GetAreaById(areaId);
                    if (area != null)
                    {
                        Vector3 spawn = area.instance.spawn;
                        player.setup.TargetSetPosition(new Vector3(spawn.x, spawn.y, spawn.z));
                        PanelManager.NextPanel(player, ui, () => Open(player));
                    }
                    else PanelManager.Notification(player, "Erreur", "Aucun terrain ne semble correspondre à votre identifiant.", NotificationManager.Type.Error);
                }
                else PanelManager.Notification(player, "Erreur", "Vous devez indiquer l'identifiant du terrain.", NotificationManager.Type.Error);
            });
            panel.AddButton("Fermer", ui => PanelManager.Quit(ui, player));

            player.ShowPanelUI(panel);
        }

        public static void SetVehiclePlate(Player player)
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
                        if (!vehicle.isStowed)
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

        public static void WatchPlayer(Player player, Player target = null, int indexNextPlayer = 0)
        {
            List<Player> allPlayers = Nova.server.GetAllInGamePlayers().Where(p => p.netId != player.netId && !p.setup.isAdminService).ToList();

            UIPanel panel = new UIPanel("MyJumper", UIPanel.PanelType.Tab).SetTitle($"Téléportation sur un joueur");

            if (target == null && allPlayers.Count != 0) target = allPlayers.First();

            panel.AddTabLine($"{(allPlayers.Count != 0 ? player.GetFullName() : "Aucun joueur en jeu")}", ui => ui.selectedTab = 0);

            if (allPlayers.Count != 0)
            {
                if (!player.setup.isFlying) player.setup.NetworkisFlying = true;
                if (!player.setup.isVanished) player.setup.NetworkisVanished = true;

                Player currentPlayerTargeted = Nova.server.GetAllInGamePlayers().Where(p => p.netId == target.netId).FirstOrDefault();
                player.setup.TargetSetPosition(new Vector3(
                    currentPlayerTargeted.setup.transform.position.x,
                    currentPlayerTargeted.setup.transform.position.y,
                    currentPlayerTargeted.setup.transform.position.z));
                Coroutine followCoroutine = player.setup.StartCoroutine(Coroutines.FollowTarget(player, currentPlayerTargeted));

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
            }
            else panel.AddButton("Fermer", ui => PanelManager.Quit(ui, player));

            player.ShowPanelUI(panel);
        }
    }
}
