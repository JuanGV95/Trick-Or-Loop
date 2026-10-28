using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ApplyTreeBoundaryLayout
{
    private const string ScenePath = "Assets/Scenes/Escenario_1.unity";
    private const string HorrorPath = "Assets/Prefabs/Environment/TreeBoundary/TreeBoundary_Horror.prefab";
    private const string FoliagePath = "Assets/Prefabs/Environment/TreeBoundary/TreeBoundary_Foliage_120.prefab";
    private const string AudioPath = "Assets/Prefabs/Environment/Forest_Audio_Object.prefab";
    private const string OutputPath = "Assets/Prefabs/Environment/TreeBoundary/ForestBackgroundSFX.prefab";
    private const string ZoneScriptPath = "Assets/Scripts/Environment/TreeBoundaryAudioZone.cs";
    private const string RootName = "ForestBackgroundSFX";

    [MenuItem("Tools/Trick or Loop/Build ForestBackgroundSFX Border")]
    public static void Apply()
    {
        GameObject horrorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HorrorPath);
        GameObject foliagePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FoliagePath);
        GameObject audioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AudioPath);
        MonoScript zoneScript = AssetDatabase.LoadAssetAtPath<MonoScript>(ZoneScriptPath);
        if (horrorPrefab == null || foliagePrefab == null || audioPrefab == null || zoneScript == null)
            throw new InvalidOperationException("Falta algún prefab o el script de audio del borde.");

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        GameObject terrain = Find(scene, "Terreno-Vecindario");
        if (terrain == null) throw new InvalidOperationException("No se encontró Terreno-Vecindario en Escenario_1.");

        BoxCollider boundaryCollider = terrain.GetComponentInChildren<BoxCollider>();
        if (boundaryCollider == null) throw new InvalidOperationException("Terreno-Vecindario no tiene BoxCollider para obtener su límite exterior.");
        Bounds border = boundaryCollider.bounds;
        Vector3 center = new Vector3(border.center.x, border.max.y, border.center.z);
        float halfX = border.extents.x;
        float halfZ = border.extents.z;

        // Keep the result as one prefab instance in the scene, removing only the two prior standalone rows.
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
        {
            if (sceneRoot.name == RootName) continue;
            if (sceneRoot.name == "TreeBoundary_Horror" || sceneRoot.name == "TreeBoundary_Foliage_120")
                UnityEngine.Object.DestroyImmediate(sceneRoot);
        }

        GameObject group = new GameObject(RootName);
        GameObject horror = (GameObject)PrefabUtility.InstantiatePrefab(horrorPrefab);
        GameObject foliage = (GameObject)PrefabUtility.InstantiatePrefab(foliagePrefab);
        horror.name = "TreeBoundary_Horror";
        foliage.name = "TreeBoundary_Foliage_120";
        horror.transform.SetParent(group.transform, false);
        foliage.transform.SetParent(group.transform, false);
        horror.transform.localPosition = Vector3.zero;
        foliage.transform.localPosition = Vector3.zero;

        // Scale each forest prefab so its horizontal bounds meet the rectangular terrain edges.
        horror.transform.localScale = new Vector3(halfX / 64.7f, 1f, halfZ / 64.8f);
        foliage.transform.localScale = new Vector3(halfX / 69.4f, 1f, halfZ / 69.3f);

        GameObject zone = new GameObject("Border_Audio_Zone");
        zone.transform.SetParent(group.transform, false);
        GameObject audio = (GameObject)PrefabUtility.InstantiatePrefab(audioPrefab);
        audio.name = "Forest_Audio_Object";
        audio.transform.SetParent(zone.transform, false);
        TreeBoundaryAudioZone controller = zone.AddComponent(zoneScript.GetClass()) as TreeBoundaryAudioZone;
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("woodSource").objectReferenceValue = audio.transform.Find("Wood_Creak_Audio").GetComponent<AudioSource>();
        so.FindProperty("distantSource").objectReferenceValue = audio.transform.Find("Distant_Flyby_Audio").GetComponent<AudioSource>();
        so.FindProperty("halfExtentX").floatValue = halfX;
        so.FindProperty("halfExtentZ").floatValue = halfZ;
        so.FindProperty("activationDistance").floatValue = 18f;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(group, OutputPath);
        UnityEngine.Object.DestroyImmediate(group);
        if (prefabAsset == null) throw new InvalidOperationException("No se pudo guardar ForestBackgroundSFX.prefab.");

        GameObject previous = Find(scene, RootName);
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, scene);
        instance.name = RootName;
        instance.transform.SetPositionAndRotation(center, Quaternion.identity);
        instance.transform.localScale = Vector3.one;
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("No se pudo guardar Escenario_1.");
        AssetDatabase.SaveAssets();
        Debug.Log("ForestBackgroundSFX creado y colocado en el límite rectangular de Terreno-Vecindario (" + (halfX * 2f).ToString("F1") + " x " + (halfZ * 2f).ToString("F1") + " m). Audio de madera y sonido lejano se activa a 18 m del borde.");
    }

    private static GameObject Find(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == objectName) return t.gameObject;
        return null;
    }
}
