using System.Collections;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using System;
using UnityEngine.UIElements;

public class IAP : MonoBehaviour, IDetailedStoreListener{
    public string environment = "production";
    
    IStoreController _storeController; // The Unity Purchasing system.

    //Your products IDs. They should match the ids of your products in your store.
    public string _donationID = "donation";
    public string _noAdsID = "noads";
    private Menu _menu;

    async void Start() {
        _menu = GameObject.FindGameObjectWithTag("MenuHandler").GetComponent<Menu>();
        
        try {
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);
 
            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception) {
            // An error occurred during initialization.
        }
        
        InitializePurchasing();
    }

    void InitializePurchasing() {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        //Add products that will be purchasable and indicate its type.
        builder.AddProduct(_donationID, ProductType.Consumable);
        builder.AddProduct(_noAdsID, ProductType.NonConsumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public void Donate() {
        _storeController.InitiatePurchase(_donationID);
        SoundManager.PlaySound("click"); //play click sound
    }

    public void NoAds() {
        _storeController.InitiatePurchase(_noAdsID);
        SoundManager.PlaySound("click"); //play click sound
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        Debug.Log("In-App Purchasing successfully initialized");
        _storeController = controller;
    }

    public void OnInitializeFailed(InitializationFailureReason error) {
        OnInitializeFailed(error, null);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message) {
        var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

        if (message != null) {
            errorMessage += $" More details: {message}";
        }

        Debug.Log(errorMessage);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) {
        //Retrieve the purchased product
        var product = args.purchasedProduct;

        //Add the purchased product to the players inventory
        if (product.definition.id == _donationID) {
            AddCoins(5000);
            var donations = PlayerPrefs.GetInt("donations", 0);
            PlayerPrefs.SetInt("donations", donations + 1);
            PlayerPrefs.SetInt("noAds", 1);
        }

        if (product.definition.id == _noAdsID) {
            PlayerPrefs.SetInt("noAds", 1);
            _menu.HideNoAdsButton();
        }

        Debug.Log($"Purchase Complete - Product: {product.definition.id}");

        //We return Complete, informing IAP that the processing on our side is done and the transaction can be closed.
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
        Debug.Log($"Purchase failed - Product: '{product.definition.id}'," + $" Purchase failure reason: {failureDescription.reason}," + $" Purchase failure details: {failureDescription.message}");
    }

    void AddCoins(int amount) {
        var currentCoins = PlayerPrefs.GetInt("coins", 0);
        PlayerPrefs.SetInt("coins", currentCoins + amount);
    }
}
