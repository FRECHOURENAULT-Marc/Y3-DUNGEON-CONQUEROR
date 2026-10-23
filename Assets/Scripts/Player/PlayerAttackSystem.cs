using System;
using System.Xml.Serialization;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class PlayerAttackSystem : MonoBehaviour
{
    
    [Header("References")]
    [SerializeField] Camera _playerCamera;
    [SerializeField] PlayerInput _playerInput;


    [Header("Setting")]
    [SerializeField] float _raycastDistance;
    [SerializeField] float _sweepAngle;
    [SerializeField] float _maxSweepAngle;


    [Header("Debug")]
    [SerializeField] bool _canAttack;


    public bool CanAttack { get => _canAttack; }

    public float SweepAngle
    {
        get => _sweepAngle;

        set
        {
            if ( value >= _maxSweepAngle )
                _sweepAngle = _maxSweepAngle;
            else if ( value <= 0 )
                _sweepAngle = 0;
            else
                _sweepAngle = value;
        }
    }

    void Awake()
    {
        _canAttack = true;
    }


    void Start()
    {
        _playerInput.OnAttack += HandleAttack;
    }

    void OnDestroy()
    {
        _playerInput.OnAttack -= HandleAttack;
    }


    void Update()
    {
        int nbRay = (int) Math.Floor( Math.Floor( _sweepAngle  * 2 ) / 2 );

        for ( int i = 0; i < nbRay; i++ )
        {
            float angle = nbRay <= 1
                ? 0f
                : Mathf.Lerp(-_sweepAngle / 2f, _sweepAngle / 2f, (float)i / (nbRay - 1));

            Vector3 direction =
                Quaternion.AngleAxis(angle, _playerCamera.transform.up) *
                _playerCamera.transform.forward;

            Debug.DrawRay(
                _playerCamera.transform.position,
                direction * _raycastDistance,
                Color.red
            );
        }
    }


    async void HandleAttack()
    {
        if( _canAttack )
        {

            // Wait for the start of the animation
            await Awaitable.WaitForSecondsAsync(0.1f);

            // Start the recovery timer (set to 3s)
            AttackRecovery();


            // Wait for the moment where the animation attack
            await Awaitable.WaitForSecondsAsync(0.5f);

            int nbRay = (int) Math.Floor( Math.Floor( _sweepAngle  * 2 ) / 2 );

            for ( int i = 0; i < nbRay; i++ )
            {
                float angle = nbRay <= 1
                    ? 0f
                    : Mathf.Lerp(-_sweepAngle / 2f, _sweepAngle / 2f, (float)i / (nbRay - 1));

                Vector3 direction =
                    Quaternion.AngleAxis(angle, _playerCamera.transform.up) *
                    _playerCamera.transform.forward;

                
                RaycastHit[] Hits;
                Hits = Physics.RaycastAll(
                    _playerCamera.transform.position,
                    direction,
                    _raycastDistance
                );

                foreach( RaycastHit hit in Hits )
                {
                    GameObject ctxObj = hit.collider.gameObject;
                    
                    
                    // Check if an object is an ennemi
                    if ( ctxObj.GetComponent<EnnemiTag>() )
                    {
                        Debug.Log("Ennemi Tag found on obj");
                        ComponentGetter OUT;
                        ctxObj.TryGetComponent<ComponentGetter>(out OUT);
                        AttackEnnemi(OUT.HealthController);
                        return;
                    }
                }
            }

        }
    }


    void AttackEnnemi(HealthController ctx)
    {
        if (ctx != null)
        {
            ctx.GetAttacked(10);
        }
    }


    async void AttackRecovery()
    {
        _canAttack = false;
        await Awaitable.WaitForSecondsAsync(2);
        _canAttack = true;
    }

}
