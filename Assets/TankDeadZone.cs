using System.Collections;
using System.Collections.Generic;
using Assets.TankGame.Scripts;
using Mirror;
using UnityEngine;

public class TankDeadZone : NetworkBehaviour
{

    [ServerCallback]
    void OnTriggerEnter(Collider co)
    {
        if (co.tag == "Player")
        {
            co.GetComponent<TankPlayerController>().health = 0;
            co.GetComponent<TankPlayerController>().enabled = false;
        }
        else
        {
            Destroy(co.gameObject);
        }
    }
}
