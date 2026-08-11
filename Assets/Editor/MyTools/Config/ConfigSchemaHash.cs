namespace MyTools
{
    /// <summary>
    /// 与导表产物 Config_X.SchemaHash 共用同一算法（FNV-1a）
    /// </summary>
    public static class ConfigSchemaHash
    {
        /// <summary>
        /// 计算配置表 Schema 哈希
        /// </summary>
        public static int Compute(AnalyzedTable table)
        {
            unchecked
            {
                uint hash = 2166136261u;

                for (int i = 0; i < table.Fields.Count; i++)
                {
                    ConfigField field = table.Fields[i];
                    hash = MixString(hash, field.Name);
                    hash = MixString(hash, field.CsType);
                    hash = MixInt(hash, field.WireSize);
                }

                return (int)hash;
            }
        }



        /// <summary>
        /// 将字符串混入哈希
        /// </summary>
        private static uint MixString(uint hash, string value)
        {
            value = value ?? "";

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619u;
            }

            hash ^= (uint)value.Length;
            hash *= 16777619u;

            return hash;
        }

        /// <summary>
        /// 将整型混入哈希
        /// </summary>
        private static uint MixInt(uint hash, int value)
        {
            hash ^= (uint)value;
            hash *= 16777619u;

            return hash;
        }
    }
}