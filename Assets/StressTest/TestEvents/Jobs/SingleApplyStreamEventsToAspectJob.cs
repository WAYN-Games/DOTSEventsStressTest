using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct SingleApplyStreamEventsToAspectJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public EventAspect.Lookup EventAspectLookup;

    public void Execute()
    {
        for (int i = 0; i < StreamDamageEvents.ForEachCount; i++)
        {
            StreamDamageEvents.BeginForEachIndex(i);
            while (StreamDamageEvents.RemainingItemCount > 0)
            {
                StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
                EventAspect.Execute(EventAspectLookup,damageEvent.DamageEvent.Source,damageEvent.Target);
            }
            StreamDamageEvents.EndForEachIndex();
        }
    }
}

public struct CombatantEntityComponent : IComponentData, IEnableableComponent
{
    
}

public readonly partial struct EventAspect : IAspect
{
    public readonly RefRO<CombatantEntityComponent> TagComponent;

    [Optional] public readonly RefRO<Damager> Damager;
    [Optional] public readonly RefRW<Health> Health;
    
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public static void Execute(EventAspect.Lookup lookup, Entity emitter, Entity target)
    {
        // Get Emitter Data
        if (!lookup[emitter].Damager.IsValid) return;
        ref readonly Damager damager = ref lookup[emitter].Damager.ValueRO;

        // Get Target Data
        if (!lookup[target].Health.IsValid) return;
        ref Health health = ref lookup[target].Health.ValueRW;
        
        // Apply rules
        health.Value -= damager.Damage;

    }
}

