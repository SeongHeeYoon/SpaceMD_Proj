using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class NewMapSceneNatureResizer {
    public static void ResizeNatures() {
        string scenePath = "Assets/Scenes/NewMap.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) {
            Debug.LogError("Failed to load scene: " + scenePath);
            EditorApplication.Exit(1);
            return;
        }

        List<Transform> naturesToResize = new List<Transform>();

        // 1. Map/Natures 부모 오브젝트가 존재하는지 확인
        GameObject naturesRoot = GameObject.Find("Map/Natures");
        if (naturesRoot != null) {
            Debug.Log("[Resizer] Found 'Map/Natures' root. Filtering its children...");
            Transform naturesTransform = naturesRoot.transform;
            for (int i = 0; i < naturesTransform.childCount; i++) {
                Transform child = naturesTransform.GetChild(i);
                string nameLower = child.name.ToLower();

                // 나무, 바위, 부쉬 키워드 필터링
                if (nameLower.Contains("tree") || nameLower.Contains("rock") || nameLower.Contains("bush")) {
                    naturesToResize.Add(child);
                }
            }
        } else {
            Debug.LogWarning("[Resizer] 'Map/Natures' not found. Scanning entire scene with filters...");
            // 2. 존재하지 않는 경우 씬 전체 탐색
            GameObject[] rootObjects = scene.GetRootGameObjects();
            System.Action<Transform> collectNatures = null;
            collectNatures = (transform) => {
                GameObject go = transform.gameObject;
                string nameLower = go.name.ToLower();

                // 프리팹 경로 수집
                string prefabPath = "";
                if (PrefabUtility.IsPartOfAnyPrefab(go)) {
                    GameObject prefabParent = PrefabUtility.GetCorrespondingObjectFromSource(go);
                    if (prefabParent != null) {
                        prefabPath = AssetDatabase.GetAssetPath(prefabParent);
                    }
                }
                string pathLower = prefabPath.ToLower();

                bool isTarget = false;
                if (nameLower.Contains("tree") || nameLower.Contains("rock") || nameLower.Contains("bush") ||
                    pathLower.Contains("/natures/") || pathLower.Contains("/nature/")) {
                    
                    isTarget = true;
                    
                    if (nameLower.Contains("grass") || nameLower.Contains("flower") || nameLower.Contains("plant") || nameLower.Contains("lane")) {
                        isTarget = false;
                    }
                }

                if (isTarget) {
                    naturesToResize.Add(transform);
                    return; // 자식 탐색 중단
                }

                for (int i = 0; i < transform.childCount; i++) {
                    collectNatures(transform.GetChild(i));
                }
            };

            foreach (GameObject rootObj in rootObjects) {
                collectNatures(rootObj.transform);
            }
        }

        Debug.Log($"[Resizer] Collected {naturesToResize.Count} nature objects to resize.");

        int count = 0;
        foreach (Transform t in naturesToResize) {
            Vector3 prevScale = t.localScale;
            t.localScale = new Vector3(1.3f, 1.5f, 1.3f);
            Debug.Log($"[Resizer] Resized Nature: '{t.name}' (Path: {GetGameObjectPath(t)}) | Scale: {prevScale} -> {t.localScale}");
            count++;
        }

        if (count > 0) {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Resizer] Successfully resized {count} nature objects and saved the scene.");
        } else {
            Debug.LogWarning("[Resizer] No nature objects (tree, rock, bush) were found to resize.");
        }

        EditorApplication.Exit(0);
    }

    private static string GetGameObjectPath(Transform transform) {
        string path = transform.name;
        while (transform.parent != null) {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
