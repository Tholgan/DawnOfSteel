using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    // Start is called before the first frame update
    public CharacterController characterController;
    public CapsuleCollider capsuleCollider;

    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float turnSensitivity = 5f;
    public float maxTurnSpeed = 150f;

    [Header("Diagnostics")]
    public float horizontal;
    public float vertical;
    public float turn;
    public float jumpSpeed;
    public bool isGrounded = true;
    public bool isFalling;
    public Vector3 velocity;
    private Vector2 _direction;
    private bool _startedJumping;

    void OnValidate()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (capsuleCollider == null)
            capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        capsuleCollider.enabled = isServer;
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

        if (jumpSpeed > 0)
            characterController.Move(direction * Time.fixedDeltaTime);
        else
            characterController.SimpleMove(direction);

        isGrounded = characterController.isGrounded;
        velocity = characterController.velocity;
    }

    private void OnMove(InputValue value)
    {
        _direction = value.Get<Vector2>();
    }
}
