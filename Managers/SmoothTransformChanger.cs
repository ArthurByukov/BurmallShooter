using System;
using System.Collections;
using UnityEngine;

public class SmoothTransformChanger : MonoBehaviour
{
    private float startYPos, targetYPos;
    private float startYScale, targetYScale;
    private float progress = 1f; 
    private float duration = 1f;
    private bool isAnimating = false;

    [SerializeField] private int[] scaleY = { 1, 5, 10, 15 };
    [SerializeField] private int[] positionY = { 0, 2, 5, 8 };
    public void AnimateHeight(int ChangeNum, float time)
    {
        switch (ChangeNum)
        {
            case 0:
                targetYPos = positionY[0];
                targetYScale = scaleY[0];
                break;
            case 1:
                targetYPos = positionY[1];
                targetYScale = scaleY[1];
                break;
            case 2:
                targetYPos = positionY[2];
                targetYScale = scaleY[2];
                break;
            case 3:
                targetYPos = positionY[3];
                targetYScale = scaleY[3];
                break;
        }
        startYPos = transform.localPosition.y;
        startYScale = transform.localScale.y;
        duration = time;
        progress = 0f;
        isAnimating = true;
    }

    void Update()
    {
        if (!isAnimating) return;

        progress += Time.deltaTime / duration;
        if (progress >= 1f)
        {
            progress = 1f;
            isAnimating = false;
        }

        
        float t = progress * progress * (3f - 2f * progress);

       
        Vector3 pos = transform.localPosition;
        pos.y = Mathf.Lerp(startYPos, targetYPos, t);
        transform.localPosition = pos;

        Vector3 scale = transform.localScale;
        scale.y = Mathf.Lerp(startYScale, targetYScale, t);
        transform.localScale = scale;
    }
}