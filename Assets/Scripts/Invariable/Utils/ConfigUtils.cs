using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;



namespace Invariable
{
    public class ConfigUtils
    {
#if UNITY_EDITOR
        public static string m_localRootPath = Application.streamingAssetsPath.Replace("Assets/StreamingAssets", ""); // 本地数据根目录
#else
        public static string m_localRootPath = Application.persistentDataPath + "/";
#endif

        private static string[] m_webData = null;
        private const string Key = "95gbt368426hyb13";
        private const string Iv = "i8g3451h5cxmj6rf";
        private static readonly byte[] KeyBytes = Encoding.UTF8.GetBytes(Key);
        private static readonly byte[] IvBytes = Encoding.UTF8.GetBytes(Iv);
        private static readonly byte[] SafeFileV2Magic = { (byte)'S', (byte)'F', (byte)'V', (byte)'2' };

        public static readonly string ConfigExcelPath = m_localRootPath + "Excel";
        public static readonly string LocalResourcePath = m_localRootPath + "Assets/Resources/LocalAssets";
        public static readonly string HotUpdateDllPath = m_localRootPath + "Assets/GameAssets/DLL";
        public static readonly string CdnPath = m_localRootPath + "CDN";
        public static readonly string MiniBuildPath = m_localRootPath + "Build";

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



        /// <summary>
        /// 读取文件全部字节
        /// </summary>
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

        /// <summary>
        /// 按字节数组创建文件
        /// </summary>
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

        /// <summary>
        /// 将对象序列化为字节数组
        /// </summary>
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

        /// <summary>
        /// 将字节数组反序列化为对象
        /// </summary>
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

        /// <summary>
        /// GZip 压缩字节数组
        /// </summary>
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

        /// <summary>
        /// GZip 解压字节数组
        /// </summary>
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

        /// <summary>
        /// AES 加密字节数组
        /// </summary>
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
                            cryptoStream.FlushFinalBlock(); // 加密会将最后一个数据块填充为满块(需要)，解密会删除填充的数据块(不需要)
                        }

                        encryptBytes = memoryStream.ToArray();
                    }
                }
            }

            return encryptBytes;
        }

        /// <summary>
        /// AES 解密字节数组
        /// </summary>
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

        /// <summary>
        /// 序列化、压缩、加密后保存安全文件（byte[] 走 v2 直存，其他类型走旧 BinaryFormatter）
        /// </summary>
        public static void SaveSafeFile(object data, string filePath)
        {
            if (data == null)
            {
                return;
            }

            byte[] payload;

            if (data is byte[] rawBytes)
            {
                payload = new byte[SafeFileV2Magic.Length + rawBytes.Length];
                Buffer.BlockCopy(SafeFileV2Magic, 0, payload, 0, SafeFileV2Magic.Length);
                Buffer.BlockCopy(rawBytes, 0, payload, SafeFileV2Magic.Length, rawBytes.Length);
            }
            else
            {
                payload = SerializeData(data);
            }

            byte[] compressBytes = CompressByteData(payload);
            byte[] encryptBytes = EncryptByteData(compressBytes, KeyBytes, IvBytes);

            CreateFileByBytes(filePath, encryptBytes);
        }

        /// <summary>
        /// 从路径读取并解密安全文件
        /// </summary>
        public static T ReadSafeFile<T>(string path)
        {
            byte[] inputBytes = ReadFileByteData(path);

            return ReadSafeFile<T>(inputBytes);
        }

        /// <summary>
        /// 从字节数组解密并反序列化安全文件（自动探测 v2 / 旧格式）
        /// </summary>
        public static T ReadSafeFile<T>(byte[] inputBytes)
        {
            byte[] decryptBytes = DecryptByteData(inputBytes, KeyBytes, IvBytes);
            byte[] decompressedBytes = DecompressByteData(decryptBytes);

            if (typeof(T) == typeof(byte[]) && IsSafeFileV2(decompressedBytes))
            {
                int magicLength = SafeFileV2Magic.Length;
                byte[] resultBytes = new byte[decompressedBytes.Length - magicLength];
                Buffer.BlockCopy(decompressedBytes, magicLength, resultBytes, 0, resultBytes.Length);

                return (T)(object)resultBytes;
            }

            return Deserialize<T>(decompressedBytes);
        }

        /// <summary>
        /// 判断解压后载荷是否为安全文件 v2
        /// </summary>
        private static bool IsSafeFileV2(byte[] data)
        {
            if (data == null || data.Length < SafeFileV2Magic.Length)
            {
                return false;
            }

            for (int i = 0; i < SafeFileV2Magic.Length; i++)
            {
                if (data[i] != SafeFileV2Magic[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 将字节大小格式化为可读字符串
        /// </summary>
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

        /// <summary>
        /// 确保路径对应目录存在
        /// </summary>
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
                // 确保路径中的所有文件夹都存在
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 设置 Web 配置数据
        /// </summary>
        public static void SetWebData(string[] webData)
        {
            m_webData = webData;
        }



        /// <summary>
        /// 按索引读取 Web 配置项
        /// </summary>
        private static string GetWebData(int index)
        {
            if (m_webData == null || index >= m_webData.Length)
            {
                return "";
            }

            string text = m_webData[index].Replace("\r", "");

            return text;
        }
    }
}