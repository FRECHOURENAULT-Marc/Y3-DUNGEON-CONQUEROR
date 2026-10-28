using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Deps = DependenciesInjector;

public class ConditionalBiome : IBiomeBehavior
{
    [SerializeField] List<ConditionalTaskSO> startConditionalTasks;
    [SerializeField] List<ConditionalTaskSO> conditionalTasks;
    
    void Reset()
    {
        conditionalTasks ??= new List<ConditionalTaskSO>();
    }

    void Start()
    {
        foreach (ConditionalTaskSO conditionalTask in startConditionalTasks)
            conditionalTask.Try();
    }

    void FixedUpdate()
    {
        foreach (ConditionalTaskSO conditionalTask in conditionalTasks)
            conditionalTask.Try();
    }
    
    
}
