using System.Collections;
using UnityEngine;

public sealed class FaultyStreetLight : MonoBehaviour
{
    [SerializeField] private Light targetLight;
    [SerializeField] private AudioSource electricitySource;
    [SerializeField] private Vector2 intervalSeconds = new Vector2(5f, 11f);
    [SerializeField] private Vector2Int flickersPerEvent = new Vector2Int(3, 6);
    [SerializeField] private Vector2 flickerStepSeconds = new Vector2(0.08f, 0.22f);

    private float normalIntensity;

    private void Start()
    {
        if (targetLight != null) normalIntensity = targetLight.intensity;
        StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(intervalSeconds.x, intervalSeconds.y));
            if (electricitySource != null && electricitySource.clip != null)
                electricitySource.PlayOneShot(electricitySource.clip);
            int count = Random.Range(flickersPerEvent.x, flickersPerEvent.y + 1);
            for (int i = 0; i < count; i++)
            {
                if (targetLight != null)
                {
                    targetLight.enabled = (i % 2) == 0;
                    targetLight.intensity = targetLight.enabled ? normalIntensity * Random.Range(0.15f, 1f) : 0f;
                }
                yield return new WaitForSeconds(Random.Range(flickerStepSeconds.x, flickerStepSeconds.y));
            }
            if (targetLight != null)
            {
                targetLight.enabled = true;
                targetLight.intensity = normalIntensity;
            }
        }
    }
}
