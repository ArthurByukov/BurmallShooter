using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public enum RotationAxes
    {
        XandY, X, Y
    }

    [Header("Основное управление мышью")]
    public RotationAxes _axes = RotationAxes.XandY;
    public float _rotationSpeedHor = 5.0f;
    public float _rotationSpeedVer = 5.0f;
    public float maxVert = 45.0f;
    public float minVert = -45.0f;

    [Header("Эффекты движения (ссылка на игрока)")]
    public PlayerMovement playerMovement;

    [Header("Настройки анимации камеры")]
    public float smoothness = 8f;
    public float maxTiltAngle = 5f;
    public float maxYawOffset = 5f;
    public float speedForMaxEffect = 10f;

    [Header("Настройки FOV")]
    public float baseFov = 60f;
    public float maxFov = 80f;
    public float fovSpeedFactor = 0.5f;
    public float fovSmoothness = 5f;      // скорость интерполяции FOV (используется и для прицеливания)

    // ----- Настройки отдачи -----
    [Header("Recoil Settings")]
    public float recoilSmoothness = 10f;
    public float recoilReturnSpeed = 5f;

    private float _rotationX = 0;
    private float _rotationY = 0;

    // Смещения от движения
    private float currentPitchOffset = 0f;
    private float currentYawOffset = 0f;
    private float currentRollOffset = 0f;

    // Текущее FOV (плавно изменяется)
    private float currentFov;

    private Camera cam;

    // Переменные для отдачи
    private Vector2 targetRecoilOffset = Vector2.zero;
    private Vector2 currentRecoilOffset = Vector2.zero;
    private float lastShotTime = 0f;
    private float returnDelay = 0.5f;
    private bool isReturning = false;

    // ----- Переменные для прицеливания (Aim) -----
    private bool isWeaponAiming = false;
    private float weaponAimFov = 60f;

    private void Start()
    {

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
            body.freezeRotation = true;

      //  Cursor.visible = false;
      //  Cursor.lockState = CursorLockMode.Locked;

        cam = GetComponent<Camera>();
        if (cam == null)
            Debug.LogError("На объекте камеры нет компонента Camera!");

        currentFov = baseFov;
        if (cam != null) cam.fieldOfView = currentFov;

        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
            Debug.LogWarning("PlayerMovement не найден! Эффекты движения не будут работать.");
    }

    private void Update()
    {
        if (MenuManager.GameplayBlocked) return;
        // ----- Обработка ввода мыши -----
        if (_axes == RotationAxes.XandY)
        {
            _rotationX -= Input.GetAxis("Mouse Y") * _rotationSpeedVer;
            _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);

            float delta = Input.GetAxis("Mouse X") * _rotationSpeedHor;
            _rotationY += delta;
        }
        else if (_axes == RotationAxes.X)
        {
            _rotationY += Input.GetAxis("Mouse X") * _rotationSpeedHor;
        }
        else if (_axes == RotationAxes.Y)
        {
            _rotationX -= Input.GetAxis("Mouse Y") * _rotationSpeedVer;
            _rotationX = Mathf.Clamp(_rotationX, minVert, maxVert);
        }

        // ----- Логика возврата отдачи (targetRecoilOffset) -----
        if (targetRecoilOffset != Vector2.zero)
        {
            if (!isReturning && Time.time - lastShotTime >= returnDelay)
            {
                isReturning = true;
            }

            if (isReturning)
            {
                float step = recoilReturnSpeed * Time.deltaTime;
                targetRecoilOffset.x = Mathf.MoveTowards(targetRecoilOffset.x, 0, step);
                targetRecoilOffset.y = Mathf.MoveTowards(targetRecoilOffset.y, 0, step);

                if (targetRecoilOffset == Vector2.zero)
                    isReturning = false;
            }
        }

        // Интерполяция текущего смещения отдачи к целевому
        currentRecoilOffset.x = Mathf.Lerp(currentRecoilOffset.x, targetRecoilOffset.x, Time.deltaTime * recoilSmoothness);
        currentRecoilOffset.y = Mathf.Lerp(currentRecoilOffset.y, targetRecoilOffset.y, Time.deltaTime * recoilSmoothness);

        ApplyMotionEffects();
    }

    private void ApplyMotionEffects()
    {
        // Эффекты от движения
        Vector3 velocity = Vector3.zero;
        if (playerMovement != null)
            velocity = playerMovement.velocity;

        float speed = velocity.magnitude;
        Vector3 localVel = transform.InverseTransformDirection(velocity);
        float speedPercent = Mathf.Clamp01(speed / speedForMaxEffect);

        float targetPitchOffset = -Mathf.Clamp(localVel.z * speedPercent * -1, -maxTiltAngle, maxTiltAngle);
        float targetRollOffset = Mathf.Clamp(localVel.x * speedPercent * -1, -maxTiltAngle, maxTiltAngle);
        float targetYawOffset = 0f;

        currentPitchOffset = Mathf.Lerp(currentPitchOffset, targetPitchOffset, Time.deltaTime * smoothness);
        currentYawOffset = Mathf.Lerp(currentYawOffset, targetYawOffset, Time.deltaTime * smoothness);
        currentRollOffset = Mathf.Lerp(currentRollOffset, targetRollOffset, Time.deltaTime * smoothness);

        // ---- Вычисление целевого FOV с учётом прицеливания ----
        float targetFov;
        if (isWeaponAiming)
            targetFov = weaponAimFov;
        else
            targetFov = Mathf.Clamp(baseFov + speed * fovSpeedFactor, baseFov, maxFov);

        // Плавная интерполяция FOV (экспоненциальная)
        currentFov = Mathf.Lerp(currentFov, targetFov, 1 - Mathf.Exp(-fovSmoothness * Time.deltaTime));
        if (cam != null) cam.fieldOfView = currentFov;

        // ---- Итоговый угол камеры (с учётом отдачи) ----
        float finalPitch = _rotationX + currentPitchOffset + currentRecoilOffset.x;
        float finalYaw = _rotationY + currentYawOffset + currentRecoilOffset.y;

        // Ограничиваем вертикаль, чтобы не улететь выше/ниже
        finalPitch = Mathf.Clamp(finalPitch, minVert, maxVert);

        transform.localEulerAngles = new Vector3(finalPitch, finalYaw, currentRollOffset);
    }

    // ---- Публичные методы для оружия ----

    // Вызов отдачи
    public void ApplyRecoil(float up, float side, float delay)
    {
        targetRecoilOffset.x += up;
        targetRecoilOffset.y += side;

        lastShotTime = Time.time;
        returnDelay = delay;
        isReturning = false;
    }

    // Установка состояния прицеливания (вызывается из оружия)
    public void SetWeaponAim(bool aiming, float targetFov, float transitionTime)
    {
        isWeaponAiming = aiming;
        weaponAimFov = targetFov;
        // Скорость интерполяции обратно пропорциональна времени перехода (чем меньше время, тем быстрее)
        fovSmoothness = 1f / Mathf.Max(transitionTime, 0.01f);
    }
}