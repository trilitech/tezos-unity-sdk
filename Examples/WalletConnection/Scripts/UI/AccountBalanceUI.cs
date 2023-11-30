using System.Collections;
using System.Collections.Generic;
using TezosSDK.Beacon;
using TezosSDK.Helpers;
using TezosSDK.Tezos;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TezosSDK
{
    public class AccountBalanceUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI balanceText;
        private readonly string _notConnectedText = "Not connected";

        private void Start()
        {
            // Subscribe to wallet events
            TezosManager.Instance.MessageReceiver.AccountConnected += OnAccountConnected;
            TezosManager.Instance.MessageReceiver.AccountDisconnected += OnAccountDisconnected;
        }

        private void OnAccountDisconnected(AccountInfo _)
        {
            balanceText.text = _notConnectedText;
        }

        private void OnAccountConnected(AccountInfo _)
        {
            // OnBalanceFetched will be called when the balance is fetched
            var routine = TezosManager.Instance.Tezos.GetCurrentWalletBalance(OnBalanceFetched);
            StartCoroutine(routine);
        }

        private void OnBalanceFetched(ulong balance)
        {
            // Balance is in microtez, so we divide it by 1.000.000 to get tez
            balanceText.text = $"{balance / 1000000f}";
        }
    }
}
