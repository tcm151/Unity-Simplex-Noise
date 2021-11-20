# UnitySimplexNoise
a compact and functional system for generating complex simplex noise layers for procedural generation

the simplest implementation with in-editor control

```
public class ProceduralGeneration : Monobehaviour
{
    // accessible from the inspector
    public Noise.Layer noiseLayer;
    
    public void Awake()
    {
        foreach (var item in collection)
        {
            // this will generate a noise value for the given vector3 in accordance
            // with the noiseLayer properties passed to the function.
            // the entire class is static so you don't need to create random instances
            // of generators across a bunch of different classes
            var noiseValue = Noise.GenerateValue(noiseLayer, vector3);
        {
    }
}
```
