using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class PlayerAttackSystem : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] PlayerReferences _PReference;

    // [SerializeField] Camera _playerCamera;
    // [SerializeField] PlayerInput _playerInput;


    [Header("Setting")]
    [SerializeField] float _raycastDistance;
    [SerializeField] float _sweepAngle;
    [SerializeField] float _maxSweepAngle;
    [SerializeField] float _damage;

    [Header("Weapon")]
    [SerializeField] bool _asWeapon;
    [SerializeField] float _weaponDamage;
    [SerializeField] float _weaponAttackDistance;
    [SerializeField] float _weaponSweepAngle;
    [SerializeField] float _weaponSpeed;

    [Header("Capacity")]
    [SerializeField] float _distanceMultiplier;
    [SerializeField] bool _asunlockDistanceMultiplier;
    [SerializeField] float _damageMultiplier;
    [SerializeField] bool _asUnlockDamageMultiplier;


    [Header("Debug")]
    [SerializeField] bool _showRaycast;
    [SerializeField] bool _canAttack;


    public bool CanAttack { get => _canAttack; }

    public bool AsWeapon { get => _asWeapon; set => _asWeapon = value; }

    public bool AsUnlockDistanceMultiplier
    {
        get => _asunlockDistanceMultiplier;
        set => _asunlockDistanceMultiplier = value;
    }

    public bool AsUnlockDamageMultiplier
    {
        get => _asUnlockDamageMultiplier;
        set => _asUnlockDamageMultiplier = value;
    }

    // Event Action
    public event Action KillProps;


    public float WeaponSweepAngle
    {
        get => _weaponSweepAngle;

        set
        {
            if ( value >= _weaponSweepAngle )
                _weaponSweepAngle = _maxSweepAngle;
            else if ( value <= 0 )
                _weaponSweepAngle = 0;
            else
                _weaponSweepAngle = value;
        }
    }

    public float WeaponDamage
    {
        get => _weaponDamage;

        set => _weaponDamage = value;
    }

    public float WeaponAttackDistance
    {
        get => _weaponAttackDistance;

        set => _weaponAttackDistance = value;
    }

    public float WeaponSpeed
    {
        get => _weaponSpeed;

        set => _weaponSpeed = value;
    }

    void Awake()
    {
        _canAttack = true;
    }


    void Start()
    {
        _PReference.PlayerInput.OnAttack += HandleAttack;
    }

    void OnDestroy()
    {
        _PReference.PlayerInput.OnAttack -= HandleAttack;
    }


    void Update()
    {
        if (_showRaycast)
        {
            float sweepAngle = _asWeapon ? _weaponSweepAngle : _sweepAngle;
            float attackDistance = _asWeapon ? _weaponAttackDistance : _raycastDistance;

            int nbRay = (int) Math.Floor( Math.Floor( sweepAngle  * 2 ) / 2 );

            for ( int i = 0; i < nbRay; i++ )
            {
                float angle = nbRay <= 1
                    ? 0f
                    : Mathf.Lerp(-sweepAngle / 2f, sweepAngle / 2f, (float)i / (nbRay - 1));

                Vector3 direction =
                    Quaternion.AngleAxis(angle, _PReference.PlayerCamera.transform.up) *
                    _PReference.PlayerCamera.transform.forward;

                Debug.DrawRay(
                    _PReference.PlayerCamera.transform.position,
                    direction * attackDistance * (float) (_asunlockDistanceMultiplier ? _distanceMultiplier : 1),
                    Color.red
                );
            }
        }
    }


    async void HandleAttack()
    {
        if( _canAttack )
        {
            List<GameObject> ennemiHits = new List<GameObject>();

            float sweepAngle = _asWeapon ? _weaponSweepAngle : _sweepAngle;
            float attackDistance = _asWeapon ? _weaponAttackDistance : _raycastDistance;

            // Wait for the start of the animation
            await Awaitable.WaitForSecondsAsync(0.1f / _weaponSpeed);

            // Start the recovery timer
            AttackRecovery();


            // Wait for the moment where the animation attack
            await Awaitable.WaitForSecondsAsync(0.5f / _weaponSpeed);

            int nbRay = (int) Math.Floor( Math.Floor( sweepAngle  * 2 ) / 2 );

            for ( int i = 0; i < nbRay; i++ )
            {
                float angle = nbRay <= 1
                    ? 0f
                    : Mathf.Lerp(-sweepAngle / 2f, sweepAngle / 2f, (float)i / (nbRay - 1));

                Vector3 direction =
                    Quaternion.AngleAxis(angle, _PReference.PlayerCamera.transform.up) *
                    _PReference.PlayerCamera.transform.forward;

                
                RaycastHit[] Hits;
                Hits = Physics.RaycastAll(
                    _PReference.PlayerCamera.transform.position,
                    direction,
                    attackDistance * (float) (_asunlockDistanceMultiplier ? _distanceMultiplier : 1)
                );

                foreach( RaycastHit hit in Hits )
                {
                    GameObject ctxObj = hit.collider.gameObject;
                    
                    
                    // Check if an object is an ennemi
                    if ( ctxObj.GetComponent<EnnemiTag>() )
                    {
                        if ( ennemiHits.IndexOf(ctxObj) != -1 )
                            continue;
                        
                        ennemiHits.Add(ctxObj);
                        
                        Debug.Log("Ennemi Tag found on obj");
                        ComponentGetter OUT;
                        ctxObj.TryGetComponent<ComponentGetter>(out OUT);
                        AttackEnnemi(OUT.HealthController);
                    }
                }
            }

        }
    }


    void AttackEnnemi(HealthController ctx)
    {
        if (ctx != null)
        {
            ctx.PropsDeath += HandleKillProps;

            float damage = _asWeapon ? _weaponDamage : _damage;

            Debug.Log($"Attack ennemi with {damage} damage");

            ctx.GetAttacked(damage);

            ctx.PropsDeath -= HandleKillProps;
        }
    }

    void HandleKillProps()
    {
        Debug.Log("Killed props");
        KillProps?.Invoke();
    }


    async void AttackRecovery()
    {
        _canAttack = false;
        await Awaitable.WaitForSecondsAsync(1 / _weaponSpeed);
        _canAttack = true;
    }

}
