using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Data")]
    public WeaponData weaponData;          // назначается в инспекторе
    public int currentLevel = 0;           // 0 = первый уровень

    [Header("Fire Mode")]
    public bool Auto = false;              // тип стрельбы (не меняется с уровнем)
    public AudioClip shootSound;
    // Внутренние переменные (берутся из данных текущего уровня)
    private float damage;
    private float range;
    private int magazineSize;
    private int maxAmmo;
    private float reloadTime;
    private float attackCooldown;
    private int currentMagazine;
    private int reserveAmmo;
    private float lastAttackTime;
    private bool isReloading = false;

    // Публичный доступ для внешних систем (например, PlayerWeaponManager)
    public bool IsReloading => isReloading;
    // Компоненты
    private Animator animator;
    private AudioSource audioSource;
    private CameraScript cameraScript;  // ваш скрипт камеры

    // Прицеливание
    private bool isAiming = false;

    // UI
    public TMP_Text GunInfo;            // назначается в инспекторе
    public GameObject crosshair;        // назначается в инспекторе

    public int MagazineSize => magazineSize;
    public int CurrentLevel => currentLevel;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("Animator не найден!");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && weaponData != null )
            audioSource = gameObject.AddComponent<AudioSource>();

        cameraScript = FindObjectOfType<CameraScript>();
        if (cameraScript == null)
            Debug.LogWarning("CameraScript не найден! Отдача и прицеливание не будут работать.");

      
    }

    
    private void Awake()
    {
        UpdateUI();
        if (weaponData != null && weaponData.levels.Length > 0)
            ApplyLevel(currentLevel);
        else
            Debug.LogError("Нет данных об оружии или уровней!");
    }
    void Update()
    {
        if (MenuManager.GameplayBlocked) return;
        // ---- Прицеливание ----
        if (weaponData != null && weaponData.canAim)
        {
            bool aimPressed = Input.GetMouseButton(1);
            if (aimPressed != isAiming)
            {
                isAiming = aimPressed;
                if (animator != null)
                    animator.SetBool("aim", isAiming);
                if (cameraScript != null)
                    cameraScript.SetWeaponAim(isAiming, weaponData.aimFOV, weaponData.aimTransitionTime);
            }
        }
        else
        {
            // Если оружие не может прицеливаться – сбрасываем
            if (isAiming)
            {
                isAiming = false;
                if (animator != null)
                    animator.SetBool("aim", false);
                if (cameraScript != null)
                    cameraScript.SetWeaponAim(false, weaponData.aimFOV, weaponData.aimTransitionTime);
            }
        }

        // ---- Стрельба и перезарядка ----
        if (isReloading) return;
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (Auto)
        {
            if (Input.GetMouseButton(0) && currentMagazine > 0)
                PerformAttack();
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && currentMagazine > 0)
                PerformAttack();
        }

        if (Input.GetKeyDown(KeyCode.R))
            StartReload();
    }

    // ---- Основные методы ----

    void PerformAttack()
    {
        lastAttackTime = Time.time;
        currentMagazine--;
        UpdateUI();

        if (!isAiming && animator != null)
            animator.SetTrigger("Shot");

        if (audioSource != null && weaponData != null && shootSound != null)
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
            float randomSide = Random.Range(-weaponData.recoilSide, weaponData.recoilSide);
            cameraScript.ApplyRecoil(weaponData.recoilUp, randomSide, weaponData.recoilReturnTime);
        }
    }

    void StartReload()
    {
        if (isReloading) return;
        if (currentMagazine == magazineSize) return;
        if (reserveAmmo <= 0) return;

        isReloading = true;
        if (animator != null)
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

    // ---- Смена уровня ----

    public void SetLevel(int newLevel)
    {
        if (weaponData == null || weaponData.levels.Length == 0)
        {
            Debug.LogError("Нет данных об оружии!");
            return;
        }
        if (newLevel < 0 || newLevel >= weaponData.levels.Length)
        {
            Debug.LogWarning($"Уровень {newLevel} вне диапазона (0..{weaponData.levels.Length - 1})");
            return;
        }
        currentLevel = newLevel;
        ApplyLevel(currentLevel);
    }

    private void ApplyLevel(int level)
    {
        WeaponLevelData data = weaponData.levels[level];
        damage = data.damage;
        range = data.range;
        magazineSize = data.magazineSize;
        maxAmmo = data.maxAmmo;
        reloadTime = data.reloadTime;
        attackCooldown = data.attackCooldown;

        // Сброс боезапаса при смене уровня (опционально)
        currentMagazine = magazineSize;
        reserveAmmo = maxAmmo - magazineSize;
        if (reserveAmmo < 0) reserveAmmo = 0;
        isReloading = false;

        UpdateUI();
        Debug.Log($"Уровень {level + 1} применён для {weaponData.weaponName}");
    }

    // ---- Пополнение боезапаса (вызывать извне) ----

    public void GetAmmo(int amount)
    {
        int currentTotal = currentMagazine + reserveAmmo;
        int maxTotal = maxAmmo;
        int freeSpace = maxTotal - currentTotal;
        int toAdd = Mathf.Min(amount, freeSpace);
        reserveAmmo += toAdd;
        UpdateUI();
        Debug.Log($"Подобрано {toAdd} патронов. Резерв: {reserveAmmo}");
    }

    public void AddAmmoFromKit()
    {
        GetAmmo(magazineSize);
    }

    // ---- UI ----

    void UpdateUI()
    {
        if (GunInfo != null && weaponData != null)
        {
            GunInfo.text = weaponData.weaponName + "\n" +
                           "Уровень: " + (currentLevel + 1) + "\n" +
                           "Урон: " + damage + "\n" +
                           "Дальность: " + range + "\n" +
                           currentMagazine + "/" + magazineSize + " (" + reserveAmmo + ")";
        }
    }

    // ---- Включение/выключение оружия ----

    private void OnEnable()
    {
        UpdateUI();
        if (weaponData != null && weaponData.canAim && crosshair != null)
            crosshair.SetActive(false);
    }

    private void OnDisable()
    {
        if (weaponData != null && weaponData.canAim && crosshair != null)
            crosshair.SetActive(true);

        // сброс прицеливания
        if (isAiming)
        {
            isAiming = false;
            if (animator != null) animator.SetBool("aim", false);
            if (cameraScript != null)
                cameraScript.SetWeaponAim(false, weaponData.aimFOV, weaponData.aimTransitionTime);
        }
    }
}