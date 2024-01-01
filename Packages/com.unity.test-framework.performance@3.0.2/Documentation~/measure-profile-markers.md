# Measure.ProfilerMarkers()

Used to record profiler markers. Profiler marker timings are recorded automatically and sampled within the scope of the `using` statement. Names should match profiler marker labels. Profiler markers are sampled once per frame. Sampling the same profiler marker per frame will result in the sum of all invocations. Note that deep and editor profiling is not available. Profiler markers created using `Profiler.BeginSample()` are not supported, switch to `ProfilerMarker` if possible. 

You can also create your own SampleGroups, specifying a custom name and the measurement units you want your results in, see [example 2](#example-2-measuring-profiler-markers-in-a-scope-with-custom-samplegroups). 

## Limitations

Currently not supported in editor tests when in edit mode.

#### Example: Measuring profiler markers in a scope

``` csharp
[Test, Performance]
public void Test()
{
    string[] markers =
    {
        "Instantiate",
        "Instantiate.Copy",
        "Instantiate.Produce",
        "Instantiate.Awake"
    };

    using(Measure.ProfilerMarkers(markers))
    {
        ...
    }
}
```

#### Example 2: Measuring profiler markers in a scope with custom SampleGroups

``` csharp

[UnityTest, Performance]
public IEnumerator Test()
{
    var sampleGroups = new []{
        new SampleGroup("Instantiate", SampleUnit.Second), 
        new SampleGroup("Instantiate.Copy", SampleUnit.Nanosecond),
        new SampleGroup("Instantiate.Awake", SampleUnit.Microsecond)
    };

    using (Measure.ProfilerMarkers(sampleGroups))
    {
        ...
    }
}
```