using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Collections.Generic;



namespace Invariable
{
    public class ConfigUtils
    {
#if UNITY_EDITOR
        public static string m_localRootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", "");//本地数据根目录
#else
        public static string m_localRootPath = Application.persistentDataPath + "/";
#endif

        private static string[] m_webData = null;
        private const string m_key = "95gbt368426hyb13";
        private const string m_iv = "i8g3451h5cxmj6rf";
        public readonly static string m_configExcelPath = m_localRootPath + "Excel";
        public readonly static string m_configBinPath = m_localRootPath + "Assets/GameAssets/Config";
        public readonly static string m_localResourcePath = m_localRootPath + "Assets/Resources/LocalAssets";
        public readonly static string m_keystorePath = m_localRootPath + "SpectraAbyss.keystore";
        public readonly static string m_hotUpdateDllPath = m_localRootPath + "Assets/GameAssets/DLL";
        public readonly static string m_cdnPath = m_localRootPath + "CDN";
        public readonly static string m_miniBuildPath = m_localRootPath + "Build";
        public readonly static string m_miniWebglPath = m_miniBuildPath + "/webgl";

        /// <summary>
        /// CDN地址
        /// </summary>
        public static string CDNPath
        {
            get
            {
                return GetWebData(0);
            }
        }

        /// <summary>
        /// 服务器的公网地址
        /// </summary>
        public static string WebIpv4Str
        {
            get
            {
                string data = GetWebData(1);

                if (string.IsNullOrEmpty(data))
                {
                    return "";
                }

                string[] list = data.Split(":");
                return list[0];
            }
        }

        /// <summary>
        /// 服务器用于连接客户端的端口号
        /// </summary>
        public static int WebPortInt
        {
            get
            {
                string data = GetWebData(1);

                if (string.IsNullOrEmpty(data))
                {
                    return 0;
                }

                string[] list = data.Split(":");
                return int.Parse(list[1]);
            }
        }

        /// <summary>
        /// 请求下载的认证用户名
        /// </summary>
        public static string Username
        {
            get
            {
                return GetWebData(2);
            }
        }

        /// <summary>
        /// 请求下载的认证密码
        /// </summary>
        public static string Password
        {
            get
            {
                return GetWebData(3);
            }
        }



        public static void GetConfigData(string configName, int id, string name = "", Action<string> callBack = null)
        {
            GetConfigData(configName, id.ToString(), name, callBack);
        }

        public static void GetConfigData(string configName, string index, string name = "", Action<string> callBack = null)
        {
            string key = "Config_" + configName;

            if (key.Contains(".bin"))
            {
                key = key.Replace(".bin", "");
            }

            YooAssetManager.Instance.AsyncLoadAsset<BinAsset>(key, (data) =>
            {
                var config = ReadSafeFile<Dictionary<string, Dictionary<string, string>>>(data.bytes);

                if (config.ContainsKey(index))
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        callBack?.Invoke(config[index].ToString());
                        return;
                    }
                    else if (config[index].ContainsKey(name))
                    {
                        callBack?.Invoke(config[index][name]);
                        return;
                    }
                }

                callBack?.Invoke("");
            });
        }

        public static byte[] ReadFileByteData(string path)
        {
            byte[] byteData = null;

            using (FileStream encryptFileStream = new FileStream(path, FileMode.Open))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    encryptFileStream.CopyTo(memoryStream);
                    byteData = memoryStream.ToArray();
                }
            }

            return byteData;
        }

        public static void CreateFileByBytes(string path, byte[] inputBytes)
        {
            InitDirectory(path);

            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    binaryWriter.Write(inputBytes);
                }
            }
        }

        public static byte[] SerializeData(object data)
        {
            byte[] serializeBytes = null;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                binaryFormatter.Serialize(memoryStream, data);

                serializeBytes = memoryStream.ToArray();
            }

            return serializeBytes;
        }

        public static T Deserialize<T>(byte[] inputBytes)
        {
            T result = default(T);

            using (MemoryStream memoryStream = new MemoryStream(inputBytes))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();

                result = (T)binaryFormatter.Deserialize(memoryStream);
            }

            return result;
        }

        public static byte[] CompressByteData(byte[] inputBytes)
        {
            byte[] compressBytes = null;

            using (MemoryStream compressMemoryStream = new MemoryStream())
            {
                using (GZipStream compressionStream = new GZipStream(compressMemoryStream, CompressionMode.Compress))
                {
                    compressionStream.Write(inputBytes, 0, inputBytes.Length);
                }

                compressBytes = compressMemoryStream.ToArray();
            }

            return compressBytes;
        }

        public static byte[] DecompressByteData(byte[] inputBytes)
        {
            byte[] decompressedBytes = null;

            using (MemoryStream compressedMemoryStream = new MemoryStream(inputBytes))
            {
                using (GZipStream compressionStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress))
                {
                    using (MemoryStream decompressedMemoryStream = new MemoryStream())
                    {
                        compressionStream.CopyTo(decompressedMemoryStream);
                        decompressedBytes = decompressedMemoryStream.ToArray();
                    }
                }
            }

            return decompressedBytes;
        }

        public static byte[] EncryptByteData(byte[] inputBytes, byte[] key, byte[] iv)
        {
            byte[] encryptBytes = null;

            using (AesManaged aes = new AesManaged())
            {
                aes.Key = key;
                aes.IV = iv;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(key, iv))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                            cryptoStream.FlushFinalBlock();//加密会将最后一个数据块填充为满块(需要)，解密会删除填充的数据块(不需要)
                        }

                        encryptBytes = memoryStream.ToArray();
                    }
                }
            }

            return encryptBytes;
        }

        public static byte[] DecryptByteData(byte[] inputBytes, byte[] key, byte[] iv)
        {
            byte[] decryptBytes = null;

            using (MemoryStream inputMemoryStream = new MemoryStream(inputBytes))
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(key, iv))
                    {
                        using (MemoryStream outputMemoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(inputMemoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                cryptoStream.CopyTo(outputMemoryStream);
                            }

                            decryptBytes = outputMemoryStream.ToArray();
                        }
                    }
                }
            }

            return decryptBytes;
        }

        public static void SaveSafeFile(object data, string filePath)
        {
            if (data == null)
            {
                return;
            }

            byte[] inputBytes = SerializeData(data);
            byte[] compressBytes = CompressByteData(inputBytes);
            byte[] encryptBytes = EncryptByteData(compressBytes, Encoding.UTF8.GetBytes(m_key), Encoding.UTF8.GetBytes(m_iv));

            CreateFileByBytes(filePath, encryptBytes);
        }

        public static T ReadSafeFile<T>(string path)
        {
            byte[] inputBytes = ReadFileByteData(path);
            byte[] decryptBytes = DecryptByteData(inputBytes, Encoding.UTF8.GetBytes(m_key), Encoding.UTF8.GetBytes(m_iv));
            byte[] decompressedBytes = DecompressByteData(decryptBytes);

            T result = Deserialize<T>(decompressedBytes);

            return result;
        }

        public static T ReadSafeFile<T>(byte[] inputBytes)
        {
            byte[] decryptBytes = DecryptByteData(inputBytes, Encoding.UTF8.GetBytes(m_key), Encoding.UTF8.GetBytes(m_iv));
            byte[] decompressedBytes = DecompressByteData(decryptBytes);

            T result = Deserialize<T>(decompressedBytes);

            return result;
        }

        public static string FormatFileByteSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "G", "T" };
            int unitIndex = 0;
            double size = bytes;

            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            return $"{size:0.##} {units[unitIndex]}";
        }

        public static void InitDirectory(string path)
        {
            path = path.Replace("\\", "/");

            string extension = Path.GetExtension(path);

            string directoryPath = "";

            if (string.IsNullOrEmpty(extension))
            {
                directoryPath = path;
            }
            else
            {
                directoryPath = Path.GetDirectoryName(path);
            }

            if (!Directory.Exists(directoryPath))
            {
                //确保路径中的所有文件夹都存在
                Directory.CreateDirectory(directoryPath);
            }
        }

        private static string GetWebData(int index)
        {
            if (m_webData == null || index >= m_webData.Length)
            {
                return "";
            }

            string text = m_webData[index].Replace("\r", "");
            return text;
        }

        public static void SetWebData(string[] webData)
        {
            m_webData = webData;
        }
    }
}