using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ExplosionAnimation : NetworkBehaviour
{
    private ParticleSystem explosion;
    // Start is called before the first frame update
    void Awake()
    {
        explosion = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (explosion.IsAlive()) return;
        NetworkServer.Destroy(gameObject);
    }
}
