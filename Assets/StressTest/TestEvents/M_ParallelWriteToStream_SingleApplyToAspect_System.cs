using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public partial struct M_ParallelWriteToStream_SingleApplyToAspect_System : ISystem
{
    private EventAspect.Lookup _eventAspectLookup;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EventStressTest>();
        _query = state.GetEntityQuery(typeof(Damager));
        _eventAspectLookup = new EventAspect.Lookup(ref state);
        _entityType = state.GetEntityTypeHandle();
        _damagerType = state.GetComponentTypeHandle<Damager>(true);
    }

    public NativeStream PendingStream;
    private EntityQuery _query;
    private EntityTypeHandle _entityType;
    private ComponentTypeHandle<Damager> _damagerType;

    void OnDestroy(ref SystemState state)
    {
        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
    }

    void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<EventStressTest>())
            return;

        if (SystemAPI.GetSingleton<EventStressTest>().EventType != EventType.M_ParallelWriteToStream_SingleApplyToAspect_System)
            return;

     

        if (PendingStream.IsCreated)
        {
            PendingStream.Dispose();
        }
        PendingStream = new NativeStream(_query.CalculateChunkCount(), Allocator.TempJob);

        _entityType.Update(ref state);
        _damagerType.Update(ref state);
        
        state.Dependency = new DamagersWriteToStreamJob
        {
            EntityType = _entityType,
            DamagerType = _damagerType,
            StreamDamageEvents = PendingStream.AsWriter(),
        }.ScheduleParallel(_query, state.Dependency);

        _eventAspectLookup.Update(ref state);
        
        state.Dependency = new SingleApplyStreamEventsToAspectJob()
        {
            StreamDamageEvents = PendingStream.AsReader(),
            EventAspectLookup = _eventAspectLookup
        }.Schedule(state.Dependency);
        state.Dependency = PendingStream.Dispose(state.Dependency);
    }
}
