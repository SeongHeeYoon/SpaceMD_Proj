using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class MDMapCreator {
    struct RoadInfo {
        public int start;
        public int end;
    }

    public static void CreateMap() {
        // 1. Create/Ensure the MD_MAP scene is active
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "MD_MAP") {
            string scenePath = "Assets/Scenes/MD_MAP.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null) {
                activeScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                activeScene.name = "MD_MAP";
                EditorSceneManager.SaveScene(activeScene, scenePath);
            } else {
                activeScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        // 2. Clean up ALL existing objects in the scene
        GameObject[] roots = activeScene.GetRootGameObjects();
        foreach (GameObject root in roots) {
            Object.DestroyImmediate(root);
        }

        // 3. Recreate the Main Camera and Directional Light
        GameObject cameraGo = new GameObject("Main Camera");
        cameraGo.tag = "MainCamera";
        Camera cam = cameraGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color32(44, 110, 120, 255);
        cam.orthographic = true;
        cam.farClipPlane = 3000f;

        GameObject lightGo = new GameObject("Directional Light");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        light.intensity = 1.2f;

        // 4. Load prefabs
        string roadsFolder = "Assets/ExternalAssets/SimplePoly City - Low Poly Assets/Prefab/Roads/";
        string naturesFolder = "Assets/ExternalAssets/SimplePoly City - Low Poly Assets/Prefab/Natures/";
        string buildingsFolder = "Assets/ExternalAssets/SimplePoly City - Low Poly Assets/Prefab/Buildings/";

        GameObject grassTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Grass Tile.prefab");
        GameObject roadTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(roadsFolder + "Road Tile.prefab");
        GameObject splitLinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(roadsFolder + "Road Split Line.prefab");

        if (grassTilePrefab == null || roadTilePrefab == null || splitLinePrefab == null) {
            Debug.LogError("Failed to load grass, road or split line prefabs.");
            return;
        }

        // Load Building prefabs
        GameObject[] bigSkyPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building Sky_big_color01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building Sky_big_color02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building Sky_big_color03.prefab")
        };
        GameObject[] smallSkyPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building Sky_small_color01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building Sky_small_color02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building Sky_small_color03.prefab")
        };
        GameObject[] shopPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Bakery.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Bar.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Books Shop.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Chicken Shop.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Clothing.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Coffee Shop.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Drug Store.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Fast Food.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Fruits  Shop.prefab"), // Two spaces
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Gift Shop.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Music Store.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Pizza.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Restaurant.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Shoes Shop.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Super Market.prefab")
        };
        GameObject[] housePrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_01_color01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_01_color02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_01_color03.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_02_color01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_02_color02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_02_color03.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_03_color01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_03_color02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_03_color03.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_04_color01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_04_color02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_House_04_color03.prefab")
        };
        GameObject[] residentialPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Residential_color01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Residential_color02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Residential_color03.prefab")
        };
        GameObject stadiumPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Stadium.prefab");
        GameObject[] industrialPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Factory.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Auto Service.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(buildingsFolder + "Building_Gas Station.prefab")
        };

        // Load Natures
        GameObject[] treePrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Big Tree.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Cube Tree.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Fir Tree.prefab")
        };
        GameObject[] bushPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Bush_01.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Bush_02.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Bush_03.prefab")
        };
        GameObject[] rockPrefabs = new GameObject[] {
            AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Rock_Big.prefab"),
            AssetDatabase.LoadAssetAtPath<GameObject>(naturesFolder + "Natures_Rock_small.prefab")
        };

        // Log safety errors
        foreach (var p in bigSkyPrefabs) if (p == null) Debug.LogError("Null Big Sky");
        foreach (var p in smallSkyPrefabs) if (p == null) Debug.LogError("Null Small Sky");
        foreach (var p in shopPrefabs) if (p == null) Debug.LogError("Null Shop");
        foreach (var p in housePrefabs) if (p == null) Debug.LogError("Null House");
        foreach (var p in residentialPrefabs) if (p == null) Debug.LogError("Null Residential");
        if (stadiumPrefab == null) Debug.LogError("Null Stadium");
        foreach (var p in industrialPrefabs) if (p == null) Debug.LogError("Null Industrial");
        foreach (var p in treePrefabs) if (p == null) Debug.LogError("Null Tree");
        foreach (var p in bushPrefabs) if (p == null) Debug.LogError("Null Bush");
        foreach (var p in rockPrefabs) if (p == null) Debug.LogError("Null Rock");

        // Programmatically calculate tile sizes
        float tileSizeX = 20f;
        float tileSizeZ = 20f;
        
        Renderer grassRenderer = grassTilePrefab.GetComponentInChildren<Renderer>();
        if (grassRenderer != null) {
            tileSizeX = grassRenderer.bounds.size.x;
            tileSizeZ = grassRenderer.bounds.size.z;
            Debug.Log($"Calculated Grass Tile size: {tileSizeX} x {tileSizeZ}");
        }

        // 5. Position Camera over the center of the 100x100 grid (2000m x 2000m)
        float mapCenter = 49.5f * tileSizeX;
        cameraGo.transform.position = new Vector3(mapCenter, 1500f, mapCenter);
        cameraGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.orthographicSize = 1100f;

        // 6. Create Parent Structures
        GameObject mapRoot = new GameObject("MapGrid");
        GameObject grassRoot = new GameObject("GrassGrid");
        GameObject roadRoot = new GameObject("RoadGrid");
        GameObject markingsRoot = new GameObject("RoadMarkings");
        GameObject buildingsRoot = new GameObject("Buildings");
        GameObject naturesRoot = new GameObject("Natures");
        
        grassRoot.transform.SetParent(mapRoot.transform);
        roadRoot.transform.SetParent(mapRoot.transform);
        markingsRoot.transform.SetParent(mapRoot.transform);
        buildingsRoot.transform.SetParent(mapRoot.transform);
        naturesRoot.transform.SetParent(mapRoot.transform);

        // Define Road Configurations
        RoadInfo[] verticalRoads = new RoadInfo[] {
            new RoadInfo { start = 38, end = 45 },
            new RoadInfo { start = 54, end = 61 },
            new RoadInfo { start = 17, end = 20 },
            new RoadInfo { start = 7,  end = 9  },
            new RoadInfo { start = 28, end = 30 },
            new RoadInfo { start = 79, end = 81 }
        };

        RoadInfo[] horizontalRoads = new RoadInfo[] {
            new RoadInfo { start = 46, end = 53 },
            new RoadInfo { start = 20, end = 23 },
            new RoadInfo { start = 75, end = 78 },
            new RoadInfo { start = 63, end = 65 }
        };

        // 7. Track Road Layout via 2D Grid Tracker to avoid overlapping
        int size = 100;
        bool[,] hasRoad = new bool[size, size];

        foreach (var r in verticalRoads) {
            for (int z = 0; z < size; z++) {
                for (int x = r.start; x <= r.end; x++) {
                    hasRoad[x, z] = true;
                }
            }
        }

        foreach (var r in horizontalRoads) {
            for (int x = 0; x < size; x++) {
                for (int z = r.start; z <= r.end; z++) {
                    hasRoad[x, z] = true;
                }
            }
        }

        // Helper functions for checking road containment
        System.Func<int, bool> IsVerticalRoadColumn = (col) => {
            foreach (var r in verticalRoads) {
                if (col >= r.start && col <= r.end) return true;
            }
            return false;
        };

        System.Func<int, bool> IsHorizontalRoadRow = (row) => {
            foreach (var r in horizontalRoads) {
                if (row >= r.start && row <= r.end) return true;
            }
            return false;
        };

        // Spawn markings helper
        System.Action<float, float, float, Quaternion, Vector3> SpawnSplitLine = (posX, posY, posZ, rot, scale) => {
            GameObject line = PrefabUtility.InstantiatePrefab(splitLinePrefab) as GameObject;
            if (line != null) {
                line.transform.position = new Vector3(posX, posY, posZ);
                line.transform.rotation = rot;
                line.transform.localScale = scale;
                line.transform.SetParent(markingsRoot.transform);
            }
        };

        // 8. Generate 100x100 Grass Grid & Road Grid
        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                // Spawn Grass
                GameObject grassTile = PrefabUtility.InstantiatePrefab(grassTilePrefab) as GameObject;
                if (grassTile != null) {
                    grassTile.transform.position = new Vector3(x * tileSizeX, 0f, z * tileSizeZ);
                    grassTile.transform.rotation = Quaternion.identity;
                    grassTile.transform.localScale = Vector3.one;
                    grassTile.transform.SetParent(grassRoot.transform);
                }

                // Spawn Road if marked
                if (hasRoad[x, z]) {
                    GameObject roadTile = PrefabUtility.InstantiatePrefab(roadTilePrefab) as GameObject;
                    if (roadTile != null) {
                        roadTile.transform.position = new Vector3(x * tileSizeX, 0.1f, z * tileSizeZ);
                        roadTile.transform.rotation = Quaternion.identity;
                        roadTile.transform.localScale = Vector3.one;
                        roadTile.transform.SetParent(roadRoot.transform);
                    }
                }
            }
        }

        // 9. Generate Single Center Lines and Outer Edge Lines for each road tile at Y = 0.12 (Skipping Intersections)
        float centerLineY = 0.12f;

        bool[,] spawnedVEdge = new bool[101, 200];
        bool[,] spawnedHEdge = new bool[101, 200];

        Vector3 scaleZ25 = new Vector3(1f, 1f, 2.5f);

        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                bool isVert = IsVerticalRoadColumn(x);
                bool isHoriz = IsHorizontalRoadRow(z);

                // Skip if this tile is an intersection of both vertical and horizontal roads
                if (isVert && isHoriz) {
                    continue;
                }

                if (isVert && !isHoriz) {
                    // Vertical road:
                    // 1. Center lines (two segments of 10 units each along Z, scale Z = 1.0)
                    SpawnSplitLine(x * tileSizeX + 10f, centerLineY, z * tileSizeZ + 5f, Quaternion.identity, Vector3.one);
                    SpawnSplitLine(x * tileSizeX + 10f, centerLineY, z * tileSizeZ + 15f, Quaternion.identity, Vector3.one);

                    // 2. Left Edge: grid line x, segments z*2 and z*2 + 1, scale Z = 2.5
                    if (!spawnedVEdge[x, z * 2]) {
                        SpawnSplitLine(x * tileSizeX, centerLineY, z * tileSizeZ + 5f, Quaternion.identity, scaleZ25);
                        spawnedVEdge[x, z * 2] = true;
                    }
                    if (!spawnedVEdge[x, z * 2 + 1]) {
                        SpawnSplitLine(x * tileSizeX, centerLineY, z * tileSizeZ + 15f, Quaternion.identity, scaleZ25);
                        spawnedVEdge[x, z * 2 + 1] = true;
                    }

                    // 3. Right Edge: grid line x+1, segments z*2 and z*2 + 1, scale Z = 2.5
                    if (!spawnedVEdge[x + 1, z * 2]) {
                        SpawnSplitLine((x + 1) * tileSizeX, centerLineY, z * tileSizeZ + 5f, Quaternion.identity, scaleZ25);
                        spawnedVEdge[x + 1, z * 2] = true;
                    }
                    if (!spawnedVEdge[x + 1, z * 2 + 1]) {
                        SpawnSplitLine((x + 1) * tileSizeX, centerLineY, z * tileSizeZ + 15f, Quaternion.identity, scaleZ25);
                        spawnedVEdge[x + 1, z * 2 + 1] = true;
                    }
                }
                else if (isHoriz && !isVert) {
                    // Horizontal road:
                    // 1. Center lines (two segments of 10 units each along X, scale Z = 1.0)
                    Quaternion rot90 = Quaternion.Euler(0f, 90f, 0f);
                    SpawnSplitLine(x * tileSizeX + 5f, centerLineY, z * tileSizeZ + 10f, rot90, Vector3.one);
                    SpawnSplitLine(x * tileSizeX + 15f, centerLineY, z * tileSizeZ + 10f, rot90, Vector3.one);

                    // 2. Bottom Edge: grid line z, segments x*2 and x*2 + 1, scale Z = 2.5
                    if (!spawnedHEdge[z, x * 2]) {
                        SpawnSplitLine(x * tileSizeX + 5f, centerLineY, z * tileSizeZ, rot90, scaleZ25);
                        spawnedHEdge[z, x * 2] = true;
                    }
                    if (!spawnedHEdge[z, x * 2 + 1]) {
                        SpawnSplitLine(x * tileSizeX + 15f, centerLineY, z * tileSizeZ, rot90, scaleZ25);
                        spawnedHEdge[z, x * 2 + 1] = true;
                    }

                    // 3. Top Edge: grid line z+1, segments x*2 and x*2 + 1, scale Z = 2.5
                    if (!spawnedHEdge[z + 1, x * 2]) {
                        SpawnSplitLine(x * tileSizeX + 5f, centerLineY, (z + 1) * tileSizeZ, rot90, scaleZ25);
                        spawnedHEdge[z + 1, x * 2] = true;
                    }
                    if (!spawnedHEdge[z + 1, x * 2 + 1]) {
                        SpawnSplitLine(x * tileSizeX + 15f, centerLineY, (z + 1) * tileSizeZ, rot90, scaleZ25);
                        spawnedHEdge[z + 1, x * 2 + 1] = true;
                    }
                }
            }
        }

        // 10. Generate Buildings and Natures on Grass Tiles
        System.Func<int, int, (int dist, string dir)> GetClosestRoad = (tx, tz) => {
            int minDist = 9999;
            string bestDir = "S";
            for (int d = 1; d <= 15; d++) {
                if (tz - d >= 0 && hasRoad[tx, tz - d]) {
                    if (d < minDist) { minDist = d; bestDir = "S"; }
                }
                if (tz + d < size && hasRoad[tx, tz + d]) {
                    if (d < minDist) { minDist = d; bestDir = "N"; }
                }
                if (tx - d >= 0 && hasRoad[tx - d, tz]) {
                    if (d < minDist) { minDist = d; bestDir = "W"; }
                }
                if (tx + d < size && hasRoad[tx + d, tz]) {
                    if (d < minDist) { minDist = d; bestDir = "E"; }
                }
                if (minDist < 9999) break;
            }
            return (minDist, bestDir);
        };

        System.Func<int, int, float> GetNoise = (tx, tz) => {
            uint val = (uint)((tx * 73856093) ^ (tz * 19349663));
            val = val * 2246822507u + 3266489917u;
            return (float)(val % 10000u) / 10000f;
        };

        for (int x = 0; x < size; x++) {
            for (int z = 0; z < size; z++) {
                if (hasRoad[x, z]) {
                    continue;
                }

                var roadInfo = GetClosestRoad(x, z);
                int dist = roadInfo.dist;
                string rDir = roadInfo.dir;

                Quaternion buildingRotation = Quaternion.identity;
                if (rDir == "S") buildingRotation = Quaternion.identity;
                else if (rDir == "N") buildingRotation = Quaternion.Euler(0f, 180f, 0f);
                else if (rDir == "W") buildingRotation = Quaternion.Euler(0f, 90f, 0f);
                else if (rDir == "E") buildingRotation = Quaternion.Euler(0f, 270f, 0f);

                float noiseVal = GetNoise(x, z);
                int seed = (int)(noiseVal * 10000f);

                // 10.1 Stadium Park Zone
                if (x >= 11 && x <= 15 && z >= 11 && z <= 15) {
                    if (x == 13 && z == 13) {
                        GameObject stadium = PrefabUtility.InstantiatePrefab(stadiumPrefab) as GameObject;
                        if (stadium != null) {
                            stadium.transform.position = new Vector3(x * tileSizeX, 0.1f, z * tileSizeZ);
                            stadium.transform.rotation = Quaternion.identity;
                            stadium.transform.localScale = Vector3.one;
                            stadium.transform.SetParent(buildingsRoot.transform);
                        }
                    } else {
                        if (noiseVal < 0.6f) {
                            GameObject naturePrefab = (seed % 2 == 0) 
                                ? treePrefabs[seed % treePrefabs.Length] 
                                : bushPrefabs[seed % bushPrefabs.Length];
                            GameObject nature = PrefabUtility.InstantiatePrefab(naturePrefab) as GameObject;
                            if (nature != null) {
                                nature.transform.position = new Vector3(x * tileSizeX, 0f, z * tileSizeZ);
                                nature.transform.rotation = Quaternion.Euler(0f, noiseVal * 360f, 0f);
                                nature.transform.localScale = Vector3.one;
                                nature.transform.SetParent(naturesRoot.transform);
                            }
                        }
                    }
                }
                // 10.2 Industrial Zone
                else if (x >= 82 && z <= 19) {
                    if (dist == 1) {
                        if (noiseVal < 0.7f) {
                            GameObject indPrefab = industrialPrefabs[seed % industrialPrefabs.Length];
                            GameObject bld = PrefabUtility.InstantiatePrefab(indPrefab) as GameObject;
                            if (bld != null) {
                                bld.transform.position = new Vector3(x * tileSizeX, 0.1f, z * tileSizeZ);
                                bld.transform.rotation = buildingRotation;
                                bld.transform.localScale = Vector3.one;
                                bld.transform.SetParent(buildingsRoot.transform);
                            }
                        }
                    } else {
                        if (noiseVal < 0.35f) {
                            if (seed % 3 == 0) {
                                GameObject rockPrefab = rockPrefabs[seed % rockPrefabs.Length];
                                GameObject rk = PrefabUtility.InstantiatePrefab(rockPrefab) as GameObject;
                                if (rk != null) {
                                    rk.transform.position = new Vector3(x * tileSizeX, 0f, z * tileSizeZ);
                                    rk.transform.rotation = Quaternion.Euler(0f, noiseVal * 360f, 0f);
                                    rk.transform.localScale = Vector3.one;
                                    rk.transform.SetParent(naturesRoot.transform);
                                }
                            } else {
                                GameObject indPrefab = industrialPrefabs[seed % industrialPrefabs.Length];
                                GameObject bld = PrefabUtility.InstantiatePrefab(indPrefab) as GameObject;
                                if (bld != null) {
                                    bld.transform.position = new Vector3(x * tileSizeX, 0.1f, z * tileSizeZ);
                                    bld.transform.rotation = buildingRotation;
                                    bld.transform.localScale = Vector3.one;
                                    bld.transform.SetParent(buildingsRoot.transform);
                                }
                            }
                        }
                    }
                }
                // 10.3 Downtown Zone
                else if (x >= 31 && x <= 78 && z >= 24 && z <= 74) {
                    if (dist == 1) {
                        if (noiseVal < 0.85f) {
                            float bType = noiseVal / 0.85f;
                            GameObject bldPrefab;
                            if (bType < 0.35f) {
                                bldPrefab = (seed % 2 == 0) 
                                    ? bigSkyPrefabs[seed % bigSkyPrefabs.Length] 
                                    : smallSkyPrefabs[seed % smallSkyPrefabs.Length];
                            } else if (bType < 0.75f) {
                                bldPrefab = shopPrefabs[seed % shopPrefabs.Length];
                            } else {
                                bldPrefab = residentialPrefabs[seed % residentialPrefabs.Length];
                            }

                            GameObject bld = PrefabUtility.InstantiatePrefab(bldPrefab) as GameObject;
                            if (bld != null) {
                                bld.transform.position = new Vector3(x * tileSizeX, 0.1f, z * tileSizeZ);
                                bld.transform.rotation = buildingRotation;
                                bld.transform.localScale = Vector3.one;
                                bld.transform.SetParent(buildingsRoot.transform);
                            }
                        } else {
                            if (seed % 2 == 0) {
                                GameObject treePrefab = treePrefabs[seed % treePrefabs.Length];
                                GameObject tree = PrefabUtility.InstantiatePrefab(treePrefab) as GameObject;
                                if (tree != null) {
                                    tree.transform.position = new Vector3(x * tileSizeX, 0f, z * tileSizeZ);
                                    tree.transform.rotation = Quaternion.Euler(0f, noiseVal * 360f, 0f);
                                    tree.transform.localScale = Vector3.one;
                                    tree.transform.SetParent(naturesRoot.transform);
                                }
                            }
                        }
                    } else {
                        if (noiseVal < 0.4f) {
                            GameObject bldPrefab = (seed % 2 == 0) 
                                ? smallSkyPrefabs[seed % smallSkyPrefabs.Length] 
                                : shopPrefabs[seed % shopPrefabs.Length];
                            GameObject bld = PrefabUtility.InstantiatePrefab(bldPrefab) as GameObject;
                            if (bld != null) {
                                bld.transform.position = new Vector3(x * tileSizeX, 0.1f, z * tileSizeZ);
                                bld.transform.rotation = buildingRotation;
                                bld.transform.localScale = Vector3.one;
                                bld.transform.SetParent(buildingsRoot.transform);
                            }
                        } else if (noiseVal < 0.75f) {
                            GameObject naturePrefab = (seed % 2 == 0) 
                                ? treePrefabs[seed % treePrefabs.Length] 
                                : bushPrefabs[seed % bushPrefabs.Length];
                            GameObject nature = PrefabUtility.InstantiatePrefab(naturePrefab) as GameObject;
                            if (nature != null) {
                                nature.transform.position = new Vector3(x * tileSizeX, 0f, z * tileSizeZ);
                                nature.transform.rotation = Quaternion.Euler(0f, noiseVal * 360f, 0f);
                                nature.transform.localScale = Vector3.one;
                                nature.transform.SetParent(naturesRoot.transform);
                            }
                        }
                    }
                }
                // 10.4 Residential / Suburban Zone
                else {
                    if (dist == 1) {
                        if (noiseVal < 0.75f) {
                            GameObject bldPrefab = (seed % 5 == 0) 
                                ? residentialPrefabs[seed % residentialPrefabs.Length] 
                                : housePrefabs[seed % housePrefabs.Length];
                            GameObject bld = PrefabUtility.InstantiatePrefab(bldPrefab) as GameObject;
                            if (bld != null) {
                                bld.transform.position = new Vector3(x * tileSizeX, 0.1f, z * tileSizeZ);
                                bld.transform.rotation = buildingRotation;
                                bld.transform.localScale = Vector3.one;
                                bld.transform.SetParent(buildingsRoot.transform);
                            }
                        }
                    } else {
                        if (noiseVal < 0.55f) {
                            if (seed % 6 == 0) {
                                GameObject bld = PrefabUtility.InstantiatePrefab(housePrefabs[seed % housePrefabs.Length]) as GameObject;
                                if (bld != null) {
                                    bld.transform.position = new Vector3(x * tileSizeX, 0.1f, z * tileSizeZ);
                                    bld.transform.rotation = buildingRotation;
                                    bld.transform.localScale = Vector3.one;
                                    bld.transform.SetParent(buildingsRoot.transform);
                                }
                            } else {
                                GameObject naturePrefab = (seed % 3 != 0) 
                                    ? treePrefabs[seed % treePrefabs.Length] 
                                    : bushPrefabs[seed % bushPrefabs.Length];
                                GameObject nature = PrefabUtility.InstantiatePrefab(naturePrefab) as GameObject;
                                if (nature != null) {
                                    nature.transform.position = new Vector3(x * tileSizeX, 0f, z * tileSizeZ);
                                    nature.transform.rotation = Quaternion.Euler(0f, noiseVal * 360f, 0f);
                                    nature.transform.localScale = Vector3.one;
                                    nature.transform.SetParent(naturesRoot.transform);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 11. Save scene
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene, "Assets/Scenes/MD_MAP.unity");
        Debug.Log("MD_MAP 100x100 grid with centered and edge road markings (Selective Scale Z = 2.5 on edges) and buildings/natures generated successfully.");
    }
}
