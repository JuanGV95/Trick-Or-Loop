using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class AudioPrefabBuilder
{
    private const string BirdSourcePath = "Assets/Zacxophone/Birds/URP/Prefabs/01BirdPrefab.prefab";
    private const string BirdWingsPath = "Assets/SFFX/dragon-studio-bird-wings-463212.mp3";
    private const string BirdAmbiencePath = "Assets/SFFX/soul_serenity_sounds-garden-ambience-236744.mp3";
    private const string ElectricityPath = "Assets/SFFX/ElectricalSound1.wav";
    private const string WoodPath = "Assets/SFFX/yodguard-door-opening-slowly-1-535465.mp3";
    private const string DistantPath = "Assets/Multiple Solutions/Basic RPG Sounds/Audio/Flying 2.wav";
    private const string PolePath = "Assets/ModularLowpolyStreetsFree/Prefabs/Other/Pole1.prefab";

    private const string BirdPrefabPath = "Assets/Prefabs/Environment/Birds/Bird_FlyBy.prefab";
    private const string SpawnerPrefabPath = "Assets/Prefabs/Environment/Birds/Birds_Ambience_Spawner.prefab";
    private const string StreetLightsPrefabPath = "Assets/Prefabs/Environment/Street Lights.prefab";
    private const string ForestAudioPrefabPath = "Assets/Prefabs/Environment/Forest_Audio_Object.prefab";
    private const string StreetMaterialsPath = "Assets/Prefabs/Environment/StreetLights_Materials";

    [MenuItem("Tools/Trick or Loop/Create Bird, Street Lights and Forest Audio Prefabs")]
    public static void CreateAll()
    {
        string[] prefabPaths = { BirdPrefabPath, SpawnerPrefabPath, StreetLightsPrefabPath, ForestAudioPrefabPath };
        foreach (string path in prefabPaths)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null &&
                !EditorUtility.DisplayDialog("Audio Prefabs", Path.GetFileName(path) + " ya existe. ¿Quieres reemplazar todos los prefabs de audio?", "Reemplazar", "Cancelar"))
                return;
        }

        EnsureFolder("Assets/Prefabs/Environment/Birds");
        EnsureFolder(StreetMaterialsPath);

        Scene previewScene = EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject birdPrefab = CreateBirdPrefab(previewScene);
            CreateBirdSpawnerPrefab(previewScene, birdPrefab);
            CreateStreetLightsPrefab(previewScene);
            CreateForestAudioPrefab(previewScene);
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(previewScene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(SpawnerPrefabPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
        EditorUtility.DisplayDialog("Audio Prefabs", "Prefabs creados en Assets/Prefabs/Environment: Bird_FlyBy, Birds_Ambience_Spawner, Street Lights y Forest_Audio_Object. El spawner usa radio 28 m y crea vuelos en oleadas. Street Lights forma un anillo de 16 farolas alrededor del césped; falla una de cada cuatro. La escena no se modificó.", "OK");
    }

    [MenuItem("Tools/Trick or Loop/Rebuild Circular Street Lights Prefab")]
    public static void RebuildCircularStreetLightsPrefab()
    {
        EnsureFolder(StreetMaterialsPath);
        Scene previewScene = EditorSceneManager.NewPreviewScene();
        try
        {
            CreateStreetLightsPrefab(previewScene);
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(previewScene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(StreetLightsPrefabPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
        EditorUtility.DisplayDialog("Street Lights", "Prefab actualizado: anillo circular de 16 farolas alrededor del césped, luces llamadas LuzPosteAmarilla ubicadas un poco debajo de la punta y ElectricalSound1.wav a volumen 0.89 solo en las cuatro farolas averiadas. La escena no se modificó.", "OK");
    }

    [MenuItem("Tools/Trick or Loop/Rebuild Bird FlyBy Prefabs")]
    public static void RebuildBirdFlyByPrefabs()
    {
        EnsureFolder("Assets/Prefabs/Environment/Birds");
        Scene previewScene = EditorSceneManager.NewPreviewScene();
        try
        {
            GameObject birdPrefab = CreateBirdPrefab(previewScene);
            CreateBirdSpawnerPrefab(previewScene, birdPrefab);
        }
        finally
        {
            EditorSceneManager.ClosePreviewScene(previewScene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(BirdPrefabPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
        EditorUtility.DisplayDialog("Bird FlyBy", "Bird_FlyBy se aumentó de tamaño y ahora cada pájaro continúa volando entre rutas aleatorias en vez de destruirse al llegar al destino. El spawner mantiene un máximo de 10 pájaros activos.", "OK");
    }

    private static GameObject CreateBirdPrefab(Scene previewScene)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(BirdSourcePath);
        AudioClip wings = AssetDatabase.LoadAssetAtPath<AudioClip>(BirdWingsPath);
        if (source == null || wings == null) throw new System.InvalidOperationException("No se encontró el prefab URP de pájaro o el clip de alas.");

        GameObject root = NewRoot("Bird_FlyBy", previewScene);
        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(source, root.transform);
        model.name = "01Bird_Animated_URP";
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one * 1.65f;

        AudioSource flyby = root.AddComponent<AudioSource>();
        Configure3D(flyby, wings, 0.28f, 3f, 45f);
        root.AddComponent<BirdFlyBy>();
        PrefabUtility.SaveAsPrefabAsset(root, BirdPrefabPath);
        Object.DestroyImmediate(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(BirdPrefabPath);
    }

    private static void CreateBirdSpawnerPrefab(Scene previewScene, GameObject birdPrefab)
    {
        GameObject root = NewRoot("Birds_Ambience_Spawner", previewScene);
        AudioClip ambience = AssetDatabase.LoadAssetAtPath<AudioClip>(BirdAmbiencePath);
        AudioSource ambienceSource = root.AddComponent<AudioSource>();
        ambienceSource.clip = ambience;
        ambienceSource.playOnAwake = true;
        ambienceSource.loop = true;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.volume = 0.16f;

        BirdAmbienceSpawner spawner = root.AddComponent<BirdAmbienceSpawner>();
        SerializedObject serializedSpawner = new SerializedObject(spawner);
        serializedSpawner.FindProperty("birdPrefab").objectReferenceValue = birdPrefab;
        serializedSpawner.FindProperty("ambienceSource").objectReferenceValue = ambienceSource;
        serializedSpawner.FindProperty("spawnRadius").floatValue = 28f;
        serializedSpawner.FindProperty("altitude").floatValue = 9f;
        serializedSpawner.FindProperty("initialDelay").floatValue = 6f;
        serializedSpawner.FindProperty("maxActiveBirds").intValue = 10;
        serializedSpawner.FindProperty("spawnIntervalSeconds").vector2Value = new Vector2(22f, 38f);
        serializedSpawner.FindProperty("birdsPerWave").vector2IntValue = new Vector2Int(2, 4);
        serializedSpawner.FindProperty("flightSpeed").vector2Value = new Vector2(7f, 10f);
        serializedSpawner.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, SpawnerPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void CreateStreetLightsPrefab(Scene previewScene)
    {
        GameObject polePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PolePath);
        AudioClip electricity = AssetDatabase.LoadAssetAtPath<AudioClip>(ElectricityPath);
        if (polePrefab == null || electricity == null) throw new System.InvalidOperationException("No se encontró Pole1 o ElectricalSound1.wav.");

        GameObject root = NewRoot("Street Lights", previewScene);
        const int lampCount = 16;
        const float ringRadius = 34f;
        for (int i = 0; i < lampCount; i++)
        {
            bool isFaulty = (i + 1) % 4 == 0;
            GameObject lamp = new GameObject("StreetLight_" + (i + 1).ToString("00") + (isFaulty ? "_Faulty" : ""));
            lamp.transform.SetParent(root.transform, false);
            float angle = i * Mathf.PI * 2f / lampCount;
            Vector3 radial = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            lamp.transform.localPosition = radial * ringRadius;
            lamp.transform.localRotation = Quaternion.LookRotation(-radial, Vector3.up);

            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(polePrefab, lamp.transform);
            model.name = "Pole1_URP";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one;
            ConvertMaterials(model);

            Bounds bounds = CalculateBounds(model);
            float scale = 6f / Mathf.Max(bounds.size.y, 0.01f);
            model.transform.localScale = Vector3.one * scale;
            model.transform.localPosition = new Vector3(0f, -bounds.min.y * scale, 0f);
            bounds = CalculateBounds(model);

            GameObject lightObject = new GameObject("LuzPosteAmarilla");
            lightObject.transform.SetParent(model.transform, false);
            lightObject.transform.localPosition = model.transform.InverseTransformPoint(new Vector3(bounds.center.x, bounds.max.y - 0.3f, bounds.center.z));
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.74f, 0.43f);
            light.intensity = 3.5f;
            light.range = 13f;
            // Six shadow maps per point light become expensive across a 16-lamp ring.
            light.shadows = LightShadows.None;
            light.bounceIntensity = 0.35f;

            if (isFaulty)
            {
                AudioSource electricitySource = lamp.AddComponent<AudioSource>();
                Configure3D(electricitySource, electricity, 0.89f, 2f, 22f);
                FaultyStreetLight faulty = lamp.AddComponent<FaultyStreetLight>();
                SerializedObject serializedFaulty = new SerializedObject(faulty);
                serializedFaulty.FindProperty("targetLight").objectReferenceValue = light;
                serializedFaulty.FindProperty("electricitySource").objectReferenceValue = electricitySource;
                serializedFaulty.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        PrefabUtility.SaveAsPrefabAsset(root, StreetLightsPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void CreateForestAudioPrefab(Scene previewScene)
    {
        AudioClip wood = AssetDatabase.LoadAssetAtPath<AudioClip>(WoodPath);
        AudioClip distant = AssetDatabase.LoadAssetAtPath<AudioClip>(DistantPath);
        GameObject root = NewRoot("Forest_Audio_Object", previewScene);

        GameObject woodObject = new GameObject("Wood_Creak_Audio");
        woodObject.transform.SetParent(root.transform, false);
        AudioSource woodSource = woodObject.AddComponent<AudioSource>();
        Configure3D(woodSource, wood, 0.32f, 2f, 16f);

        GameObject distantObject = new GameObject("Distant_Flyby_Audio");
        distantObject.transform.SetParent(root.transform, false);
        AudioSource distantSource = distantObject.AddComponent<AudioSource>();
        Configure3D(distantSource, distant, 0.18f, 8f, 48f);

        ForestObjectAudio controller = root.AddComponent<ForestObjectAudio>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("woodSource").objectReferenceValue = woodSource;
        serializedController.FindProperty("distantSource").objectReferenceValue = distantSource;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, ForestAudioPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void Configure3D(AudioSource source, AudioClip clip, float volume, float minDistance, float maxDistance)
    {
        source.clip = clip;
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 1f;
        source.volume = volume;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.dopplerLevel = 0f;
    }

    private static void ConvertMaterials(GameObject root)
    {
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader == null) return;
        var cache = new Dictionary<int, Material>();
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material source = materials[i];
                if (source == null || (source.shader != null && source.shader.name.StartsWith("Universal Render Pipeline"))) continue;
                if (!cache.TryGetValue(source.GetInstanceID(), out Material converted))
                {
                    converted = new Material(urpShader) { name = "StreetLights_" + source.name.Replace(' ', '_') + "_URP" };
                    Texture baseTexture = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : (source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null);
                    Color baseColor = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : (source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white);
                    converted.SetTexture("_BaseMap", baseTexture);
                    converted.SetColor("_BaseColor", baseColor);
                    if (source.HasProperty("_Metallic") && converted.HasProperty("_Metallic")) converted.SetFloat("_Metallic", source.GetFloat("_Metallic"));
                    if (source.HasProperty("_Glossiness") && converted.HasProperty("_Smoothness")) converted.SetFloat("_Smoothness", source.GetFloat("_Glossiness"));
                    if (source.HasProperty("_EmissionColor") && converted.HasProperty("_EmissionColor"))
                    {
                        Color emission = source.GetColor("_EmissionColor");
                        converted.SetColor("_EmissionColor", emission);
                        if (emission.maxColorComponent > 0.01f) converted.EnableKeyword("_EMISSION");
                    }
                    string assetPath = StreetMaterialsPath + "/" + converted.name + ".mat";
                    if (AssetDatabase.LoadAssetAtPath<Material>(assetPath) == null) AssetDatabase.CreateAsset(converted, assetPath);
                    else converted = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                    cache.Add(source.GetInstanceID(), converted);
                }
                materials[i] = converted;
            }
            renderer.sharedMaterials = materials;
        }
    }

    private static Bounds CalculateBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(root.transform.position, Vector3.one);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static GameObject NewRoot(string name, Scene scene)
    {
        GameObject root = new GameObject(name);
        SceneManager.MoveGameObjectToScene(root, scene);
        return root;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
