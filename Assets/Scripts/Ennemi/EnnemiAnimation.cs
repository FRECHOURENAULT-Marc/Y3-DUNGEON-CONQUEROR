using Unity.VisualScripting;
using UnityEngine;

public class EnnemiAnimation : MonoBehaviour
{
    [SerializeField] EnnemiReferences _EReferences;

    // Update is called once per frame
    void Update()
    {
        bool onwalk = _EReferences.EnnemiMovement.OnMovement;

        _EReferences.EnnemiAnimator.SetBool("isWalking", onwalk);
    }

    void Start()
    {
        _EReferences.EnnemiBrain.OnAttack += HandleAttack;
    }

    void Oestroy()
    {
        _EReferences.EnnemiBrain.OnAttack += HandleAttack;
    }

    void HandleAttack() => _EReferences.EnnemiAnimator.SetTrigger("Attack");
}
