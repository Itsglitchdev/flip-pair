using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GameLevelData))]
public class GameLevelDataEditor : Editor
{
    private GameLevelData gameData;
    private SerializedProperty levelsProperty;
    
    // State tracking
    private List<bool> levelFoldouts = new List<bool>();
    private Vector2 scrollPosition;
    
    // Style variables - declared but initialized in OnGUI
    private GUIStyle headerStyle;
    private GUIStyle levelHeaderStyle;
    private GUIStyle matchPairStyle;
    private GUIStyle infoBoxStyle;
    
    private void OnEnable()
    {
        gameData = (GameLevelData)target;
        levelsProperty = serializedObject.FindProperty("levels");
        
        // Initialize foldouts list
        levelFoldouts.Clear();
        for (int i = 0; i < gameData.levels.Count; i++)
        {
            levelFoldouts.Add(false);
        }
    }
    
    private void EnsureStylesInitialized()
    {
        // Safely initialize styles during OnGUI
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
        }
        
        if (levelHeaderStyle == null)
        {
            levelHeaderStyle = new GUIStyle(EditorStyles.foldout);
            levelHeaderStyle.fontStyle = FontStyle.Bold;
        }
        
        if (matchPairStyle == null)
        {
            matchPairStyle = new GUIStyle(EditorStyles.helpBox);
            matchPairStyle.padding = new RectOffset(10, 10, 10, 10);
            matchPairStyle.margin = new RectOffset(5, 5, 5, 5);
        }
        
        if (infoBoxStyle == null)
        {
            infoBoxStyle = new GUIStyle(EditorStyles.helpBox);
            infoBoxStyle.normal.textColor = Color.yellow;
            infoBoxStyle.fontSize = 11;
            infoBoxStyle.padding = new RectOffset(10, 10, 7, 7);
        }
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Initialize styles during OnGUI to avoid errors
        EnsureStylesInitialized();
        
        // Header
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Game Level Data Manager", headerStyle);
        
        // Level Management Buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Level", GUILayout.Height(25)))
        {
            AddNewLevel();
        }
        
        if (gameData.levels.Count > 0)
        {
            if (GUILayout.Button("Remove Last Level", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Remove Level", 
                    "Are you sure you want to remove the last level and all its cards?", 
                    "Yes", "Cancel"))
                {
                    RemoveLastLevel();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Info about levels
        if (gameData.levels.Count > 0)
        {
            EditorGUILayout.BeginVertical(infoBoxStyle);
            EditorGUILayout.LabelField($"Total Levels: {gameData.levels.Count}", EditorStyles.miniLabel);
            
            int totalCards = 0;
            foreach (var level in gameData.levels)
            {
                totalCards += level.cardDetails.Count;
            }
            EditorGUILayout.LabelField($"Total Cards Across All Levels: {totalCards}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.Space(10);
        
        // Draw levels
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Draw all levels
        for (int i = 0; i < gameData.levels.Count; i++)
        {
            DrawLevel(i);
        }
        
        EditorGUILayout.EndScrollView();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawLevel(int levelIndex)
    {
        SerializedProperty levelProperty = levelsProperty.GetArrayElementAtIndex(levelIndex);
        SerializedProperty levelNameProperty = levelProperty.FindPropertyRelative("levelName");
        SerializedProperty matchPairCountProperty = levelProperty.FindPropertyRelative("matchPairCount");
        SerializedProperty cardsProperty = levelProperty.FindPropertyRelative("cardDetails");
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Level header and controls
        EditorGUILayout.BeginHorizontal();
        
        // Use a foldout for the level
        // Ensure the levelFoldouts list is properly sized
        while (levelIndex >= levelFoldouts.Count)
        {
            levelFoldouts.Add(false);
        }
        
        levelFoldouts[levelIndex] = EditorGUILayout.Foldout(levelFoldouts[levelIndex], 
            $"Level {levelIndex + 1}: {gameData.levels[levelIndex].levelName}", true);
        
        // Display card count
        EditorGUILayout.LabelField($"Cards: {gameData.levels[levelIndex].cardDetails.Count}", 
            GUILayout.Width(70));
        
        if (GUILayout.Button("Add Card", EditorStyles.miniButton, GUILayout.Width(70)))
        {
            AddCardToLevel(levelIndex);
        }
        
        if (gameData.levels[levelIndex].cardDetails.Count > 0 && 
            GUILayout.Button("Clear Cards", EditorStyles.miniButton, GUILayout.Width(70)))
        {
            if (EditorUtility.DisplayDialog("Clear Cards", 
                $"Are you sure you want to remove all cards from Level {levelIndex + 1}?", 
                "Yes", "Cancel"))
            {
                ClearCardsInLevel(levelIndex);
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Level name and match pair count fields
        EditorGUILayout.PropertyField(levelNameProperty, new GUIContent("Level Name"));
        
        // Match Pair Count field with validation and helper
        EditorGUILayout.BeginVertical(matchPairStyle);
        
        // Display warning if matchPairCount is greater than available unique cards
        int uniqueCardCount = GetUniqueCardCount(levelIndex);
        bool isInvalid = gameData.levels[levelIndex].matchPairCount > uniqueCardCount && uniqueCardCount > 0;
        
        GUI.color = isInvalid ? new Color(1f, 0.7f, 0.7f) : Color.white;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(matchPairCountProperty, new GUIContent("Match Pair Count"));
        
        // Auto-set button
        if (gameData.levels[levelIndex].cardDetails.Count > 0)
        {
            if (GUILayout.Button("Auto-Set", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                AutoSetMatchPairCount(levelIndex);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Helpful information about match pairs
        if (isInvalid)
        {
            EditorGUILayout.HelpBox($"Warning: Match pair count ({gameData.levels[levelIndex].matchPairCount}) exceeds unique card count ({uniqueCardCount}).", MessageType.Warning);
        }
        
        GUI.color = Color.white;
        
        // Simple explanation of what this field means
        EditorGUILayout.LabelField("Number of pairs players need to match to complete this level.", 
            EditorStyles.miniLabel);
        
        EditorGUILayout.EndVertical();
        
        // Draw cards if level is expanded
        if (levelFoldouts[levelIndex])
        {
            EditorGUI.indentLevel++;
            
            for (int cardIndex = 0; cardIndex < gameData.levels[levelIndex].cardDetails.Count; cardIndex++)
            {
                DrawCard(levelIndex, cardIndex, cardsProperty);
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private int GetUniqueCardCount(int levelIndex)
    {
        // Simple implementation - just returns the card count
        // Could be enhanced to actually check for unique FaceOn/FaceOff combinations if needed
        return gameData.levels[levelIndex].cardDetails.Count;
    }
    
    private void AutoSetMatchPairCount(int levelIndex)
    {
        Undo.RecordObject(gameData, "Auto-Set Match Pair Count");
        
        // Set match pair count to the total number of unique cards
        int uniqueCardCount = GetUniqueCardCount(levelIndex);
        gameData.levels[levelIndex].matchPairCount = uniqueCardCount;
        
        EditorUtility.SetDirty(gameData);
    }
    
    private void DrawCard(int levelIndex, int cardIndex, SerializedProperty cardsProperty)
    {
        SerializedProperty cardProperty = cardsProperty.GetArrayElementAtIndex(cardIndex);
        SerializedProperty faceOnProperty = cardProperty.FindPropertyRelative("faceOn");
        SerializedProperty faceOffProperty = cardProperty.FindPropertyRelative("faceOff");
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Card header with remove button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Card #{cardIndex + 1}", EditorStyles.boldLabel);
        
        // Card navigation and removal
        EditorGUILayout.BeginHorizontal(GUILayout.Width(150));
        
        // Duplicate button
        if (GUILayout.Button("Duplicate", EditorStyles.miniButtonLeft, GUILayout.Width(70)))
        {
            DuplicateCard(levelIndex, cardIndex);
        }
        
        // Up button
        GUI.enabled = cardIndex > 0;
        if (GUILayout.Button("↑", EditorStyles.miniButtonMid, GUILayout.Width(25)))
        {
            MoveCardUp(levelIndex, cardIndex);
        }
        
        // Down button
        GUI.enabled = cardIndex < gameData.levels[levelIndex].cardDetails.Count - 1;
        if (GUILayout.Button("↓", EditorStyles.miniButtonMid, GUILayout.Width(25)))
        {
            MoveCardDown(levelIndex, cardIndex);
        }
        
        // Remove button
        GUI.enabled = true;
        if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(25)))
        {
            if (EditorUtility.DisplayDialog("Remove Card", 
                $"Are you sure you want to remove Card #{cardIndex + 1}?", 
                "Yes", "Cancel"))
            {
                RemoveCard(levelIndex, cardIndex);
                return; // Early exit to avoid accessing deleted card
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndHorizontal();
        
        // Face On
        EditorGUILayout.LabelField("Face On", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(faceOnProperty.FindPropertyRelative("name"), new GUIContent("Type"));
        EditorGUILayout.PropertyField(faceOnProperty.FindPropertyRelative("sprite"), new GUIContent("Sprite"));
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(5);
        
        // Face Off
        EditorGUILayout.LabelField("Face Off", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(faceOffProperty.FindPropertyRelative("name"), new GUIContent("Type"));
        EditorGUILayout.PropertyField(faceOffProperty.FindPropertyRelative("sprite"), new GUIContent("Sprite"));
        EditorGUI.indentLevel--;
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    // --- Level Operations ---
    
    private void AddNewLevel()
    {
        Undo.RecordObject(gameData, "Add Level");
        
        LevelData newLevel = new LevelData();
        newLevel.levelName = $"Level {gameData.levels.Count + 1}";
        gameData.levels.Add(newLevel);
        levelFoldouts.Add(true); // Auto-expand new level
        
        EditorUtility.SetDirty(gameData);
    }
    
    private void RemoveLastLevel()
    {
        if (gameData.levels.Count == 0) return;
        
        Undo.RecordObject(gameData, "Remove Level");
        
        int lastIndex = gameData.levels.Count - 1;
        gameData.levels.RemoveAt(lastIndex);
        
        // Make sure we don't go out of bounds
        if (lastIndex < levelFoldouts.Count)
        {
            levelFoldouts.RemoveAt(lastIndex);
        }
        
        EditorUtility.SetDirty(gameData);
    }
    
    // --- Card Operations ---
    
    private void AddCardToLevel(int levelIndex)
    {
        Undo.RecordObject(gameData, "Add Card");
        
        CardDetails newCard = new CardDetails
        {
            faceOn = new FaceOn(),
            faceOff = new FaceOff()
        };
        
        gameData.levels[levelIndex].cardDetails.Add(newCard);
        EditorUtility.SetDirty(gameData);
    }
    
    private void DuplicateCard(int levelIndex, int cardIndex)
    {
        Undo.RecordObject(gameData, "Duplicate Card");
        
        // Create a new card with same data
        CardDetails original = gameData.levels[levelIndex].cardDetails[cardIndex];
        CardDetails duplicate = new CardDetails
        {
            faceOn = new FaceOn
            {
                name = original.faceOn.name,
                sprite = original.faceOn.sprite
            },
            faceOff = new FaceOff
            {
                name = original.faceOff.name,
                sprite = original.faceOff.sprite
            }
        };
        
        // Insert the duplicate right after the original
        gameData.levels[levelIndex].cardDetails.Insert(cardIndex + 1, duplicate);
        EditorUtility.SetDirty(gameData);
    }
    
    private void RemoveCard(int levelIndex, int cardIndex)
    {
        Undo.RecordObject(gameData, "Remove Card");
        
        gameData.levels[levelIndex].cardDetails.RemoveAt(cardIndex);
        EditorUtility.SetDirty(gameData);
    }
    
    private void ClearCardsInLevel(int levelIndex)
    {
        Undo.RecordObject(gameData, "Clear Cards");
        
        gameData.levels[levelIndex].cardDetails.Clear();
        EditorUtility.SetDirty(gameData);
    }
    
    private void MoveCardUp(int levelIndex, int cardIndex)
    {
        if (cardIndex <= 0) return;
        
        Undo.RecordObject(gameData, "Move Card Up");
        
        var cards = gameData.levels[levelIndex].cardDetails;
        var temp = cards[cardIndex];
        cards[cardIndex] = cards[cardIndex - 1];
        cards[cardIndex - 1] = temp;
        
        EditorUtility.SetDirty(gameData);
    }
    
    private void MoveCardDown(int levelIndex, int cardIndex)
    {
        var cards = gameData.levels[levelIndex].cardDetails;
        if (cardIndex >= cards.Count - 1) return;
        
        Undo.RecordObject(gameData, "Move Card Down");
        
        var temp = cards[cardIndex];
        cards[cardIndex] = cards[cardIndex + 1];
        cards[cardIndex + 1] = temp;
        
        EditorUtility.SetDirty(gameData);
    }
}