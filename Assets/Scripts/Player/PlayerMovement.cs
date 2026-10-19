using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("References")]
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] GameObject _playerRoot;
    [SerializeField] Rigidbody _playerRB;
    [SerializeField] Camera _playerCamera;
    [SerializeField] GameObject _playerArms;

    [Space]
    [SerializeField] Vector3 _cameraOffset;



    [Space]
    [Header("Movement Setting")]
    [SerializeField] float _jumpRaycastDistance;
    [SerializeField] Vector2 _lookDirection;
    [SerializeField] float _movementSpeed;
    [SerializeField] float _sprintAdditionnalSpeed;
    [SerializeField] float _jumpForce;
    [SerializeField] float _lookMultiplier;

    float _cameraPitch;
    float _cameraYawOffset;

    [Space]
    [Header("Debug Info")]
    [SerializeField] Vector3 _moveDirection;
    [ShowNonSerializedField] bool _isGrounded;
    [ShowNonSerializedField] bool _isOnSprint;






    void Start()
    {
        _cameraPitch = Mathf.DeltaAngle(0f, _playerCamera.transform.eulerAngles.x);
        _cameraYawOffset = Mathf.DeltaAngle(
            _playerRoot.transform.eulerAngles.y,
            _playerCamera.transform.eulerAngles.y
        );

        _playerInput.OnMove += HandleMove;
        _playerInput.OnLook += HandleLook;

        _playerInput.OnSprint += HandleSprint;
        _playerInput.OnJump += HandleJump;
    }

    void OnDestroy()
    {
        _playerInput.OnLook -= HandleLook;
        _playerInput.OnMove -= HandleMove;
        _playerInput.OnSprint -= HandleSprint;
        _playerInput.OnJump -= HandleJump;
    }

    // Update is called once per frame
    void Update()
    {
        // PlayerPosition
        _playerRoot.transform.Translate(
            _moveDirection * 
            ( _movementSpeed + 
            ( _isOnSprint ? _sprintAdditionnalSpeed : 0 ) ) * 
            Time.deltaTime
        );

        // Set camera to player position
        _playerCamera.transform.position = _playerRoot.transform.position + _cameraOffset;

        // PlayerRotation
        Vector2 lookDelta = _lookDirection;
        _lookDirection = Vector2.zero;

        _playerRoot.transform.Rotate(
            Vector3.up,
            lookDelta.x * 0.1f * _lookMultiplier,
            Space.World
        );

        _cameraPitch = Mathf.Clamp(
            _cameraPitch - lookDelta.y * 0.1f * _lookMultiplier,
            -59f,
            59f
        );
        _playerCamera.transform.rotation = Quaternion.Euler(
            _cameraPitch,
            _playerRoot.transform.eulerAngles.y + _cameraYawOffset,
            0f
        );

        // _isgrounded
        _isGrounded = Physics.Raycast(_playerRoot.transform.position, Vector3.down, _jumpRaycastDistance);
        Debug.DrawRay(_playerRoot.transform.position, Vector3.down * _jumpRaycastDistance, Color.red);
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
        if (_playerRB != null && _isGrounded)
        {
            if(_playerRB.linearVelocity.y <= 0)
                _playerRB.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }
}
