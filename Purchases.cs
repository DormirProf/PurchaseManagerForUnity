using System;
using System.Collections.Generic;
using Scripts.Secure;
using Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Scripts.Shop
{
    public class Purchases : MonoBehaviour, IStoreListener
    {
        private static IStoreController m_StoreController;          // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        private IAppleExtensions m_AppleExtensions;
        private IGooglePlayStoreExtensions m_GoogleExtensions;
		
		//Example
        private const string _10Coins = "pay_coins10";
        private const string _50Coins = "pay_coins50";
        private const string _100Coins = "pay_coins100";
        private const string _bird = "pay_bird4";
        private const string _noAds = "pay_noads";

        private static readonly string[] CONSUMABLE = {_10Coins, _50Coins, _100Coins};

        private void Start()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            builder.AddProduct(_bird, ProductType.NonConsumable);
            foreach (var cons in CONSUMABLE)
            {
                builder.AddProduct(cons, ProductType.Consumable);
            }
            builder.AddProduct(_noAds, ProductType.Subscription);
            UnityPurchasing.Initialize(this, builder);
        }

        public void OnPurchaseComplete(Product product)
        {
            switch (product.definition.id)
            {
                case _10Coins:
                    PlayerPrefsSafe.SetInt("savescoins", _secureCoin.Coin + 10);
                    break;
                case _50Coins:
                    PlayerPrefsSafe.SetInt("savescoins", _secureCoin.Coin + 50);
                    break;
                case _100Coins:
                    PlayerPrefsSafe.SetInt("savescoins", _secureCoin.Coin + 100);
                    break;
                case _bird:
                    //code
                    break;
                case _noAds:
                    PurchaseSubscription();
                    break;
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            OnPurchaseComplete(purchaseEvent.purchasedProduct);
            
            Debug.Log(string.Format("ProcessPurchase: " + purchaseEvent.purchasedProduct.definition.id));

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            Debug.Log( $"Purchase of {product.metadata.localizedTitle} failed due to {reason}");
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("OnInitialized: PASS");

            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
            m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
            m_GoogleExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();

            m_GoogleExtensions?.SetDeferredPurchaseListener(OnPurchaseDeferred);
            
            Dictionary<string, string> dict = m_AppleExtensions.GetIntroductoryPriceDictionary();
            foreach (Product item in controller.products.all)
            {

                if (item.receipt != null)
                {
                    string intro_json = (dict == null || !dict.ContainsKey(item.definition.storeSpecificId)) ? null : dict[item.definition.storeSpecificId];

                    if (item.definition.type == ProductType.Subscription)
                    {
                        SubscriptionManager p = new SubscriptionManager(item, intro_json);
                        SubscriptionInfo info = p.getSubscriptionInfo();
						//check subscription at startup
                        if (info.isSubscribed() == Result.True || info.isFreeTrial() == Result.True)
                        {
                            //code
                        }
                        else
                        {
                            //code
                        }
                    }
                }
            }
        }
        
        public void OnPurchaseDeferred(Product product)
        {

            Debug.Log("Deferred product " + product.definition.id.ToString());
        }    
        
        private void PurchaseSubscription()
        {
            //code
        }
    }
}

