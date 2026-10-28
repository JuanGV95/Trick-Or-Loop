using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildHorrorTreeBoundary
{
    private const string OutputFolder = "Assets/Prefabs/TreeBoundary";
    private const string MaterialsFolder = OutputFolder + "/Materials";
    private const string BoundaryPath = OutputFolder + "/TreeBoundary_Horror.prefab";

    private const string ScaryTreePath = "Assets/polyperfect/Poly Halloween/Prefabs/Halloween/Tree_Scary_Dead.prefab";
    private const string PossessedStumpPath = "Assets/polyperfect/Poly Halloween/Prefabs/Halloween/Stump_Possesed.prefab";
    private const string PineDeadPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_dead.prefab";
    private const string FruitDeadPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_dead.prefab";
    private const string PineLogsPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_logs.prefab";
    private const string PineGreenPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Pine_Tree_03_green.prefab";
    private const string FruitGreenPath = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/PT_Fruit_Tree_01_green.prefab";
    private const string FoliageBoundaryPath = OutputFolder + "/TreeBoundary_Foliage_120.prefab";

    [MenuItem("Tools/Trick or Loop/Create Horror Tree Boundary Prefab")]
    public static void Create()
    {
        EnsureFolder("Assets/Prefabs", "TreeBoundary");
        EnsureFolder(OutputFolder, "Materials");

        if (AssetDatabase.LoadAssetAtPath<GameObject>(BoundaryPath) != null &&
            !EditorUtility.DisplayDialog("Tree Boundary", "TreeBoundary_Horror.prefab ya existe. ¿Reemplazarlo?", "Reemplazar", "Cancelar"))
            return;

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            EditorUtility.DisplayDialog("Tree Boundary", "No encontré el shader URP/Lit en este proyecto.", "OK");
            return;
        }

        GameObject scaryTree = AssetDatabase.LoadAssetAtPath<GameObject>(ScaryTreePath);
        GameObject possessedStump = AssetDatabase.LoadAssetAtPath<GameObject>(PossessedStumpPath);
        GameObject deadPine = CreateUrpVariant(PineDeadPath, "PT_Pine_Tree_03_dead_Dark_URP.prefab", urpLit);
        GameObject deadFruit = CreateUrpVariant(FruitDeadPath, "PT_Fruit_Tree_01_dead_Dark_URP.prefab", urpLit);
        GameObject logs = CreateUrpVariant(PineLogsPath, "PT_Pine_Tree_03_logs_Dark_URP.prefab", urpLit);

        if (scaryTree == null || possessedStump == null || deadPine == null || deadFruit == null || logs == null)
        {
            EditorUtility.DisplayDialog("Tree Boundary", "Falta uno de los prefabs de árbol o madera. No se creó el borde.", "OK");
            return;
        }

        var root = new GameObject("TreeBoundary_Horror");
        var rng = new System.Random(271028);
        const int treesPerSidePerRow = 16;
        float[] rowOffsets = { 56f, 63f };
        int treeIndex = 0;

        for (int row = 0; row < rowOffsets.Length; row++)
        {
            float extent = rowOffsets[row];
            float step = (extent * 2f) / treesPerSidePerRow;
            for (int side = 0; side < 4; side++)
            {
                for (int i = 0; i < treesPerSidePerRow; i++)
                {
                    float along = -extent + step * (i + 0.5f) + Range(rng, -1.35f, 1.35f);
                    float outward = extent + Range(rng, -1.0f, 1.8f);
                    Vector3 position = SidePosition(side, along, outward);

                    double choice = rng.NextDouble();
                    GameObject treePrefab = choice < 0.42 ? scaryTree : (choice < 0.76 ? deadPine : deadFruit);
                    float targetHeight = treePrefab == scaryTree ? Range(rng, 7.0f, 10.0f) :
                                         treePrefab == deadPine ? Range(rng, 7.0f, 9.2f) : Range(rng, 5.8f, 7.8f);
                    float yaw = Range(rng, 0f, 360f);
                    AddSizedPrefab(root.transform, treePrefab, position, yaw, targetHeight, Range(rng, 0.92f, 1.12f), "Tree_" + treeIndex++);
                }
            }
        }

        int propIndex = 0;
        for (int side = 0; side < 4; side++)
        {
            for (int i = 1; i <= 4; i++)
            {
                float along = -56f + i * 28f + Range(rng, -4f, 4f);
                float outward = (i % 2 == 0 ? 63f : 56f) + Range(rng, -1f, 1f);
                Vector3 position = SidePosition(side, along, outward);
                if (i % 2 == 0)
                    AddSizedPrefab(root.transform, logs, position, Range(rng, 0f, 360f), 0f, Range(rng, 0.8f, 1.25f), "Wood_" + propIndex++);
                else
                    AddSizedPrefab(root.transform, possessedStump, position, Range(rng, 0f, 360f), 0f, Range(rng, 0.8f, 1.15f), "Stump_" + propIndex++);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(root, BoundaryPath, out bool saved);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (!saved)
        {
            EditorUtility.DisplayDialog("Tree Boundary", "Unity no pudo guardar el prefab.", "OK");
            return;
        }

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(BoundaryPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
        EditorUtility.DisplayDialog("Tree Boundary", "Prefab creado con 128 árboles en dos filas y 16 acentos de madera/tocón. No se modificó la escena.", "OK");
    }

    [MenuItem("Tools/Trick or Loop/Create 120 Tree Foliage Boundary Prefab")]
    public static void CreateFoliageBoundary()
    {
        EnsureFolder("Assets/Prefabs", "TreeBoundary");
        EnsureFolder(OutputFolder, "Materials");

        if (AssetDatabase.LoadAssetAtPath<GameObject>(FoliageBoundaryPath) != null &&
            !EditorUtility.DisplayDialog("Foliage Tree Boundary", "TreeBoundary_Foliage_120.prefab ya existe. ¿Reemplazarlo?", "Reemplazar", "Cancelar"))
            return;

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            EditorUtility.DisplayDialog("Foliage Tree Boundary", "No encontré el shader URP/Lit en este proyecto.", "OK");
            return;
        }

        Scene buildScene = EditorSceneManager.NewPreviewScene();
        GameObject root = null;
        try
        {
            GameObject pine = CreateUrpVariant(PineGreenPath, "PT_Pine_Tree_03_green_DarkURP.prefab", urpLit, buildScene);
            GameObject fruit = CreateUrpVariant(FruitGreenPath, "PT_Fruit_Tree_01_green_DarkURP.prefab", urpLit, buildScene);
            if (pine == null || fruit == null)
            {
                EditorUtility.DisplayDialog("Foliage Tree Boundary", "No encontré los prefabs de pino verde y frutal verde.", "OK");
                return;
            }

            root = new GameObject("TreeBoundary_Foliage_120");
            SceneManager.MoveGameObjectToScene(root, buildScene);
            var rng = new System.Random(281028);
            const int treesPerSidePerRow = 15;
            float[] rowOffsets = { 54.5f, 68f };
            int treeIndex = 0;

            for (int row = 0; row < rowOffsets.Length; row++)
            {
                float extent = rowOffsets[row];
                float step = (extent * 2f) / treesPerSidePerRow;
                for (int side = 0; side < 4; side++)
                {
                    for (int i = 0; i < treesPerSidePerRow; i++)
                    {
                        float phase = row == 0 ? 0f : step * 0.5f;
                        float along = -extent + step * (i + 0.5f) + phase + Range(rng, -1.3f, 1.3f);
                        along = Mathf.Clamp(along, -extent, extent);
                        float outward = extent + Range(rng, -0.8f, 1.4f);
                        Vector3 position = SidePosition(side, along, outward);
                        GameObject tree = rng.NextDouble() < 0.55 ? pine : fruit;
                        float targetHeight = tree == pine ? Range(rng, 7.5f, 9.4f) : Range(rng, 6.2f, 8.1f);
                        AddSizedPrefab(root.transform, tree, position, Range(rng, 0f, 360f), targetHeight, Range(rng, 0.92f, 1.12f), "FoliageTree_" + treeIndex++);
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, FoliageBoundaryPath, out bool saved);
            if (!saved)
            {
                EditorUtility.DisplayDialog("Foliage Tree Boundary", "Unity no pudo guardar el prefab.", "OK");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(FoliageBoundaryPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
            EditorUtility.DisplayDialog("Foliage Tree Boundary", "Prefab nuevo creado con exactamente 120 árboles con follaje en dos filas escalonadas. No se modificó la escena ni el borde seco.", "OK");
        }
        finally
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            if (buildScene.IsValid()) EditorSceneManager.ClosePreviewScene(buildScene);
        }
    }

    private static GameObject CreateUrpVariant(string sourcePath, string outputFileName, Shader urpLit, Scene targetScene = default)
    {
        GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
        if (sourcePrefab == null) return null;

        string outputPath = OutputFolder + "/" + outputFileName;
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(outputPath);
        if (existing != null) return existing;

        var wrapper = new GameObject(Path.GetFileNameWithoutExtension(outputPath));
        if (targetScene.IsValid()) SceneManager.MoveGameObjectToScene(wrapper, targetScene);
        var sourceInstance = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab, wrapper.transform);

        foreach (Renderer renderer in wrapper.GetComponentsInChildren<Renderer>(true))
        {
            Material[] sourceMaterials = renderer.sharedMaterials;
            var converted = new Material[sourceMaterials.Length];
            for (int i = 0; i < sourceMaterials.Length; i++)
                converted[i] = ConvertMaterial(sourceMaterials[i], urpLit);
            renderer.sharedMaterials = converted;
        }

        PrefabUtility.SaveAsPrefabAsset(wrapper, outputPath, out bool saved);
        UnityEngine.Object.DestroyImmediate(wrapper);
        return saved ? AssetDatabase.LoadAssetAtPath<GameObject>(outputPath) : null;
    }

    private static Material ConvertMaterial(Material source, Shader urpLit)
    {
        if (source == null) return null;
        string sourcePath = AssetDatabase.GetAssetPath(source);
        string guid = AssetDatabase.AssetPathToGUID(sourcePath);
        string suffix = guid.Length >= 8 ? guid.Substring(0, 8) : source.name;
        string outputPath = MaterialsFolder + "/" + MakeSafeFileName(source.name + "_DarkURP_" + suffix) + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(outputPath);
        if (existing != null) return existing;

        var material = new Material(urpLit) { name = Path.GetFileNameWithoutExtension(outputPath) };
        Texture texture = GetTexture(source, "_BaseTexture") ?? GetTexture(source, "_BaseMap") ?? GetTexture(source, "_MainTex");
        if (texture != null && material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);

        Color color = ReadSourceColor(source);
        string lowerName = source.name.ToLowerInvariant();
        bool foliage = lowerName.Contains("leaf") || lowerName.Contains("leaves") || lowerName.Contains("foliage");
        if (foliage)
            color = Color.Lerp(color, new Color(0.09f, 0.15f, 0.09f, 1f), 0.64f);
        else
            color = Color.Lerp(color, new Color(0.22f, 0.16f, 0.12f, 1f), 0.38f);

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.05f);
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 0f);
        if (foliage && material.HasProperty("_AlphaClip"))
        {
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.42f);
            material.EnableKeyword("_ALPHATEST_ON");
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
        }
        AssetDatabase.CreateAsset(material, outputPath);
        return material;
    }

    private static Color ReadSourceColor(Material source)
    {
        if (source.HasProperty("_TopColor") && source.HasProperty("_GroundColor"))
            return Color.Lerp(source.GetColor("_TopColor"), source.GetColor("_GroundColor"), 0.5f);
        if (source.HasProperty("_BaseColor")) return source.GetColor("_BaseColor");
        if (source.HasProperty("_Color")) return source.GetColor("_Color");
        return Color.white;
    }

    private static Texture GetTexture(Material material, string property)
    {
        return material.HasProperty(property) ? material.GetTexture(property) : null;
    }

    private static void AddSizedPrefab(Transform parent, GameObject prefab, Vector3 position, float yaw, float targetHeight, float variation, string label)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        if (instance == null) return;
        instance.name = label + "_" + prefab.name;
        instance.transform.localPosition = position;
        instance.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        if (targetHeight > 0f)
        {
            Bounds bounds = GetRenderBounds(instance);
            if (bounds.size.y > 0.01f)
                instance.transform.localScale = Vector3.one * (targetHeight * variation / bounds.size.y);
        }
        else
        {
            instance.transform.localScale = Vector3.one * variation;
        }

        Bounds finalBounds = GetRenderBounds(instance);
        instance.transform.position += Vector3.up * -finalBounds.min.y;
    }

    private static Bounds GetRenderBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static Vector3 SidePosition(int side, float along, float outward)
    {
        switch (side)
        {
            case 0: return new Vector3(along, 0f, outward);
            case 1: return new Vector3(along, 0f, -outward);
            case 2: return new Vector3(outward, 0f, along);
            default: return new Vector3(-outward, 0f, along);
        }
    }

    private static float Range(System.Random rng, float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private static string MakeSafeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        return name.Replace('/', '_').Replace('\\', '_');
    }
}
