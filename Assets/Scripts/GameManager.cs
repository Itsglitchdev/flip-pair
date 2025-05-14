using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float delayBeforeFlipBack = 1f; 
    
    private GameObject firstCard;
    private GameObject secondCard;
    private string firstCardName;
    private string secondCardName;
    
    private bool canSelect = true; 
    private int matchedPairs = 0;
    private int totalPairs; 

    private void Start()
    {
        totalPairs = 2; 
    }

    private void OnEnable()
    {
        ObjectTest.OnObjectClicked += HandleCardSelection;
    }

    private void OnDisable()
    {
        ObjectTest.OnObjectClicked -= HandleCardSelection;
    }

    private void HandleCardSelection(string objectName, GameObject card)
    {
        if (!canSelect) return; 
        
        if (firstCard == null)
        {
            // First card selection
            firstCard = card;
            firstCardName = objectName;
            Debug.Log("First card selected: " + firstCardName);
        }
        else if (firstCard != card) // Ensure we're not clicking the same card twice
        {
            // Second card selection
            secondCard = card;
            secondCardName = objectName;
            Debug.Log("Second card selected: " + secondCardName);
            
            // Prevent further selection while checking this pair
            canSelect = false;
            
            // Check for a match
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        if (firstCardName == secondCardName)
        {
            Debug.Log("Match found: " + firstCardName);
            
            yield return new WaitForSeconds(1.5f);
            
            if (firstCard != null)
                Destroy(firstCard);
            
            if (secondCard != null)
                Destroy(secondCard);
            
            matchedPairs++;
            
            if (matchedPairs >= totalPairs && totalPairs > 0)
            {
                Debug.Log("Game Complete! All pairs matched!");
            }
        }
        else
        {
            Debug.Log("Not a match. Flipping cards back.");
            
            yield return new WaitForSeconds(delayBeforeFlipBack);
            
            if (firstCard != null)
            {
                ObjectTest firstCardScript = firstCard.GetComponent<ObjectTest>();
                if (firstCardScript != null)
                    firstCardScript.FlipBack();
            }
            
            if (secondCard != null)
            {
                ObjectTest secondCardScript = secondCard.GetComponent<ObjectTest>();
                if (secondCardScript != null)
                    secondCardScript.FlipBack();
            }
        }
        
        firstCard = null;
        secondCard = null;
        firstCardName = null;
        secondCardName = null;
        
        yield return new WaitForSeconds(0.5f);
        canSelect = true;
    }
}