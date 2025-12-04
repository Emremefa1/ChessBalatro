using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Chess.Editor
{
    /// <summary>
    /// Custom Editor Window for managing Chess Balatro game settings.
    /// Access via: Window -> Chess Balatro -> Game Manager
    /// </summary>
    public class ChessBalatroEditorWindow : EditorWindow
    {
        // Tab management
        private int selectedTab = 0;
        private readonly string[] tabNames = { "Scene Setup", "Materials", "Gambits", "Scrolls", "Win Conditions", "Settings" };

        // Scroll positions
        private Vector2 scrollPosition;
        private Vector2 gambitScrollPosition;
        private Vector2 scrollScrollPosition;

        // Cached references
        private GameObject roguelikeTestGameObject;
        private GameObject roguelikeGameManagerObject;

        // Material references for quick setup
        private Material whiteMaterial;
        private Material blackMaterial;
        private Material highlightMaterial;
        private Material selectedMaterial;

        // Gambit/Scroll database
        private List<Object> allGambits = new List<Object>();
        private List<Object> allScrolls = new List<Object>();

        [MenuItem("Window/Chess Balatro/Game Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<ChessBalatroEditorWindow>("Chess Balatro");
            window.minSize = new Vector2(450, 500);
        }

        private void OnEnable()
        {
            RefreshSceneReferences();
            LoadSavedPreferences();
        }

        private void OnGUI()
        {
            DrawHeader();
            
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (selectedTab)
            {
                case 0:
                    DrawSceneSetupTab();
                    break;
                case 1:
                    DrawMaterialsTab();
                    break;
                case 2:
                    DrawGambitsTab();
                    break;
                case 3:
                    DrawScrollsTab();
                    break;
                case 4:
                    DrawWinConditionsTab();
                    break;
                case 5:
                    DrawSettingsTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(5);
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("♟ Chess Balatro Manager ♟", headerStyle);
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Refresh Scene References", GUILayout.Height(25)))
            {
                RefreshSceneReferences();
            }
            
            EditorGUILayout.Space(5);
        }

        #region Scene Setup Tab

        private void DrawSceneSetupTab()
        {
            EditorGUILayout.LabelField("Scene Objects", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // RoguelikeTestGame status
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RoguelikeTestGame:", GUILayout.Width(150));
            if (roguelikeTestGameObject != null)
            {
                EditorGUILayout.ObjectField(roguelikeTestGameObject, typeof(GameObject), true);
                GUI.color = Color.green;
                EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Not Found", GUILayout.Width(100));
                GUI.color = Color.white;
                if (GUILayout.Button("Create", GUILayout.Width(60)))
                {
                    CreateRoguelikeTestGame();
                }
            }
            EditorGUILayout.EndHorizontal();

            // RoguelikeGameManager status
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("RoguelikeGameManager:", GUILayout.Width(150));
            if (roguelikeGameManagerObject != null)
            {
                EditorGUILayout.ObjectField(roguelikeGameManagerObject, typeof(GameObject), true);
                GUI.color = Color.green;
                EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Not Found", GUILayout.Width(100));
                GUI.color = Color.white;
                if (GUILayout.Button("Create", GUILayout.Width(60)))
                {
                    CreateRoguelikeGameManager();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);

            // Quick Setup
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Click the button below to automatically set up the entire Chess Balatro scene with all required components.",
                MessageType.Info);
            
            EditorGUILayout.Space(5);
            
            GUI.color = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("🎮 Setup Complete Scene", GUILayout.Height(40)))
            {
                SetupCompleteScene();
            }
            GUI.color = Color.white;

            EditorGUILayout.Space(15);

            // Component Status
            DrawComponentStatus();
        }

        private void DrawComponentStatus()
        {
            EditorGUILayout.LabelField("Component Status", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (roguelikeTestGameObject != null)
            {
                var renderer = roguelikeTestGameObject.GetComponent<UI.IsometricBoardRenderer>();
                DrawStatusRow("IsometricBoardRenderer", renderer != null);
                
                if (renderer != null)
                {
                    // Check if materials are assigned using SerializedObject
                    var so = new SerializedObject(renderer);
                    var whiteMat = so.FindProperty("whiteMaterial");
                    var blackMat = so.FindProperty("blackMaterial");
                    
                    EditorGUI.indentLevel++;
                    DrawStatusRow("White Material", whiteMat?.objectReferenceValue != null);
                    DrawStatusRow("Black Material", blackMat?.objectReferenceValue != null);
                    EditorGUI.indentLevel--;
                }
            }

            if (roguelikeGameManagerObject != null)
            {
                var manager = roguelikeGameManagerObject.GetComponent<Roguelike.RoguelikeGameManager>();
                DrawStatusRow("RoguelikeGameManager", manager != null);
                
                if (manager != null)
                {
                    var so = new SerializedObject(manager);
                    var gambits = so.FindProperty("allGambits");
                    var scrolls = so.FindProperty("allScrolls");
                    
                    EditorGUI.indentLevel++;
                    DrawStatusRow($"Gambits ({gambits?.arraySize ?? 0})", gambits?.arraySize > 0);
                    DrawStatusRow($"Scrolls ({scrolls?.arraySize ?? 0})", scrolls?.arraySize > 0);
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawStatusRow(string label, bool isGood)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(200));
            GUI.color = isGood ? Color.green : Color.red;
            EditorGUILayout.LabelField(isGood ? "✓" : "✗", GUILayout.Width(20));
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Materials Tab

        private void DrawMaterialsTab()
        {
            EditorGUILayout.LabelField("Board Materials", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Assign materials here and click 'Apply to Renderer' to update the IsometricBoardRenderer component.",
                MessageType.Info);
            EditorGUILayout.Space(10);

            whiteMaterial = (Material)EditorGUILayout.ObjectField("White Tile Material", whiteMaterial, typeof(Material), false);
            blackMaterial = (Material)EditorGUILayout.ObjectField("Black Tile Material", blackMaterial, typeof(Material), false);
            highlightMaterial = (Material)EditorGUILayout.ObjectField("Highlight Material", highlightMaterial, typeof(Material), false);
            selectedMaterial = (Material)EditorGUILayout.ObjectField("Selected Material", selectedMaterial, typeof(Material), false);

            EditorGUILayout.Space(15);

            // Quick material creation
            EditorGUILayout.LabelField("Quick Material Setup", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Default Materials"))
            {
                CreateDefaultMaterials();
            }
            if (GUILayout.Button("Find Materials in Project"))
            {
                FindMaterialsInProject();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            GUI.enabled = roguelikeTestGameObject != null && (whiteMaterial != null || blackMaterial != null);
            GUI.color = new Color(0.4f, 0.7f, 1f);
            if (GUILayout.Button("Apply to Renderer", GUILayout.Height(35)))
            {
                ApplyMaterialsToRenderer();
            }
            GUI.color = Color.white;
            GUI.enabled = true;

            if (roguelikeTestGameObject == null)
            {
                EditorGUILayout.HelpBox("RoguelikeTestGame not found in scene. Create it first.", MessageType.Warning);
            }
        }

        private void CreateDefaultMaterials()
        {
            string folderPath = "Assets/Materials/Chess";
            
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Materials", "Chess");

            // Create white material
            whiteMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            whiteMaterial.color = new Color(0.9f, 0.85f, 0.75f);
            AssetDatabase.CreateAsset(whiteMaterial, $"{folderPath}/WhiteTile.mat");

            // Create black material
            blackMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            blackMaterial.color = new Color(0.3f, 0.25f, 0.2f);
            AssetDatabase.CreateAsset(blackMaterial, $"{folderPath}/BlackTile.mat");

            // Create highlight material
            highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            highlightMaterial.color = new Color(0.5f, 0.8f, 0.5f, 0.8f);
            AssetDatabase.CreateAsset(highlightMaterial, $"{folderPath}/HighlightTile.mat");

            // Create selected material
            selectedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            selectedMaterial.color = new Color(0.9f, 0.9f, 0.3f, 0.8f);
            AssetDatabase.CreateAsset(selectedMaterial, $"{folderPath}/SelectedTile.mat");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ChessBalatro] Created default materials at " + folderPath);
        }

        private void FindMaterialsInProject()
        {
            // Try to find materials with common names
            string[] guids = AssetDatabase.FindAssets("t:Material");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (name.Contains("white") && (name.Contains("tile") || name.Contains("chess") || name.Contains("board")))
                    whiteMaterial = mat;
                else if (name.Contains("black") && (name.Contains("tile") || name.Contains("chess") || name.Contains("board")))
                    blackMaterial = mat;
                else if (name.Contains("highlight"))
                    highlightMaterial = mat;
                else if (name.Contains("select"))
                    selectedMaterial = mat;
            }

            Debug.Log("[ChessBalatro] Searched for materials in project");
        }

        private void ApplyMaterialsToRenderer()
        {
            if (roguelikeTestGameObject == null) return;

            var renderer = roguelikeTestGameObject.GetComponent<UI.IsometricBoardRenderer>();
            if (renderer == null)
            {
                renderer = roguelikeTestGameObject.AddComponent<UI.IsometricBoardRenderer>();
            }

            var so = new SerializedObject(renderer);
            
            if (whiteMaterial != null)
                so.FindProperty("whiteMaterial").objectReferenceValue = whiteMaterial;
            if (blackMaterial != null)
                so.FindProperty("blackMaterial").objectReferenceValue = blackMaterial;
            if (highlightMaterial != null)
                so.FindProperty("highlightMaterial").objectReferenceValue = highlightMaterial;
            if (selectedMaterial != null)
                so.FindProperty("selectedMaterial").objectReferenceValue = selectedMaterial;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(renderer);

            Debug.Log("[ChessBalatro] Applied materials to IsometricBoardRenderer");
        }

        #endregion

        #region Gambits Tab

        private void DrawGambitsTab()
        {
            EditorGUILayout.LabelField("Gambit Management", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find All Gambits in Project"))
            {
                FindAllGambits();
            }
            if (GUILayout.Button("Create New Gambit"))
            {
                ShowGambitCreationMenu();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField($"Found Gambits: {allGambits.Count}", EditorStyles.boldLabel);
            
            gambitScrollPosition = EditorGUILayout.BeginScrollView(gambitScrollPosition, GUILayout.Height(200));
            foreach (var gambit in allGambits)
            {
                if (gambit != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(gambit, typeof(ScriptableObject), false);
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = gambit;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            GUI.color = new Color(0.4f, 0.7f, 1f);
            if (GUILayout.Button("Apply Gambits to Game Manager", GUILayout.Height(30)))
            {
                ApplyGambitsToManager();
            }
            GUI.color = Color.white;
        }

        private void FindAllGambits()
        {
            allGambits.Clear();
            
            // Find all ScriptableObjects that are or inherit from Gambit
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            System.Type gambitType = typeof(Roguelike.Gambits.Gambit);
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                if (obj != null && gambitType.IsAssignableFrom(obj.GetType()))
                {
                    allGambits.Add(obj);
                }
            }

            Debug.Log($"[ChessBalatro] Found {allGambits.Count} gambits");
        }

        private void ShowGambitCreationMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Greedy Gambit"), false, () => CreateGambit("GreedyGambit"));
            menu.AddItem(new GUIContent("Queen's Favor Gambit"), false, () => CreateGambit("QueensFavorGambit"));
            menu.AddItem(new GUIContent("Bigger Board Gambit"), false, () => CreateGambit("BiggerBoardGambit"));
            menu.AddItem(new GUIContent("Check Bonus Gambit"), false, () => CreateGambit("CheckBonusGambit"));
            menu.ShowAsContext();
        }

        private void CreateGambit(string typeName)
        {
            string folderPath = "Assets/ScriptableObjects/Gambits";
            
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Gambits");

            var type = System.Type.GetType($"Chess.Roguelike.Gambits.Examples.{typeName}, Assembly-CSharp");
            if (type != null)
            {
                var instance = ScriptableObject.CreateInstance(type);
                string path = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{typeName}.asset");
                AssetDatabase.CreateAsset(instance, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = instance;
                Debug.Log($"[ChessBalatro] Created {typeName} at {path}");
                FindAllGambits();
            }
            else
            {
                Debug.LogError($"Could not find type: {typeName}");
            }
        }

        private void ApplyGambitsToManager()
        {
            if (roguelikeGameManagerObject == null)
            {
                Debug.LogError("[ChessBalatro] RoguelikeGameManager not found!");
                return;
            }

            var manager = roguelikeGameManagerObject.GetComponent<Roguelike.RoguelikeGameManager>();
            if (manager == null) return;

            var so = new SerializedObject(manager);
            var gambitsProp = so.FindProperty("allGambits");
            
            gambitsProp.ClearArray();
            for (int i = 0; i < allGambits.Count; i++)
            {
                gambitsProp.InsertArrayElementAtIndex(i);
                gambitsProp.GetArrayElementAtIndex(i).objectReferenceValue = allGambits[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            Debug.Log($"[ChessBalatro] Applied {allGambits.Count} gambits to RoguelikeGameManager");
        }

        #endregion

        #region Scrolls Tab

        private void DrawScrollsTab()
        {
            EditorGUILayout.LabelField("Scroll Management", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find All Scrolls in Project"))
            {
                FindAllScrolls();
            }
            if (GUILayout.Button("Create New Scroll"))
            {
                ShowScrollCreationMenu();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField($"Found Scrolls: {allScrolls.Count}", EditorStyles.boldLabel);
            
            scrollScrollPosition = EditorGUILayout.BeginScrollView(scrollScrollPosition, GUILayout.Height(200));
            foreach (var scroll in allScrolls)
            {
                if (scroll != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(scroll, typeof(ScriptableObject), false);
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeObject = scroll;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            GUI.color = new Color(0.4f, 0.7f, 1f);
            if (GUILayout.Button("Apply Scrolls to Game Manager", GUILayout.Height(30)))
            {
                ApplyScrollsToManager();
            }
            GUI.color = Color.white;
        }

        private void FindAllScrolls()
        {
            allScrolls.Clear();
            
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            System.Type scrollType = typeof(Roguelike.Scrolls.Scroll);
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                
                if (obj != null && scrollType.IsAssignableFrom(obj.GetType()))
                {
                    allScrolls.Add(obj);
                }
            }

            Debug.Log($"[ChessBalatro] Found {allScrolls.Count} scrolls");
        }

        private void ShowScrollCreationMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Promotion Scroll"), false, () => CreateScroll("PromotionScroll"));
            menu.AddItem(new GUIContent("Wealth Scroll"), false, () => CreateScroll("WealthScroll"));
            menu.AddItem(new GUIContent("Destruction Scroll"), false, () => CreateScroll("DestructionScroll"));
            menu.ShowAsContext();
        }

        private void CreateScroll(string typeName)
        {
            string folderPath = "Assets/ScriptableObjects/Scrolls";
            
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Scrolls");

            var type = System.Type.GetType($"Chess.Roguelike.Scrolls.Examples.{typeName}, Assembly-CSharp");
            if (type != null)
            {
                var instance = ScriptableObject.CreateInstance(type);
                string path = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{typeName}.asset");
                AssetDatabase.CreateAsset(instance, path);
                AssetDatabase.SaveAssets();
                Selection.activeObject = instance;
                Debug.Log($"[ChessBalatro] Created {typeName} at {path}");
                FindAllScrolls();
            }
            else
            {
                Debug.LogError($"Could not find type: {typeName}");
            }
        }

        private void ApplyScrollsToManager()
        {
            if (roguelikeGameManagerObject == null)
            {
                Debug.LogError("[ChessBalatro] RoguelikeGameManager not found!");
                return;
            }

            var manager = roguelikeGameManagerObject.GetComponent<Roguelike.RoguelikeGameManager>();
            if (manager == null) return;

            var so = new SerializedObject(manager);
            var scrollsProp = so.FindProperty("allScrolls");
            
            scrollsProp.ClearArray();
            for (int i = 0; i < allScrolls.Count; i++)
            {
                scrollsProp.InsertArrayElementAtIndex(i);
                scrollsProp.GetArrayElementAtIndex(i).objectReferenceValue = allScrolls[i];
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            Debug.Log($"[ChessBalatro] Applied {allScrolls.Count} scrolls to RoguelikeGameManager");
        }

        #endregion

        #region Win Conditions Tab

        private void DrawWinConditionsTab()
        {
            EditorGUILayout.LabelField("Win Condition Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure how trials can be won or lost.\n\n" +
                "• Checkmate: Traditional chess win\n" +
                "• Elimination: Capture all pieces except king\n" +
                "• Value Dominance: Have X+ more piece value for Y rounds",
                MessageType.Info);
            EditorGUILayout.Space(10);

            if (roguelikeGameManagerObject != null)
            {
                var manager = roguelikeGameManagerObject.GetComponent<Roguelike.RoguelikeGameManager>();
                if (manager != null)
                {
                    var so = new SerializedObject(manager);
                    
                    // Enable/Disable toggles
                    EditorGUILayout.LabelField("Enabled Win Conditions", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    var checkmateEnabled = so.FindProperty("enableCheckmate");
                    var eliminationEnabled = so.FindProperty("enableElimination");
                    var valueDominanceEnabled = so.FindProperty("enableValueDominance");
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("♔ Checkmate", GUILayout.Width(150));
                    checkmateEnabled.boolValue = EditorGUILayout.Toggle(checkmateEnabled.boolValue);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("   Win by putting enemy king in checkmate", EditorStyles.miniLabel);
                    
                    EditorGUILayout.Space(5);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("⚔ Elimination", GUILayout.Width(150));
                    eliminationEnabled.boolValue = EditorGUILayout.Toggle(eliminationEnabled.boolValue);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("   Win by capturing all pieces except enemy king", EditorStyles.miniLabel);
                    
                    EditorGUILayout.Space(5);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("★ Value Dominance", GUILayout.Width(150));
                    valueDominanceEnabled.boolValue = EditorGUILayout.Toggle(valueDominanceEnabled.boolValue);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("   Win by having superior army value for consecutive rounds", EditorStyles.miniLabel);
                    
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space(15);
                    
                    // Value Dominance settings
                    GUI.enabled = valueDominanceEnabled.boolValue;
                    EditorGUILayout.LabelField("Value Dominance Settings", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    var threshold = so.FindProperty("valueDominanceThreshold");
                    var rounds = so.FindProperty("valueDominanceRounds");
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Value Threshold", GUILayout.Width(150));
                    threshold.intValue = EditorGUILayout.IntSlider(threshold.intValue, 5, 30);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField($"   Must have {threshold.intValue}+ more piece value than enemy", EditorStyles.miniLabel);
                    
                    EditorGUILayout.Space(5);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Rounds Required", GUILayout.Width(150));
                    rounds.intValue = EditorGUILayout.IntSlider(rounds.intValue, 1, 5);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.LabelField($"   Must maintain dominance for {rounds.intValue} consecutive round(s)", EditorStyles.miniLabel);
                    
                    EditorGUILayout.EndVertical();
                    
                    GUI.enabled = true;
                    
                    EditorGUILayout.Space(15);
                    
                    // Piece Values Reference
                    EditorGUILayout.LabelField("Piece Values (Reference)", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("♟ Pawn = 1");
                    EditorGUILayout.LabelField("♞ Knight = 3");
                    EditorGUILayout.LabelField("♝ Bishop = 3");
                    EditorGUILayout.LabelField("♜ Rook = 5");
                    EditorGUILayout.LabelField("♛ Queen = 9");
                    EditorGUILayout.LabelField("♚ King = 0 (cannot be captured)");
                    EditorGUILayout.EndVertical();
                    
                    so.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("RoguelikeGameManager not found in scene. Create it first in the Scene Setup tab.", MessageType.Warning);
            }
        }

        #endregion

        #region Settings Tab

        private void DrawSettingsTab()
        {
            EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (roguelikeGameManagerObject != null)
            {
                var manager = roguelikeGameManagerObject.GetComponent<Roguelike.RoguelikeGameManager>();
                if (manager != null)
                {
                    var so = new SerializedObject(manager);
                    
                    EditorGUILayout.PropertyField(so.FindProperty("startingMoney"));
                    EditorGUILayout.PropertyField(so.FindProperty("aiDifficulty"));
                    
                    so.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("RoguelikeGameManager not found in scene.", MessageType.Warning);
            }

            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("Debug Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Print Run State (Play Mode Only)"))
            {
                if (Application.isPlaying)
                {
                    var manager = Object.FindFirstObjectByType<Roguelike.RoguelikeGameManager>();
                    if (manager != null)
                    {
                        manager.DebugPrintState();
                    }
                }
                else
                {
                    Debug.Log("This action only works in Play Mode.");
                }
            }
        }

        #endregion

        #region Helper Methods

        private void RefreshSceneReferences()
        {
            // Find RoguelikeTestGame
            var testGames = Object.FindObjectsByType<Examples.RoguelikeTestGame>(FindObjectsSortMode.None);
            roguelikeTestGameObject = testGames.Length > 0 ? testGames[0].gameObject : null;

            // Find RoguelikeGameManager
            var managers = Object.FindObjectsByType<Roguelike.RoguelikeGameManager>(FindObjectsSortMode.None);
            roguelikeGameManagerObject = managers.Length > 0 ? managers[0].gameObject : null;

            Repaint();
        }

        private void CreateRoguelikeTestGame()
        {
            var go = new GameObject("RoguelikeTestGame");
            go.AddComponent<Examples.RoguelikeTestGame>();
            go.AddComponent<UI.IsometricBoardRenderer>();
            Selection.activeGameObject = go;
            roguelikeTestGameObject = go;
            
            Debug.Log("[ChessBalatro] Created RoguelikeTestGame");
        }

        private void CreateRoguelikeGameManager()
        {
            var go = new GameObject("RoguelikeGameManager");
            go.AddComponent<Roguelike.RoguelikeGameManager>();
            Selection.activeGameObject = go;
            roguelikeGameManagerObject = go;
            
            Debug.Log("[ChessBalatro] Created RoguelikeGameManager");
        }

        private void SetupCompleteScene()
        {
            // Create game manager if needed
            if (roguelikeGameManagerObject == null)
            {
                CreateRoguelikeGameManager();
            }

            // Create test game if needed
            if (roguelikeTestGameObject == null)
            {
                CreateRoguelikeTestGame();
            }

            // Link them together
            var testGame = roguelikeTestGameObject.GetComponent<Examples.RoguelikeTestGame>();
            if (testGame != null)
            {
                var so = new SerializedObject(testGame);
                so.FindProperty("gameManager").objectReferenceValue = roguelikeGameManagerObject.GetComponent<Roguelike.RoguelikeGameManager>();
                so.ApplyModifiedProperties();
            }

            // Create materials if none exist
            if (whiteMaterial == null || blackMaterial == null)
            {
                CreateDefaultMaterials();
                ApplyMaterialsToRenderer();
            }

            // Find and apply gambits/scrolls
            FindAllGambits();
            FindAllScrolls();
            ApplyGambitsToManager();
            ApplyScrollsToManager();

            Debug.Log("[ChessBalatro] Complete scene setup finished!");
            EditorUtility.DisplayDialog("Setup Complete", 
                "Chess Balatro scene has been set up!\n\n" +
                "✓ RoguelikeGameManager created\n" +
                "✓ RoguelikeTestGame created\n" +
                "✓ Materials created/applied\n" +
                "✓ Gambits and Scrolls linked", 
                "OK");
        }

        private void LoadSavedPreferences()
        {
            // Could load EditorPrefs here for saved material references
        }

        #endregion
    }
}
