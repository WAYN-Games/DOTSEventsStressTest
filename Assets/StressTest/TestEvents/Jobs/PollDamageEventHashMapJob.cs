using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct SinglePollDamageEventHashMapJob : IJob
{
    [ReadOnly]
    public EntityTypeHandle EntityType;
    [ReadOnly]
    public NativeParallelMultiHashMap<Entity, DamageEvent> DamageEventsMap;
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Health> HealthFromEntity;

    public void Execute()
    {
        if (DamageEventsMap.Count() > 0)
        {
            var enumerator = DamageEventsMap.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Entity targetEntity = enumerator.Current.Key;
                DamageEvent damageEvent = enumerator.Current.Value;
                if (HealthFromEntity.HasComponent(targetEntity))
                {
                    Health health = HealthFromEntity[targetEntity];
                    health.Value -= damageEvent.Value;
                    HealthFromEntity[targetEntity] = health;
                }
            }
        }
    }
}


[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct ParallelPollDamageEventHashMapJob : IJobNativeMultiHashMapVisitKeyValue<Entity, DamageEvent>
{
    [NativeDisableParallelForRestriction]
    public ComponentLookup<Health> HealthFromEntity;

    public void ExecuteNext(Entity targetEntity, DamageEvent damageEvent)
    {
        if (HealthFromEntity.HasComponent(targetEntity))
        {
            Health health = HealthFromEntity[targetEntity];
            health.Value -= damageEvent.Value;
            HealthFromEntity[targetEntity] = health;
        }
    }
}

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct ChunkPoolJob : IJobChunk
{
    [ReadOnly] public NativeParallelMultiHashMap<Entity, DamageEvent> DamageEventsMap;
    public ComponentTypeHandle<Health> HealthTypeHandle;
    public EntityTypeHandle EntityTypeHandle;
    
    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        NativeArray<Health> HealthArray = chunk.GetNativeArray<Health>(HealthTypeHandle);
        NativeArray<Entity> EntityArray = chunk.GetNativeArray(EntityTypeHandle);

        for (int entityIndexInChunk = 0; entityIndexInChunk < chunk.Count; entityIndexInChunk++)
        {
            Entity entity = EntityArray[entityIndexInChunk];
            NativeParallelMultiHashMap<Entity, DamageEvent>.Enumerator events = DamageEventsMap.GetValuesForKey(entity);
            while (events.MoveNext())
            {
                DamageEvent damageEvent = events.Current;
                Health health = HealthArray[entityIndexInChunk];
                health.Value -= damageEvent.Value;
                HealthArray[entityIndexInChunk] = health;
            }
        }

    }
}

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct ClearDamageEventHashMapJob : IJob
{
    public NativeParallelMultiHashMap<Entity, DamageEvent> DamageEventsMap;

    public void Execute()
    {
        if (DamageEventsMap.Count() > 0)
        {
            DamageEventsMap.Clear();
        }
    }
}
