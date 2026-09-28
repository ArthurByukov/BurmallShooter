using UnityEngine;
using UnityEngine.UI;

public class WeaponFollowCamera : MonoBehaviour
{
    [Header("Настройки перемещения")]
    public bool enablePositionOffset = true;   // включить смещение позиции
    public float horizontalOffset = 10f;       // макс. смещение по X (пиксели)
    public float verticalOffset = 5f;          // макс. смещение по Y (пиксели)
    public bool invertHorizontal = false;      // инвертировать горизонталь
    public bool invertVertical = false;        // инвертировать вертикаль

    [Header("Настройки поворота (наклона)")]
    public bool enableRotation = true;         // включить поворот
    public float maxTiltAngle = 5f;            // макс. угол наклона (градусы)
    public bool invertRotation = false;        // инвертировать направление наклона

    [Header("Сглаживание")]
    public float smoothness = 8f;              // скорость интерполяции

    private RectTransform rectTransform;
    private Vector2 originalAnchoredPos;
    private Vector2 currentAnchoredPos;
    private Vector2 targetAnchoredPos;

    private float currentRotation;
    private float targetRotation;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("WeaponFollowCamera: на объекте нет RectTransform!");
            return;
        }
        originalAnchoredPos = rectTransform.anchoredPosition;
        currentAnchoredPos = originalAnchoredPos;
        targetAnchoredPos = originalAnchoredPos;
        currentRotation = 0f;
        targetRotation = 0f;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // --- Перемещение (смещение позиции) ---
        if (enablePositionOffset)
        {
            float offsetX = mouseX * horizontalOffset * (invertHorizontal ? -1 : 1);
            float offsetY = -mouseY * verticalOffset * (invertVertical ? -1 : 1);
            targetAnchoredPos = originalAnchoredPos + new Vector2(offsetX, offsetY);
        }
        else
        {
            targetAnchoredPos = originalAnchoredPos;
        }

        currentAnchoredPos = Vector2.Lerp(currentAnchoredPos, targetAnchoredPos, Time.deltaTime * smoothness);
        rectTransform.anchoredPosition = currentAnchoredPos;

        // --- Поворот (наклон) ---
        if (enableRotation)
        {
            // Обычно при движении мыши вправо оружие наклоняется вправо (отрицательный угол Z)
            float rotationSign = invertRotation ? 1 : -1;
            targetRotation = rotationSign * mouseX * maxTiltAngle;
        }
        else
        {
            targetRotation = 0f;
        }

        currentRotation = Mathf.Lerp(currentRotation, targetRotation, Time.deltaTime * smoothness);
        rectTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
    }
}