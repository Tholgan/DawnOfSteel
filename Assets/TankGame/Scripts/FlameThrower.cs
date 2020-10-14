using Assets.TankGame.Scripts;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrower : NetworkBehaviour
{
    public float damagePoints = 1;
    private GameObject source;

    private void Start()
    {
        source = transform.root.gameObject;
    }

    private void OnTriggerStay(Collider co)
    {
        if (co.gameObject.tag.Equals("Player") && co.gameObject != source)
        {
            co.GetComponent<TankPlayerController>().health -= damagePoints;
        }
    }

}
