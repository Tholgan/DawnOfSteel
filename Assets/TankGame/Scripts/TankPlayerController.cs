using Mirror;
using Mirror.Examples.Tanks;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public Transform projectileMount;
        public float shootWaitTime = 1f;
        public float shootTime;

        [Header("Game Stats")]
        [SyncVar]
        public int health;
        [SyncVar]
        public int score;
        [SyncVar]
        public string playerName;

        [SyncVar] public bool isTouched;

        public bool isDead => health <= 0;
        public TextMesh nameText;


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
        }

        public override void OnStartLocalPlayer()
        {
            characterController.enabled = true;

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

        //void Update()
        //{
        //    if (!isLocalPlayer)
        //        return;

        //    vertical = _direction.y;
        //    turn = Mathf.MoveTowards(turn, _direction.x, turnSensitivity);
        //}

        //void FixedUpdate()
        //{
        //    if (!isLocalPlayer || characterController == null)
        //        return;

        //    transform.Rotate(0f, turn * Time.fixedDeltaTime, 0f);

        //    Vector3 direction = new Vector3(0, jumpSpeed, vertical);
        //    direction = Vector3.ClampMagnitude(direction, 1f);
        //    direction = transform.TransformDirection(direction);
        //    direction *= moveSpeed;

        //    if (jumpSpeed > 0)
        //        characterController.Move(direction * Time.fixedDeltaTime);
        //    else
        //        characterController.SimpleMove(direction);

        //    isGrounded = characterController.isGrounded;
        //    velocity = characterController.velocity;


        //}
        //Region for Old Input System
        void Update()
        {
            if (!isLocalPlayer)
                return;

            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");

            // Q and E cancel each other out, reducing the turn to zero
            if (Input.GetKey(KeyCode.Q))
                turn = Mathf.MoveTowards(turn, -maxTurnSpeed, turnSensitivity);
            if (Input.GetKey(KeyCode.D))
                turn = Mathf.MoveTowards(turn, maxTurnSpeed, turnSensitivity);
            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.D))
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);
            if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.D))
                turn = Mathf.MoveTowards(turn, 0, turnSensitivity);

            if (Input.GetKeyDown(KeyCode.Space) && Time.time > shootTime + shootWaitTime)
            {
                shootTime = Time.time;
                CmdFire();
            }
        }

        void FixedUpdate()
        {
            if (!isLocalPlayer || characterController == null)
                return;

            transform.Rotate(0f, turn * Time.fixedDeltaTime, 0f);

            Vector3 direction = new Vector3(horizontal, jumpSpeed, vertical);
            direction = Vector3.ClampMagnitude(direction, 1f);
            direction = transform.TransformDirection(direction);
            direction *= moveSpeed;

            if (jumpSpeed > 0)
                characterController.Move(direction * Time.fixedDeltaTime);
            else
                characterController.SimpleMove(direction);

            velocity = characterController.velocity;
        }

        //Used with New Input System
        //private void OnMove(InputValue value)
        //{
        //    _direction = value.Get<Vector2>();
        //}

        //private void OnShoot()
        //{
        //    if (Time.time > shootTime + shootWaitTime)
        //    {
        //        shootTime = Time.time;
        //        CmdFire();
        //    }
        //}

        [Command]
        void CmdFire()
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileMount.position, transform.rotation);
            projectile.GetComponent<TankProjectile>().source = gameObject;
            NetworkServer.Spawn(projectile);
        }
    }
}
