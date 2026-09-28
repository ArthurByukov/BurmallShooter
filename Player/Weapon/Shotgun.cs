using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shotgun : MonoBehaviour
{
    [Header("Attack Settings")]
    public bool Auto = false;
    public float Damage = 25f;
    public float Range = 1.5f;
    public float attackCooldown = 0.5f;

    private float lastAttackTime;
    public Animator animator;
    public int maxAmmo = 30;
    public int magazineSize = 6;
    private int currentMagazine;
    private int reserveAmmo;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("Visual Effects")]
    public string GunName;
    public AudioClip shootSound;
    public TMP_Text GunInfo;
    private AudioSource audioSource;
    public GameObject crosshair;
    // ----- Настройки отдачи -----
    [Header("Recoil Settings")]
    public float recoilUp = 2f;
    public float recoilSide = 1f;
    public float recoilReturnTime = 0.5f;
    public CameraScript cameraScript;

    // ----- Настройки прицеливания (Aim) -----
    [Header("Aim Settings")]
    public bool CanAim = false;
    public float aimFOV = 40f;
    public float aimTransitionTime = 0.3f;

    private bool isAiming = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log("найден аниматор");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        currentMagazine = magazineSize;
        reserveAmmo = maxAmmo - magazineSize;
        if (reserveAmmo < 0) reserveAmmo = 0;
        UpdateUI();

        if (cameraScript == null)
            cameraScript = FindObjectOfType<CameraScript>();
        if (cameraScript == null)
            Debug.LogWarning("CameraScript не найден! Отдача и прицеливание не будут работать.");
    }

    public void Update()
    {
        // ---- Прицеливание ----
        if (CanAim)
        {
            bool aimPressed = Input.GetMouseButton(1); // удержание правой кнопки
            if (aimPressed != isAiming)
            {
                isAiming = aimPressed;
                if (animator != null)
                    animator.SetBool("aim", isAiming);
                if (cameraScript != null)
                    cameraScript.SetWeaponAim(isAiming, aimFOV, aimTransitionTime);
            }
        }
        else
        {
            // Если оружие не может прицеливаться, сбрасываем состояние
            if (isAiming)
            {
                isAiming = false;
                if (animator != null)
                    animator.SetBool("aim", false);
                if (cameraScript != null)
                    cameraScript.SetWeaponAim(false, aimFOV, aimTransitionTime);
            }
        }

        // ---- Стрельба и перезарядка ----
        if (isReloading) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (Auto)
        {
            if (Input.GetMouseButton(0) && currentMagazine > 0)
                PerformAttack(Damage, Range);
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && currentMagazine > 0)
                PerformAttack(Damage, Range);
        }

        if (Input.GetKeyDown(KeyCode.R))
            StartReload();
    }

    void PerformAttack(float damage, float range)
    {
        lastAttackTime = Time.time;
        currentMagazine--;
        UpdateUI();

        if (!isAiming) { animator.SetTrigger("Shot"); }
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        // Raycast
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range))
        {
            HealthNPC enemy = hit.collider.GetComponent<HealthNPC>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Нанесено {damage} урона врагу!");
            }
        }

        // Отдача
        if (cameraScript != null)
        {
            float randomSide = Random.Range(-recoilSide, recoilSide);
            cameraScript.ApplyRecoil(recoilUp, randomSide, recoilReturnTime);
        }
    }

    void StartReload()
    {
        if (isReloading) return;
        if (currentMagazine == magazineSize) return;
        if (reserveAmmo <= 0) return;

        isReloading = true;
        animator.SetTrigger("Rel");
        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        int needed = magazineSize - currentMagazine;
        int toLoad = Mathf.Min(needed, reserveAmmo);
        currentMagazine += toLoad;
        reserveAmmo -= toLoad;

        isReloading = false;
        UpdateUI();
        Debug.Log($"Перезарядка завершена. В магазине: {currentMagazine}, резерв: {reserveAmmo}");
    }

    void GetAmmo(int amount)
    {
        int currentTotal = currentMagazine + reserveAmmo;
        int maxTotal = maxAmmo;
        int freeSpace = maxTotal - currentTotal;
        int toAdd = Mathf.Min(amount, freeSpace);
        reserveAmmo += toAdd;
        UpdateUI();
        Debug.Log($"Подобрано {toAdd} патронов. Резерв: {reserveAmmo}");
    }

    void UpdateUI()
    {
        if (GunInfo != null)
        {
            GunInfo.text = GunName + "\n" +
                           "Damage: " + Damage + "\n" +
                           "distance:" + Range + "\n" +
                           "InMag/MagSize\n" +
                           currentMagazine + "/" + magazineSize + " (" + reserveAmmo + ")";
        }
    }

    private void OnEnable()
    {
        if (CanAim) { crosshair.SetActive(false); }
    }

    private void OnDisable()
    {
        if (CanAim) { crosshair.SetActive(true); }
    }
}