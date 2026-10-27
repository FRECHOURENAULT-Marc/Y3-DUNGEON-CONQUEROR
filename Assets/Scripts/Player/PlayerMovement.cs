using System;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{

    [Header("References")]
    [SerializeField] PlayerReferences _PReference;

    // [SerializeField] PlayerInput _playerInput;
    // [SerializeField] GameObject _playerRoot;
    // [SerializeField] Rigidbody _playerRB;
    // [SerializeField] Camera _playerCamera;
    // [SerializeField] GameObject _playerArms;

    [Space]
    [SerializeField] Vector3 _cameraOffset;



    [Space]
    [Header("Movement Setting")]
    [SerializeField] float _jumpRaycastDistance;
    [SerializeField] float _jumpForce;
    [Space]
    [SerializeField] float _dashSpeed;
    [SerializeField] float _dashDuration = 0.15f;
    [Space]
    [SerializeField] float _movementSpeed;
    [SerializeField] float _sprintAdditionnalSpeed;
    [Space]
    [SerializeField] Vector2 _lookDirection;
    [SerializeField] float _lookMultiplier;

    [Header("Capacity")]
    [SerializeField] float _speedMultiplier;
    [SerializeField] bool _asUnlockSpeedMultiplier;
    [SerializeField] bool _asUnlockDoubleJump;
    [SerializeField] bool _asUnlockDash;

    float _cameraPitch;
    float _cameraYawOffset;

    [Space]
    [Header("Debug Info")]
    [SerializeField] Vector3 _moveDirection;
    [ShowNonSerializedField] bool _isGrounded;
    [ShowNonSerializedField] bool _predictedIsGrounded;
    [ShowNonSerializedField] bool _isOnSprint;
    [ShowNonSerializedField] float _velocity;

    [SerializeField] bool _canDash;
    [SerializeField] bool _canDoubleJump;
    bool _isDashing;
    float _dashTimeRemaining;
    Vector3 _dashDirection;
    CollisionDetectionMode _previousCollisionDetectionMode;


    public bool AsUnlockDash { get => _asUnlockDash; set => _asUnlockDash = value; }
    public bool AsUnlockDoubleJump { get => _asUnlockDoubleJump; set => _asUnlockDoubleJump = value; }
    public bool AsUnlockSpeedMultiplier { get => _asUnlockSpeedMultiplier; set => _asUnlockSpeedMultiplier = value; }

    public bool IsGrounded { get => _isGrounded; }
    public bool PredictedIsGrounded { get => _predictedIsGrounded; }

    public float Velocity { get => _velocity; }





    void Start()
    {
        _canDash = true;
        _canDoubleJump = true;

        _cameraPitch = Mathf.DeltaAngle(0f, _PReference.PlayerCamera.transform.eulerAngles.x);
        _cameraYawOffset = Mathf.DeltaAngle(
            _PReference.PlayerRoot.transform.eulerAngles.y,
            _PReference.PlayerCamera.transform.eulerAngles.y
        );

        _PReference.PlayerInput.OnMove += HandleMove;
        _PReference.PlayerInput.OnLook += HandleLook;

        _PReference.PlayerInput.OnSprint += HandleSprint;
        _PReference.PlayerInput.OnJump += HandleJump;
        _PReference.PlayerInput.OnDash += HandleDash;
    }

    void OnDestroy()
    {
        _PReference.PlayerInput.OnLook -= HandleLook;
        _PReference.PlayerInput.OnMove -= HandleMove;
        _PReference.PlayerInput.OnSprint -= HandleSprint;
        _PReference.PlayerInput.OnJump -= HandleJump;
        _PReference.PlayerInput.OnDash -= HandleDash;
    }

    // Update is called once per frame
    void Update()
    {
        _velocity = _moveDirection.magnitude * ( _movementSpeed + ( _isOnSprint ? _sprintAdditionnalSpeed : 0 ) );
        float maxVel = _movementSpeed + _sprintAdditionnalSpeed;
        _velocity /= maxVel;


        // Set camera to player position
        _PReference.PlayerCamera.transform.position = _PReference.PlayerRoot.transform.position + _cameraOffset;

        // PlayerRotation
        Vector2 lookDelta = _lookDirection;
        _lookDirection = Vector2.zero;

        _PReference.PlayerRoot.transform.Rotate(
            Vector3.up,
            lookDelta.x * 0.1f * _lookMultiplier,
            Space.World
        );

        _cameraPitch = Mathf.Clamp(
            _cameraPitch - lookDelta.y * 0.1f * _lookMultiplier,
            -59f,
            59f
        );
        _PReference.PlayerCamera.transform.rotation = Quaternion.Euler(
            _cameraPitch,
            _PReference.PlayerRoot.transform.eulerAngles.y + _cameraYawOffset,
            0f
        );

        // _isgrounded
        _isGrounded = Physics.Raycast(_PReference.PlayerRoot.transform.position, Vector3.down, _jumpRaycastDistance);
        _predictedIsGrounded = Physics.Raycast(_PReference.PlayerRoot.transform.position, Vector3.down, _jumpRaycastDistance + 1);
        Debug.DrawRay(_PReference.PlayerRoot.transform.position, Vector3.down * _jumpRaycastDistance, Color.red);
        Debug.DrawRay(_PReference.PlayerRoot.transform.position, Vector3.down * (_jumpRaycastDistance + 1), Color.blue);
    }

    void FixedUpdate()
    {
        if ( _isDashing )
        {
            _dashTimeRemaining -= Time.fixedDeltaTime;

            Rigidbody playerRigidbody = _PReference.PlayerRigidbody;

            Vector3 velocity = playerRigidbody.linearVelocity;

            if ( _dashTimeRemaining > 0f )
            {
                playerRigidbody.linearVelocity = new Vector3(
                    _dashDirection.x * _dashSpeed * (float) (_asUnlockSpeedMultiplier ? 1.5 : 1),
                    velocity.y,
                    _dashDirection.z * _dashSpeed * (float) (_asUnlockSpeedMultiplier ? 1.5 : 1)
                );
                return;
            }

            playerRigidbody.linearVelocity = new Vector3(0f, velocity.y, 0f);
            playerRigidbody.collisionDetectionMode = _previousCollisionDetectionMode;
            _isDashing = false;
        }
        else
        {
            Rigidbody rb = _PReference.PlayerRigidbody;

            Vector3 direction = _PReference.PlayerRoot.transform.TransformDirection(_moveDirection);
            
            float speed = (_movementSpeed + (_isOnSprint ? _sprintAdditionnalSpeed : 0f)) * (float) (_asUnlockSpeedMultiplier ? _speedMultiplier : 1);

            rb.linearVelocity = new Vector3(
                direction.x * speed,
                rb.linearVelocity.y,
                direction.z * speed
            );
        }
    }

    void HandleMove(Vector2 moveInput)
    {
        _moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
    }

    void HandleSprint(bool ctx)
    {
        _isOnSprint = ctx;
    }

    void HandleLook(Vector2 lookInput)
    {
        _lookDirection += lookInput;
    }

    void HandleJump()
    {
        if ( _asUnlockDoubleJump )
        {
            if (_PReference.PlayerRigidbody != null && (_isGrounded || _canDoubleJump))
            {
                // if(_PReference.PlayerRigidbody.linearVelocity.y <= 0)
                _PReference.PlayerRigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

                if ( _canDoubleJump && _PReference.PlayerRigidbody.linearVelocity.y > 0 ) DoubleJumpRecovery();
            }
        }
        else
        {
            if (_PReference.PlayerRigidbody != null && _isGrounded)
            {
                // if(_PReference.PlayerRigidbody.linearVelocity.y <= 0)
                _PReference.PlayerRigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
            }
        }
    }

    async void DoubleJumpRecovery()
    {
        _canDoubleJump = false;

        while ( !_isGrounded ) await Awaitable.NextFrameAsync();

        _canDoubleJump = true;
    }

    void HandleDash()
    {
        if ( !_asUnlockDash ) return;
        if ( !_canDash ) return;

        Rigidbody playerRigidbody = _PReference.PlayerRigidbody;
        if ( playerRigidbody == null )
        {
            Debug.LogError("Cannot dash because the PlayerRigidbody reference is missing.", this);
            return;
        }

        Vector3 dashDirection = _moveDirection == Vector3.zero
            ? _PReference.PlayerRoot.transform.forward
            : _PReference.PlayerRoot.transform.TransformDirection(_moveDirection.normalized);
        dashDirection.y = 0f;
        dashDirection.Normalize();

        _dashDirection = dashDirection;
        _previousCollisionDetectionMode = playerRigidbody.collisionDetectionMode;

        playerRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        playerRigidbody.linearVelocity = new Vector3(
            dashDirection.x * _dashSpeed,
            playerRigidbody.linearVelocity.y,
            dashDirection.z * _dashSpeed
        );

        _isDashing = true;
        _dashTimeRemaining = _dashDuration;
        _canDash = false;
        RecoveryDash();
    }

    

    async void RecoveryDash()
    {
        _canDash = false;
        await Awaitable.WaitForSecondsAsync(1);
        _canDash = true;
    }
}
