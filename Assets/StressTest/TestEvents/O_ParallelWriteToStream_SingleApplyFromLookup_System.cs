using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial class O_ParallelWriteToStream_SingleApplyFromLookup_System : SystemBase
{
    public NativeStream PendingStream;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
    }

    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<EventStressTest>())
            return;

        if (SystemAPI.GetSingleton<EventStressTest>().EventType != EventType.O_ParallelWriteToStream_SingleApplyFromLookup)
            return;

        EntityQuery damagersQuery = GetEntityQuery(typeof(Damager));

        PendingStream = new NativeStream(damagersQuery.CalculateChunkCount(), Allocator.TempJob);

        Dependency = new DamagersWriteToStreamJob
        {
            EntityType = GetEntityTypeHandle(),
            DamagerType = GetComponentTypeHandle<Damager>(true),
            StreamDamageEvents = PendingStream.AsWriter(),
        }.ScheduleParallel(damagersQuery, Dependency);

        Dependency = new SingleApplyStreamEventsToEntitiesFromsLookupJob
        {
            StreamDamageEvents = PendingStream.AsReader(),
            HealthFromEntity = GetComponentLookup<Health>(false),
            DamagerFromEntity = GetComponentLookup<Damager>(true),
        }.Schedule(Dependency);
        Dependency = PendingStream.Dispose(Dependency);
    }
}
