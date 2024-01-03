using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;

public class UIManager : MonoBehaviour
{
    public TMPro.TMP_Dropdown Approach;

    public Slider NbTarget;
    public TMP_Text NbTargetTxt;
    public Slider NbAttacker;
    public TMP_Text NbAttackerTxt;
    public TMP_Text Current;
    public Slider NbWorker;
    public TMP_Text NbWorkerTxt;

    public AnimationCurve curve;
    // Start is called before the first frame update
    void Start()
    {
        List<TMPro.TMP_Dropdown.OptionData> options = new List<TMPro.TMP_Dropdown.OptionData>();
        foreach (var option in Enum.GetValues(typeof(EventType)))
        {
            
            options.Add((new TMPro.TMP_Dropdown.OptionData($"{option}")));
        }
        Approach.options = options;
        DestroyAllEntities();
    }
    public void DestroyAllEntities()
    {
        World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery);
        World.DisposeAllWorlds();
        DefaultWorldInitialization.Initialize("Base World", false);
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(World.DefaultGameObjectInjectionWorld);
    }
    public void UpdateNbTarget()
    {
        NbTargetTxt.text = $"({(int)(curve.Evaluate(NbTarget.value)*5000)*100})";
    }
    
    public void UpdateNbWorker()
    {
        NbWorkerTxt.text = $"({(int)(NbWorker.value * JobsUtility.JobWorkerMaximumCount)})";
    }
    
    public void UpdateTestScenario()
    {
        DestroyAllEntities();
        JobsUtility.JobWorkerCount = (int)(NbWorker.value * JobsUtility.JobWorkerMaximumCount);
        EntityManager EntityManager= World.DefaultGameObjectInjectionWorld.EntityManager;
        Entity e = EntityManager.CreateEntity();
        var targetCount = (int)(curve.Evaluate(NbTarget.value) * 5000) * 100;
        var attackerCount = (int)NbAttacker.value;
        EntityManager.AddComponentData(e, new EventStressTest()
        {
            EventType = (EventType)Approach.value,
            
            HealthEntityCount = targetCount,
            DamagersPerHealths    = attackerCount,
        });
        
        Current.text =
            $"Simulating {(EventType)Approach.value} with {targetCount} being attacked by {attackerCount} attackers each resulting in {targetCount * attackerCount} damage event per frame across {JobsUtility.JobWorkerCount}.";
    }
    
    public void UpdateNbAttacker()
    {
        NbAttackerTxt.text = $"({(int)NbAttacker.value})";
    }

}
