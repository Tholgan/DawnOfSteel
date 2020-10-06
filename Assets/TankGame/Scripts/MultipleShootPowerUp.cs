using Assets.TankGame.Scripts;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleShootPowerUp : NetworkBehaviour
{
    private void Update()
    {
        transform.Rotate(Vector3.left, Space.Self);
        transform.Rotate(Vector3.up, Space.World);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider co)
    {
        if(co.gameObject.tag.Equals("Player"))
        {
            co.GetComponent<TankPlayerController>().RpcSetMultipleShoot();
            NetworkServer.Destroy(gameObject);
        }
    }
}
