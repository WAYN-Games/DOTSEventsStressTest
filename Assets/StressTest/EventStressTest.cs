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
    I_ParallelWriteToStream_ParallelPollHashMap,
    J_SingleDirectModification,
    K_ParallelWriteToStream_ParallelWriteToHashMap_ParallelPollHashMap,
    L_ParallelWriteToStream_SingleWriteToHashMap_ChunkPool,
    M_ParallelWriteToStream_SingleApplyToAspect_System,
    N_ParallelWriteToStream_ParallelWriteToHashMap_ChunkPool,
    O_ParallelWriteToStream_SingleApplyFromLookup
}

[Serializable]
public struct EventStressTest : IComponentData
{
    public EventType EventType;
    public int HealthEntityCount;
    public int DamagersPerHealths;
}


[Serializable]
public struct IsInitialized : IComponentData
{
}
