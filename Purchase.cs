using Scripts.Payes;
using Scripts.Secure;
using Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

[RequireComponent(typeof(PurchaseManager))]
public class Purchase : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _txtCoinsInShop;
    public delegate void PayEvents(int value);
    public static event PayEvents PayBirdEvent;
    private WorkWithCoinsAndCout _secureCoin;
    private PurchaseManager _purchaseManager;
    

    private void Awake()
    {
        _purchaseManager = GetComponent<PurchaseManager>();
        PurchaseManager.OnPurchaseConsumable += OnPurchaseConsumable;
        PurchaseManager.OnPurchaseNonConsumable += OnPurchaseNonConsumable;
        PurchaseManager.OnPurchaseSubscription += OnPurchaseSubscription;
    }

    private void OnPurchaseConsumable(PurchaseEventArgs args)
    {
        var id = args.purchasedProduct.definition.id;
        var payId = _purchaseManager.C_PRODUCTS;
        int[] coinsValue = {10, 50, 100};
        for (int i = 0; i < payId.Length; i++)
        {
            if (id != payId[i]) continue;
            _secureCoin.CoinUpdate();
            PlayerPrefsSafe.SetInt("savescoins", _secureCoin.Coin + coinsValue[i]);
            break;
        }
        _txtCoinsInShop.text = $"{PlayerPrefsSafe.GetInt("savescoins")}";
    }
    private void OnPurchaseNonConsumable(PurchaseEventArgs args)
    {
        var id = args.purchasedProduct.definition.id;
        if (id == "pay_bird4")
        {
            PayBirdEvent?.Invoke(4);
            return;
        }
    }
    private void OnPurchaseSubscription(PurchaseEventArgs args)
    {
        var id = args.purchasedProduct.definition.id;
        if (id == "pay_noads")
        {
            PlayerPrefsSafe.SetInt("ADS", 1);
            return;
        }
    }
}