using UnityEngine;

/// <summary>
/// Particulas con distintos parametros
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ParticleSystem))]
public sealed class VoxelFog : MonoBehaviour
{
    [Header("Fog appearance")]
    [SerializeField] private Color fogColor = new Color(0.42f, 0.48f, 0.62f, 0.52f);
    [SerializeField, Range(0f, 1f)] private float density = 0.85f;
    [SerializeField] private Material fogMaterial;

    [Header("Fog area")]
    [SerializeField] private Vector3 areaSize = new Vector3(12f, 1.6f, 12f);
    [SerializeField, Min(0.05f)] private float cubeSize = 0.22f;
    [SerializeField, Min(0f)] private float driftSpeed = 0.08f;

    private ParticleSystem fogParticles;
    private ParticleSystemRenderer fogRenderer;

    private void Awake()
    {
        Configure();
    }

    private void OnEnable()
    {
        Configure();
    }

    private void OnValidate()
    {
        density = Mathf.Clamp01(density);
        areaSize = new Vector3(Mathf.Max(0.1f, areaSize.x), Mathf.Max(0.1f, areaSize.y), Mathf.Max(0.1f, areaSize.z));
        cubeSize = Mathf.Max(0.05f, cubeSize);
        if (isActiveAndEnabled)
            Configure();
    }

    private void Configure()
    {
        if (!TryGetComponent(out fogParticles))
            return;

        fogRenderer = GetComponent<ParticleSystemRenderer>();
        var main = fogParticles.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startLifetime = 18f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(driftSpeed * 0.25f, driftSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(cubeSize * 0.65f, cubeSize * 1.2f);
        main.startColor = new Color(fogColor.r, fogColor.g, fogColor.b, fogColor.a * Mathf.Lerp(0.08f, 0.72f, density));
        main.maxParticles = 2600;

        var emission = fogParticles.emission;
        emission.rateOverTime = Mathf.Lerp(0f, 150f, density);

        var shape = fogParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = areaSize;

        var noise = fogParticles.noise;
        noise.enabled = true;
        noise.strength = 0.14f;
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.08f;

        fogRenderer.renderMode = ParticleSystemRenderMode.Mesh;
        fogRenderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        fogRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        fogRenderer.receiveShadows = false;

        if (fogMaterial != null)
            fogRenderer.sharedMaterial = fogMaterial;

        if (!fogParticles.isPlaying)
            fogParticles.Play();
    }
}
