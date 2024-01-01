using System;
using Unity.Entities;

public enum EventType
{
    None,

    A_ParallelWriteToStream_ParallelPollBuffers,
    B_SingleWriteToBuffers_ParallelPollBuffers,
    C_ParallelWriteToBuffersECB_ParallelPollBuffers,

    D_ParallelWriteToStream_SingleApplyToEntities,
    E_ParallelWriteToStream_ParallelApplyToEntities,

    F_ParallelCreateEventEntities_SingleApplyToEntities,

    G_ParallelWriteToStream_SinglePollList,

    H_ParallelWriteToStream_SinglePollHashMap,
    ParallelWriteToStream_ParallelPollHashMap,

    J_SingleDirectModification,
    K_ParallelWriteToStream_ParallelWriteToHashMap_ParallelPollHashMap,
    W_ParallelWriteToStream_ParallelWriteToHashMap_ChunkPool
}

[Serializable]
public struct EventStressTest : IComponentData
{
    public EventType EventType;
    public Entity HealthPrefab;
    public int HealthEntityCount;
    public float Spacing;
    public int DamagersPerHealths;
}


[Serializable]
public struct IsInitialized : IComponentData
{
}
