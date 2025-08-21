using System.IO;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GlobalCrypt
{
    /// <summary>
    /// AES暗号
    /// この部分はこういうものとして置いておいてもいいし
    /// 理解しようと思うのであればURLは置いておきます
    /// AES暗号の説明 URL:https://xtech.nikkei.com/atcl/nxt/keyword/18/00002/030800119/
    /// AES暗号も実装 URL:https://yuyu-code.com/programming-languages/c-sharp/aes-encryption-decryption/
    /// PaddingMode   URL:https://learn.microsoft.com/ja-jp/dotnet/api/system.security.cryptography.paddingmode?view=net-7.0
    /// CipherMode    URL:https://it-trend.jp/encryption/article/64-0066
    /// 
    /// </summary>
    public class AES
    {
        #region FIeld
        //初期ベクトル
        //各ゲーム毎(多分1タイトルごと)に変更するほうがいい
        private const string k_AesIV = @"g6GUhUg7yASzRCkS";

        //暗号鍵
        //こちらもゲーム毎に変更するほうがいい
        private const string k_AesKey = @"y8iW6LAG3cLhM6hS";

        #endregion

        #region Public Function
        /// <summary>
        /// 暗号化
        /// </summary>
        /// <param name="data">暗号化するデータ</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;//暗号操作のBlockSizeをbit単位で設定
            aes.KeySize = 128;  //共有鍵のサイズをbit単位で設定
            aes.Padding = PaddingMode.PKCS7;//暗号操作に必要なバイト数を見たないとき場合
            aes.Mode = CipherMode.CBC;//特定のビット数を一度に暗号化する方法
            aes.Key = System.Text.Encoding.UTF8.GetBytes(k_AesKey); //共有鍵の設定
            aes.IV = System.Text.Encoding.UTF8.GetBytes(k_AesIV);　//初期化ベクターの設定


            ICryptoTransform enCrypto = aes.CreateEncryptor();
            //メモリにデータを書き込む
            using (MemoryStream memoryStream = new MemoryStream())
            {
                //暗号化されるデータ(のメモリに)書き込み
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, enCrypto, CryptoStreamMode.Write))
                {
                    //書き込み
                    cryptoStream.Write(data, 0, data.Length);
                    //これを呼ぶことで暗号化済みバイナリが完成する
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }

            //**************************************************
            //                  using()
            //**************************************************
            //ファイルやメモリ操作系(アンマネージド リソース)を
            //使用す際には自身で「リソースの使用権を破棄」を
            //しなければめんどうなことになる
            //そんなときに便利なのがusing->破棄してくれる
        }

        /// <summary>
        /// 複合化
        /// </summary>
        /// <param name="cryptData">複合化するデータ</param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] cryptData)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.BlockSize = 128;//暗号操作のBlockSizeをbit単位で設定
            aes.KeySize = 128;  //共有鍵のサイズをbit単位で設定
            aes.Padding = PaddingMode.PKCS7;//暗号操作に必要なバイト数を見たないとき場合
            aes.Mode = CipherMode.CBC;//特定のビット数を一度に暗号化する方法
            aes.Key = System.Text.Encoding.UTF8.GetBytes(k_AesKey); //共有鍵の設定
            aes.IV = System.Text.Encoding.UTF8.GetBytes(k_AesIV); //初期化ベクターの設定

            ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] planeText = new byte[cryptData.Length];
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(cryptData))
                {
                    using (CryptoStream cryptStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        //暗号を解読
                        cryptStream.Read(planeText, 0, planeText.Length);
                        return planeText;
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }

}

