using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [Tooltip("Number of horizontal cells in the maze.")]
    public int width = 10;
    
    [Tooltip("Number of vertical cells in the maze.")]
    public int height = 10;

    [Range(0f, 0.2f)]
    [Tooltip("Probability (0 to 0.2) of carving additional doors to create loops/coherent bifurcations.")]
    public float loopPercentage = 0.08f;

    [Header("Visual Prefabs")]
    [Tooltip("The 1x1x5 wall prefab. If left empty, we will try to load it from Assets/Recursos/laberinto/wall.")]
    public GameObject wallPrefab;

    [Tooltip("Optional prefab for the exit/goal. If empty, a beautiful glowing pedestal will be created procedurally.")]
    public GameObject goalPrefab;

    [Header("References")]
    [Tooltip("Reference to the player. If empty, we will try to find a GameObject named 'Capsule PLAYER' or with tag 'Player'.")]
    public Transform player;

    [Tooltip("Reference to the floor plane. If empty, we will try to find a GameObject named 'Plane' and scale/position it automatically.")]
    public Transform floorPlane;

    [Header("Placement Offsets")]
    [Tooltip("Y position for the walls. Typically 0 if the wall pivot is at the bottom.")]
    public float wallYPosition = 0f;

    [Tooltip("Y offset for spawning the player, to ensure they do not clip into the floor.")]
    public float playerYOffset = 1.0f;

    [Tooltip("Height of the player's eyes/camera to align spawning.")]
    public float playerSpawnY = 1.0f;

    // References to generated components
    [HideInInspector]
    [SerializeField] private GameObject wallsParent;
    [HideInInspector]
    [SerializeField] private GameObject goalInstance;
    [SerializeField] private GameObject winCanvasInstance;

    // Grid tracking (True = Wall, False = Walkable Path)
    private bool[,] physicalGrid;
    private int gridW;
    private int gridH;

    // Game state
    private float timer = 0f;
    private bool gameFinished = false;
    private bool gameStarted = false;
    private TextMeshProUGUI timerText;

    private void Start()
    {
        // Automatically find resources if not assigned
        FindDependencies();

        // Generate the maze
        GenerateLabyrinth();

        // Start game state
        gameStarted = true;
        gameFinished = false;
        timer = 0f;

        // Hide win UI
        if (winCanvasInstance != null)
        {
            winCanvasInstance.SetActive(true);
        }
    }

    private void Update()
    {
        if (gameStarted && !gameFinished)
        {
            timer += Time.deltaTime;
            UpdateTimerText();
        }
    }

    private void FindDependencies()
    {
        // Find Wall Prefab if null
        if (wallPrefab == null)
        {
#if UNITY_EDITOR
            wallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Recursos/laberinto/wall.prefab");
#endif
            if (wallPrefab == null)
            {
                Debug.LogWarning("Wall Prefab not assigned! Trying to find it in the project.");
            }
        }

        // Find Player if null
        if (player == null)
        {
            GameObject pObj = GameObject.Find("Capsule PLAYER");
            if (pObj == null) pObj = GameObject.FindWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        // Find Floor Plane if null
        if (floorPlane == null)
        {
            GameObject planeObj = GameObject.Find("Plane");
            if (planeObj != null) floorPlane = planeObj.transform;
        }
    }

    public void GenerateLabyrinth()
    {
        // 1. Clear any existing labyrinth geometry
        ClearLabyrinth();

        // 2. Validate wall prefab
        FindDependencies();
        if (wallPrefab == null)
        {
            Debug.LogError("Cannot generate maze: Wall Prefab is missing! Please assign it in the inspector.");
            return;
        }

        // 3. Scale and align floor plane
        ScaleFloorPlane();

        // 4. Generate the Maze Logic Grid (Upscaled binary grid)
        // Cell size K = 4. Corridors are 3 units wide, walls are 1 unit thick.
        gridW = width * 4 + 1;
        gridH = height * 4 + 1;
        physicalGrid = new bool[gridW, gridH];

        // Initialize all as Wall (true)
        for (int x = 0; x < gridW; x++)
        {
            for (int z = 0; z < gridH; z++)
            {
                physicalGrid[x, z] = true;
            }
        }

        // Carve all cells to walkable initially (3x3 squares)
        for (int cx = 0; cx < width; cx++)
        {
            for (int cz = 0; cz < height; cz++)
            {
                CarveCell(cx, cz);
            }
        }

        // 5. Generate topological maze using DFS
        GenerateTopologyDFS();

        // 6. Add coherent loops (bifurcaciones coherentes)
        AddCoherentLoops();

        // 7. Instantiate physical walls
        BuildWalls();

        // 8. Place Player at Start
        TeleportPlayerToStart();

        // 9. Place Goal at Exit
        SpawnGoal();

        // 10. Setup Win UI Canvas
        SetupWinCanvas();

        Debug.Log($"Labyrinth generated successfully! Size: {width}x{height} cells ({gridW}x{gridH} physical units).");
    }

    public void ClearLabyrinth()
    {
        // Destroy walls parent and its children
        if (wallsParent != null)
        {
            DestroyHelper(wallsParent);
            wallsParent = null;
        }
        else
        {
            // Fallback: search and destroy any object named "Maze_Walls_Parent" in children
            Transform oldParent = transform.Find("Maze_Walls_Parent");
            if (oldParent != null)
            {
                DestroyHelper(oldParent.gameObject);
            }
        }

        // Destroy goal instance
        if (goalInstance != null)
        {
            DestroyHelper(goalInstance);
            goalInstance = null;
        }
        else
        {
            Transform oldGoal = transform.Find("Maze_Goal");
            if (oldGoal != null)
            {
                DestroyHelper(oldGoal.gameObject);
            }
        }

        // Destroy UI
        if (winCanvasInstance != null)
        {
            DestroyHelper(winCanvasInstance);
            winCanvasInstance = null;
        }
        else
        {
            GameObject oldUI = GameObject.Find("MazeWinCanvas");
            if (oldUI != null)
            {
                DestroyHelper(oldUI);
            }
        }

        gameFinished = false;
        gameStarted = false;
        timer = 0f;
    }

    private void DestroyHelper(UnityEngine.Object obj)
    {
        if (obj == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(obj);
        }
        else
        {
            Destroy(obj);
        }
#else
        Destroy(obj);
#endif
    }

    private void ScaleFloorPlane()
    {
        if (floorPlane == null) return;

        // A 10x10 cell maze is 41x41 units.
        // The maze goes from X = 0 to 40 and Z = 0 to 40.
        // Center of maze is at X = (gridW - 1) / 2 = 20, Z = (gridH - 1) / 2 = 20.
        float mazeWidthUnits = width * 4 + 1;
        float mazeHeightUnits = height * 4 + 1;

        // Position the floor plane at the center of the maze
        // Move local to the generator's position
        Vector3 localCenter = new Vector3((mazeWidthUnits - 1f) / 2f, 0f, (mazeHeightUnits - 1f) / 2f);
        floorPlane.position = transform.TransformPoint(localCenter);

        // A standard Unity Plane is 10x10 units at scale 1,1,1.
        // We scale it so it covers the maze plus a little padding of 2 units on each side
        float scaleX = (mazeWidthUnits + 4f) / 10f;
        float scaleZ = (mazeHeightUnits + 4f) / 10f;

        floorPlane.localScale = new Vector3(scaleX, 1f, scaleZ);
    }

    private void CarveCell(int cx, int cz)
    {
        int sx = cx * 4 + 1;
        int sz = cz * 4 + 1;

        for (int x = sx; x < sx + 3; x++)
        {
            for (int z = sz; z < sz + 3; z++)
            {
                physicalGrid[x, z] = false; // Set as path (walkable)
            }
        }
    }

    private void CarveConnection(int cx, int cz, int nx, int nz)
    {
        if (nx == cx + 1) // Moving East
        {
            int wallX = cx * 4 + 4;
            int sz = cz * 4 + 1;
            for (int z = sz; z < sz + 3; z++) physicalGrid[wallX, z] = false;
        }
        else if (nx == cx - 1) // Moving West
        {
            int wallX = nx * 4 + 4;
            int sz = cz * 4 + 1;
            for (int z = sz; z < sz + 3; z++) physicalGrid[wallX, z] = false;
        }
        else if (nz == cz + 1) // Moving North
        {
            int sx = cx * 4 + 1;
            int wallZ = cz * 4 + 4;
            for (int x = sx; x < sx + 3; x++) physicalGrid[x, wallZ] = false;
        }
        else if (nz == cz - 1) // Moving South
        {
            int sx = cx * 4 + 1;
            int wallZ = nz * 4 + 4;
            for (int x = sx; x < sx + 3; x++) physicalGrid[x, wallZ] = false;
        }
    }

    private void GenerateTopologyDFS()
    {
        // Simple class to track logical cell state
        bool[,] visited = new bool[width, height];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        // Start DFS at cell (0, 0)
        Vector2Int start = new Vector2Int(0, 0);
        visited[start.x, start.y] = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> unvisitedNeighbors = new List<Vector2Int>();

            // Check neighbors
            if (current.x + 1 < width && !visited[current.x + 1, current.y])
                unvisitedNeighbors.Add(new Vector2Int(current.x + 1, current.y));
            if (current.x - 1 >= 0 && !visited[current.x - 1, current.y])
                unvisitedNeighbors.Add(new Vector2Int(current.x - 1, current.y));
            if (current.y + 1 < height && !visited[current.x, current.y + 1])
                unvisitedNeighbors.Add(new Vector2Int(current.x, current.y + 1));
            if (current.y - 1 >= 0 && !visited[current.x, current.y - 1])
                unvisitedNeighbors.Add(new Vector2Int(current.x, current.y - 1));

            if (unvisitedNeighbors.Count > 0)
            {
                // Choose a random neighbor
                Vector2Int chosen = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];

                // Carve connection in physical grid
                CarveConnection(current.x, current.y, chosen.x, chosen.y);

                // Mark as visited and push to stack
                visited[chosen.x, chosen.y] = true;
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    private void AddCoherentLoops()
    {
        if (loopPercentage <= 0.001f) return;

        // Scan all internal walls between adjacent logical cells and remove them with probability loopPercentage
        for (int cx = 0; cx < width; cx++)
        {
            for (int cz = 0; cz < height; cz++)
            {
                // Check East wall
                if (cx + 1 < width)
                {
                    int wallX = cx * 4 + 4;
                    int centerZ = cz * 4 + 2;
                    // If there is currently a wall here
                    if (physicalGrid[wallX, centerZ] == true)
                    {
                        if (Random.value < loopPercentage)
                        {
                            // Carve it!
                            CarveConnection(cx, cz, cx + 1, cz);
                        }
                    }
                }

                // Check North wall
                if (cz + 1 < height)
                {
                    int centerX = cx * 4 + 2;
                    int wallZ = cz * 4 + 4;
                    // If there is currently a wall here
                    if (physicalGrid[centerX, wallZ] == true)
                    {
                        if (Random.value < loopPercentage)
                        {
                            // Carve it!
                            CarveConnection(cx, cz, cx, cz + 1);
                        }
                    }
                }
            }
        }
    }

    private void BuildWalls()
    {
        wallsParent = new GameObject("Maze_Walls_Parent");
        wallsParent.transform.parent = this.transform;
        wallsParent.transform.localPosition = Vector3.zero;
        wallsParent.transform.localRotation = Quaternion.identity;

        // Iterate the entire grid and instantiate walls
        for (int x = 0; x < gridW; x++)
        {
            for (int z = 0; z < gridH; z++)
            {
                if (physicalGrid[x, z])
                {
                    Vector3 localPos = new Vector3(x, wallYPosition, z);
                    Vector3 worldPos = transform.TransformPoint(localPos);

                    GameObject w = Instantiate(wallPrefab, worldPos, Quaternion.identity, wallsParent.transform);
                    w.name = $"Wall_{x}_{z}";
                }
            }
        }
    }

    private void TeleportPlayerToStart()
    {
        if (player == null) return;

        // Start cell (0, 0) center is at grid coordinate (2, 2)
        Vector3 startLocalPos = new Vector3(2f, playerYOffset, 2f);
        Vector3 startWorldPos = transform.TransformPoint(startLocalPos);

        // Turn player to face into the maze (along positive Z axis)
        player.rotation = Quaternion.LookRotation(transform.forward);

        // Check if player has CharacterController and disable temporarily to allow teleportation
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            player.position = startWorldPos;
            cc.enabled = true;
        }
        else
        {
            player.position = startWorldPos;
        }

        Debug.Log($"Teleported player to {startWorldPos}");
    }

    private void SpawnGoal()
    {
        // Exit cell (width-1, height-1) center is at grid coordinate ((width-1)*4 + 2, (height-1)*4 + 2)
        Vector3 goalLocalPos = new Vector3((width - 1) * 4 + 2f, 0.1f, (height - 1) * 4 + 2f);
        Vector3 goalWorldPos = transform.TransformPoint(goalLocalPos);

        if (goalPrefab != null)
        {
            goalInstance = Instantiate(goalPrefab, goalWorldPos, Quaternion.identity, transform);
            goalInstance.name = "Maze_Goal";
        }
        else
        {
            // Build a gorgeous procedural Goal Pedestal!
            goalInstance = new GameObject("Maze_Goal");
            goalInstance.transform.parent = this.transform;
            goalInstance.transform.position = goalWorldPos;
            goalInstance.transform.rotation = Quaternion.identity;

            // 1. Column base
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "PedestalBase";
            pedestal.transform.parent = goalInstance.transform;
            pedestal.transform.localPosition = new Vector3(0, 0.5f, 0);
            pedestal.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
            
            // Give it a sleek dark metal color
            Renderer pedRen = pedestal.GetComponent<Renderer>();
            if (pedRen != null)
            {
                pedRen.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                pedRen.material.color = new Color(0.12f, 0.12f, 0.15f, 1f);
                pedRen.material.SetFloat("_Smoothness", 0.8f);
                pedRen.material.SetFloat("_Metallic", 0.9f);
            }

            // 2. Glowing Orb
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "GlowingOrb";
            orb.transform.parent = goalInstance.transform;
            orb.transform.localPosition = new Vector3(0, 1.4f, 0);
            orb.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            // Destroy the collider on the orb to prevent player physics issues
            Collider orbCol = orb.GetComponent<Collider>();
            if (orbCol != null) DestroyHelper(orbCol.gameObject.GetComponent<Collider>());

            // Give orb an emissive vibrant neon-green material
            Renderer orbRen = orb.GetComponent<Renderer>();
            if (orbRen != null)
            {
                Material glowMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                glowMat.color = new Color(0.1f, 1f, 0.4f, 1f);
                glowMat.EnableKeyword("_EMISSION");
                glowMat.SetColor("_EmissionColor", new Color(0.1f, 1.5f, 0.4f) * 2f); // HDR glowing effect!
                orbRen.material = glowMat;
            }

            // 3. Rotator Component for micro-animation
            orb.AddComponent<OrbAnimate>();

            // 4. Point Light for visual flair
            GameObject lightObj = new GameObject("GoalLight");
            lightObj.transform.parent = goalInstance.transform;
            lightObj.transform.localPosition = new Vector3(0, 1.4f, 0);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0.2f, 1f, 0.5f);
            l.range = 5f;
            l.intensity = 3f;

            // 5. Trigger Box Collider for goal detection
            BoxCollider bc = goalInstance.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.center = new Vector3(0, 1f, 0);
            bc.size = new Vector3(1.5f, 2f, 1.5f);
        }

        // Add MazeGoal trigger logic script
        MazeGoal goalScript = goalInstance.AddComponent<MazeGoal>();
        goalScript.mazeGenerator = this;
    }

    private void SetupWinCanvas()
    {
        // 1. Create a beautiful Canvas if it doesn't exist
        GameObject uiObj = new GameObject("MazeWinCanvas");
        winCanvasInstance = uiObj;
        winCanvasInstance.transform.position = Vector3.zero;

        Canvas canvas = uiObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;

        CanvasScaler scaler = uiObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        uiObj.AddComponent<GraphicRaycaster>();

        // 2. Create modern semi-transparent blur/dark panel
        GameObject panelObj = new GameObject("WinPanel");
        panelObj.transform.parent = uiObj.transform;
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500, 380);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);

        Image panelImg = panelObj.AddComponent<Image>();
        // Sleek premium dark glassmorphism styling
        panelImg.color = new Color(0.07f, 0.07f, 0.09f, 0.94f); 

        // 3. Create Title Text (TMP)
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.parent = panelObj.transform;
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 100);
        titleRect.sizeDelta = new Vector2(460, 60);
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);

        TextMeshProUGUI titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "¡LABERINTO RESUELTO!";
        titleTxt.fontSize = 32;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = new Color(0.12f, 0.98f, 0.47f, 1f); // Beautiful electric green

        // 4. Create Subtitle Text
        GameObject subObj = new GameObject("SubtitleText");
        subObj.transform.parent = panelObj.transform;

        RectTransform subRect = subObj.AddComponent<RectTransform>();
        subRect.anchoredPosition = new Vector2(0, 45);
        subRect.sizeDelta = new Vector2(460, 40);
        subRect.anchorMin = new Vector2(0.5f, 0.5f);
        subRect.anchorMax = new Vector2(0.5f, 0.5f);
        subRect.pivot = new Vector2(0.5f, 0.5f);

        TextMeshProUGUI subTxt = subObj.AddComponent<TextMeshProUGUI>();
        subTxt.text = "¡Buen trabajo superando los obstáculos!";
        subTxt.fontSize = 18;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.8f, 0.8f, 0.85f, 1f);

        // 5. Create Time Text
        GameObject timeObj = new GameObject("TimeText");
        timeObj.transform.parent = panelObj.transform;

        RectTransform timeRect = timeObj.AddComponent<RectTransform>();
        timeRect.anchoredPosition = new Vector2(0, -25);
        timeRect.sizeDelta = new Vector2(460, 50);
        timeRect.anchorMin = new Vector2(0.5f, 0.5f);
        timeRect.anchorMax = new Vector2(0.5f, 0.5f);
        timeRect.pivot = new Vector2(0.5f, 0.5f);

        timerText = timeObj.AddComponent<TextMeshProUGUI>();
        timerText.text = "Tiempo: 00:00.00";
        timerText.fontSize = 24;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = Color.white;

        // 6. Create Restart Button
        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.parent = panelObj.transform;

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -110);
        btnRect.sizeDelta = new Vector2(250, 50);
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.12f, 0.55f, 0.98f, 1f); // Royal Electric Blue

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        
        // Button colors transitions
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.12f, 0.55f, 0.98f, 1f);
        cb.highlightedColor = new Color(0.25f, 0.65f, 1f, 1f);
        cb.pressedColor = new Color(0.08f, 0.4f, 0.75f, 1f);
        btn.colors = cb;

        // Add click listener
        btn.onClick.AddListener(OnRestartButtonClicked);

        // Button text
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.parent = btnObj.transform;

        RectTransform btnTxtRect = btnTextObj.AddComponent<RectTransform>();
        btnTxtRect.anchoredPosition = Vector2.zero;
        btnTxtRect.sizeDelta = new Vector2(240, 40);
        btnTxtRect.anchorMin = Vector2.zero;
        btnTxtRect.anchorMax = Vector2.one;

        TextMeshProUGUI btnTxt = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnTxt.text = "Jugar de Nuevo";
        btnTxt.fontSize = 20;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.alignment = TextAlignmentOptions.Center;
        btnTxt.color = Color.white;

        // 7. Timer text in top-right corner of screen during gameplay
        GameObject HUDObj = new GameObject("GameplayHUD");
        HUDObj.transform.parent = uiObj.transform;

        RectTransform hudRect = HUDObj.AddComponent<RectTransform>();
        hudRect.anchoredPosition = new Vector2(-40, -40);
        hudRect.sizeDelta = new Vector2(300, 50);
        hudRect.anchorMin = new Vector2(1f, 1f);
        hudRect.anchorMax = new Vector2(1f, 1f);
        hudRect.pivot = new Vector2(1f, 1f);

        // Add a nice dark background tag for the timer HUD
        Image hudBg = HUDObj.AddComponent<Image>();
        hudBg.color = new Color(0f, 0f, 0f, 0.5f);

        GameObject hudTextObj = new GameObject("HUDTimerText");
        hudTextObj.transform.parent = HUDObj.transform;
        RectTransform hudTextRect = hudTextObj.AddComponent<RectTransform>();
        hudTextRect.anchoredPosition = Vector2.zero;
        hudTextRect.sizeDelta = new Vector2(280, 40);
        hudTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        hudTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        hudTextRect.pivot = new Vector2(0.5f, 0.5f);

        TextMeshProUGUI hudTxt = hudTextObj.AddComponent<TextMeshProUGUI>();
        hudTxt.text = "00:00.00";
        hudTxt.fontSize = 22;
        hudTxt.fontStyle = FontStyles.Bold;
        hudTxt.alignment = TextAlignmentOptions.Center;
        hudTxt.color = Color.white;

        // Keep local reference to sync HUD text
        timerHUDText = hudTxt;

        // Initially hide the Win Panel, keep HUD active
        panelObj.SetActive(false);
        winPanelInstance = panelObj;

        // Disable canvas by default in editor, Start will control it
        if (!Application.isPlaying)
        {
            uiObj.SetActive(false);
        }
    }

    private TextMeshProUGUI timerHUDText;
    private GameObject winPanelInstance;

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        int fraction = Mathf.FloorToInt((timer * 100f) % 100f);

        string timeString = string.Format("{0:00}:{1:00}.{2:02}", minutes, seconds, fraction);
        
        if (timerHUDText != null)
        {
            timerHUDText.text = timeString;
        }

        if (timerText != null)
        {
            timerText.text = "Tiempo: " + timeString;
        }
    }

    public void OnGoalReached()
    {
        if (gameFinished) return;

        gameFinished = true;
        Debug.Log("GOAL REACHED! Finished in: " + timer + " seconds.");

        // Show cursor so player can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable shooting or controls if necessary (optional, but nice)
        PlayerShooting ps = FindFirstObjectByType<PlayerShooting>();
        if (ps != null) ps.enabled = false;

        // Enable Win Canvas and show Win Panel
        if (winCanvasInstance != null)
        {
            winCanvasInstance.SetActive(true);
        }
        if (winPanelInstance != null)
        {
            winPanelInstance.SetActive(true);
        }

        // Play Win Sound if audio source exists
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.Play();
        }
    }

    public void OnRestartButtonClicked()
    {
        Debug.Log("Restarting game...");

        // Re-enable player controls
        PlayerShooting ps = FindFirstObjectByType<PlayerShooting>();
        if (ps != null) ps.enabled = true;

        // Re-lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Regenerate labyrinth
        GenerateLabyrinth();

        // Reset timer states
        gameStarted = true;
        gameFinished = false;
        timer = 0f;

        // Hide win UI panel, keep Canvas HUD active
        if (winPanelInstance != null)
        {
            winPanelInstance.SetActive(false);
        }
    }
}

// Simple orbital micro-animation component to make the goal look high-end
public class OrbAnimate : MonoBehaviour
{
    private float startY;
    
    private void Start()
    {
        startY = transform.localPosition.y;
    }

    private void Update()
    {
        // 1. Slow, satisfying floating up and down
        float newY = startY + Mathf.Sin(Time.time * 2f) * 0.12f;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);

        // 2. Continuous rotating on multiple axes for high-end styling
        transform.Rotate(new Vector3(15f, 30f, 45f) * Time.deltaTime, Space.Self);
    }
}
