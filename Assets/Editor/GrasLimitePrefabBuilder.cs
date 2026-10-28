using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public static class GrasLimitePrefabBuilder
{
    private const string ScenePath = "Assets/Scenes/Escenario_1.unity";
    private const string GrassSourcePath = "Assets/Prefabs/Environment/Grass_Center_Scarecrow.prefab";
    private const string MeshPath = "Assets/Models/GrasLimite_BorderMesh.asset";
    private const string PrefabPath = "Assets/Prefabs/Environment/TreeBoundary/GrasLimite.prefab";
    private const string FirstClipPath = "Assets/SFFX/se_ve6kibo47vhkvhp2.wav";
    private const string SecondClipPath = "Assets/SFFX/se_tcu4mjiebqzvh62v.wav";
    private const string PrefabName = "GrasLimite";
    private const int SourceClumpCount = 2400;
    private const int BorderClumpCount = 18000;

    [MenuItem("Tools/Trick or Loop/Create GrasLimite Border Prefab")]
    public static void Build()
    {
        GameObject grassSource = AssetDatabase.LoadAssetAtPath<GameObject>(GrassSourcePath);
        AudioClip firstClip = AssetDatabase.LoadAssetAtPath<AudioClip>(FirstClipPath);
        AudioClip secondClip = AssetDatabase.LoadAssetAtPath<AudioClip>(SecondClipPath);
        if (grassSource == null || firstClip == null || secondClip == null)
            throw new InvalidOperationException("No se encontró el prefab de pasto o alguno de los clips WAV.");

        MeshFilter sourceFilter = grassSource.GetComponent<MeshFilter>();
        MeshRenderer sourceRenderer = grassSource.GetComponent<MeshRenderer>();
        if (sourceFilter == null || sourceFilter.sharedMesh == null || sourceRenderer == null || sourceRenderer.sharedMaterial == null)
            throw new InvalidOperationException("Grass_Center_Scarecrow no tiene malla y material válidos.");

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        GameObject terrain = Find(scene, "Terreno-Vecindario");
        if (terrain == null) throw new InvalidOperationException("No se encontró Terreno-Vecindario en Escenario_1.");
        BoxCollider boundary = terrain.GetComponentInChildren<BoxCollider>();
        if (boundary == null) throw new InvalidOperationException("Terreno-Vecindario no tiene BoxCollider para medir el límite.");
        Bounds bounds = boundary.bounds;

        Mesh borderMesh = CreateIrregularBorderMesh(sourceFilter.sharedMesh, bounds.extents.x, bounds.extents.z);
        Mesh oldMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
        if (oldMesh != null) AssetDatabase.DeleteAsset(MeshPath);
        AssetDatabase.CreateAsset(borderMesh, MeshPath);

        Scene previewScene = EditorSceneManager.NewPreviewScene();
        GameObject root = null;
        GameObject prefabAsset;
        try
        {
            root = new GameObject(PrefabName);
            SceneManager.MoveGameObjectToScene(root, previewScene);
            GameObject ground = new GameObject("Irregular_Forest_Grass_Bed");
            ground.transform.SetParent(root.transform, false);
            ground.AddComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            MeshRenderer renderer = ground.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = sourceRenderer.sharedMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = true;

            GameObject audioZone = new GameObject("Border_Ambience_Zone");
            audioZone.transform.SetParent(root.transform, false);
            AudioSource firstSource = CreateAmbienceSource(audioZone.transform, "Forest_Ambience_A", firstClip, 0.07f, 34f);
            AudioSource secondSource = CreateAmbienceSource(audioZone.transform, "Forest_Ambience_B", secondClip, 0.055f, 42f);

            GrasLimiteAudioZone controller = audioZone.AddComponent<GrasLimiteAudioZone>();
            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("firstAmbience").objectReferenceValue = firstSource;
            serialized.FindProperty("secondAmbience").objectReferenceValue = secondSource;
            serialized.FindProperty("halfExtentX").floatValue = bounds.extents.x;
            serialized.FindProperty("halfExtentZ").floatValue = bounds.extents.z;
            serialized.FindProperty("activationDistance").floatValue = 18f;
            serialized.FindProperty("firstVolume").floatValue = 0.07f;
            serialized.FindProperty("secondVolume").floatValue = 0.055f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            EditorSceneManager.ClosePreviewScene(previewScene);
        }

        if (prefabAsset == null) throw new InvalidOperationException("No se pudo guardar GrasLimite.prefab.");
        GameObject previous = Find(scene, PrefabName);
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, scene);
        instance.transform.SetPositionAndRotation(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z), Quaternion.identity);
        instance.transform.localScale = Vector3.one;

        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("No se pudo guardar Escenario_1.");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("GrasLimite creado: franja irregular de pasto en torno al límite de 225 x 225, con dos ambientes 3D suaves que se activan a 18 m del borde.");
    }

    private static AudioSource CreateAmbienceSource(Transform parent, string objectName, AudioClip clip, float volume, float maxDistance)
    {
        GameObject emitter = new GameObject(objectName);
        emitter.transform.SetParent(parent, false);
        AudioSource source = emitter.AddComponent<AudioSource>();
        source.clip = clip;
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.volume = volume;
        source.minDistance = 8f;
        source.maxDistance = maxDistance;
        source.dopplerLevel = 0f;
        return source;
    }

    private static Mesh CreateIrregularBorderMesh(Mesh source, float halfX, float halfZ)
    {
        Vector3[] sourceVertices = source.vertices;
        Vector2[] sourceUvs = source.uv;
        int[] sourceTriangles = source.GetTriangles(0);
        int verticesPerClump = sourceVertices.Length / SourceClumpCount;
        int indicesPerClump = sourceTriangles.Length / SourceClumpCount;
        if (verticesPerClump * SourceClumpCount != sourceVertices.Length ||
            indicesPerClump * SourceClumpCount != sourceTriangles.Length || sourceUvs.Length != sourceVertices.Length)
            throw new InvalidOperationException("La malla del pasto central no tiene la estructura esperada.");

        var vertices = new List<Vector3>(BorderClumpCount * verticesPerClump);
        var uvs = new List<Vector2>(BorderClumpCount * verticesPerClump);
        var triangles = new List<int>(BorderClumpCount * indicesPerClump);
        var random = new System.Random(481732);
        float referenceHalf = (halfX + halfZ) * 0.5f;
        float maxCandidateX = referenceHalf * 1.24f * halfX / referenceHalf + 5f;
        float maxCandidateZ = referenceHalf * 1.24f * halfZ / referenceHalf + 5f;
        int accepted = 0;
        int attempts = 0;
        while (accepted < BorderClumpCount && attempts++ < BorderClumpCount * 20)
        {
            float x = (float)(random.NextDouble() * 2.0 - 1.0) * maxCandidateX;
            float z = (float)(random.NextDouble() * 2.0 - 1.0) * maxCandidateZ;
            float nx = Mathf.Abs(x / halfX);
            float nz = Mathf.Abs(z / halfZ);
            float metric = referenceHalf * Mathf.Pow(Mathf.Pow(nx, 4.6f) + Mathf.Pow(nz, 4.6f), 1f / 4.6f);
            float angle = Mathf.Atan2(z, x);
            float along = nx >= nz ? z : x;
            float wave = 3.7f * Mathf.Sin(along * 0.071f + 1.3f) +
                         2.1f * Mathf.Sin(along * 0.193f + 0.4f) +
                         1.8f * Mathf.Sin(angle * 5.2f + 0.8f);
            float inner = referenceHalf * 0.90f + wave * 0.7f;
            float outer = referenceHalf * 1.18f + wave;
            if (metric < inner || metric > outer || random.NextDouble() < 0.045) continue;

            int sourceClump = random.Next(SourceClumpCount);
            int sourceOffset = sourceClump * verticesPerClump;
            int destinationOffset = vertices.Count;
            Vector2 center = Vector2.zero;
            for (int j = 0; j < verticesPerClump; j++)
            {
                center.x += sourceVertices[sourceOffset + j].x;
                center.y += sourceVertices[sourceOffset + j].z;
            }
            center /= verticesPerClump;
            float scale = Mathf.Lerp(0.76f, 1.12f, (float)random.NextDouble());
            float yaw = (float)random.NextDouble() * Mathf.PI * 2f;
            float cos = Mathf.Cos(yaw);
            float sin = Mathf.Sin(yaw);
            for (int j = 0; j < verticesPerClump; j++)
            {
                Vector3 v = sourceVertices[sourceOffset + j];
                float dx = (v.x - center.x) * scale;
                float dz = (v.z - center.y) * scale;
                vertices.Add(new Vector3(x + dx * cos - dz * sin, v.y, z + dx * sin + dz * cos));
                uvs.Add(sourceUvs[sourceOffset + j]);
            }

            int indexOffset = sourceClump * indicesPerClump;
            for (int j = 0; j < indicesPerClump; j++)
                triangles.Add(sourceTriangles[indexOffset + j] - sourceOffset + destinationOffset);
            accepted++;
        }

        if (accepted < BorderClumpCount * 0.95f)
            throw new InvalidOperationException("No se pudo distribuir suficiente pasto en la banda irregular del borde.");
        var mesh = new Mesh { name = "GrasLimite_Irregular_Border_Mesh", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static GameObject Find(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                if (child.name == objectName) return child.gameObject;
        return null;
    }
}
