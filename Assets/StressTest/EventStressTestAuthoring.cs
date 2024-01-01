using Unity.Entities;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EventStressTestAuthoring : MonoBehaviour
{

    public EventStressTest data;
    public GameObject Prefab;
    
    public class EventStressTestBaker : Baker<EventStressTestAuthoring>
    {
        public override void Bake(EventStressTestAuthoring authoring)
        {
           Entity bakedEntity =  GetEntity(TransformUsageFlags.Dynamic);
           authoring.data.HealthPrefab = GetEntity(authoring.Prefab,TransformUsageFlags.Dynamic);
           AddComponent(bakedEntity,authoring.data);
        }
    }


}
