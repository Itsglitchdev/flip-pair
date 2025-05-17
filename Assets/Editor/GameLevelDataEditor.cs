using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameLevelData))]
public class GameLevelDataEditor : Editor
{
    private GameLevelData gameLevelData;
    private bool[] showLevelsFoldout;
    private bool[] showCardsFoldout;
    private Vector2 scrollPosition;
    
    private void OnEnable()
    {
        gameLevelData = (GameLevelData)target;
        InitializeFoldouts();
    }
    
    private void InitializeFoldouts()
    {
        // Initialize foldout arrays
        if (gameLevelData.levels != null)
        {
            showLevelsFoldout = new bool[gameLevelData.levels.Count];
            showCardsFoldout = new bool[gameLevelData.levels.Count];
            
            // Default to showing all levels
            for (int i = 0; i < showLevelsFoldout.Length; i++)
            {
                showLevelsFoldout[i] = true;
            }
        }
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Game Level Data Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Add Level and Delete Last Level buttons in the same row
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Add Level", GUILayout.Height(30)))
        {
            AddNewLevel();
        }
        
        GUI.enabled = gameLevelData.levels.Count > 0;
        if (GUILayout.Button("Delete Last Level", GUILayout.Height(30)))
        {
            DeleteLastLevel();
        }
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Check if there are levels to display
        if (gameLevelData.levels == null || gameLevelData.levels.Count == 0)
        {
            EditorGUILayout.HelpBox("No levels available. Add a level to get started.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }
        
        // Begin scroll view for levels
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Display all levels
        for (int levelIndex = 0; levelIndex < gameLevelData.levels.Count; levelIndex++)
        {
            DrawLevelSection(levelIndex);
        }
        
        EditorGUILayout.EndScrollView();
        
        serializedObject.ApplyModifiedProperties();
        
        // Save changes when something is modified
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
    
    private void DrawLevelSection(int levelIndex)
    {
        LevelData level = gameLevelData.levels[levelIndex];
        SerializedProperty levelProperty = serializedObject.FindProperty("levels").GetArrayElementAtIndex(levelIndex);
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        
        // Level header and foldout
        EditorGUILayout.BeginHorizontal();
        showLevelsFoldout[levelIndex] = EditorGUILayout.Foldout(showLevelsFoldout[levelIndex], $"Level {levelIndex + 1}: {level.levelName}", true, EditorStyles.foldoutHeader);
        
        // Delete level button
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            if (EditorUtility.DisplayDialog("Delete Level", $"Are you sure you want to delete level '{level.levelName}'?", "Delete", "Cancel"))
            {
                DeleteLevel(levelIndex);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (showLevelsFoldout[levelIndex])
        {
            EditorGUI.indentLevel++;
            
            // Level name
            SerializedProperty levelNameProp = levelProperty.FindPropertyRelative("levelName");
            EditorGUILayout.PropertyField(levelNameProp, new GUIContent("Level Name"));
            
            // Description
            SerializedProperty descriptionProp = levelProperty.FindPropertyRelative("levelDescription");
            EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Level Description"));

            // Match pair count
            SerializedProperty matchPairCountProp = levelProperty.FindPropertyRelative("matchPairCount");
            EditorGUILayout.PropertyField(matchPairCountProp, new GUIContent("Match Pair Count"));
            
            EditorGUILayout.Space(5);
            
            // Card Details Section
            EditorGUILayout.LabelField("Card Details", EditorStyles.boldLabel);
            
            SerializedProperty cardDetailsProp = levelProperty.FindPropertyRelative("cardDetails");
            
            // Display existing cards
            if (level.cardDetails != null && level.cardDetails.Count > 0)
            {
                for (int cardIndex = 0; cardIndex < level.cardDetails.Count; cardIndex++)
                {
                    DrawCardSection(cardDetailsProp, cardIndex);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No cards added yet. Add cards to this level.", MessageType.Info);
            }
            
            // Add Card button
            if (GUILayout.Button("Add Card"))
            {
                AddNewCard(levelIndex);
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private void DrawCardSection(SerializedProperty cardDetailsProp, int cardIndex)
    {
        SerializedProperty cardProp = cardDetailsProp.GetArrayElementAtIndex(cardIndex);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Card {cardIndex + 1}", EditorStyles.boldLabel);
        
        // Delete card button
        if (GUILayout.Button("Remove", GUILayout.Width(60)))
        {
            cardDetailsProp.DeleteArrayElementAtIndex(cardIndex);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.indentLevel++;
        
        // Face On properties
        SerializedProperty faceOnProp = cardProp.FindPropertyRelative("faceOn");
        EditorGUILayout.LabelField("Face On", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(faceOnProp.FindPropertyRelative("name"), new GUIContent("Type"));
        EditorGUILayout.PropertyField(faceOnProp.FindPropertyRelative("sprite"), new GUIContent("Sprite"));
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(5);
        
        // Face Off properties
        SerializedProperty faceOffProp = cardProp.FindPropertyRelative("faceOff");
        EditorGUILayout.LabelField("Face Off", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(faceOffProp.FindPropertyRelative("name"), new GUIContent("Type"));
        EditorGUILayout.PropertyField(faceOffProp.FindPropertyRelative("sprite"), new GUIContent("Sprite"));
        EditorGUI.indentLevel--;
        
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }
    
    private void AddNewLevel()
    {
        // Create a new level and add it to the list
        LevelData newLevel = new LevelData
        {
            levelName = $"Level {gameLevelData.levels.Count + 1}",
            matchPairCount = 0,
            cardDetails = new List<CardDetails>()
        };
        
        gameLevelData.levels.Add(newLevel);
        
        // Update foldouts array
        System.Array.Resize(ref showLevelsFoldout, gameLevelData.levels.Count);
        System.Array.Resize(ref showCardsFoldout, gameLevelData.levels.Count);
        showLevelsFoldout[gameLevelData.levels.Count - 1] = true;
        
        EditorUtility.SetDirty(target);
    }
    
    private void DeleteLastLevel()
    {
        if (gameLevelData.levels.Count > 0)
        {
            gameLevelData.levels.RemoveAt(gameLevelData.levels.Count - 1);
            
            // Update foldouts array
            System.Array.Resize(ref showLevelsFoldout, gameLevelData.levels.Count);
            System.Array.Resize(ref showCardsFoldout, gameLevelData.levels.Count);
            
            EditorUtility.SetDirty(target);
        }
    }
    
    private void DeleteLevel(int levelIndex)
    {
        gameLevelData.levels.RemoveAt(levelIndex);
        
        // Update the foldout arrays
        bool[] newLevelsFoldout = new bool[gameLevelData.levels.Count];
        bool[] newCardsFoldout = new bool[gameLevelData.levels.Count];
        
        // Copy the remaining foldout states
        for (int i = 0, j = 0; i < showLevelsFoldout.Length; i++)
        {
            if (i != levelIndex)
            {
                newLevelsFoldout[j] = showLevelsFoldout[i];
                newCardsFoldout[j] = showCardsFoldout[i];
                j++;
            }
        }
        
        showLevelsFoldout = newLevelsFoldout;
        showCardsFoldout = newCardsFoldout;
        
        EditorUtility.SetDirty(target);
    }
    
    private void AddNewCard(int levelIndex)
    {
        // Create a new card and add it to the level
        CardDetails newCard = new CardDetails
        {
            faceOn = new FaceOn(),
            faceOff = new FaceOff()
        };
        
        gameLevelData.levels[levelIndex].cardDetails.Add(newCard);
        EditorUtility.SetDirty(target);
    }
}
