using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;
using Algorand.Unity;
using UnityEngine.UI;
using Algorand.Unity.Samples.YourFirstTransaction;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MainMenuManager : MonoBehaviour
{
    public static string PublicKey { get; private set; } = "";
    public static string _PrivateKey { get; private set; } = "";
    public Text PubKey_Txt;
    public Text AlgoBal_Txt;
    public Text SCTBal_Txt;
    public GameObject DepositFunds_Btn, Play_Btn, Refresh_Btn, canvas;
    public GameObject GameBG;
    private YourFirstTransaction transactionManager;
    public float TokenToMint;

    private void Start()
    {
        transactionManager = FindAnyObjectByType<YourFirstTransaction>();
        if (IsFirstTime())
        {
            print("Its First Time, Creating a New Account!");
            CreateAndStoreNewAccount();
            DepositFunds_Btn.SetActive(true);
            Play_Btn.SetActive(false);
        }
        else
        {
            FetchAndPrintAccountPublicKey();
        }
        SetUpUI();
    }
    private void SetUpUI()
    {
        transactionManager = FindAnyObjectByType<YourFirstTransaction>();
        transactionManager.CheckAlgodStatus();
        transactionManager.CheckIndexerStatus();

        var MyAcc = new Account(PrivateKey.FromString(DecryptPrivateKey(_PrivateKey, "mysecretsalt")));
        transactionManager.Account = MyAcc;
        transactionManager.GenerateAccount();
        transactionManager.CheckBalance();
        var formattedKey = PublicKey.Substring(0, 7) + "......" + PublicKey.Substring(PublicKey.Length - 7, 7);
        PubKey_Txt.text = formattedKey;
        transactionManager.RecipientText = PublicKey;
        transactionManager.PayAmountText = (TokenToMint).ToString();
    }
    private bool IsFirstTime()
    {
        return !PlayerPrefs.HasKey("FirstTime");
    }
    public void setPaymentAmount()
    {
        transactionManager.PayAmountText = (TokenToMint).ToString();
        transactionManager.PaymentAmountField.text = TokenToMint.ToString();
    }
    private void SetNotFirstTime()
    {
        PlayerPrefs.SetInt("FirstTime", 1);
        PlayerPrefs.Save();
    }
    private void CreateAndStoreNewAccount()
    {
        var (privateKey, address) = Account.GenerateAccount();
        Debug.Log($"My address: {address}");

        PlayerPrefs.SetString("MyAddress", address);
        PlayerPrefs.SetString("MyPrivateKey", EncryptPrivateKey(privateKey.ToString(), "mysecretsalt"));

        PublicKey = address;
        _PrivateKey = EncryptPrivateKey(privateKey.ToString(), "mysecretsalt");
        transactionManager.Account = new Account(PrivateKey.FromString(DecryptPrivateKey(_PrivateKey, "mysecretsalt")));
        SetNotFirstTime();
    }
    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
    private void FetchAndPrintAccountPublicKey()
    {
        if (PlayerPrefs.HasKey("MyAddress"))
        {
            string address = PlayerPrefs.GetString("MyAddress");
            string privatekey = PlayerPrefs.GetString("MyPrivateKey");
            Debug.Log("Account Public Key: " + address);
            Debug.Log("Account Private Key Encrypted: " + privatekey);
            PublicKey = address;
            _PrivateKey = privatekey;
            //Debug.Log("Account Memenomic Key Decrypted: " + PrivateKey.FromString(DecryptPrivateKey(privatekey, "mysecretsalt")).ToString());
        }
        else
        {
            Debug.LogWarning("No accounts available in PlayerPrefs.");
        }
    }

    public static string EncryptPrivateKey(string privateKey, string salt)
    {
        if (string.IsNullOrEmpty(privateKey) || string.IsNullOrEmpty(salt) || salt.Length < 8)
        {
            throw new ArgumentException("Private key and salt must not be empty, and salt must be at least eight characters long.");
        }

        using (Aes aesAlg = Aes.Create())
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(saltBytes, saltBytes, 1000);
            aesAlg.Key = keyDerivation.GetBytes(16); // AES-256
            aesAlg.IV = keyDerivation.GetBytes(16);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(privateKey);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public static string DecryptPrivateKey(string encryptedPrivateKey, string salt)
    {
        if (string.IsNullOrEmpty(encryptedPrivateKey) || string.IsNullOrEmpty(salt) || salt.Length < 8)
        {
            throw new ArgumentException("Encrypted private key and salt must not be empty, and salt must be at least eight characters long.");
        }

        using (Aes aesAlg = Aes.Create())
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(saltBytes, saltBytes, 1000);
            aesAlg.Key = keyDerivation.GetBytes(16); // AES-256
            aesAlg.IV = keyDerivation.GetBytes(16);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedPrivateKey)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

/*    [MenuItem("Developer/Delete All Data")]
    private static void DeleteAllData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("All player data has been deleted.");
    }*/
}
