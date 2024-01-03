using Unity.Entities;
using UnityEngine;

public class EventStressTestAuthoring : MonoBehaviour
{

    public EventStressTest data;
    
    public class EventStressTestBaker : Baker<EventStressTestAuthoring>
    {
        public override void Bake(EventStressTestAuthoring authoring)
        {
           Entity bakedEntity =  GetEntity(TransformUsageFlags.Dynamic);
           AddComponent(bakedEntity,authoring.data);
        }
    }


}
