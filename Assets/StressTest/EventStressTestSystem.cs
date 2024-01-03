using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
public partial struct EventStressTestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EventStressTest>();
    }

    [BurstCompile]
    void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        entityManager.CompleteAllTrackedJobs();

        EventStressTest eventStressTest = SystemAPI.GetSingleton<EventStressTest>();
        Random random = Random.CreateFromIndex(1);

        Entity damagerEntityPrefab = entityManager.CreateEntity();
        entityManager.AddComponent<Damager>(damagerEntityPrefab);
        entityManager.AddComponent<CombatantEntityComponent>(damagerEntityPrefab);
        var damagerEntities = entityManager.Instantiate(damagerEntityPrefab, eventStressTest.HealthEntityCount*eventStressTest.DamagersPerHealths, Allocator.Temp);
        
        
        
        Entity healthEntityPrefab = entityManager.CreateEntity();
        entityManager.AddComponentData(healthEntityPrefab, new Health { Value = 5000 });
        entityManager.AddBuffer<DamageEvent>(healthEntityPrefab);
        entityManager.AddComponent<CombatantEntityComponent>(healthEntityPrefab);
        var healthEntities = entityManager.Instantiate(healthEntityPrefab, eventStressTest.HealthEntityCount, Allocator.Temp);
        


        entityManager.DestroyEntity(damagerEntityPrefab);
        entityManager.DestroyEntity(healthEntityPrefab);
        Shuffle(ref damagerEntities,random);

        for (int i = 0; i < healthEntities.Length ; i++)
        {
            Entity healthEntity = healthEntities[i];
            for (int j = 0; j < eventStressTest.DamagersPerHealths; j++)
            {
                entityManager.SetComponentData(damagerEntities[i*eventStressTest.DamagersPerHealths + j], new Damager()
                {
                    Target = healthEntity,
                    Damage = random.NextFloat()
                });
            }
        }

        state.Enabled = false;
    }
   
    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Shuffle<T>(ref NativeArray<T> list,Random random)  where T: struct
    {  
        var n = list.Length;  
        while (n > 1) {  
            n--;  
            var k = random.NextInt(n + 1);  
            (list[k], list[n]) = (list[n], list[k]);
        }  
    }
}
