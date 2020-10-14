using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.TankGame.Scripts
{
    public class TankPlayerController : NetworkBehaviour
    {
        // Start is called before the first frame update
        public CharacterController characterController;
        public BoxCollider boxCollider;

        [Header("Movement Settings")]
        public float moveSpeed = 8f;
        public float turnSensitivity = 5f;
        private float horizontal;
        private int maxTurnSpeed = 70;
        [Header("Diagnostics")]
        public float vertical;
        public float turn;
        public float jumpSpeed;
        public bool isGrounded = true;
        public Vector3 velocity;


        private Vector2 _direction;

        [Header("Firing")]
        public GameObject projectilePrefab;
        public Transform projectileMountMain;
        public Transform projectileMountLeft;
        public Transform projectileMountRight;
        public float shootWaitTime = 1f;
        public float shootTime;
        public GameObject multipleFiringAddon;
        public GameObject FlameThrower;

        [Header("Game Stats")]
        [SyncVar]
        public float health;
        [SyncVar]
        public int score;
        [SyncVar]
        public string playerName;

        [SyncVar] public bool isTouched;

        public bool isDead => health <= 0;

        [SyncVar]
        public bool isTouchedByFlame;

        public TextMesh nameText;
        public Text playerNameText;
        public Animator animator;
        public GameObject smokeObject;

        void OnValidate()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();
        }

        void Start()
        {
            boxCollider.enabled = isServer;
            playerNameText.text = playerName;
            RpcSetNormalShoot();
        }

        public override void OnStartLocalPlayer()
        {
            characterController.enabled = true;
            GetComponent<PlayerInput>().enabled = true;

            Camera.main.orthographic = false;
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0f, 1.3f, -2.7f);
            Camera.main.transform.localEulerAngles = new Vector3(12f, 0f, 0f);
        }

        void OnDisable()
        {
            if (isLocalPlayer && Camera.main != null)
            {
                Camera.main.orthographic = true;
                Camera.main.transform.SetParent(null);
                Camera.main.transform.localPosition = new Vector3(0f, 70f, 0f);
                Camera.main.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            }
        }

        void Update()
        {
            if (!isLocalPlayer)
                return;

            vertical = _direction.y;
            turn = Mathf.MoveTowards(turn, _direction.x, turnSensitivity);
        }

        void FixedUpdate()
        {
            if (!isLocalPlayer || characterController == null)
                return;

            transform.Rotate(0f, turn * Time.fixedDeltaTime, 0f);

            Vector3 direction = new Vector3(0, jumpSpeed, vertical);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= moveSpeed;
            animator.SetBool("Moving", direction != Vector3.zero);

            if (jumpSpeed > 0)
                characterController.Move(direction * Time.fixedDeltaTime);
            else
                characterController.SimpleMove(direction);

            isGrounded = characterController.isGrounded;
            velocity = characterController.velocity;
            smokeObject.SetActive(isTouchedByFlame);
        }

        private void OnMove(InputValue value)
        {
            _direction = value.Get<Vector2>();
        }

        private void OnShoot()
        {
            if (Time.time > shootTime + shootWaitTime)
            {
                shootTime = Time.time;
                CmdFire();
            }
        }

        private void OnFlameThrower()
        {
            CmdFlameThrower();
        }

        private void OnMultipleShoot()
        {
            if (Time.time > shootTime + shootWaitTime)
            {
                shootTime = Time.time;
                CmdMultiShoot();
            }
        }

        [Command]
        void CmdMultiShoot()
        {
            GameObject projectile1 = Instantiate(projectilePrefab, projectileMountMain.position, transform.rotation);
            GameObject projectile2 = Instantiate(projectilePrefab, projectileMountLeft.position, transform.rotation);
            GameObject projectile3 = Instantiate(projectilePrefab, projectileMountRight.position, transform.rotation);

            var projectiles = new List<GameObject>() { projectile1, projectile2, projectile3 };

            foreach (GameObject projectile in projectiles)
            {
                projectile.GetComponent<TankProjectile>().source = gameObject;
                projectile.GetComponent<TankProjectile>().damage = 10;

                NetworkServer.Spawn(projectile);
            }
            RpcOnFire();
        }

        [Command]
        void CmdFlameThrower()
        {
            FlameThrower.SetActive(!FlameThrower.activeInHierarchy);
            RpcFlameThrowerOn();
        }

        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMountMain.position, transform.rotation);
            projectile.GetComponent<TankProjectile>().source = gameObject;
            NetworkServer.Spawn(projectile);
            RpcOnFire();
        }

        [ClientRpc]
        void RpcOnFire()
        {
            animator.SetTrigger("Shoot");
        }

        [ClientRpc]
        void RpcFlameThrowerOn()
        {
            FlameThrower.SetActive(!FlameThrower.activeInHierarchy);
        }

        [ClientRpc]
        public void RpcSetMultipleShoot()
        {
            var playerInput = GetComponent<PlayerInput>();
            playerInput.actions.FindAction("MultipleShoot").Enable();
            playerInput.actions.FindAction("Shoot").Disable();
            playerInput.actions.FindAction("FlameThrower").Disable();
        }

        [ClientRpc]
        public void RpcSetFlameThrower()
        {
            var playerInput = GetComponent<PlayerInput>();
            playerInput.actions.FindAction("FlameThrower").Enable();
            playerInput.actions.FindAction("Shoot").Disable();
            playerInput.actions.FindAction("MultipleShoot").Disable();
        }

        [ClientRpc]
        public void RpcSetNormalShoot()
        {
            var playerInput = GetComponent<PlayerInput>();
            playerInput.actions.FindAction("Shoot").Enable();
            playerInput.actions.FindAction("FlameThrower").Disable();
            playerInput.actions.FindAction("MultipleShoot").Disable();
        }
    }
}
