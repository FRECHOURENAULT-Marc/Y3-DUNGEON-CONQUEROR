using Unity.VisualScripting;
using UnityEngine;

public class PlayerReferences : MonoBehaviour
{
    [Header("GameObject")]
    [SerializeField] GameObject _playerRoot;
    [SerializeField] GameObject _playerArms;
    [SerializeField] GameObject _weaponObject;

    [Header("Component")]
    [SerializeField] Rigidbody _playerRB;
    [SerializeField] Camera _playerCamera;
    [SerializeField] Animator _playerAnimator;

    [Header("Script")]
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] PlayerAttackSystem _attackSystem;
    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] PlayerXPSystem _playerXPSystem;
    [SerializeField] MouseController _mouseController;


    // Getter GameObject
    public GameObject PlayerRoot { get => _playerRoot; }
    public GameObject PlayerArms { get => _playerArms; }
    public GameObject WeaponObject { get => _weaponObject; }

    // Getter Component
    public Rigidbody PlayerRigidbody { get => _playerRB; }
    public Camera PlayerCamera { get => _playerCamera; }
    public Animator PlayerAnimator { get => _playerAnimator; }

    // Getter Script
    public PlayerInput PlayerInput { get => _playerInput; }
    public PlayerAttackSystem PlayerAttackSystem { get => _attackSystem; }
    public PlayerMovement PlayerMovement { get => _playerMovement; }
    public PlayerXPSystem PlayerXPSystem { get => _playerXPSystem; }
    public MouseController MouseController { get => _mouseController; }
}
