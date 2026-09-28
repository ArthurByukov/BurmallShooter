using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YG;

public class WeaponSlotUI : MonoBehaviour
{
    public WeaponType slotType;             // Primary или Secondary

    [Header("Display")]
    public Image weaponImage;
    public TMP_Text weaponNameText;
    public TMP_Text statsText;              // урон / дальность / магазин / боезапас
    public TMP_Text levelText;              // "Уровень: 0/3" или "Не куплено"
    public TMP_Text priceText;              // цена покупки/апгрейда (опционально, дублирует кнопки)

    [Header("Buttons")]
    public Button leftButton;
    public Button rightButton;
    public Button buyButton;
    public Button upgradeButton;
    public Button selectButton;

    private WeaponDatabase database;
    private WeaponShop shop;
    private WeaponShopUI shopUI;

    private readonly List<WeaponData> weapons = new();
    private int currentIndex = 0;

    public void Init(WeaponDatabase db, WeaponShop sh, WeaponShopUI ui)
    {
        database = db;
        shop = sh;
        shopUI = ui;

        weapons.Clear();
        foreach (var w in database.allWeapons)
            if (w != null && w.weaponType == slotType)
                weapons.Add(w);

        // Стартуем с уже экипированного ствола, если он есть в списке
        int equippedId = GetEquippedId();
        int found = weapons.FindIndex(w => w.weaponID == equippedId);
        currentIndex = found >= 0 ? found : 0;

        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();
        buyButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.RemoveAllListeners();
        selectButton.onClick.RemoveAllListeners();

        leftButton.onClick.AddListener(() => ChangeWeapon(-1));
        rightButton.onClick.AddListener(() => ChangeWeapon(1));
        buyButton.onClick.AddListener(OnBuy);
        upgradeButton.onClick.AddListener(OnUpgrade);
        selectButton.onClick.AddListener(OnSelect);

        Refresh();
    }

    int GetEquippedId()
    {
        switch (slotType)
        {
            case WeaponType.Primary: return YG2.saves.equippedPrimary;
            case WeaponType.Secondary: return YG2.saves.equippedSecondary;
            case WeaponType.Melee: return YG2.saves.equippedMelee;
        }
        return -1;
    }

    void ChangeWeapon(int dir)
    {
        if (weapons.Count == 0) return;
        currentIndex = (currentIndex + dir + weapons.Count) % weapons.Count;
        Refresh();
    }

    public void Refresh()
    {
        if (weapons.Count == 0 || currentIndex >= weapons.Count)
        {
            weaponNameText.text = "—";
            if (statsText) statsText.text = "";
            if (levelText) levelText.text = "";
            if (priceText) priceText.text = "";
            if (weaponImage) weaponImage.sprite = null;
            buyButton.gameObject.SetActive(false);
            upgradeButton.gameObject.SetActive(false);
            selectButton.gameObject.SetActive(false);
            return;
        }

        WeaponData data = weapons[currentIndex];

        weaponNameText.text = data.weaponName;
        if (weaponImage) weaponImage.sprite = data.icon;

        bool owned = shop.IsOwned(data.weaponID);
        int level = owned ? shop.GetLevel(data.weaponID) : -1;
        bool equipped = GetEquippedId() == data.weaponID;

        // ---- Stats ----
        int statsLevel = owned ? Mathf.Clamp(level, 0, data.levels.Length - 1) : 0;
        var ld = data.levels[statsLevel];
        if (statsText)
            statsText.text =
                $"Урон: {ld.damage}\n" +
                $"Дальность: {ld.range}\n" +
                $"Магазин: {ld.magazineSize}\n" +
                $"Боезапас: {ld.maxAmmo}";

        // ---- Level ----
        if (levelText)
            levelText.text = owned
                ? $"Уровень: {level} / {data.MaxLevel}"
                : "Не куплено";

        // ---- Buy ----
        if (!owned)
        {
            buyButton.gameObject.SetActive(true);
            bool canAfford = YG2.saves.coins >= data.price;
            buyButton.interactable = canAfford;
            SetButtonLabel(buyButton, $"Купить ({data.price})");
            if (priceText) priceText.text = "";
        }
        else
        {
            buyButton.gameObject.SetActive(false);
        }

        // ---- Upgrade ----
        if (owned && level < data.MaxLevel)
        {
            upgradeButton.gameObject.SetActive(true);
            int cost = (level >= 0 && level < data.upgradeCosts.Length)
                ? data.upgradeCosts[level] : 0;
            upgradeButton.interactable = YG2.saves.coins >= cost;
            SetButtonLabel(upgradeButton, $"Улучшить ({cost})");
        }
        else
        {
            upgradeButton.gameObject.SetActive(false);
        }

        // ---- Select ----
        if (owned)
        {
            selectButton.gameObject.SetActive(true);
            selectButton.interactable = !equipped;
            SetButtonLabel(selectButton, equipped ? "Выбрано" : "Выбрать");
        }
        else
        {
            selectButton.gameObject.SetActive(false);
        }
    }

    void SetButtonLabel(Button btn, string text)
    {
        var label = btn.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = text;
    }

    // ---------------- Actions ----------------

    void OnBuy()
    {
        if (weapons.Count == 0) return;
        var data = weapons[currentIndex];
        if (shop.TryBuy(data.weaponID))
        {
            // Автовыбор, если слот для этого типа пуст
            if (GetEquippedId() < 0)
                shop.TryEquip(data.weaponID);

            shopUI.RefreshAllSlots();
        }
        else
        {
            shopUI.FlashMessage("Недостаточно монет или оружие уже куплено.");
        }
    }

    void OnUpgrade()
    {
        if (weapons.Count == 0) return;
        var data = weapons[currentIndex];
        if (shop.TryUpgrade(data.weaponID))
            shopUI.RefreshAllSlots();
        else
            shopUI.FlashMessage("Недостаточно монет или достигнут максимум.");
    }

    void OnSelect()
    {
        if (weapons.Count == 0) return;
        var data = weapons[currentIndex];
        if (shop.TryEquip(data.weaponID))
            shopUI.RefreshAllSlots();
    }
}