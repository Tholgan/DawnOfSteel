using Assets.TankGame.Scripts;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBloc : NetworkBehaviour
{
    [ServerCallback]
    void OnTriggerEnter(Collider co)
    {
        if (co.gameObject.tag == "Player") ;
            //co.GetComponent<TankPlayerController>().GetHit(gameObject.GetComponent<Rigidbody>().velocity);
    }
}
