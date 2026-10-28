using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class CentralGrassPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefabs/Environment/Grass_Center_Scarecrow.prefab";
    private const string MaterialPath = "Assets/Materials/Grass_Center_URP.mat";
    private const string MeshPath = "Assets/Models/Grass_Center_Clumps.asset";
    private const string TexturePath = "Assets/Textures/Grass/Grass_Small_B.png";

    [MenuItem("Tools/Trick or Loop/Create Central Scarecrow Grass Prefab")]
    public static void CreatePrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null &&
            !EditorUtility.DisplayDialog("Central Grass", "Grass_Center_Scarecrow.prefab ya existe. ¿Quieres reemplazarlo?", "Reemplazar", "Cancelar"))
            return;

        EnsureFolder("Assets/Prefabs/Environment");
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Models");

        var grassTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (grassTexture == null || shader == null)
        {
            EditorUtility.DisplayDialog("Central Grass", "No encontré el asset Grass_Small_B o el shader URP/Lit.", "OK");
            return;
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            material = new Material(shader) { name = "Grass_Center_URP" };
            ConfigureMaterial(material, grassTexture);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }
        else
        {
            material.shader = shader;
            ConfigureMaterial(material, grassTexture);
            EditorUtility.SetDirty(material);
        }

        var mesh = BuildGrassMesh();
        var oldMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
        if (oldMesh != null)
        {
            AssetDatabase.DeleteAsset(MeshPath);
        }
        AssetDatabase.CreateAsset(mesh, MeshPath);

        Scene previewScene = EditorSceneManager.NewPreviewScene();
        GameObject root = null;
        try
        {
            root = new GameObject("Grass_Center_Scarecrow");
            SceneManager.MoveGameObjectToScene(root, previewScene);
            var filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshPath);
            var renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            EditorSceneManager.ClosePreviewScene(previewScene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
        EditorUtility.DisplayDialog("Central Grass", "Prefab creado con 2400 mechones de pasto en un disco de 60 m de diámetro para cubrir el círculo central hasta la acera. La escena no se modificó.", "OK");
    }

    private static void ConfigureMaterial(Material material, Texture2D grassTexture)
    {
        material.SetTexture("_BaseMap", grassTexture);
        material.SetColor("_BaseColor", new Color(0.34f, 0.48f, 0.20f, 1f));
        if (material.HasProperty("_AlphaClip")) material.SetFloat("_AlphaClip", 1f);
        if (material.HasProperty("_Cutoff")) material.SetFloat("_Cutoff", 0.36f);
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 0f);
        if (material.HasProperty("_Cull")) material.SetFloat("_Cull", (float)CullMode.Off);
        material.EnableKeyword("_ALPHATEST_ON");
        material.renderQueue = (int)RenderQueue.AlphaTest;
    }

    private static Mesh BuildGrassMesh()
    {
        const int clumpCount = 2400;
        const float outerRadius = 30.0f;
        const float innerRadius = 1.0f;
        var random = new System.Random(236625);
        var vertices = new List<Vector3>(clumpCount * 16);
        var uv = new List<Vector2>(clumpCount * 16);
        var triangles = new List<int>(clumpCount * 24);

        for (int i = 0; i < clumpCount; i++)
        {
            double angle = random.NextDouble() * Math.PI * 2.0;
            float radial = Mathf.Sqrt(innerRadius * innerRadius + (float)random.NextDouble() * (outerRadius * outerRadius - innerRadius * innerRadius));
            float x = Mathf.Cos((float)angle) * radial + ((float)random.NextDouble() - 0.5f) * 0.20f;
            float z = Mathf.Sin((float)angle) * radial + ((float)random.NextDouble() - 0.5f) * 0.20f;
            float width = Mathf.Lerp(1.0f, 1.45f, (float)random.NextDouble());
            float height = Mathf.Lerp(1.20f, 1.85f, (float)random.NextDouble());
            float yaw = (float)random.NextDouble() * 360f;
            AddBladeCard(vertices, uv, triangles, new Vector3(x, 0.015f, z), width, height, yaw);
            AddBladeCard(vertices, uv, triangles, new Vector3(x, 0.015f, z), width * 0.9f, height * 0.94f, yaw + 90f);
        }

        var mesh = new Mesh { name = "Grass_Center_Clumps_Mesh", indexFormat = IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private static void AddBladeCard(List<Vector3> vertices, List<Vector2> uv, List<int> triangles, Vector3 center, float width, float height, float yaw)
    {
        int start = vertices.Count;
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 right = rotation * Vector3.right * (width * 0.5f);
        vertices.Add(center - right);
        vertices.Add(center + right);
        vertices.Add(center + right + Vector3.up * height);
        vertices.Add(center - right + Vector3.up * height);
        uv.Add(new Vector2(0f, 0f));
        uv.Add(new Vector2(1f, 0f));
        uv.Add(new Vector2(1f, 1f));
        uv.Add(new Vector2(0f, 1f));
        triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 1);
        triangles.Add(start); triangles.Add(start + 3); triangles.Add(start + 2);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string folder = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
