using Life;
using Life.UI;
using System;
using UnityEngine;
using MyMenu.Entities;
using Life.Network;
using System.Drawing;
using Life.AreaSystem;
using UIPanelManager;

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

            new SChatCommand("/tp", "Permet d'aller sur un terrain", "/tp areaId", (player, arg) =>
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
            Debug.Log("open myjumper panel");
        }
    }
}