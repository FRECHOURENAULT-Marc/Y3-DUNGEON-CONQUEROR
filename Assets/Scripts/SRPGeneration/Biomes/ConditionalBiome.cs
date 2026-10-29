using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Deps = DependenciesInjector;

public class ConditionalBiome : BiomeBehavior
{
    [SerializeField] List<ConditionalTask> startConditionalTasks;
    [SerializeField] List<ConditionalTask> conditionalTasks;
    
    void Reset()
    {
        conditionalTasks ??= new List<ConditionalTask>();
    }

    void Start()
    {
        InitializeConditionalTasks(startConditionalTasks);
        InitializeConditionalTasks(conditionalTasks);
        
        foreach (ConditionalTask conditionalTask in startConditionalTasks)
            conditionalTask.Try();
    }

    void FixedUpdate()
    {
        foreach (ConditionalTask conditionalTask in conditionalTasks)
            conditionalTask.Try();
    }
    
    
    void InitializeConditionalTasks(List<ConditionalTask> condTasks)
    {
        foreach (ConditionalTask conditionalTask in condTasks)
        {
            InitializeTasks(conditionalTask._tasks);
            InitializeConditions(conditionalTask._conditions);
        }
    }
    
}
