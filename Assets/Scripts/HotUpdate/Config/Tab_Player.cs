using System;
using System.Collections.Generic;



namespace HotUpdate
{
    public static class Tab_Player
    {
        /// <summary>
        /// 配置表行数据
        /// </summary>
        [System.Serializable]
        public class Row
        {
            public string Index; // 索引
            public int Lv; // 转生等级
            public int Stage; // 阶段
            public int MaxStage; // 最大阶段
            public int StageDiff; // 转生任务阶段 1:对话任务 2:转生点收集任务 3:BOSS任务 4:星盘小游戏
            public string Stageicon; // 阶段icon
            public int StartLv; // 转生开启等级
        }

        private static List<Row> s_configs; // 所有配置行

        private static Dictionary<string, Row> s_configDict; // 索引到行的快速查找

        /// <summary>
        /// 预加载配置表（数据已嵌入 .cs，调用即完成初始化）
        /// </summary>
        /// <param name="onComplete">加载完成回调</param>
        public static void Init(Action onComplete = null)
        {
            if (s_configs != null)
            {
                onComplete?.Invoke();
                return;
            }

            BuildConfigs();
            onComplete?.Invoke();
        }

        /// <summary>
        /// 根据整数索引获取一行配置
        /// </summary>
        /// <param name="index">行索引</param>
        /// <returns>配置行数据，不存在返回 null</returns>
        public static Row GetConfigByIndex(int index)
        {
            return GetConfigByIndex(index.ToString());
        }

        /// <summary>
        /// 根据字符串索引获取一行配置
        /// </summary>
        /// <param name="index">行索引</param>
        /// <returns>配置行数据，不存在返回 null</returns>
        public static Row GetConfigByIndex(string index)
        {
            if (s_configDict == null)
            {
                BuildConfigs();
            }

            if (s_configDict != null && s_configDict.TryGetValue(index, out Row row))
            {
                return row;
            }

            return null;
        }

        /// <summary>
        /// 获取所有配置行
        /// </summary>
        /// <returns>配置行列表</returns>
        public static List<Row> GetAllConfigs()
        {
            if (s_configs == null)
            {
                BuildConfigs();
            }

            return s_configs ?? new List<Row>();
        }

        /// <summary>
        /// 配置表总行数
        /// </summary>
        public static int Count
        {
            get
            {
                if (s_configs == null)
                {
                    BuildConfigs();
                }

                return s_configs != null ? s_configs.Count : 0;
            }
        }

        /// <summary>
        /// 手动构建全部配置行（数据直接嵌入在 .cs 中）
        /// </summary>
        private static void BuildConfigs()
        {
            s_configs = new List<Row>();
            s_configDict = new Dictionary<string, Row>();

            AddRow(new Row { Index = "1_1", Lv = 1, Stage = 1, MaxStage = 7, StageDiff = 1, Stageicon = "0", StartLv = 421 });
            AddRow(new Row { Index = "1_2", Lv = 1, Stage = 2, MaxStage = 7, StageDiff = 1, Stageicon = "0", StartLv = 421 });
            AddRow(new Row { Index = "1_3", Lv = 1, Stage = 3, MaxStage = 7, StageDiff = 1, Stageicon = "0", StartLv = 421 });
            AddRow(new Row { Index = "1_4", Lv = 1, Stage = 4, MaxStage = 7, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 421 });
            AddRow(new Row { Index = "1_5", Lv = 1, Stage = 5, MaxStage = 7, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 461 });
            AddRow(new Row { Index = "1_6", Lv = 1, Stage = 6, MaxStage = 7, StageDiff = 3, Stageicon = "0", StartLv = 500 });
            AddRow(new Row { Index = "1_7", Lv = 1, Stage = 7, MaxStage = 7, StageDiff = 4, Stageicon = "0", StartLv = 500 });
            AddRow(new Row { Index = "2_1", Lv = 2, Stage = 1, MaxStage = 6, StageDiff = 1, Stageicon = "0", StartLv = 941 });
            AddRow(new Row { Index = "2_2", Lv = 2, Stage = 2, MaxStage = 6, StageDiff = 1, Stageicon = "0", StartLv = 941 });
            AddRow(new Row { Index = "2_3", Lv = 2, Stage = 3, MaxStage = 6, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 941 });
            AddRow(new Row { Index = "2_4", Lv = 2, Stage = 4, MaxStage = 6, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 971 });
            AddRow(new Row { Index = "2_5", Lv = 2, Stage = 5, MaxStage = 6, StageDiff = 3, Stageicon = "0", StartLv = 1000 });
            AddRow(new Row { Index = "2_6", Lv = 2, Stage = 6, MaxStage = 6, StageDiff = 4, Stageicon = "0", StartLv = 1000 });
            AddRow(new Row { Index = "3_1", Lv = 3, Stage = 1, MaxStage = 6, StageDiff = 1, Stageicon = "0", StartLv = 1431 });
            AddRow(new Row { Index = "3_2", Lv = 3, Stage = 2, MaxStage = 6, StageDiff = 1, Stageicon = "0", StartLv = 1431 });
            AddRow(new Row { Index = "3_3", Lv = 3, Stage = 3, MaxStage = 6, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 1431 });
            AddRow(new Row { Index = "3_4", Lv = 3, Stage = 4, MaxStage = 6, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 1481 });
            AddRow(new Row { Index = "3_5", Lv = 3, Stage = 5, MaxStage = 6, StageDiff = 3, Stageicon = "0", StartLv = 1500 });
            AddRow(new Row { Index = "3_6", Lv = 3, Stage = 6, MaxStage = 6, StageDiff = 4, Stageicon = "0", StartLv = 1500 });
            AddRow(new Row { Index = "4_1", Lv = 4, Stage = 1, MaxStage = 6, StageDiff = 1, Stageicon = "0", StartLv = 1921 });
            AddRow(new Row { Index = "4_2", Lv = 4, Stage = 2, MaxStage = 6, StageDiff = 1, Stageicon = "0", StartLv = 1921 });
            AddRow(new Row { Index = "4_3", Lv = 4, Stage = 3, MaxStage = 6, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 1921 });
            AddRow(new Row { Index = "4_4", Lv = 4, Stage = 4, MaxStage = 6, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 1971 });
            AddRow(new Row { Index = "4_5", Lv = 4, Stage = 5, MaxStage = 6, StageDiff = 3, Stageicon = "0", StartLv = 2000 });
            AddRow(new Row { Index = "4_6", Lv = 4, Stage = 6, MaxStage = 6, StageDiff = 4, Stageicon = "0", StartLv = 2000 });
            AddRow(new Row { Index = "5_1", Lv = 5, Stage = 1, MaxStage = 4, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 1500 });
            AddRow(new Row { Index = "5_2", Lv = 5, Stage = 2, MaxStage = 4, StageDiff = 2, Stageicon = "hbs1/hbs1", StartLv = 1500 });
            AddRow(new Row { Index = "5_3", Lv = 5, Stage = 3, MaxStage = 4, StageDiff = 3, Stageicon = "0", StartLv = 1500 });
        }

        /// <summary>
        /// 添加一行配置到容器中
        /// </summary>
        private static void AddRow(Row row)
        {
            string key = row.Index.ToString();
            s_configs.Add(row);
            s_configDict[key] = row;
        }
    }
}
