using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData")]
public class GameLevelData : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();
}

[System.Serializable]
public class LevelData
{
    public int matchPairCount = 0;
    public string levelName = "New Level";
    public string levelDescription = "";
    public List<CardDetails> cardDetails = new List<CardDetails>();
}

[System.Serializable]
public class CardDetails
{
    public FaceOn faceOn;
    public FaceOff faceOff;
}

[System.Serializable]
public class FaceOn
{
    public FaceOnType name;
    public Sprite sprite;
}

[System.Serializable]
public class FaceOff
{
    public FaceOffType name = FaceOffType.FaceOff_1;
    public Sprite sprite;
}