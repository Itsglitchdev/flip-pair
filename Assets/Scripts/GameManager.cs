using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Constants Settings")]
    private const float DELAYBEFOREFLIPBACK = 1f;
    private const float DELAYBEFOREDESTROY = 1.5f;
    private const float DELAYFORTHEBLAST = 2f;

    [Header("Game Data")]
    [SerializeField] private GameLevelData gameData;

    [Header("UI Elements")]
    [SerializeField] private Text currentLevelText;
    [SerializeField] private Timer timer;

    [Header("Popup Panels")]
    [SerializeField] private GameObject popupAboutPanel;
    [SerializeField] private GameObject popupWinPanel;
    [SerializeField] private GameObject popupLosePanel;

    [Header("Popup settings")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Text currentLevelDesciptionText;

    [Header("Card Settings")]
    [SerializeField] private GameObject cardHolderParent;
    [SerializeField] private GameObject cardPrefab;

    private GameObject firstCard;
    private GameObject secondCard;
    private FaceOnType firstCardType;
    private FaceOnType secondCardType;

    private bool canSelect = true;
    private int matchedPairs = 0;
    private int currentLevelIndex = 0;
    private int currentLevelTotalPairs;
    private int currentLevelCardCount;
    private bool isGameStarted = false;


    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (gameData == null)
        {
            Debug.LogError("Game Data is not assigned in the inspector.");
            return;
        }

        currentLevelIndex = PlayerPrefs.GetInt("LevelIndex", 0);

        // Clamp index to prevent out-of-range errors
        if (currentLevelIndex >= gameData.levels.Count)
        {
            currentLevelIndex = 0;
            PlayerPrefs.SetInt("LevelIndex", currentLevelIndex);
            PlayerPrefs.Save();
        }

        // Initialize level data
        currentLevelTotalPairs = gameData.levels[currentLevelIndex].matchPairCount;
        currentLevelCardCount = gameData.levels[currentLevelIndex].cardDetails.Count;
        currentLevelText.text = gameData.levels[currentLevelIndex].levelName;
        currentLevelDesciptionText.text = gameData.levels[currentLevelIndex].levelDescription;

        // Panels
        popupAboutPanel.SetActive(true);
        popupWinPanel.SetActive(false);
        popupLosePanel.SetActive(false);

    }

    private void Start()
    {
        StartCoroutine(InitializeCardsAndStartGame());
        EventListners();
    }

    private void OnEnable()
    {
        Card.OnObjectClicked += HandleCardSelection;
    }

    private void OnDisable()
    {
        Card.OnObjectClicked -= HandleCardSelection;
    }

    private void EventListners()
    {
        startButton.onClick.AddListener(StartGame);
        nextLevelButton.onClick.AddListener(NextLevel);
        restartButton.onClick.AddListener(RestartGame);
    }

    private void StartGame()
    {
        popupAboutPanel.SetActive(false);
        isGameStarted = true;
        timer.StartTimer();
    }


    private IEnumerator InitializeCardsAndStartGame()
    {
        GridLayoutGroup layout = cardHolderParent.GetComponent<GridLayoutGroup>();
        layout.enabled = true;

        for (int i = 0; i < currentLevelCardCount; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolderParent.transform);
            Card cardScript = newCard.GetComponent<Card>();

            if (cardScript != null)
            {
                var cardData = gameData.levels[currentLevelIndex].cardDetails[i];
                cardScript.objectName = cardData.faceOn.name;
                cardScript.frontImage.sprite = cardData.faceOn.sprite;
                cardScript.backImage.sprite = cardData.faceOff.sprite;
            }
        }

        yield return null;
        layout.enabled = false;

    }

    private void HandleCardSelection(FaceOnType objectName, GameObject card)
    {
        if (!isGameStarted) return;
        if (!canSelect) return;
        if (firstCard == null)
        {
            firstCard = card;
            firstCardType = objectName;
            // Debug.Log("First card selected: " + firstCardName);
        }
        else if (firstCard != card)
        {
            secondCard = card;
            secondCardType = objectName;
            // Debug.Log("Second card selected: " + secondCardName);

            canSelect = false;

            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {

        // Bomb match check first
        if (firstCardType == FaceOnType.Bomb_1 && secondCardType == FaceOnType.Bomb_1)
        {
            Debug.Log("Bomb Match! Game Over.");
            yield return new WaitForSeconds(DELAYFORTHEBLAST);
            isGameStarted = false;
            timer.StopTimer();
            popupLosePanel.SetActive(true);

            yield break;
        }

        // Match check
        if (firstCardType == secondCardType)
        {
            Debug.Log("Match found: " + firstCard);

            yield return new WaitForSeconds(DELAYBEFOREDESTROY);

            if (firstCard != null)
                Destroy(firstCard);

            if (secondCard != null)
                Destroy(secondCard);

            matchedPairs++;

            if (matchedPairs >= currentLevelTotalPairs && currentLevelTotalPairs > 0)
            {
                isGameStarted = false;
                timer.StopTimer();
                popupWinPanel.SetActive(true);
                Debug.Log("Game Complete! All pairs matched!");
            }
        }
        else
        {
            Debug.Log("Not a match. Flipping cards back.");

            yield return new WaitForSeconds(DELAYBEFOREFLIPBACK);

            if (firstCard != null)
            {
                Card firstCardScript = firstCard.GetComponent<Card>();
                if (firstCardScript != null)
                    firstCardScript.FlipBack();
            }

            if (secondCard != null)
            {
                Card secondCardScript = secondCard.GetComponent<Card>();
                if (secondCardScript != null)
                    secondCardScript.FlipBack();
            }
        }

        firstCard = null;
        secondCard = null;
        firstCardType = default(FaceOnType);
        secondCardType = default(FaceOnType);

        yield return new WaitForSeconds(0.5f);
        canSelect = true;
    }

    private void NextLevel()
    {

        if (currentLevelIndex >= gameData.levels.Count - 1)
        {
            currentLevelIndex = 0;
        }
        else
        {
            currentLevelIndex++;
        }

        PlayerPrefs.SetInt("LevelIndex", currentLevelIndex);
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneName.Loading.ToString());

    }
    
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}