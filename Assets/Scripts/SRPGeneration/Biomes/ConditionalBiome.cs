using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Deps = DependenciesInjector;

public class ConditionalBiome : BiomeBehavior
{
    [SerializeField] List<ConditionalTaskSO> startConditionalTasks;
    [SerializeField] List<ConditionalTaskSO> conditionalTasks;
    
    void Reset()
    {
        conditionalTasks ??= new List<ConditionalTaskSO>();
    }

    void Start()
    {
        InitializeConditionalTasks(startConditionalTasks);
        InitializeConditionalTasks(conditionalTasks);
        
        foreach (ConditionalTaskSO conditionalTask in startConditionalTasks)
            conditionalTask.Try();
    }

    void FixedUpdate()
    {
        foreach (ConditionalTaskSO conditionalTask in conditionalTasks)
            conditionalTask.Try();
    }
    
    
    void InitializeConditionalTasks(List<ConditionalTaskSO> condTasks)
    {
        foreach (ConditionalTaskSO conditionalTask in condTasks)
        {
            InitializeTasks(conditionalTask._tasks);
            InitializeConditions(conditionalTask._conditions);
        }
    }
    
}
