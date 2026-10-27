using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    [Header("References")]
    [SerializeField] PlayerReferences _PReference;

    // [SerializeField] Animator _playerAnimator;
    // [SerializeField] PlayerAttackSystem _attackSystem;
    // [SerializeField] Rigidbody _rb;
    // [SerializeField] PlayerInput _playerInput;
    // [SerializeField] PlayerMovement _playerMovement;
    // [SerializeField] GameObject _weaponObject;

    [Header("Setting")]
    [SerializeField] bool _asWeapon;



    public bool AsWeapon { get => _asWeapon; set => _asWeapon = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _PReference.PlayerInput.OnJump += HandleJump;
        _PReference.PlayerInput.OnAttack += HandleAttack;
    }

    void OnDestroy()
    {
        _PReference.PlayerInput.OnAttack -= HandleAttack;
        _PReference.PlayerInput.OnJump += HandleJump;
    }

    // Update is called once per frame
    void Update()
    {
        _PReference.PlayerAnimator.SetBool("isGrounded", _PReference.PlayerMovement.PredictedIsGrounded);

        _PReference.PlayerAnimator.SetFloat("velocity", _PReference.PlayerMovement.Velocity);

        _PReference.PlayerAnimator.SetBool("AsWeapon", _asWeapon);
        _PReference.WeaponObject.SetActive(_asWeapon);
    }


    void HandleJump()
    {
        _PReference.PlayerAnimator.SetTrigger("jump");
    }

    void HandleAttack()
    {
        if (_PReference.PlayerAttackSystem.CanAttack)
            _PReference.PlayerAnimator.SetTrigger("attack");
    }
}
