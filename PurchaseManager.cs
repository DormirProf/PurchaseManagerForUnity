using System;
using System.Collections.Generic;
using Scripts.Secure;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Scripts.Payes
{
public class PurchaseManager : MonoBehaviour, IStoreListener 
    {
        private static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;
        private IGooglePlayStoreExtensions m_GooglePlayStoreExtensions;
        private int currentProductIndex;

        [Tooltip("Не многоразовые товары. Больше подходит для отключения рекламы и т.п.")]
        public string[] NC_PRODUCTS;    
        [Tooltip("Многоразовые товары. Больше подходит для покупки игровой валюты и т.п.")]
        public string[] C_PRODUCTS;
        [Tooltip("Подписки")]
        public string[] SUBSCRIPTIONS;

        /// <summary>
        /// Событие, которое запускается при удачной покупке многоразового товара.
        /// An event that is triggered when a successful purchase of a consumable product.
        /// </summary>
        public static event OnSuccessConsumable OnPurchaseConsumable;
        /// <summary>
        /// Событие, которое запускается при удачной покупке не многоразового товара.
        /// An event that is triggered when a successful purchase of a non-consumable product.
        /// </summary>
        public static event OnSuccessNonConsumable OnPurchaseNonConsumable;
        /// <summary>
        /// Событие, которое запускается при удачной покупке подписки.
        /// An event that is triggered when a successful purchase of a subscription.
        /// </summary>
        public static event OnSuccessSubscription OnPurchaseSubscription;
        /// <summary>
        /// Событие, которое запускается при неудачной покупке какого-либо товара.
        /// An event that is triggered when an unsuccessful purchase of any product.
        /// </summary>
        public static event OnFailedPurchase PurchaseFailed;

        private void Start()
        {
            InitializePurchasing();
        }
        
        /// <summary>
        /// Проверить, куплен ли товар.
        /// Check if the item has been purchased.
        /// </summary>
        /// <param //name="id">Индекс товара в списке.</param>
        /// <param //name="id">The index of the item in the list.</param>
        /// <returns></returns>
        /// 
        public static bool CheckBuyState(string id)
        {
            Product product = m_StoreController.products.WithID(id);
            if (product.hasReceipt) { return true; }
            else { return false; }
        }

        public void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            foreach (string s in C_PRODUCTS) builder.AddProduct(s, ProductType.Consumable);
            foreach (string s in NC_PRODUCTS) builder.AddProduct(s, ProductType.NonConsumable);
            foreach (string s in SUBSCRIPTIONS) builder.AddProduct(s, ProductType.Subscription);
            UnityPurchasing.Initialize(this, builder);
        }

        private bool IsInitialized()
        {
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public void BuyConsumable(int index)
        {
            currentProductIndex = index;
            BuyProductID(C_PRODUCTS[index]);
        }

        public void BuyNonConsumable(int index)
        {
            currentProductIndex = index;
            BuyProductID(NC_PRODUCTS[index]);
        }

        public void BuySubscription(int index)
        {
            currentProductIndex = index;
            BuyProductID(SUBSCRIPTIONS[index]);
        }
        
        void BuyProductID(string productId)
        {
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    print(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    m_StoreController.InitiatePurchase(product);
                }
                else
                {
                    print("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    OnPurchaseFailed(product, PurchaseFailureReason.ProductUnavailable);
                }
            }
        }

        //Проверка покупки подписки.
        //Subscription purchase verification.
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_GooglePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
            Dictionary<string, string> Dict = m_GooglePlayStoreExtensions.GetProductJSONDictionary();
            foreach (Product item in controller.products.all)
            {
                if (item.receipt != null)
                { 
                    if (item.definition.type == ProductType.Subscription)
                    {
                        string json = (Dict == null || !Dict.ContainsKey(item.definition.storeSpecificId))
                            ? null
                            : Dict[item.definition.storeSpecificId];
                        SubscriptionManager s = new SubscriptionManager(item, json);
                        SubscriptionInfo info = s.getSubscriptionInfo();
                        if (info.getProductId() == "pay_noads")
                        {
                            if (info.isSubscribed() == Result.True)
                            {
                                PlayerPrefsSafe.SetInt("ADS", 1);
                            }else
                            {
                                PlayerPrefsSafe.SetInt("ADS", 0);
                            }
                        }
                    }
                }
            }
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            print("OnInitializeFailed InitializationFailureReason:" + error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (C_PRODUCTS.Length > 0 && String.Equals(args.purchasedProduct.definition.id, C_PRODUCTS[currentProductIndex], StringComparison.Ordinal))
                OnSuccessC(args);
            else if (NC_PRODUCTS.Length > 0 && String.Equals(args.purchasedProduct.definition.id, NC_PRODUCTS[currentProductIndex], StringComparison.Ordinal))
                OnSuccessNC(args);
            else if (SUBSCRIPTIONS.Length > 0 && String.Equals(args.purchasedProduct.definition.id, SUBSCRIPTIONS[currentProductIndex], StringComparison.Ordinal))
                OnSuccessSub(args);
            else print(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            return PurchaseProcessingResult.Complete;
        }

        public delegate void OnSuccessConsumable(PurchaseEventArgs args);
        protected virtual void OnSuccessC(PurchaseEventArgs args)
        {
            if (OnPurchaseConsumable != null) OnPurchaseConsumable(args);
            print(C_PRODUCTS[currentProductIndex] + " Buyed!");
        }
        
        public delegate void OnSuccessNonConsumable(PurchaseEventArgs args);
        protected virtual void OnSuccessNC(PurchaseEventArgs args)
        {
            if (OnPurchaseNonConsumable != null) OnPurchaseNonConsumable(args);
            print(NC_PRODUCTS[currentProductIndex] + " Buyed!");
        }
        
        public delegate void OnSuccessSubscription(PurchaseEventArgs args);
        protected virtual void OnSuccessSub(PurchaseEventArgs args)
        {
            if (OnPurchaseSubscription != null) OnPurchaseSubscription(args);
            print(SUBSCRIPTIONS[currentProductIndex] + " Buyed!");
        }
        
        public delegate void OnFailedPurchase(Product product, PurchaseFailureReason failureReason);
        protected virtual void OnFailedP(Product product, PurchaseFailureReason failureReason)
        {
            if (PurchaseFailed != null) PurchaseFailed(product, failureReason);
            print(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnFailedP(product, failureReason);
        }
    }
}