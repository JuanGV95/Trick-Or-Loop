using System.Collections;
using UnityEngine;

public sealed class TreeBoundaryAudioZone : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private AudioSource woodSource;
    [SerializeField] private AudioSource distantSource;
    [SerializeField] private float halfExtentX = 37.5f;
    [SerializeField] private float halfExtentZ = 37.5f;
    [SerializeField] private float activationDistance = 18f;
    [SerializeField] private float sourceHeight = 2.4f;

    private bool isNearBorder;
    private Coroutine sequence;
    private float nextSearchTime;

    private void Update()
    {
        if (player == null && Time.time >= nextSearchTime)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
            nextSearchTime = Time.time + 1f;
        }
        if (player == null) return;

        Vector3 p = transform.InverseTransformPoint(player.position);
        float dx = Mathf.Max(Mathf.Abs(p.x) - halfExtentX, 0f);
        float dz = Mathf.Max(Mathf.Abs(p.z) - halfExtentZ, 0f);
        bool near = dx <= activationDistance && dz <= activationDistance &&
                    (Mathf.Abs(p.x) >= halfExtentX - activationDistance || Mathf.Abs(p.z) >= halfExtentZ - activationDistance);
        if (near == isNearBorder) return;
        isNearBorder = near;
        if (near) sequence = StartCoroutine(PlayBorderSounds());
        else
        {
            if (sequence != null) StopCoroutine(sequence);
            sequence = null;
            if (woodSource != null) woodSource.Stop();
            if (distantSource != null) distantSource.Stop();
        }
    }

    private IEnumerator PlayBorderSounds()
    {
        Vector3 p = transform.InverseTransformPoint(player.position);
        Vector3 edge = new Vector3(Mathf.Clamp(p.x, -halfExtentX, halfExtentX), sourceHeight,
            Mathf.Clamp(p.z, -halfExtentZ, halfExtentZ));
        if (Mathf.Abs(Mathf.Abs(p.x) - halfExtentX) < Mathf.Abs(Mathf.Abs(p.z) - halfExtentZ))
            edge.x = Mathf.Sign(p.x) * halfExtentX;
        else edge.z = Mathf.Sign(p.z) * halfExtentZ;
        Vector3 world = transform.TransformPoint(edge);
        if (woodSource != null) { woodSource.transform.position = world; woodSource.Play(); }
        yield return new WaitForSeconds(2.2f);
        if (!isNearBorder) yield break;
        if (distantSource != null) { distantSource.transform.position = world; distantSource.Play(); }
        sequence = null;
    }
}
