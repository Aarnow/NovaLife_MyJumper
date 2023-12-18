using Life.Network;
using System.Collections;
using UnityEngine;

namespace MyJumper
{
    abstract class Coroutines
    {
        public static IEnumerator FollowTarget(Player player, Player target)
        {
            while (true)
            {
                if (!target.isInGame) yield return null;
                Vector3 position;
                if (target.setup.driver.NetworkcurrentVehicle != 0)
                {
                    position = new Vector3(target.setup.driver.vehicle.transform.localPosition.x, target.setup.driver.vehicle.transform.position.y + 3, target.setup.driver.vehicle.transform.position.z);
                }
                else position = new Vector3(target.setup.transform.position.x, target.setup.transform.position.y + 4, target.setup.transform.position.z);

                player.setup.TargetSetPosition(Vector3.Lerp(player.setup.transform.position, position, 100f * Time.deltaTime));

                yield return null;
            }
        }
    }
}
