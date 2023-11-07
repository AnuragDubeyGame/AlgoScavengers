using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Algorand.Unity.Samples.YourFirstTransaction
{
    public class YourFirstTransaction : MonoBehaviour
    {
        MainMenuManager mainMenuManager;

        public AlgodClient algod = new("https://testnet-api.algonode.cloud");

        public IndexerClient indexer = new("https://testnet-idx.algonode.cloud");

        public string testnetFaucetUrl = "https://dispenser.testnet.aws.algodev.network/?account={0}";

        public string viewTransactionUrl = "https://testnet.algoexplorer.io/tx/{0}";

        [field: SerializeField]
        public string RecipientText { get; set; }

        [field: SerializeField]
        public string PayAmountText { get; set; }

        [field: SerializeField]
        public InputField RecepientTxtField;
        [field: SerializeField]
        public InputField PaymentAmountField;
        public Text SCTBlanceField;

        [Space]
        public UnityEvent<string> onCheckAlgodStatus;

        public UnityEvent<string> onCheckIndexerStatus;

        public UnityEvent<string> onAccountGenerated;

        public UnityEvent<string> onBalanceTextUpdated;

        public UnityEvent onEnoughBalanceForPayment;

        public UnityEvent<string> onTxnStatusUpdate;

        public UnityEvent onTransactionConfirmedSuccessfully;

        private string txnStatus;

        public string AlgodHealth { get; set; }

        public string IndexerHealth { get; set; }

        public MicroAlgos Balance { get; set; }

        public ulong SCTBalance { get; set; }
        public Account Account { get; set; }

        private string OwnerPK = "Q+iArXlfKuQ+SPjXgprZBTYZfnKQqmWHH8nsctCOiOWpgx0hDtpwHOKNOfFf9NYPD27+OCIx+RdjBNjEkeD05g==";

        AssetIndex SCT_INDEX = new AssetIndex(468833490);

        public string TxnStatus
        {
            get => txnStatus;
            set
            {
                txnStatus = value;
                onTxnStatusUpdate?.Invoke(value);
            }
        }

        public string ConfirmedTxnId { get; set; }

        private void Start()
        {
            mainMenuManager = FindObjectOfType<MainMenuManager>();
        }
        public void CheckAlgodStatus()
        {
            CheckAlgodStatusAsync().Forget();
        }

        public void CheckIndexerStatus()
        {
            CheckIndexerStatusAsync().Forget();
        }

        private async UniTaskVoid CheckAlgodStatusAsync()
        {
            var response = await algod.HealthCheck();
            AlgodHealth = response.Error ? response.Error : "Connected";
            onCheckAlgodStatus?.Invoke(AlgodHealth);
        }

        private async UniTaskVoid CheckIndexerStatusAsync()
        {
            var response = await indexer.MakeHealthCheck();
            IndexerHealth = response.Error ? response.Error : "Connected";
            onCheckIndexerStatus?.Invoke(IndexerHealth);
        }

        public void GenerateAccount()
        {
            onAccountGenerated?.Invoke(Account.Address.ToString());
            onBalanceTextUpdated?.Invoke(Balance.ToAlgos().ToString("F6"));
        }

        public void OpenFaucetUrl()
        {
            Application.OpenURL(string.Format(testnetFaucetUrl, Account.Address.ToString()));
        }

        public void CheckBalance()
        {
            CheckBalanceAsync().Forget();
        }

        private async UniTaskVoid CheckBalanceAsync()
        {
            var (err, resp) = await indexer.LookupAccountByID(Account.Address);
            
            if (err)
            {
                Balance = 0;
                SCTBalance = 0;
                if (!err.Message.Contains("no accounts found for address")) Debug.LogError(err);
            }
            else
            {
                try
                {

                    Balance = resp.Account.Amount;
                    foreach (var asa in resp.Account.Assets)
                    {
                        if(asa.AssetId == SCT_INDEX)
                        {
                            print("SCT Token Found : "+asa.AssetId);
                            print("SCT Token Balance : "+asa.Amount);
                            SCTBalance = asa.Amount;
                        }
                        else
                        {
                            Debug.Log("Didnt Found SCT Token!");
                        }
                    }
                }catch(Exception e)
                {
                    print(e.ToString());
                }
                mainMenuManager.AlgoBal_Txt.text = $"Algos : {Balance.ToAlgos()}";
                mainMenuManager.SCTBal_Txt.text = $"SCT : {SCTBalance / 10}";
                SCTBlanceField.text = (SCTBalance / 10).ToString();

                if(Balance.ToAlgos() > 0)
                {
                    mainMenuManager.Play_Btn.SetActive(true);
                    mainMenuManager.Refresh_Btn.SetActive(false);
                    mainMenuManager.DepositFunds_Btn.SetActive(false);
                }
                else
                {
                    mainMenuManager.Play_Btn.SetActive(false);
                    mainMenuManager.Refresh_Btn.SetActive(true);
                    mainMenuManager.DepositFunds_Btn.SetActive(true);
                }
            }

            onBalanceTextUpdated?.Invoke(Balance.ToAlgos().ToString("F6"));
            if (Balance >= 1_000) onEnoughBalanceForPayment?.Invoke();

            RecepientTxtField.text = RecipientText;
            PaymentAmountField.text = PayAmountText;
        }

        public void MakePayment()
        {
            MakePaymentAsync().Forget();
        }

        public async UniTaskVoid MakePaymentAsync()
        {
            var addressParseError = Address.TryParse(RecipientText, out var recipientAddress);
            if (addressParseError > AddressFormatError.None)
            {
                TxnStatus = $"Recipient address formatted incorrectly: {addressParseError}";
                return;
            }

            if (!double.TryParse(PayAmountText, out var payAmountAlgos))
            {
                TxnStatus = $"Invalid format: Pay amount must be a double, instead it was \"{PayAmountText}\"";
                return;
            }

            // Get the suggested transaction params
            var (txnParamsError, txnParams) = await algod.TransactionParams();
            if (txnParamsError)
            {
                Debug.LogError(txnParamsError);
                TxnStatus = $"error: {txnParamsError}";
                return;
            }

            //OPTIN FOR SCT
            var optinTxn = Transaction.AssetTransfer(Account.Address, txnParams, SCT_INDEX, 0, Account.Address);
            var signedoptinTxn = Account.SignTxn(optinTxn);
            var (OptinsendTxnError, Optintxid) = await algod.SendTransaction(signedoptinTxn);
            if (OptinsendTxnError)
            {
                Debug.LogError(OptinsendTxnError);
                TxnStatus = $"error: {OptinsendTxnError}";
                return;
            }
            else
            {
                print("Successfully Opted For SCT Token");
            }

            //Send SCT
            Account OwnerAcc = new Account(PrivateKey.FromString(OwnerPK));
            var assetTransferTransaction = Transaction.AssetTransfer(OwnerAcc.Address, txnParams, SCT_INDEX, ulong.Parse(PayAmountText) * 10, Account.Address);
            var signedAssetTxn = OwnerAcc.SignTxn(assetTransferTransaction);
            var (sendTxnError, txid) = await algod.SendTransaction(signedAssetTxn);
            if (sendTxnError)
            {
                Debug.LogError(sendTxnError);
                TxnStatus = $"error: {sendTxnError}";
                return;
            }

            // Wait for the transaction to be confirmed
            var (confirmErr, confirmed) = await algod.WaitForConfirmation(txid.TxId);
            if (confirmErr)
            {
                Debug.LogError(confirmErr);
                TxnStatus = $"error: {confirmErr}";
                return;
            }

            TxnStatus = "Transaction confirmed!";
            ConfirmedTxnId = txid.TxId;
            onTransactionConfirmedSuccessfully?.Invoke();
        }

        public void ViewConfirmedTransaction()
        {
            CheckBalance();
            Application.OpenURL(string.Format(viewTransactionUrl, ConfirmedTxnId));
        }
    }
}
