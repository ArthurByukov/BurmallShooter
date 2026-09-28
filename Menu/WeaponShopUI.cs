using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YG;

public class WeaponShopUI : MonoBehaviour
{
    [Header("Refs")]
    public WeaponDatabase database;
    public WeaponShop shop;             // оставь пустым — найдём на сцене
    public PlayerWeaponManager playerWeapons;

    [Header("Slots")]
    public WeaponSlotUI primarySlot;
    public WeaponSlotUI secondarySlot;

    [Header("Common UI")]
    public TMP_Text coinsText;
    public TMP_Text messageText;        // для "недостаточно монет" и т.п.
    public Button closeButton;
    public GameObject shopRoot;         // корневая панель магазина

    [Header("Optional")]
    public MenuManager menuManager;     // чтобы вернуться в главное меню

    void Awake()
    {
        if (shop == null) shop = FindObjectOfType<WeaponShop>();
        if (playerWeapons == null) playerWeapons = FindObjectOfType<PlayerWeaponManager>();
        if (menuManager == null) menuManager = FindObjectOfType<MenuManager>();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseShop);
        }
    }

    void OnEnable()
    {
        // Если нужно — можно тут перезагружать данные перед показом
        InitSlots();
        RefreshAllSlots();
        SetMessage("");
    }

    void InitSlots()
    {
        if (primarySlot != null && primarySlot.slotType != WeaponType.Primary)
            Debug.LogWarning("primarySlot имеет не тот slotType!");
        if (secondarySlot != null && secondarySlot.slotType != WeaponType.Secondary)
            Debug.LogWarning("secondarySlot имеет не тот slotType!");

        if (primarySlot != null) primarySlot.Init(database, shop, this);
        if (secondarySlot != null) secondarySlot.Init(database, shop, this);
    }

    public void RefreshAllSlots()
    {
        if (primarySlot != null) primarySlot.Refresh();
        if (secondarySlot != null) secondarySlot.Refresh();
        RefreshCoins();
    }

    void RefreshCoins()
    {
        if (coinsText != null)
            coinsText.text = $"Монеты: {YG2.saves.coins}";
    }

    public void FlashMessage(string msg)
    {
        SetMessage(msg);
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 2f);
    }

    void SetMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
    }

    void ClearMessage()
    {
        if (messageText != null) messageText.text = "";
    }

    // ---------- Кнопки ----------

    public void OpenShop()
    {
        if (shopRoot != null) shopRoot.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void CloseShop()
    {
        // Применяем экипировку к игроку
        if (shop != null) shop.ApplyToPlayer();

        if (shopRoot != null) shopRoot.SetActive(false);
        else gameObject.SetActive(false);
    }
}