using System.Collections;
using UnityEngine;

public sealed class GrasLimiteAudioZone : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private AudioSource firstAmbience;
    [SerializeField] private AudioSource secondAmbience;
    [SerializeField] private float halfExtentX = 112.5f;
    [SerializeField] private float halfExtentZ = 112.5f;
    [SerializeField] private float activationDistance = 18f;
    [SerializeField] private float sourceHeight = 1.6f;
    [SerializeField] private float fadeSeconds = 1.25f;
    [SerializeField] private float firstVolume = 0.07f;
    [SerializeField] private float secondVolume = 0.055f;

    private bool isNearBorder;
    private Coroutine firstFade;
    private Coroutine secondFade;
    private float nextPlayerSearch;

    private void Awake()
    {
        ConfigureSource(firstAmbience, 0.07f, 34f);
        ConfigureSource(secondAmbience, 0.055f, 42f);
    }

    private static void ConfigureSource(AudioSource source, float volume, float maxDistance)
    {
        if (source == null) return;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.volume = volume;
        source.minDistance = 8f;
        source.maxDistance = maxDistance;
        source.dopplerLevel = 0f;
    }

    private void Update()
    {
        if (player == null && Time.time >= nextPlayerSearch)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
            nextPlayerSearch = Time.time + 1f;
        }
        if (player == null) return;

        Vector3 p = transform.InverseTransformPoint(player.position);
        float outsideX = Mathf.Max(Mathf.Abs(p.x) - halfExtentX, 0f);
        float outsideZ = Mathf.Max(Mathf.Abs(p.z) - halfExtentZ, 0f);
        bool near = outsideX <= activationDistance && outsideZ <= activationDistance &&
                    (Mathf.Abs(p.x) >= halfExtentX - activationDistance || Mathf.Abs(p.z) >= halfExtentZ - activationDistance);

        if (near != isNearBorder)
        {
            isNearBorder = near;
            if (near) StartAmbience();
            else StopAmbienceGradually();
        }

        if (near) PositionEmitters(p);
    }

    private void StartAmbience()
    {
        StartSource(firstAmbience, firstVolume, ref firstFade);
        StartSource(secondAmbience, secondVolume, ref secondFade);
    }

    private void StartSource(AudioSource source, float targetVolume, ref Coroutine fade)
    {
        if (source == null) return;
        if (fade != null) StopCoroutine(fade);
        if (!source.isPlaying)
        {
            source.volume = 0f;
            if (source.clip != null && source.clip.length > 1f)
                source.time = Random.Range(0f, source.clip.length - 0.5f);
            source.Play();
        }
        fade = StartCoroutine(FadeSource(source, targetVolume, false));
    }

    private void StopAmbienceGradually()
    {
        StopSource(firstAmbience, ref firstFade);
        StopSource(secondAmbience, ref secondFade);
    }

    private void StopSource(AudioSource source, ref Coroutine fade)
    {
        if (source == null) return;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeSource(source, 0f, true));
    }

    private IEnumerator FadeSource(AudioSource source, float target, bool stopWhenDone)
    {
        float start = source.volume;
        float duration = Mathf.Max(0.05f, fadeSeconds);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        source.volume = target;
        if (stopWhenDone) source.Stop();
    }

    private void PositionEmitters(Vector3 playerPosition)
    {
        float xDistance = Mathf.Abs(Mathf.Abs(playerPosition.x) - halfExtentX);
        float zDistance = Mathf.Abs(Mathf.Abs(playerPosition.z) - halfExtentZ);
        bool onXSide = xDistance <= zDistance;
        Vector3 edge = new Vector3(Mathf.Clamp(playerPosition.x, -halfExtentX, halfExtentX), sourceHeight,
            Mathf.Clamp(playerPosition.z, -halfExtentZ, halfExtentZ));
        if (onXSide)
        {
            edge.x = Mathf.Sign(playerPosition.x) * halfExtentX;
            SetPosition(firstAmbience, edge + Vector3.forward * -11f);
            SetPosition(secondAmbience, edge + Vector3.forward * 11f);
        }
        else
        {
            edge.z = Mathf.Sign(playerPosition.z) * halfExtentZ;
            SetPosition(firstAmbience, edge + Vector3.right * -11f);
            SetPosition(secondAmbience, edge + Vector3.right * 11f);
        }
    }

    private void SetPosition(AudioSource source, Vector3 localPosition)
    {
        if (source != null) source.transform.position = transform.TransformPoint(localPosition);
    }

    private void OnDisable()
    {
        if (firstFade != null) StopCoroutine(firstFade);
        if (secondFade != null) StopCoroutine(secondFade);
        if (firstAmbience != null) firstAmbience.Stop();
        if (secondAmbience != null) secondAmbience.Stop();
        isNearBorder = false;
    }
}
