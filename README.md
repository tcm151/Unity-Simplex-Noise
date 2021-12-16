# UnitySimplexNoise
a compact and functional system for generating complex simplex noise layers for procedural generation

the simplest implementation with in-editor control

```cs
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

```cs
//> NOISE TYPES
public enum Type { Simple, Rigid, Brownian, Swiss };

//> NOISE LAYER SETTINGS
[System.Serializable] public class Layer
{
    [Header("Properties")]
    public bool enabled = true;
    public bool subtract;
    public Type noiseType;
    
    [Header("Masking")]
    public bool useMask;
    public Layer mask;

    public SimplexGenerator generator = new SimplexGenerator();

    [Header("Settings")]
    [Range(01, 004)] public int octaves = 3;
    [Range(00, 001)] public float strength = 0.25f;
    [Range(00, .1f)] public float baseRoughness = 0.001f;
    [Range(00, 010)] public float roughness = 4f;
    [Range(00, 002)] public float persistence = 0.5f;
    [Range(00, 001)] public float threshold = 0.5f;
    public Vector3 offset = Vector3.zero;
}
```
