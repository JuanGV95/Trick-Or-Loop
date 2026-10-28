using System;
using UnityEngine;

public sealed class BirdFlyBy : MonoBehaviour
{
    [SerializeField] private float flightSpeed = 7f;
    [SerializeField] private float bobHeight = 0.15f;
    [SerializeField] private float bobFrequency = 2f;

    private Vector3 destination;
    private Vector3 flightCenter;
    private float routeRadius;
    private float baseY;
    private float phase;
    private bool initialized;
    private Action onFinished;

    public void Initialize(Vector3 target, float speed, Action finished, Vector3 center, float radius)
    {
        destination = target;
        flightCenter = center;
        routeRadius = radius;
        flightSpeed = speed;
        onFinished = finished;
        baseY = transform.position.y;
        phase = UnityEngine.Random.value * Mathf.PI * 2f;
        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        initialized = true;

        AudioSource flybyAudio = GetComponent<AudioSource>();
        if (flybyAudio != null && flybyAudio.clip != null) flybyAudio.Play();
    }

    private void Update()
    {
        if (!initialized) return;
        Vector3 next = Vector3.MoveTowards(transform.position, destination, flightSpeed * Time.deltaTime);
        next.y = baseY + Mathf.Sin(Time.time * bobFrequency + phase) * bobHeight;
        transform.position = next;
        if ((destination - transform.position).sqrMagnitude < 0.25f)
            ChooseNextDestination();
    }

    private void ChooseNextDestination()
    {
        Vector2 heading = UnityEngine.Random.insideUnitCircle.normalized;
        if (heading.sqrMagnitude < 0.1f) heading = Vector2.right;

        float distance = UnityEngine.Random.Range(routeRadius * 0.65f, routeRadius);
        destination = flightCenter + new Vector3(heading.x * distance, baseY, heading.y * distance);

        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void OnDestroy()
    {
        onFinished?.Invoke();
        onFinished = null;
    }
}
