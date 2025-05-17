using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public static Action<FaceOnType, GameObject> OnObjectClicked;

    [SerializeField] public FaceOnType objectName;
    [SerializeField] public Image frontImage;
    [SerializeField] public Image backImage;
    
    [SerializeField] private float rotationDuration = 0.5f; 
    
    private bool isRotating = false;
    private bool isShowingFront = false;

    private void Start()
    {
        if (frontImage != null && backImage != null)
        {
            frontImage.enabled = isShowingFront;
            backImage.enabled = !isShowingFront;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isRotating)
        {
            StartCoroutine(RotateObject());
            OnObjectClicked?.Invoke(objectName, this.gameObject);
        }
    }

    public void FlipBack()
    {
        if (!isRotating && isShowingFront)
        {
            StartCoroutine(RotateObject());
        }
    }

    private IEnumerator RotateObject()
    {
        isRotating = true;
        
        Quaternion startRotation = transform.rotation;
        
        Vector3 targetEuler = transform.eulerAngles + new Vector3(0, 180f, 0);
        Quaternion targetRotation = Quaternion.Euler(targetEuler);

        float elapsedTime = 0f;
        bool imageSwitched = false;

        // Smooth rotation using Slerp
        while (elapsedTime < rotationDuration)
        {
            float t = elapsedTime / rotationDuration;
            
            // Use smooth step for more natural easing
            float smoothT = Mathf.SmoothStep(0, 1, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
            
            
            // Switch images at the halfway point of rotation
            if (!imageSwitched && t > 0.5f)
            {
                isShowingFront = !isShowingFront;
                UpdateImages();
                imageSwitched = true;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at exactly the target rotation
        transform.rotation = targetRotation;
        
        // Make sure images are properly set in case we missed the 0.5 point
        if (!imageSwitched)
        {
            isShowingFront = !isShowingFront;
            UpdateImages();
        }
        
        isRotating = false;
    }
    
    private void UpdateImages()
    {
        if (frontImage != null && backImage != null)
        {
            frontImage.enabled = isShowingFront;
            backImage.enabled = !isShowingFront;
        }
    }
}