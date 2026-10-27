using System;
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


    [Header("Debug")]
    [SerializeField] bool _canAttack;


    public bool CanAttack { get => _canAttack; }

    // Event Action
    public event Action KillProps;


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
        _PReference.PlayerInput.OnAttack += HandleAttack;
    }

    void OnDestroy()
    {
        _PReference.PlayerInput.OnAttack -= HandleAttack;
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
                Quaternion.AngleAxis(angle, _PReference.PlayerCamera.transform.up) *
                _PReference.PlayerCamera.transform.forward;

            Debug.DrawRay(
                _PReference.PlayerCamera.transform.position,
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
                    Quaternion.AngleAxis(angle, _PReference.PlayerCamera.transform.up) *
                    _PReference.PlayerCamera.transform.forward;

                
                RaycastHit[] Hits;
                Hits = Physics.RaycastAll(
                    _PReference.PlayerCamera.transform.position,
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
            ctx.PropsDeath += HandleKillProps;

            ctx.GetAttacked(_damage);

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
        await Awaitable.WaitForSecondsAsync(2);
        _canAttack = true;
    }

}
