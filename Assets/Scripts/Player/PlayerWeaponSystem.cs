using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerWeaponSystem : MonoBehaviour
{
    [SerializeField] PlayerReferences _PReferences;

    [Header("Setting")]
    [SerializeField] bool _asWeapon;
    [SerializeField] GameObject _activeWeaponObject;
    [SerializeField] List<SOWeapon> _listOfWeapon;

    SOWeapon _activeWeapon;

    void Start()
    {
        _PReferences.PlayerInput.DropWeapon += HandleDropWeapon;

        DisableWeapon();
    }

    void OnDestroy()
    {
        _PReferences.PlayerInput.DropWeapon -= HandleDropWeapon;
    }


    void Update()
    {
        _PReferences.WeaponObject.SetActive(_asWeapon);

        if ( _asWeapon )
        {
            
        }
    }


    [ContextMenu("Equipe weapon")]
    void sword()
    {
        EnableWeapon(_listOfWeapon[2]);
    }

    [ContextMenu("UnEquipe weapon")]
    void sworddrop()
    {
        DropWeaponOnFloor(_activeWeapon);
    }


    void EnableWeapon(SOWeapon weapon)
    {
        if (_activeWeapon != null) DropWeaponOnFloor(_activeWeapon);

        _asWeapon = true;

        _activeWeapon = weapon;

        _PReferences.PlayerAnimation.AsWeapon = true;
        _PReferences.PlayerAttackSystem.AsWeapon = true;

        _PReferences.PlayerAttackSystem.WeaponAttackDistance = _activeWeapon.weaponAtttackDistance;
        _PReferences.PlayerAttackSystem.WeaponDamage = _activeWeapon.weaponDamage;
        _PReferences.PlayerAttackSystem.WeaponSweepAngle = _activeWeapon.weaponSweepAngle;

        // initiate weapon
        GameObject obj = Instantiate(_activeWeapon.weaponPrefabe);
        obj.transform.position = _PReferences.WeaponObject.transform.position;
        obj.transform.localScale = _activeWeapon.scale;

        // create constraint to the hand
        ParentConstraint constraint = obj.AddComponent<ParentConstraint>();

        ConstraintSource hand = new ConstraintSource();
        hand.sourceTransform = _PReferences.WeaponObject.transform;
        hand.weight = 1;
        constraint.AddSource(hand);

        // Active constraint
        constraint.constraintActive = true;

        constraint.SetTranslationOffset(0, _activeWeapon.positionOffset);
        constraint.SetRotationOffset(0, _activeWeapon.rotationOffset);

        _activeWeaponObject = obj;

        
        _PReferences.WeaponObject.SetActive(_asWeapon);

        _PReferences.PlayerAnimator.SetFloat("AttackSpeed", _activeWeapon.weaponAttackSpeed);
    }

    void DisableWeapon()
    {
        _activeWeapon = null;
        _activeWeaponObject = null;

        _asWeapon = false;
        _PReferences.PlayerAttackSystem.AsWeapon = false;
        _PReferences.PlayerAnimation.AsWeapon = false;

        _PReferences.WeaponObject.SetActive(_asWeapon);
        _PReferences.PlayerAnimator.SetFloat("AttackSpeed", 1);
    }

    void HandleDropWeapon()
    {
        DropWeaponOnFloor(_activeWeapon);
    }

    void DropWeaponOnFloor(SOWeapon weapon)
    {
        // Logic of drop item

        DestroyImmediate(_activeWeaponObject);
        _asWeapon = false;
        DisableWeapon();
    }
}
