using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    [Header("References")]
    [SerializeField] Animator _playerAnimator;
    [SerializeField] Rigidbody _rb;
    [SerializeField] PlayerInput _playerInput;
    [SerializeField] PlayerMovement _playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerInput.OnJump += HandleJump;
    }

    void OnDestroy()
    {
        _playerInput.OnJump += HandleJump;
    }

    // Update is called once per frame
    void Update()
    {
        _playerAnimator.SetBool("isGrounded", _playerMovement.PredictedIsGrounded);

        _playerAnimator.SetFloat("velocity", _playerMovement.Velocity);

    }


    void HandleJump()
    {
        _playerAnimator.SetTrigger("jump");
    }
}
