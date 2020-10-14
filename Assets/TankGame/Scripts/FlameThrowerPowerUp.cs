using Mirror;
using UnityEngine;

namespace Assets.TankGame.Scripts
{
    public class FlameThrowerPowerUp : NetworkBehaviour
    {
        private void Update()
        {
            transform.Rotate(Vector3.left, Space.Self);
            transform.Rotate(Vector3.up, Space.World);
        }

        [ServerCallback]
        void OnTriggerEnter(Collider co)
        {
            if (co.gameObject.tag.Equals("Player"))
            {
                co.GetComponent<TankPlayerController>().RpcSetFlameThrower();
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}
