using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Input Actions References")]
    [SerializeField] InputActionReference _moveAction; // good
    [SerializeField] InputActionReference _jumpAction; // good
    [SerializeField] InputActionReference _interactAction;
    [SerializeField] InputActionReference _attackAction;
    [SerializeField] InputActionReference _lookAction; // good
    [SerializeField] InputActionReference _sprintAction; // good


    // Events for input actions
    public event Action<Vector2> OnMove;
    public event Action OnJump;
    public event Action OnInteract;
    public event Action OnAttack;

    public event Action<bool> OnSprint;
    public event Action<Vector2> OnLook;


    private void Start()
    {
        _moveAction.action.Enable();
        _jumpAction.action.Enable();
        _interactAction.action.Enable();
        _attackAction.action.Enable();
        _lookAction.action.Enable();
        _sprintAction.action.Enable();


        _moveAction.action.performed += HandleMove;
        _moveAction.action.canceled += HandleMoveCanceled;

        _jumpAction.action.performed += HandleJump;

        _interactAction.action.started += HandleInteract;

        _attackAction.action.performed += HandleAttack;

        
        _lookAction.action.performed += HandleLook;
        _lookAction.action.canceled += HandleLookCanceled;

        _sprintAction.action.performed += HandleSprint;
        _sprintAction.action.canceled += HandleSprintCanceled;
    }

    private void OnDestroy()
    {
        _moveAction.action.performed -= HandleMove;
        _jumpAction.action.performed -= HandleJump;
        _interactAction.action.started -= HandleInteract;
        _attackAction.action.performed -= HandleAttack;
        _moveAction.action.canceled -= HandleMoveCanceled;

        _lookAction.action.performed -= HandleLook;
        _sprintAction.action.performed -= HandleSprint;
        _sprintAction.action.canceled += HandleSprintCanceled;
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        OnMove?.Invoke(moveInput);
    }

    private void HandleMoveCanceled(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(Vector2.zero);
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }

    private void HandleInteract(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }

    private void HandleAttack(InputAction.CallbackContext context)
    {
        OnAttack?.Invoke();
    }

    private void HandleLook(InputAction.CallbackContext context)
    {
        Vector2 lookDirection = context.ReadValue<Vector2>();
        OnLook?.Invoke(lookDirection);
    }

    private void HandleLookCanceled(InputAction.CallbackContext context)
    {
        OnLook?.Invoke(Vector2.zero);
    }

    private void HandleSprint(InputAction.CallbackContext context)
    {
        OnSprint?.Invoke(true);
    }

    private void HandleSprintCanceled(InputAction.CallbackContext context)
    {
        OnSprint?.Invoke(false);
    }
}
