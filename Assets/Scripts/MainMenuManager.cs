using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;
using Algorand.Unity;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private Text publicKey_Txt;

    public static string PublicKey { get; private set; } = "";
    public static string PrivateKey { get; private set; } = "";

    private void Start()
    {
        if (IsFirstTime())
        {
            print("Its First Time, Creating a New Account!");
            CreateAndStoreNewAccount();
        }
        else
        {
            FetchAndPrintAccountPublicKey();
            SetUpUI();
        }
    }
    private void SetUpUI()
    {
        publicKey_Txt = GameObject.Find("PublicKey_Txt").GetComponent<Text>();
        publicKey_Txt.text = PublicKey;
            
    }
    private bool IsFirstTime()
    {
        return !PlayerPrefs.HasKey("FirstTime");
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
        SetNotFirstTime();
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
            PrivateKey = privatekey;
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
}
