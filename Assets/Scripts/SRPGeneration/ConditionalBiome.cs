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

    protected override void Start()
    {
        base.Start();
        
        InitializeConditionalTasks(startConditionalTasks);
        InitializeConditionalTasks(conditionalTasks);
        
        foreach (ConditionalTask conditionalTask in startConditionalTasks)
            conditionalTask.Try();
    }

    protected override void FixedUpdate()
    {
        if (room.IsFinished)
            return;
        
        base.FixedUpdate();
        
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
