using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Характеристики здоровья")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float currentHealth;
    public Text healthText;
    public Text ArmorText;
    public string Htext;
    public string Atext;
    [Header("Характеристики брони")]
    [SerializeField] private float maxArmor = 300f;
    [SerializeField] private float currentArmor;

    [Header("События")]
    public UnityEvent OnDamageTaken;
    public UnityEvent OnDeath;
    public UnityEvent<float, float> OnHealthChanged; // текущее здоровье, максимальное здоровье
    public UnityEvent<float, float> OnArmorChanged; // текущая броня, максимальная броня

    [Header("Параметры защиты")]
    [SerializeField] private float armorDamageReduction = 0.5f; // 50% защиты

    private bool isDead = false;

    // Свойства для доступа к значениям
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentArmor => currentArmor;
    public float MaxArmor => maxArmor;
    public bool IsDead => isDead;

    [Header("Visual Effects")]
    public AudioClip HealSound;        // Звук выстрела
    private AudioSource audioSource;
    private void Start()
    {
        // Инициализация
        currentHealth = maxHealth;
        currentArmor = 0f;
        updateUI();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && HealSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Вызываем события при старте
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnArmorChanged?.Invoke(currentArmor, maxArmor);
    }

    // Метод для получения урона
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float remainingDamage = damage;
        float damageToArmor = 0f;
        float damageToHealth = 0f;

        // Если есть броня, она защищает от половины урона
        if (currentArmor > 0)
        {
            // Броня поглощает половину урона
            damageToArmor = damage * armorDamageReduction;
            damageToHealth = damage - damageToArmor;

            // Уменьшаем броню
            float newArmor = currentArmor - damageToArmor;

            if (newArmor < 0)
            {
                // Если броня закончилась, остаток урона идет в здоровье
                damageToHealth += Mathf.Abs(newArmor);
                currentArmor = 0;
            }
            else
            {
                currentArmor = newArmor;
            }
        }
        else
        {
            // Брони нет - весь урон идет в здоровье
            damageToHealth = damage;
        }

        // Применяем урон к здоровью
        currentHealth -= damageToHealth;
        updateUI();

        // Вызываем события
        OnDamageTaken?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnArmorChanged?.Invoke(currentArmor, maxArmor);

        // Проверка на смерть
        if (currentHealth <= 0)
        {
            Die();
        }

        // Логирование
        Debug.Log($"Получен урон: {damage} (в броню: {damageToArmor}, в здоровье: {damageToHealth}). " +
                  $"Здоровье: {currentHealth}/{maxHealth}, Броня: {currentArmor}/{maxArmor}");
    }

    // Метод для лечения
    public void Heal(float amount, bool isBigMedkit = false)
    {
        if (isDead) return;

        float oldHealth = currentHealth;

        // Проверяем можно ли лечить
        if (currentHealth >= maxHealth)
        {
            Debug.Log("Здоровье уже максимально!");
            return;
        }

        // Если здоровье больше 100, лечим только большими аптечками
        if (currentHealth >= 100 && !isBigMedkit)
        {
            Debug.Log("Маленькая аптечка не работает при здоровье выше 100! Используйте большую аптечку.");
            return;
        }

        // Лечим
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        updateUI();
        audioSource.PlayOneShot(HealSound);
        // Вызываем событие
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"Вылечено {currentHealth - oldHealth} HP. Текущее здоровье: {currentHealth}/{maxHealth}");
    }


    public void AddArmor(float amount,bool isBigMedkit = false)
    {
        if (isDead) return;

        float oldArmor = currentArmor;

        // Проверяем можно ли лечить
        if (currentArmor >= maxArmor)
        {
            Debug.Log("Броня уже максимальна!");
            return;
        }

        // Если здоровье больше 100, лечим только большими аптечками
        if (currentArmor >= 100 && !isBigMedkit)
        {
            Debug.Log("Маленькая броня не работает при здоровье выше 100! Используйте большую броню.");
            return;
        }

        currentArmor = Mathf.Min(currentArmor + amount, maxArmor);

        // Вызываем событие
        OnArmorChanged?.Invoke(currentArmor, maxArmor);

        updateUI();
        Debug.Log($"Добавлено {currentArmor - oldArmor} брони. Текущая броня: {currentArmor}/{maxArmor}");
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
        Debug.Log("Игрок погиб!");
        // Здесь можно добавить логику смерти (перезагрузка уровня, экран смерти и т.д.)
    }

    // Метод для полного восстановления (например, при загрузке уровня)
    public void FullRestore()
    {
        currentHealth = maxHealth;
        currentArmor = 0;
        isDead = false;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnArmorChanged?.Invoke(currentArmor, maxArmor);

        Debug.Log("Полное восстановление!");
    }

    public void updateUI()
    {
        healthText.text = Htext + currentHealth.ToString()+"/" + maxHealth;
        ArmorText.text = Atext + currentArmor.ToString() + "/" + maxArmor;
    }
}