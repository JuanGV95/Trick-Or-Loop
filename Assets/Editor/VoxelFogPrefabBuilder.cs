using UnityEditor;
using UnityEngine;

public static class VoxelFogPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefabs/Environment/VoxelFog.prefab";
    private const string MaterialPath = "Assets/Materials/VoxelFog_Material.mat";

    [MenuItem("Tools/Trick or Loop/Create Voxel Fog Prefab")]
    public static void CreatePrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
        {
            bool replace = EditorUtility.DisplayDialog("Voxel Fog", "VoxelFog.prefab ya existe. ¿Quieres reemplazarlo?", "Reemplazar", "Cancelar");
            if (!replace) return;
        }

        var fog = new GameObject("Voxel Fog");
        var particles = fog.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.duration = 18f;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = 18f;
        main.startSpeed = 0.08f;
        main.startSize = 0.22f;
        main.maxParticles = 2600;
        var emission = particles.emission;
        emission.rateOverTime = 127.5f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(12f, 1.6f, 12f);

        var renderer = fog.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Object.DestroyImmediate(fog);
                EditorUtility.DisplayDialog("Voxel Fog", "No encontré un shader Unlit de URP. Revisa que el paquete URP esté compilado.", "OK");
                return;
            }
            material = new Material(shader) { name = "VoxelFog_Material" };
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        renderer.sharedMaterial = material;

        var fogController = fog.AddComponent<VoxelFog>();
        var serializedFog = new SerializedObject(fogController);
        serializedFog.FindProperty("fogMaterial").objectReferenceValue = material;
        serializedFog.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.SaveAsPrefabAsset(fog, PrefabPath);
        Object.DestroyImmediate(fog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Voxel Fog", "Prefab creado en " + PrefabPath + ". Arrástralo a la escena y ajusta Fog Color y Density en el Inspector.", "OK");
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}
