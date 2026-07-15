using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class NewMapSceneSorter {
    public static void SortScene() {
        string scenePath = "Assets/Scenes/NewMap.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) {
            Debug.LogError("Failed to load scene: " + scenePath);
            EditorApplication.Exit(1);
            return;
        }

        // 1. Ensure Map root exists
        GameObject mapRoot = GameObject.Find("Map");
        if (mapRoot == null) {
            mapRoot = new GameObject("Map");
            Debug.Log("Created 'Map' root GameObject.");
        }
        Transform mapTransform = mapRoot.transform;

        // 2. Ensure parent category folders exist under Map
        Transform roadsParent = GetOrCreateParent(mapTransform, "Roads");
        Transform naturesParent = GetOrCreateParent(mapTransform, "Natures");
        Transform buildingsParent = GetOrCreateParent(mapTransform, "Buildings");
        Transform vehiclesParent = GetOrCreateParent(mapTransform, "Vehicles");
        Transform propsParent = GetOrCreateParent(mapTransform, "Props");

        HashSet<Transform> categories = new HashSet<Transform> {
            roadsParent, naturesParent, buildingsParent, vehiclesParent, propsParent
        };

        // If an old 'Others' folder exists under Map, we will unpack it as well
        Transform othersParent = mapTransform.Find("Others");

        List<Transform> targets = new List<Transform>();
        List<GameObject> containersToDelete = new List<GameObject>();

        // 3. Recursive function to find leaf targets and container shells
        System.Action<Transform> collectObjects = null;
        collectObjects = (transform) => {
            GameObject go = transform.gameObject;

            // Stop recursion if we hit main categories
            if (categories.Contains(transform)) {
                // Collect all children nested inside existing categories to re-evaluate them
                List<Transform> nestedChildren = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++) {
                    nestedChildren.Add(transform.GetChild(i));
                }
                foreach (Transform child in nestedChildren) {
                    collectObjects(child);
                }
                return;
            }

            // Check if this object is a container shell (no renderer/collider and has children)
            bool isContainer = false;
            if (transform.childCount > 0 && !PrefabUtility.IsPartOfAnyPrefab(go)) {
                Renderer r = go.GetComponent<Renderer>();
                Collider c = go.GetComponent<Collider>();
                if (r == null && c == null) {
                    isContainer = true;
                }
            }

            if (isContainer) {
                // Record container for deletion, and recurse into children
                containersToDelete.Add(go);
                List<Transform> children = new List<Transform>();
                for (int i = 0; i < transform.childCount; i++) {
                    children.Add(transform.GetChild(i));
                }
                foreach (Transform child in children) {
                    collectObjects(child);
                }
            } else {
                // Leaf object or prefab instance, this is a target for classification
                targets.Add(transform);
            }
        };

        // 4. Collect objects from all roots
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject rootObj in rootObjects) {
            if (rootObj.name == "Main Camera" || rootObj.name == "Directional Light" || rootObj.tag == "MainCamera") {
                continue;
            }

            if (rootObj == mapRoot) {
                List<Transform> mapChildren = new List<Transform>();
                for (int i = 0; i < mapTransform.childCount; i++) {
                    mapChildren.Add(mapTransform.GetChild(i));
                }
                foreach (Transform child in mapChildren) {
                    collectObjects(child);
                }
            } else {
                collectObjects(rootObj.transform);
            }
        }

        Debug.Log($"Collected {targets.Count} leaf targets and {containersToDelete.Count} container shells.");

        // 5. Classify each target
        int roadsCount = 0, naturesCount = 0, buildingsCount = 0, vehiclesCount = 0, propsCount = 0;

        foreach (Transform target in targets) {
            GameObject obj = target.gameObject;
            string prefabPath = "";
            if (PrefabUtility.IsPartOfAnyPrefab(obj)) {
                GameObject prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                if (prefabParent != null) {
                    prefabPath = AssetDatabase.GetAssetPath(prefabParent);
                }
            }

            string nameLower = obj.name.ToLower();
            string pathLower = prefabPath.ToLower();

            // Prioritized classification rules
            if (pathLower.Contains("/roads/") || nameLower.Contains("road") || nameLower.Contains("intersection") || nameLower.Contains("lane") || nameLower.Contains("tile")) {
                target.SetParent(roadsParent, true);
                roadsCount++;
            }
            else if (pathLower.Contains("/natures/") || nameLower.Contains("nature") || nameLower.Contains("tree") || 
                     nameLower.Contains("bush") || nameLower.Contains("rock") || nameLower.Contains("grass") || nameLower.Contains("flower") || nameLower.Contains("plant")) {
                target.SetParent(naturesParent, true);
                naturesCount++;
            }
            else if (pathLower.Contains("/buildings/") || nameLower.Contains("building") || nameLower.Contains("house") || 
                     nameLower.Contains("shop") || nameLower.Contains("residential") || nameLower.Contains("stadium") || nameLower.Contains("factory")) {
                target.SetParent(buildingsParent, true);
                buildingsCount++;
            }
            else if (pathLower.Contains("/vehicles/") || nameLower.Contains("vehicle") || nameLower.Contains("car") || 
                     nameLower.Contains("taxi") || nameLower.Contains("suv") || nameLower.Contains("truck") || nameLower.Contains("bus") || nameLower.Contains("van")) {
                target.SetParent(vehiclesParent, true);
                vehiclesCount++;
            }
            else {
                // If it doesn't match roads, nature, building, or vehicle, default to Props (no "Others" category allowed as per request)
                target.SetParent(propsParent, true);
                propsCount++;
            }
        }

        // 6. Delete empty containers
        int deletedContainers = 0;
        // Delete in reverse order to delete deeper child containers first
        for (int i = containersToDelete.Count - 1; i >= 0; i--) {
            GameObject container = containersToDelete[i];
            if (container != null && container.transform.childCount == 0) {
                Object.DestroyImmediate(container);
                deletedContainers++;
            }
        }

        // Delete old 'Others' folder if empty
        if (othersParent != null && othersParent.childCount == 0) {
            Object.DestroyImmediate(othersParent.gameObject);
            Debug.Log("Deleted empty 'Others' folder.");
        }

        Debug.Log($"Sorting completed:\n" +
                  $"- Roads: {roadsCount}\n" +
                  $"- Natures: {naturesCount}\n" +
                  $"- Buildings: {buildingsCount}\n" +
                  $"- Vehicles: {vehiclesCount}\n" +
                  $"- Props: {propsCount}\n" +
                  $"- Empty Containers Cleaned: {deletedContainers}");

        // Save modifications to the scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        
        Debug.Log("Scene saved successfully.");
        EditorApplication.Exit(0);
    }

    private static Transform GetOrCreateParent(Transform parent, string name) {
        Transform child = parent.Find(name);
        if (child == null) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            child = go.transform;
        }
        return child;
    }
}
