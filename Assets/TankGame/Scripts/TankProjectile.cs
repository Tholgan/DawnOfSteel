using Mirror;
using System;
using UnityEngine;

namespace Assets.TankGame.Scripts
{
    public class TankProjectile : NetworkBehaviour
    {
        public float destroyAfter = 5;
        public Rigidbody rigidBody;
        public float force = 1000;
        public GameObject effect;

        [Header("Game Stats")]
        public int damage;
        public GameObject source;

        [SyncVar]
        private bool _hasStarted;

        public override void OnStartServer()
        {
            Invoke(nameof(DestroySelf), destroyAfter);
        }

        // set velocity for server and client. this way we don't have to sync the
        // position, because both the server and the client simulate it.
        void Start()
        {
            rigidBody.AddForce(transform.forward * force);
        }

        // destroy for everyone on the server
        [Server]
        void DestroySelf()
        {
            NetworkServer.Destroy(gameObject);
        }

        // ServerCallback because we don't want a warning if OnTriggerEnter is
        // called on the client
        [ServerCallback]
        void OnTriggerEnter(Collider co)
        {
            //Hit another player
            if (co.tag.Equals("Player") && co.gameObject != source)
            {
                //Apply damage
                co.GetComponent<TankPlayerController>().health -= damage;

                //update score on source
                source.GetComponent<TankPlayerController>().score += damage;

                RpcInstantiateExplosion();
                NetworkServer.Destroy(gameObject);
            }

            if (co.tag.Equals("Level") && co.gameObject != source)
            {
                RpcInstantiateExplosion();
                NetworkServer.Destroy(gameObject);
            }
        }

        [ClientRpc]
        void RpcInstantiateExplosion()
        {
            Instantiate(effect, transform.position, transform.rotation);
        }
    }
}
