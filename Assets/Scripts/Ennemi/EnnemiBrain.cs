using System;
using Unity.VisualScripting;
using UnityEngine;

public class EnnemiBrain : MonoBehaviour
{
    [SerializeField] EnnemiReferences _EReference;

    [Space]
    [Header("Logic Setting")]
    [SerializeField] bool _isWakeUp;
    [SerializeField] float _wakeUpDistance;
    [SerializeField] float _attackDistance;

    [Header("Debug")]
    [SerializeField] bool _followPlayer;
    [SerializeField] bool _canAttack;
    [SerializeField] bool _isAlive;


    public bool IsWakeUp
    {
        get => _isWakeUp;
        set => _isWakeUp = value; 
    }

    public bool IsAlive
    {
        get => _isAlive;
        set => _isAlive = value; 
    }


    public event Action<Transform> MoveTo;
    public event Action StopMove;

    public event Action OnAttack;

    void Start()
    {
        _isAlive = true;
        _canAttack = true;
        RunningBrain();
    }



    async void RunningBrain()
    {
        while (_isAlive)
        {
            if (_isWakeUp)
            {
                // Get the distance between the target and him self
                float distanceWithTarget = Vector3.Distance(
                    _EReference.EnnemiRoot.transform.position,
                    _EReference.Player.transform.position
                );

                // check if the target is in the wake up distance
                if (distanceWithTarget < _wakeUpDistance)
                    _followPlayer = true;
                else if (distanceWithTarget > _wakeUpDistance)
                    _followPlayer = false;
                
                if (_followPlayer)
                    MoveTo?.Invoke(_EReference.Player.transform);
                else
                    StopMove?.Invoke();

                // check if the target is in the attack distance
                if (distanceWithTarget <= _attackDistance && _canAttack)
                {
                    StopMove?.Invoke();
                    OnAttack?.Invoke();

                    AttackRecovery();

                    await Awaitable.WaitForSecondsAsync(2f);
                }
            }

            await Awaitable.NextFrameAsync();
        }
    }


    async void AttackRecovery()
    {
        _canAttack = false;
        await Awaitable.WaitForSecondsAsync(4);
        _canAttack = true;
    }


    [ContextMenu("Attck anim")]
    void AttackAnim()
    {
        OnAttack?.Invoke();
    }
}
