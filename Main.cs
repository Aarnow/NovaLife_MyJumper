﻿using Life;
using Life.UI;
using System;
using UnityEngine;
using MyMenu.Entities;
using Life.Network;
using System.Drawing;
using Life.AreaSystem;
using UIPanelManager;
using Life.VehicleSystem;
using Life.DB;

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
                            player.setup.TargetSetPosition(new Vector3(vehicle.x, vehicle.y+3, vehicle.z));
                        }
                        else PanelManager.Notification(player, "Erreur", "Aucun véhicule ne semble correspondre à votre identifiant.", NotificationManager.Type.Error);
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
            /*panel.AddTabLine("Téléportation à un véhicule", ui => Debug.Log("tp vehicle"));
            panel.AddTabLine("Téléportation à un joueur", ui => Debug.Log("tp player"));
            panel.AddTabLine("Téléportation à une société", ui => Debug.Log("tp biz"));*/


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
    }
}