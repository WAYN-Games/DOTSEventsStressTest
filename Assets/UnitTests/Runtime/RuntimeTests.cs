using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.PerformanceTesting;
using Unity.Transforms;

/// <summary>
/// To run performance measurements:
/// - open StressTest scene
/// - open: Window => Analysis => Performance Test Report (and enable "Auto Refresh")
/// - open: Window => General => Test Runner
/// Run tests in Test Runner. Then check Performance Test Report window or the saved TestResults.xml (see Console).
/// 
/// Observations:
/// - With Burst compilation disabled, performance (and testing time) is 10 times slower! (12-core CPU)
/// - Jobs => Burst => Safety Checks => Off ... affects some tests more than others! This should be considered in summary.
/// - Jobs => Jobs Debugger ... has practically no effect on measurements
/// - Jobs => Use Job Threads ... as expected: hardly affects "Single" tests, if "off" makes companion Single/Parallel tests perform about the same
/// - [BurstCompile(OptimizeFor = OptimizeFor.Performance)] ... some tests benefit from this, between 0.5-1.0 ms faster (A, D, G, H, I) 
/// </summary>
public class RuntimeTests : ECSTestsFixture
{
	// TODO: move these into a ScriptableObject for easier/faster value tweaking
	//private readonly int _entityCount = 500000;
	private readonly float _healthValue = 1000f;

	private void MeasureWorldUpdate(EventType eventType,int entityCount,int damagersPerHealths)
	{
		// create the HealthPrefab entity
		// NOTE: I found it difficult to verify whether this is 100% identical to the scene's Health prefab entity after conversion.
		// I add the systems from the Archetypes listed for HealthPrefab in play mode as well as the conversion code.
		// Should be identical but it's kind of hard to use the DOTS windows while tests are running whereas with breakpoints the UI is frozen.
		var healthPrefab = m_Manager.CreateEntity(typeof(LocalToWorld), typeof(LocalTransform), typeof(Health), typeof(Prefab));
		m_Manager.AddComponentData(healthPrefab, new Health { Value = _healthValue });
		m_Manager.AddBuffer<DamageEvent>(healthPrefab);
		
		// we need this to spawn all the entities during the first world update
		var spawner = m_Manager.CreateEntity(typeof(EventStressTest));
		m_Manager.SetComponentData(spawner, CreateEventStressTest(eventType,entityCount,damagersPerHealths));

		Measure.Method(() =>
			{
				// update the world once, running all systems once
				m_World.Update();

				// this did not seem to affect measurements (by much) but it's better to be safe,
				// we don't want any jobs to continue running past the measurement cycle
				m_Manager.CompleteAllTrackedJobs();
			})
			// First update creates gazillion entities, don't measure this... (actually: could call Update outside Measure once)
			// Second update seems generally unstable, skip that too ...
			// From third run onwards measurements are stable ...
			.WarmupCount(2)
			// 10 seems enough to get a decently low deviation
			.MeasurementCount(10)
			// only measure once to keep numbers comparable to original forum post
			.IterationsPerMeasurement(1)
			.Run();
	}

	private EventStressTest CreateEventStressTest(EventType eventType,int entityCount,int damagersPerHealths) => new EventStressTest
	{
		EventType = eventType,
		HealthEntityCount = entityCount,
		DamagersPerHealths = damagersPerHealths,
	};

	[Test, Performance]
	public void A_ParallelWriteToStream_ParallelPollBuffers([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.A_ParallelWriteToStream_ParallelPollBuffers,entityCount,damagersPerHealths);

	[Test, Performance]
	public void B_SingleWriteToBuffers_ParallelPollBuffers([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.B_SingleWriteToBuffers_ParallelPollBuffers,entityCount,damagersPerHealths);

	[Test, Performance]
	public void C_ParallelWriteToBuffersECB_ParallelPollBuffers([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.C_ParallelWriteToBuffersECB_ParallelPollBuffers,entityCount,damagersPerHealths);

	[Test, Performance]
	public void D_ParallelWriteToStream_SingleApplyToEntities([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.D_ParallelWriteToStream_SingleApplyToEntities,entityCount,damagersPerHealths);

	[Test, Performance]
	public void E_ParallelWriteToStream_ParallelApplyToEntities([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.E_ParallelWriteToStream_ParallelApplyToEntities,entityCount,damagersPerHealths);

	[Test, Performance]
	public void F_ParallelCreateEventEntities_SingleApplyToEntities([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.F_ParallelCreateEventEntities_SingleApplyToEntities,entityCount,damagersPerHealths);

	[Test, Performance]
	public void G_ParallelWriteToStream_SinglePollList([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.G_ParallelWriteToStream_SinglePollList,entityCount,damagersPerHealths);

	[Test, Performance]
	public void H_ParallelWriteToStream_SinglePollHashMap([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.H_ParallelWriteToStream_SinglePollHashMap,entityCount,damagersPerHealths);

	[Test, Performance]
	public void I_ParallelWriteToStream_ParallelPollHashMap([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.I_ParallelWriteToStream_ParallelPollHashMap,entityCount,damagersPerHealths);

	[Test, Performance]
	public void J_SingleDirectModification([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.J_SingleDirectModification,entityCount,damagersPerHealths);
	
	[Test, Performance]
	public void K_ParallelWriteToStream_ParallelWriteToHashMap_ParallelPollHashMap([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.K_ParallelWriteToStream_ParallelWriteToHashMap_ParallelPollHashMap,entityCount,damagersPerHealths);
	[Test, Performance]
	public void L_ParallelWriteToStream_SingleWriteToHashMap_ChunkPool([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.L_ParallelWriteToStream_SingleWriteToHashMap_ChunkPool,entityCount,damagersPerHealths);
	[Test, Performance]
	public void M_ParallelWriteToStream_SingleApplyToAspect_System([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.M_ParallelWriteToStream_SingleApplyToAspect_System,entityCount,damagersPerHealths);

	[Test, Performance]
	public void N_ParallelWriteToStream_ParallelWriteToHashMap_ChunkPool([Values(0, 10, 100, 1000,10000,100000,500000)] int entityCount,[Values(0, 1, 2, 5,10)] int damagersPerHealths) => MeasureWorldUpdate(EventType.N_ParallelWriteToStream_ParallelWriteToHashMap_ChunkPool,entityCount,damagersPerHealths);

	public override void Setup()
	{
		// custom flag in ECSTestsFixture that allows creating the default world, rather than an empty (no systems) world
		// must be set before base.Setup() !
		CreateDefaultWorld = true;

		// setup the Entities world
		base.Setup();
	}
}
