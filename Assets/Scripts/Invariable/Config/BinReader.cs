using System;
using System.Text;
using UnityEngine;



namespace Invariable
{
    /// <summary>
    /// 托管二进制读取器，m_position 为相对当前缓冲起始的偏移
    /// </summary>
    public struct BinReader
    {
        private readonly byte[] Buffer;

        public int m_position;

        public int Length
        {
            get
            {
                return Buffer.Length;
            }
        }



        public BinReader(byte[] buffer, int position = 0)
        {
            if (buffer == null)
            {
                GameLog.Error("BinReader buffer is null");
                Buffer = Array.Empty<byte>();
            }
            else
            {
                Buffer = buffer;
            }

            m_position = position;
        }



        /// <summary>
        /// 将读取位置移动到指定偏移
        /// </summary>
        public void Seek(int position)
        {
            m_position = position;
        }

        /// <summary>
        /// 读取一个 Int32
        /// </summary>
        public int ReadInt32()
        {
            int value = BitConverter.ToInt32(Buffer, m_position);
            m_position += 4;

            return value;
        }

        /// <summary>
        /// 读取一个 Single
        /// </summary>
        public float ReadSingle()
        {
            float value = BitConverter.ToSingle(Buffer, m_position);
            m_position += 4;

            return value;
        }

        /// <summary>
        /// 从当前位置读取 UTF-16 字符串记录（int32 长度 + chars + pad4）
        /// </summary>
        public string ReadStringRecord()
        {
            int charCount = ReadInt32();

            if (charCount < 0)
            {
                GameLog.Error("Invalid string length");

                return "";
            }

            if (charCount == 0)
            {
                Align4();

                return "";
            }

            string result = Encoding.Unicode.GetString(Buffer, m_position, charCount * 2);
            m_position += charCount * 2;
            Align4();

            return result;
        }

        /// <summary>
        /// 将读取位置按 4 字节对齐
        /// </summary>
        public void Align4()
        {
            int rem = m_position & 3;

            if (rem != 0)
            {
                m_position += 4 - rem;
            }
        }
    }
}