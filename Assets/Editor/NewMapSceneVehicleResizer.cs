using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class NewMapSceneVehicleResizer {
    public static void ResizeVehicles() {
        string scenePath = "Assets/Scenes/NewMap.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid()) {
            Debug.LogError("Failed to load scene: " + scenePath);
            EditorApplication.Exit(1);
            return;
        }

        List<Transform> vehiclesToResize = new List<Transform>();

        // 1. Map/Vehicles가 존재하는지 확인
        GameObject vehiclesRoot = GameObject.Find("Map/Vehicles");
        if (vehiclesRoot != null) {
            Debug.Log("[Resizer] Found 'Map/Vehicles' root. Collecting its direct children...");
            Transform vehiclesTransform = vehiclesRoot.transform;
            for (int i = 0; i < vehiclesTransform.childCount; i++) {
                vehiclesToResize.Add(vehiclesTransform.GetChild(i));
            }
        } else {
            Debug.LogWarning("[Resizer] 'Map/Vehicles' not found. Scanning entire scene with filters...");
            // 2. 존재하지 않는 경우에만 씬 전체 탐색 + 엄격한 필터링 적용
            GameObject[] rootObjects = scene.GetRootGameObjects();
            System.Action<Transform> collectVehicles = null;
            collectVehicles = (transform) => {
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

                bool isVehicle = false;

                // 차량 키워드 포함
                if (pathLower.Contains("/vehicles/") || 
                    nameLower.Contains("vehicle") || 
                    nameLower.Contains("car") || 
                    nameLower.Contains("taxi") || 
                    nameLower.Contains("suv") || 
                    nameLower.Contains("truck") || 
                    nameLower.Contains("bus") || 
                    nameLower.Contains("van")) {
                    
                    isVehicle = true;

                    // 단, 차량이 아닌 것들 예외 필터링 (네거티브 필터)
                    if (nameLower.Contains("bush") || 
                        nameLower.Contains("stop") || 
                        nameLower.Contains("station") || 
                        nameLower.Contains("terminal") || 
                        nameLower.Contains("lane") || 
                        nameLower.Contains("sign") || 
                        nameLower.Contains("light") || 
                        nameLower.Contains("charger") || 
                        nameLower.Contains("spawner") || 
                        nameLower.Contains("point") ||
                        nameLower.Contains("waypoint") ||
                        nameLower.Contains("node") ||
                        pathLower.Contains("/props/") || 
                        pathLower.Contains("/natures/") || 
                        pathLower.Contains("/roads/") || 
                        pathLower.Contains("/buildings/")) {
                        
                        isVehicle = false;
                    }

                    // 부모 카테고리 필터링
                    if (transform.parent != null) {
                        string pName = transform.parent.name;
                        if (pName == "Roads" || pName == "Natures" || pName == "Buildings" || pName == "Props") {
                            isVehicle = false;
                        }
                    }
                }

                if (isVehicle) {
                    vehiclesToResize.Add(transform);
                    return; // 자식은 탐색 중단
                }

                for (int i = 0; i < transform.childCount; i++) {
                    collectVehicles(transform.GetChild(i));
                }
            };

            foreach (GameObject rootObj in rootObjects) {
                collectVehicles(rootObj.transform);
            }
        }

        Debug.Log($"[Resizer] Collected {vehiclesToResize.Count} vehicles to resize.");

        int count = 0;
        foreach (Transform t in vehiclesToResize) {
            Vector3 prevScale = t.localScale;
            t.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            Debug.Log($"[Resizer] Resized: '{t.name}' (Path: {GetGameObjectPath(t)}) | Scale: {prevScale} -> {t.localScale}");
            count++;
        }

        if (count > 0) {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Resizer] Successfully resized {count} vehicles and saved the scene.");
        } else {
            Debug.LogWarning("[Resizer] No vehicles were found to resize.");
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
