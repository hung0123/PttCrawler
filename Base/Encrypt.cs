using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;
using System.IO;

namespace PttCrawler.Base
{
    public class Encrypt
    {
        private static string _aeskey = ConfigurationManager.AppSettings["aesKey"];
        private static string _aesiv = ConfigurationManager.AppSettings["aesIv"];
        public static string EncryptSHA512(string target)
        {
            string result = "";
            SHA512 sha512 = new SHA512CryptoServiceProvider();//建立一個SHA512
            byte[] source = Encoding.Default.GetBytes(target);//將字串轉為Byte[]
            byte[] crypto = sha512.ComputeHash(source);//進行SHA512加密
            result = Convert.ToBase64String(crypto);//把加密後的字串從Byte[]轉為字串

            return result;
        }

        public static string EncryptAES(string target)
        {
            string result = "";
            try
            {
                AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(_aeskey));
                byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(_aesiv));
                byte[] dataByteArray = Encoding.UTF8.GetBytes(target);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(keyData, ivData), CryptoStreamMode.Write))
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();
                        result = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            return result;
        }
        public static string DecryptAES(string target)
        {
            string result = "";
            try
            {
                SymmetricAlgorithm aes = new AesCryptoServiceProvider();
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
                byte[] keyData = sha256.ComputeHash(Encoding.UTF8.GetBytes(_aeskey));
                byte[] ivData = md5.ComputeHash(Encoding.UTF8.GetBytes(_aesiv));
                byte[] dataByteArray = Convert.FromBase64String(target);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (
                        CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(keyData, ivData), CryptoStreamMode.Write)
                    )
                    {
                        cs.Write(dataByteArray, 0, dataByteArray.Length);
                        cs.FlushFinalBlock();
                        result = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            //解密成功
            return result;
        }
    }
}