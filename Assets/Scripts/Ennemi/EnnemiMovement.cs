using System;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;

public class EnnemiMovement : MonoBehaviour
{
    [SerializeField] EnnemiReferences _EReferences;

    [Space]
    [Header("Movement Settings")]
    [SerializeField] float _speed;
    [SerializeField] float _angularSpeed;
    [SerializeField] float _acceleration;
    [SerializeField] float _stopDistance;

    [SerializeField] bool _onMovement;

    public bool OnMovement { get => _onMovement; }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitNavMehAgent();

        _EReferences.EnnemiBrain.MoveTo += HandleMoveTo;
        _EReferences.EnnemiBrain.StopMove += HandleStopMove;
    }

    void OnDestroy()
    {
        _EReferences.EnnemiBrain.MoveTo += HandleMoveTo;
        _EReferences.EnnemiBrain.StopMove += HandleStopMove;
    }

    void Update()
    {
        float vel = _EReferences.NavMeshAgent.velocity.magnitude;

        // Debug.Log(vel);

        if (vel == 0)
            _onMovement = false;
        else
            _onMovement = true;
    }


    void InitNavMehAgent()
    {
        _EReferences.NavMeshAgent.speed = _speed;
        _EReferences.NavMeshAgent.angularSpeed = _angularSpeed;
        _EReferences.NavMeshAgent.acceleration = _acceleration;
        _EReferences.NavMeshAgent.stoppingDistance = _stopDistance;
    }


    void HandleMoveTo(Transform targetTrasform)
    {
        _EReferences.NavMeshAgent.isStopped = false;
        _EReferences.NavMeshAgent.SetDestination(targetTrasform.position);
    }

    void HandleStopMove()
    {
        _EReferences.NavMeshAgent.isStopped = true;
    }
}
