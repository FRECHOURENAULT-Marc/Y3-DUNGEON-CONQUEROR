using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    [Header("References")]
    [SerializeField] Animator _playerAnimator;
    [SerializeField] PlayerAttackSystem _attackSystem;
    [SerializeField] Rigidbody _rb;
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] GameObject _weaponObject;

    [Header("Setting")]
    [SerializeField] bool _asWeapon;



    public bool AsWeapon { get => _asWeapon; set => _asWeapon = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerInput.OnJump += HandleJump;
        _playerInput.OnAttack += HandleAttack;
    }

    void OnDestroy()
    {
        _playerInput.OnAttack -= HandleAttack;
        _playerInput.OnJump += HandleJump;
    }

    // Update is called once per frame
    void Update()
    {
        _playerAnimator.SetBool("isGrounded", _playerMovement.PredictedIsGrounded);

        _playerAnimator.SetFloat("velocity", _playerMovement.Velocity);

        _playerAnimator.SetBool("AsWeapon", _asWeapon);
        _weaponObject.SetActive(_asWeapon);
    }


    void HandleJump()
    {
        _playerAnimator.SetTrigger("jump");
    }

    void HandleAttack()
    {
        if (_attackSystem.CanAttack)
            _playerAnimator.SetTrigger("attack");
    }
}
