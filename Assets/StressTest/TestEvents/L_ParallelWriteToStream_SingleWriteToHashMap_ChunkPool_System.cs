using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial class L_ParallelWriteToStream_SingleWriteToHashMap_ChunkPool_System : SystemBase
{
    public NativeStream PendingStream;
    public NativeParallelMultiHashMap<Entity, DamageEvent> DamageEventsMap;
    public ComponentTypeHandle<Health> HealthTypeHandle;
    public EntityTypeHandle EntityTypeHandle;
    public EntityQuery TargetQuery; 

    protected override void OnCreate()
    {
        base.OnCreate();
        DamageEventsMap = new NativeParallelMultiHashMap<Entity, DamageEvent>(500000, Allocator.Persistent);
        TargetQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<Health>().Build(EntityManager);
        HealthTypeHandle = GetComponentTypeHandle<Health>(false);
        EntityTypeHandle = GetEntityTypeHandle();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
        if (DamageEventsMap.IsCreated)
        {
            DamageEventsMap.Dispose();
        }
    }

    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<EventStressTest>())
            return;

        if (SystemAPI.GetSingleton<EventStressTest>().EventType != EventType.L_ParallelWriteToStream_SingleWriteToHashMap_ChunkPool)
            return;

        EntityQuery damagersQuery = GetEntityQuery(typeof(Damager));

        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
        PendingStream = new NativeStream(damagersQuery.CalculateChunkCount(), Allocator.TempJob);

        Dependency = new DamagersWriteToStreamJob
        {
            EntityType = GetEntityTypeHandle(),
            DamagerType = GetComponentTypeHandle<Damager>(true),
            StreamDamageEvents = PendingStream.AsWriter(),
        }.ScheduleParallel(damagersQuery, Dependency);

        Dependency = new SingleWriteStreamEventsToHashMapJob
        {
            StreamDamageEvents = PendingStream.AsReader(),
            DamageEventsMap = DamageEventsMap,
        }.Schedule(Dependency);

        HealthTypeHandle.Update(this);
        EntityTypeHandle.Update(this);
        
        Dependency = new ChunkPoolJob()
        {
            DamageEventsMap = DamageEventsMap,
            EntityTypeHandle = EntityTypeHandle,
            HealthTypeHandle = HealthTypeHandle
        }.ScheduleParallel(TargetQuery, Dependency);

        Dependency = new ClearDamageEventHashMapJob
        {
            DamageEventsMap = DamageEventsMap,
        }.Schedule(Dependency);
    }
}
