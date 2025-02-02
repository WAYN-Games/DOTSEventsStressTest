using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct SingleApplyStreamEventsToEntitiesJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public ComponentLookup<Health> HealthFromEntity;

    public void Execute()
    {
        for (int i = 0; i < StreamDamageEvents.ForEachCount; i++)
        {
            StreamDamageEvents.BeginForEachIndex(i);
            while (StreamDamageEvents.RemainingItemCount > 0)
            {
                StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
                if (HealthFromEntity.HasComponent(damageEvent.Target))
                {
                    Health health = HealthFromEntity[damageEvent.Target];
                    health.Value -= damageEvent.DamageEvent.Value;
                    HealthFromEntity[damageEvent.Target] = health;
                }
            }
            StreamDamageEvents.EndForEachIndex();
        }
    }
}


[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct SingleApplyStreamEventsToEntitiesFromsLookupJob : IJob
{
    public NativeStream.Reader StreamDamageEvents;
    public ComponentLookup<Health> HealthFromEntity;
    [ReadOnly] public ComponentLookup<Damager> DamagerFromEntity;

    public void Execute()
    {
        for (int i = 0; i < StreamDamageEvents.ForEachCount; i++)
        {
            StreamDamageEvents.BeginForEachIndex(i);
            while (StreamDamageEvents.RemainingItemCount > 0)
            {
                StreamDamageEvent damageEvent = StreamDamageEvents.Read<StreamDamageEvent>();
                if (HealthFromEntity.HasComponent(damageEvent.Target))
                {
                    Health health = HealthFromEntity[damageEvent.Target];
                    health.Value -= DamagerFromEntity[damageEvent.DamageEvent.Source].Damage;
                    HealthFromEntity[damageEvent.Target] = health;
                }
            }
            StreamDamageEvents.EndForEachIndex();
        }
    }
}