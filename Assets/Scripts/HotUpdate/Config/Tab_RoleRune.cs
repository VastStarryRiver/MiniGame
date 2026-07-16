using System;
using System.Collections.Generic;



namespace HotUpdate
{
    public static class Tab_RoleRune
    {
        /// <summary>
        /// 配置表行数据
        /// </summary>
        [System.Serializable]
        public class Row
        {
            public int Index; // 索引
            public float Param; // 符文页参数
            public string NameText; // 名字
            public string NamePic; // 标签图-未选中
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

            AddRow(new Row { Index = 1, Param = 0.333333343f, NameText = "DH_RunesPageNameText1", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn1" });
            AddRow(new Row { Index = 2, Param = 1.33333337f, NameText = "DH_RunesPageNameText2", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn2" });
            AddRow(new Row { Index = 3, Param = 2.33333325f, NameText = "DH_RunesPageNameText3", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn3" });
            AddRow(new Row { Index = 4, Param = 3.33333325f, NameText = "DH_RunesPageNameText4", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn4" });
            AddRow(new Row { Index = 5, Param = 3.83333325f, NameText = "DH_RunesPageNameText5", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn5" });
            AddRow(new Row { Index = 6, Param = 4.33333349f, NameText = "DH_RunesPageNameText6", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn6" });
            AddRow(new Row { Index = 7, Param = 4.83333349f, NameText = "DH_RunesPageNameText7", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn7" });
            AddRow(new Row { Index = 8, Param = 5.33333349f, NameText = "DH_RunesPageNameText8", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn8" });
            AddRow(new Row { Index = 9, Param = 5.83333349f, NameText = "DH_RunesPageNameText9", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn9" });
            AddRow(new Row { Index = 10, Param = 6.33333349f, NameText = "DH_RunesPageNameText10", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn10" });
            AddRow(new Row { Index = 11, Param = 6.83333349f, NameText = "DH_RunesPageNameText11", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn11" });
            AddRow(new Row { Index = 12, Param = 7.33333349f, NameText = "DH_RunesPageNameText12", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn12" });
            AddRow(new Row { Index = 13, Param = 7.83333349f, NameText = "DH_RunesPageNameText13", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn13" });
            AddRow(new Row { Index = 14, Param = 8.33333302f, NameText = "DH_RunesPageNameText14", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn14" });
            AddRow(new Row { Index = 15, Param = 8.83333302f, NameText = "DH_RunesPageNameText15", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn15" });
            AddRow(new Row { Index = 16, Param = 9.33333302f, NameText = "DH_RunesPageNameText16", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn16" });
            AddRow(new Row { Index = 17, Param = 9.83333302f, NameText = "DH_RunesPageNameText17", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn17" });
            AddRow(new Row { Index = 18, Param = 10.333333f, NameText = "DH_RunesPageNameText18", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn18" });
            AddRow(new Row { Index = 19, Param = 10.833333f, NameText = "DH_RunesPageNameText19", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn19" });
            AddRow(new Row { Index = 20, Param = 11.333333f, NameText = "DH_RunesPageNameText20", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn20" });
            AddRow(new Row { Index = 21, Param = 11.833333f, NameText = "DH_RunesPageNameText21", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn21" });
            AddRow(new Row { Index = 22, Param = 12.333333f, NameText = "DH_RunesPageNameText22", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn22" });
            AddRow(new Row { Index = 23, Param = 12.833333f, NameText = "DH_RunesPageNameText23", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn23" });
            AddRow(new Row { Index = 24, Param = 13.333333f, NameText = "DH_RunesPageNameText24", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn24" });
            AddRow(new Row { Index = 25, Param = 13.833333f, NameText = "DH_RunesPageNameText25", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn25" });
            AddRow(new Row { Index = 26, Param = 14.333333f, NameText = "DH_RunesPageNameText26", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn26" });
            AddRow(new Row { Index = 27, Param = 14.833333f, NameText = "DH_RunesPageNameText27", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn27" });
            AddRow(new Row { Index = 28, Param = 15.333333f, NameText = "DH_RunesPageNameText28", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn28" });
            AddRow(new Row { Index = 29, Param = 15.833333f, NameText = "DH_RunesPageNameText29", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn29" });
            AddRow(new Row { Index = 30, Param = 16.333334f, NameText = "DH_RunesPageNameText30", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn30" });
            AddRow(new Row { Index = 31, Param = 16.833334f, NameText = "DH_RunesPageNameText31", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn31" });
            AddRow(new Row { Index = 32, Param = 17.333334f, NameText = "DH_RunesPageNameText32", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn32" });
            AddRow(new Row { Index = 33, Param = 17.833334f, NameText = "DH_RunesPageNameText33", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn33" });
            AddRow(new Row { Index = 34, Param = 18.333334f, NameText = "DH_RunesPageNameText34", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn34" });
            AddRow(new Row { Index = 35, Param = 18.833334f, NameText = "DH_RunesPageNameText35", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn35" });
            AddRow(new Row { Index = 36, Param = 19.333334f, NameText = "DH_RunesPageNameText36", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn36" });
            AddRow(new Row { Index = 37, Param = 19.833334f, NameText = "DH_RunesPageNameText37", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn37" });
            AddRow(new Row { Index = 38, Param = 20.333334f, NameText = "DH_RunesPageNameText38", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn38" });
            AddRow(new Row { Index = 39, Param = 20.833334f, NameText = "DH_RunesPageNameText39", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn39" });
            AddRow(new Row { Index = 40, Param = 21.333334f, NameText = "DH_RunesPageNameText40", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn40" });
            AddRow(new Row { Index = 41, Param = 21.833334f, NameText = "DH_RunesPageNameText41", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn41" });
            AddRow(new Row { Index = 42, Param = 22.333334f, NameText = "DH_RunesPageNameText42", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn42" });
            AddRow(new Row { Index = 43, Param = 22.833334f, NameText = "DH_RunesPageNameText43", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn43" });
            AddRow(new Row { Index = 44, Param = 23.333334f, NameText = "DH_RunesPageNameText44", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn44" });
            AddRow(new Row { Index = 45, Param = 23.833334f, NameText = "DH_RunesPageNameText45", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn45" });
            AddRow(new Row { Index = 46, Param = 24.333334f, NameText = "DH_RunesPageNameText46", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn46" });
            AddRow(new Row { Index = 47, Param = 24.833334f, NameText = "DH_RunesPageNameText47", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn47" });
            AddRow(new Row { Index = 48, Param = 25.333334f, NameText = "DH_RunesPageNameText48", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn48" });
            AddRow(new Row { Index = 49, Param = 25.833334f, NameText = "DH_RunesPageNameText49", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn49" });
            AddRow(new Row { Index = 50, Param = 26.333334f, NameText = "DH_RunesPageNameText50", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn50" });
            AddRow(new Row { Index = 51, Param = 26.833334f, NameText = "DH_RunesPageNameText51", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn51" });
            AddRow(new Row { Index = 52, Param = 27.333334f, NameText = "DH_RunesPageNameText52", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn52" });
            AddRow(new Row { Index = 53, Param = 27.833334f, NameText = "DH_RunesPageNameText53", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn53" });
            AddRow(new Row { Index = 54, Param = 28.333334f, NameText = "DH_RunesPageNameText54", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn54" });
            AddRow(new Row { Index = 55, Param = 28.833334f, NameText = "DH_RunesPageNameText55", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn55" });
            AddRow(new Row { Index = 56, Param = 29.333334f, NameText = "DH_RunesPageNameText56", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn56" });
            AddRow(new Row { Index = 57, Param = 29.833334f, NameText = "DH_RunesPageNameText57", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn57" });
            AddRow(new Row { Index = 58, Param = 30.333334f, NameText = "DH_RunesPageNameText58", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn58" });
            AddRow(new Row { Index = 59, Param = 30.833334f, NameText = "DH_RunesPageNameText59", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn59" });
            AddRow(new Row { Index = 60, Param = 31.333334f, NameText = "DH_RunesPageNameText60", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn60" });
            AddRow(new Row { Index = 61, Param = 31.833334f, NameText = "DH_RunesPageNameText61", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn61" });
            AddRow(new Row { Index = 62, Param = 32.3333321f, NameText = "DH_RunesPageNameText62", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn62" });
            AddRow(new Row { Index = 63, Param = 32.8333321f, NameText = "DH_RunesPageNameText63", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn63" });
            AddRow(new Row { Index = 64, Param = 33.3333321f, NameText = "DH_RunesPageNameText64", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn64" });
            AddRow(new Row { Index = 65, Param = 33.8333321f, NameText = "DH_RunesPageNameText65", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn65" });
            AddRow(new Row { Index = 66, Param = 34.3333321f, NameText = "DH_RunesPageNameText66", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn66" });
            AddRow(new Row { Index = 67, Param = 34.8333321f, NameText = "DH_RunesPageNameText67", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn67" });
            AddRow(new Row { Index = 68, Param = 35.3333321f, NameText = "DH_RunesPageNameText68", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn68" });
            AddRow(new Row { Index = 69, Param = 35.8333321f, NameText = "DH_RunesPageNameText69", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn69" });
            AddRow(new Row { Index = 70, Param = 36.3333321f, NameText = "DH_RunesPageNameText70", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn70" });
            AddRow(new Row { Index = 71, Param = 36.8333321f, NameText = "DH_RunesPageNameText71", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn71" });
            AddRow(new Row { Index = 72, Param = 37.3333321f, NameText = "DH_RunesPageNameText72", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn72" });
            AddRow(new Row { Index = 73, Param = 37.8333321f, NameText = "DH_RunesPageNameText73", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn73" });
            AddRow(new Row { Index = 74, Param = 38.3333321f, NameText = "DH_RunesPageNameText74", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn74" });
            AddRow(new Row { Index = 75, Param = 38.8333321f, NameText = "DH_RunesPageNameText75", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn75" });
            AddRow(new Row { Index = 76, Param = 39.3333321f, NameText = "DH_RunesPageNameText76", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn76" });
            AddRow(new Row { Index = 77, Param = 39.8333321f, NameText = "DH_RunesPageNameText77", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn77" });
            AddRow(new Row { Index = 78, Param = 40.3333321f, NameText = "DH_RunesPageNameText78", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn78" });
            AddRow(new Row { Index = 79, Param = 40.8333321f, NameText = "DH_RunesPageNameText79", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn79" });
            AddRow(new Row { Index = 80, Param = 41.3333321f, NameText = "DH_RunesPageNameText80", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn80" });
            AddRow(new Row { Index = 81, Param = 41.8333321f, NameText = "DH_RunesPageNameText81", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn81" });
            AddRow(new Row { Index = 82, Param = 42.3333321f, NameText = "DH_RunesPageNameText82", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn82" });
            AddRow(new Row { Index = 83, Param = 42.8333321f, NameText = "DH_RunesPageNameText83", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn83" });
            AddRow(new Row { Index = 84, Param = 43.3333321f, NameText = "DH_RunesPageNameText84", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn84" });
            AddRow(new Row { Index = 85, Param = 43.8333321f, NameText = "DH_RunesPageNameText85", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn85" });
            AddRow(new Row { Index = 86, Param = 44.3333321f, NameText = "DH_RunesPageNameText86", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn86" });
            AddRow(new Row { Index = 87, Param = 44.8333321f, NameText = "DH_RunesPageNameText87", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn87" });
            AddRow(new Row { Index = 88, Param = 45.3333321f, NameText = "DH_RunesPageNameText88", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn88" });
            AddRow(new Row { Index = 89, Param = 45.8333321f, NameText = "DH_RunesPageNameText89", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn89" });
            AddRow(new Row { Index = 90, Param = 46.3333321f, NameText = "DH_RunesPageNameText90", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn90" });
            AddRow(new Row { Index = 91, Param = 46.8333321f, NameText = "DH_RunesPageNameText91", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn91" });
            AddRow(new Row { Index = 92, Param = 47.3333321f, NameText = "DH_RunesPageNameText92", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn92" });
            AddRow(new Row { Index = 93, Param = 47.8333321f, NameText = "DH_RunesPageNameText93", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn93" });
            AddRow(new Row { Index = 94, Param = 48.3333321f, NameText = "DH_RunesPageNameText94", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn94" });
            AddRow(new Row { Index = 95, Param = 48.8333321f, NameText = "DH_RunesPageNameText95", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn95" });
            AddRow(new Row { Index = 96, Param = 49.3333321f, NameText = "DH_RunesPageNameText96", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn96" });
            AddRow(new Row { Index = 97, Param = 49.8333321f, NameText = "DH_RunesPageNameText97", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn97" });
            AddRow(new Row { Index = 98, Param = 50.3333321f, NameText = "DH_RunesPageNameText98", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn98" });
            AddRow(new Row { Index = 99, Param = 50.8333321f, NameText = "DH_RunesPageNameText99", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn99" });
            AddRow(new Row { Index = 100, Param = 51.3333321f, NameText = "DH_RunesPageNameText100", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn100" });
            AddRow(new Row { Index = 101, Param = 51.8333321f, NameText = "DH_RunesPageNameText101", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn101" });
            AddRow(new Row { Index = 102, Param = 52.3333321f, NameText = "DH_RunesPageNameText102", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn102" });
            AddRow(new Row { Index = 103, Param = 52.8333321f, NameText = "DH_RunesPageNameText103", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn103" });
            AddRow(new Row { Index = 104, Param = 53.3333321f, NameText = "DH_RunesPageNameText104", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn104" });
            AddRow(new Row { Index = 105, Param = 53.8333321f, NameText = "DH_RunesPageNameText105", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn105" });
            AddRow(new Row { Index = 106, Param = 54.3333321f, NameText = "DH_RunesPageNameText106", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn106" });
            AddRow(new Row { Index = 107, Param = 54.8333321f, NameText = "DH_RunesPageNameText107", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn107" });
            AddRow(new Row { Index = 108, Param = 55.3333321f, NameText = "DH_RunesPageNameText108", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn108" });
            AddRow(new Row { Index = 109, Param = 55.8333321f, NameText = "DH_RunesPageNameText109", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn109" });
            AddRow(new Row { Index = 110, Param = 56.3333321f, NameText = "DH_RunesPageNameText110", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn110" });
            AddRow(new Row { Index = 111, Param = 56.8333321f, NameText = "DH_RunesPageNameText111", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn111" });
            AddRow(new Row { Index = 112, Param = 57.3333321f, NameText = "DH_RunesPageNameText112", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn112" });
            AddRow(new Row { Index = 113, Param = 57.8333321f, NameText = "DH_RunesPageNameText113", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn113" });
            AddRow(new Row { Index = 114, Param = 58.3333321f, NameText = "DH_RunesPageNameText114", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn114" });
            AddRow(new Row { Index = 115, Param = 58.8333321f, NameText = "DH_RunesPageNameText115", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn115" });
            AddRow(new Row { Index = 116, Param = 59.3333321f, NameText = "DH_RunesPageNameText116", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn116" });
            AddRow(new Row { Index = 117, Param = 59.8333321f, NameText = "DH_RunesPageNameText117", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn117" });
            AddRow(new Row { Index = 118, Param = 60.3333321f, NameText = "DH_RunesPageNameText118", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn118" });
            AddRow(new Row { Index = 119, Param = 60.8333321f, NameText = "DH_RunesPageNameText119", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn119" });
            AddRow(new Row { Index = 120, Param = 61.3333321f, NameText = "DH_RunesPageNameText120", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn120" });
            AddRow(new Row { Index = 121, Param = 61.8333321f, NameText = "DH_RunesPageNameText121", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn121" });
            AddRow(new Row { Index = 122, Param = 62.3333321f, NameText = "DH_RunesPageNameText122", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn122" });
            AddRow(new Row { Index = 123, Param = 62.8333321f, NameText = "DH_RunesPageNameText123", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn123" });
            AddRow(new Row { Index = 124, Param = 63.3333321f, NameText = "DH_RunesPageNameText124", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn124" });
            AddRow(new Row { Index = 125, Param = 63.8333321f, NameText = "DH_RunesPageNameText125", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn125" });
            AddRow(new Row { Index = 126, Param = 64.3333359f, NameText = "DH_RunesPageNameText126", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn126" });
            AddRow(new Row { Index = 127, Param = 64.8333359f, NameText = "DH_RunesPageNameText127", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn127" });
            AddRow(new Row { Index = 128, Param = 65.3333359f, NameText = "DH_RunesPageNameText128", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn128" });
            AddRow(new Row { Index = 129, Param = 65.8333359f, NameText = "DH_RunesPageNameText129", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn129" });
            AddRow(new Row { Index = 130, Param = 66.3333359f, NameText = "DH_RunesPageNameText130", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn130" });
            AddRow(new Row { Index = 131, Param = 66.8333359f, NameText = "DH_RunesPageNameText131", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn131" });
            AddRow(new Row { Index = 132, Param = 67.3333359f, NameText = "DH_RunesPageNameText132", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn132" });
            AddRow(new Row { Index = 133, Param = 67.8333359f, NameText = "DH_RunesPageNameText133", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn133" });
            AddRow(new Row { Index = 134, Param = 68.3333359f, NameText = "DH_RunesPageNameText134", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn134" });
            AddRow(new Row { Index = 135, Param = 68.8333359f, NameText = "DH_RunesPageNameText135", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn135" });
            AddRow(new Row { Index = 136, Param = 69.3333359f, NameText = "DH_RunesPageNameText136", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn136" });
            AddRow(new Row { Index = 137, Param = 69.8333359f, NameText = "DH_RunesPageNameText137", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn137" });
            AddRow(new Row { Index = 138, Param = 70.3333359f, NameText = "DH_RunesPageNameText138", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn138" });
            AddRow(new Row { Index = 139, Param = 70.8333359f, NameText = "DH_RunesPageNameText139", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn139" });
            AddRow(new Row { Index = 140, Param = 71.3333359f, NameText = "DH_RunesPageNameText140", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn140" });
            AddRow(new Row { Index = 141, Param = 71.8333359f, NameText = "DH_RunesPageNameText141", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn141" });
            AddRow(new Row { Index = 142, Param = 72.3333359f, NameText = "DH_RunesPageNameText142", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn142" });
            AddRow(new Row { Index = 143, Param = 72.8333359f, NameText = "DH_RunesPageNameText143", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn143" });
            AddRow(new Row { Index = 144, Param = 73.3333359f, NameText = "DH_RunesPageNameText144", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn144" });
            AddRow(new Row { Index = 145, Param = 73.8333359f, NameText = "DH_RunesPageNameText145", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn145" });
            AddRow(new Row { Index = 146, Param = 74.3333359f, NameText = "DH_RunesPageNameText146", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn146" });
            AddRow(new Row { Index = 147, Param = 74.8333359f, NameText = "DH_RunesPageNameText147", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn147" });
            AddRow(new Row { Index = 148, Param = 75.3333359f, NameText = "DH_RunesPageNameText148", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn148" });
            AddRow(new Row { Index = 149, Param = 75.8333359f, NameText = "DH_RunesPageNameText149", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn149" });
            AddRow(new Row { Index = 150, Param = 76.3333359f, NameText = "DH_RunesPageNameText150", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn150" });
            AddRow(new Row { Index = 151, Param = 76.8333359f, NameText = "DH_RunesPageNameText151", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn151" });
            AddRow(new Row { Index = 152, Param = 77.3333359f, NameText = "DH_RunesPageNameText152", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn152" });
            AddRow(new Row { Index = 153, Param = 77.8333359f, NameText = "DH_RunesPageNameText153", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn153" });
            AddRow(new Row { Index = 154, Param = 78.3333359f, NameText = "DH_RunesPageNameText154", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn154" });
            AddRow(new Row { Index = 155, Param = 78.8333359f, NameText = "DH_RunesPageNameText155", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn155" });
            AddRow(new Row { Index = 156, Param = 79.3333359f, NameText = "DH_RunesPageNameText156", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn156" });
            AddRow(new Row { Index = 157, Param = 79.8333359f, NameText = "DH_RunesPageNameText157", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn157" });
            AddRow(new Row { Index = 158, Param = 80.3333359f, NameText = "DH_RunesPageNameText158", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn158" });
            AddRow(new Row { Index = 159, Param = 80.8333359f, NameText = "DH_RunesPageNameText159", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn159" });
            AddRow(new Row { Index = 160, Param = 81.3333359f, NameText = "DH_RunesPageNameText160", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn160" });
            AddRow(new Row { Index = 161, Param = 81.8333359f, NameText = "DH_RunesPageNameText161", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn161" });
            AddRow(new Row { Index = 162, Param = 82.3333359f, NameText = "DH_RunesPageNameText162", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn162" });
            AddRow(new Row { Index = 163, Param = 82.8333359f, NameText = "DH_RunesPageNameText163", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn163" });
            AddRow(new Row { Index = 164, Param = 83.3333359f, NameText = "DH_RunesPageNameText164", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn164" });
            AddRow(new Row { Index = 165, Param = 83.8333359f, NameText = "DH_RunesPageNameText165", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn165" });
            AddRow(new Row { Index = 166, Param = 84.3333359f, NameText = "DH_RunesPageNameText166", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn166" });
            AddRow(new Row { Index = 167, Param = 84.8333359f, NameText = "DH_RunesPageNameText167", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn167" });
            AddRow(new Row { Index = 168, Param = 85.3333359f, NameText = "DH_RunesPageNameText168", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn168" });
            AddRow(new Row { Index = 169, Param = 85.8333359f, NameText = "DH_RunesPageNameText169", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn169" });
            AddRow(new Row { Index = 170, Param = 86.3333359f, NameText = "DH_RunesPageNameText170", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn170" });
            AddRow(new Row { Index = 171, Param = 86.8333359f, NameText = "DH_RunesPageNameText171", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn171" });
            AddRow(new Row { Index = 172, Param = 87.3333359f, NameText = "DH_RunesPageNameText172", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn172" });
            AddRow(new Row { Index = 173, Param = 87.8333359f, NameText = "DH_RunesPageNameText173", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn173" });
            AddRow(new Row { Index = 174, Param = 88.3333359f, NameText = "DH_RunesPageNameText174", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn174" });
            AddRow(new Row { Index = 175, Param = 88.8333359f, NameText = "DH_RunesPageNameText175", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn175" });
            AddRow(new Row { Index = 176, Param = 89.3333359f, NameText = "DH_RunesPageNameText176", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn176" });
            AddRow(new Row { Index = 177, Param = 89.8333359f, NameText = "DH_RunesPageNameText177", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn177" });
            AddRow(new Row { Index = 178, Param = 90.3333359f, NameText = "DH_RunesPageNameText178", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn178" });
            AddRow(new Row { Index = 179, Param = 90.8333359f, NameText = "DH_RunesPageNameText179", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn179" });
            AddRow(new Row { Index = 180, Param = 91.3333359f, NameText = "DH_RunesPageNameText180", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn180" });
            AddRow(new Row { Index = 181, Param = 91.8333359f, NameText = "DH_RunesPageNameText181", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn181" });
            AddRow(new Row { Index = 182, Param = 92.3333359f, NameText = "DH_RunesPageNameText182", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn182" });
            AddRow(new Row { Index = 183, Param = 92.8333359f, NameText = "DH_RunesPageNameText183", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn183" });
            AddRow(new Row { Index = 184, Param = 93.3333359f, NameText = "DH_RunesPageNameText184", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn184" });
            AddRow(new Row { Index = 185, Param = 93.8333359f, NameText = "DH_RunesPageNameText185", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn185" });
            AddRow(new Row { Index = 186, Param = 94.3333359f, NameText = "DH_RunesPageNameText186", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn186" });
            AddRow(new Row { Index = 187, Param = 94.8333359f, NameText = "DH_RunesPageNameText187", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn187" });
            AddRow(new Row { Index = 188, Param = 95.3333359f, NameText = "DH_RunesPageNameText188", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn188" });
            AddRow(new Row { Index = 189, Param = 95.8333359f, NameText = "DH_RunesPageNameText189", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn189" });
            AddRow(new Row { Index = 190, Param = 96.3333359f, NameText = "DH_RunesPageNameText190", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn190" });
            AddRow(new Row { Index = 191, Param = 96.8333359f, NameText = "DH_RunesPageNameText191", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn191" });
            AddRow(new Row { Index = 192, Param = 97.3333359f, NameText = "DH_RunesPageNameText192", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn192" });
            AddRow(new Row { Index = 193, Param = 97.8333359f, NameText = "DH_RunesPageNameText193", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn193" });
            AddRow(new Row { Index = 194, Param = 98.3333359f, NameText = "DH_RunesPageNameText194", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn194" });
            AddRow(new Row { Index = 195, Param = 98.8333359f, NameText = "DH_RunesPageNameText195", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn195" });
            AddRow(new Row { Index = 196, Param = 99.3333359f, NameText = "DH_RunesPageNameText196", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn196" });
            AddRow(new Row { Index = 197, Param = 99.8333359f, NameText = "DH_RunesPageNameText197", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn197" });
            AddRow(new Row { Index = 198, Param = 100.333336f, NameText = "DH_RunesPageNameText198", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn198" });
            AddRow(new Row { Index = 199, Param = 100.833336f, NameText = "DH_RunesPageNameText199", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn199" });
            AddRow(new Row { Index = 200, Param = 101.333336f, NameText = "DH_RunesPageNameText200", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn200" });
            AddRow(new Row { Index = 201, Param = 101.833336f, NameText = "DH_RunesPageNameText201", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn201" });
            AddRow(new Row { Index = 202, Param = 102.333336f, NameText = "DH_RunesPageNameText202", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn202" });
            AddRow(new Row { Index = 203, Param = 102.833336f, NameText = "DH_RunesPageNameText203", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn203" });
            AddRow(new Row { Index = 204, Param = 103.333336f, NameText = "DH_RunesPageNameText204", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn204" });
            AddRow(new Row { Index = 205, Param = 103.833336f, NameText = "DH_RunesPageNameText205", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn205" });
            AddRow(new Row { Index = 206, Param = 104.333336f, NameText = "DH_RunesPageNameText206", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn206" });
            AddRow(new Row { Index = 207, Param = 104.833336f, NameText = "DH_RunesPageNameText207", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn207" });
            AddRow(new Row { Index = 208, Param = 105.333336f, NameText = "DH_RunesPageNameText208", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn208" });
            AddRow(new Row { Index = 209, Param = 105.833336f, NameText = "DH_RunesPageNameText209", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn209" });
            AddRow(new Row { Index = 210, Param = 106.333336f, NameText = "DH_RunesPageNameText210", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn210" });
            AddRow(new Row { Index = 211, Param = 106.833336f, NameText = "DH_RunesPageNameText211", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn211" });
            AddRow(new Row { Index = 212, Param = 107.333336f, NameText = "DH_RunesPageNameText212", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn212" });
            AddRow(new Row { Index = 213, Param = 107.833336f, NameText = "DH_RunesPageNameText213", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn213" });
            AddRow(new Row { Index = 214, Param = 108.333336f, NameText = "DH_RunesPageNameText214", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn214" });
            AddRow(new Row { Index = 215, Param = 108.833336f, NameText = "DH_RunesPageNameText215", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn215" });
            AddRow(new Row { Index = 216, Param = 109.333336f, NameText = "DH_RunesPageNameText216", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn216" });
            AddRow(new Row { Index = 217, Param = 109.833336f, NameText = "DH_RunesPageNameText217", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn217" });
            AddRow(new Row { Index = 218, Param = 110.333336f, NameText = "DH_RunesPageNameText218", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn218" });
            AddRow(new Row { Index = 219, Param = 110.833336f, NameText = "DH_RunesPageNameText219", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn219" });
            AddRow(new Row { Index = 220, Param = 111.333336f, NameText = "DH_RunesPageNameText220", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn220" });
            AddRow(new Row { Index = 221, Param = 111.833336f, NameText = "DH_RunesPageNameText221", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn221" });
            AddRow(new Row { Index = 222, Param = 112.333336f, NameText = "DH_RunesPageNameText222", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn222" });
            AddRow(new Row { Index = 223, Param = 112.833336f, NameText = "DH_RunesPageNameText223", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn223" });
            AddRow(new Row { Index = 224, Param = 113.333336f, NameText = "DH_RunesPageNameText224", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn224" });
            AddRow(new Row { Index = 225, Param = 113.833336f, NameText = "DH_RunesPageNameText225", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn225" });
            AddRow(new Row { Index = 226, Param = 114.333336f, NameText = "DH_RunesPageNameText226", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn226" });
            AddRow(new Row { Index = 227, Param = 114.833336f, NameText = "DH_RunesPageNameText227", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn227" });
            AddRow(new Row { Index = 228, Param = 115.333336f, NameText = "DH_RunesPageNameText228", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn228" });
            AddRow(new Row { Index = 229, Param = 115.833336f, NameText = "DH_RunesPageNameText229", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn229" });
            AddRow(new Row { Index = 230, Param = 116.333336f, NameText = "DH_RunesPageNameText230", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn230" });
            AddRow(new Row { Index = 231, Param = 116.833336f, NameText = "DH_RunesPageNameText231", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn231" });
            AddRow(new Row { Index = 232, Param = 117.333336f, NameText = "DH_RunesPageNameText232", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn232" });
            AddRow(new Row { Index = 233, Param = 117.833336f, NameText = "DH_RunesPageNameText233", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn233" });
            AddRow(new Row { Index = 234, Param = 118.333336f, NameText = "DH_RunesPageNameText234", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn234" });
            AddRow(new Row { Index = 235, Param = 118.833336f, NameText = "DH_RunesPageNameText235", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn235" });
            AddRow(new Row { Index = 236, Param = 119.333336f, NameText = "DH_RunesPageNameText236", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn236" });
            AddRow(new Row { Index = 237, Param = 119.833336f, NameText = "DH_RunesPageNameText237", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn237" });
            AddRow(new Row { Index = 238, Param = 120.333336f, NameText = "DH_RunesPageNameText238", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn238" });
            AddRow(new Row { Index = 239, Param = 120.833336f, NameText = "DH_RunesPageNameText239", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn239" });
            AddRow(new Row { Index = 240, Param = 121.333336f, NameText = "DH_RunesPageNameText240", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn240" });
            AddRow(new Row { Index = 241, Param = 121.833336f, NameText = "DH_RunesPageNameText241", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn241" });
            AddRow(new Row { Index = 242, Param = 122.333336f, NameText = "DH_RunesPageNameText242", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn242" });
            AddRow(new Row { Index = 243, Param = 122.833336f, NameText = "DH_RunesPageNameText243", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn243" });
            AddRow(new Row { Index = 244, Param = 123.333336f, NameText = "DH_RunesPageNameText244", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn244" });
            AddRow(new Row { Index = 245, Param = 123.833336f, NameText = "DH_RunesPageNameText245", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn245" });
            AddRow(new Row { Index = 246, Param = 124.333336f, NameText = "DH_RunesPageNameText246", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn246" });
            AddRow(new Row { Index = 247, Param = 124.833336f, NameText = "DH_RunesPageNameText247", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn247" });
            AddRow(new Row { Index = 248, Param = 125.333336f, NameText = "DH_RunesPageNameText248", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn248" });
            AddRow(new Row { Index = 249, Param = 125.833336f, NameText = "DH_RunesPageNameText249", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn249" });
            AddRow(new Row { Index = 250, Param = 126.333336f, NameText = "DH_RunesPageNameText250", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn250" });
            AddRow(new Row { Index = 251, Param = 126.833336f, NameText = "DH_RunesPageNameText251", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn251" });
            AddRow(new Row { Index = 252, Param = 127.333336f, NameText = "DH_RunesPageNameText252", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn252" });
            AddRow(new Row { Index = 253, Param = 127.833336f, NameText = "DH_RunesPageNameText253", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn253" });
            AddRow(new Row { Index = 254, Param = 128.333328f, NameText = "DH_RunesPageNameText254", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn254" });
            AddRow(new Row { Index = 255, Param = 128.833328f, NameText = "DH_RunesPageNameText255", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn255" });
            AddRow(new Row { Index = 256, Param = 129.333328f, NameText = "DH_RunesPageNameText256", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn256" });
            AddRow(new Row { Index = 257, Param = 129.833328f, NameText = "DH_RunesPageNameText257", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn257" });
            AddRow(new Row { Index = 258, Param = 130.333328f, NameText = "DH_RunesPageNameText258", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn258" });
            AddRow(new Row { Index = 259, Param = 130.833328f, NameText = "DH_RunesPageNameText259", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn259" });
            AddRow(new Row { Index = 260, Param = 131.333328f, NameText = "DH_RunesPageNameText260", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn260" });
            AddRow(new Row { Index = 261, Param = 131.833328f, NameText = "DH_RunesPageNameText261", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn261" });
            AddRow(new Row { Index = 262, Param = 132.333328f, NameText = "DH_RunesPageNameText262", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn262" });
            AddRow(new Row { Index = 263, Param = 132.833328f, NameText = "DH_RunesPageNameText263", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn263" });
            AddRow(new Row { Index = 264, Param = 133.333328f, NameText = "DH_RunesPageNameText264", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn264" });
            AddRow(new Row { Index = 265, Param = 133.833328f, NameText = "DH_RunesPageNameText265", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn265" });
            AddRow(new Row { Index = 266, Param = 134.333328f, NameText = "DH_RunesPageNameText266", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn266" });
            AddRow(new Row { Index = 267, Param = 134.833328f, NameText = "DH_RunesPageNameText267", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn267" });
            AddRow(new Row { Index = 268, Param = 135.333328f, NameText = "DH_RunesPageNameText268", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn268" });
            AddRow(new Row { Index = 269, Param = 135.833328f, NameText = "DH_RunesPageNameText269", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn269" });
            AddRow(new Row { Index = 270, Param = 136.333328f, NameText = "DH_RunesPageNameText270", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn270" });
            AddRow(new Row { Index = 271, Param = 136.833328f, NameText = "DH_RunesPageNameText271", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn271" });
            AddRow(new Row { Index = 272, Param = 137.333328f, NameText = "DH_RunesPageNameText272", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn272" });
            AddRow(new Row { Index = 273, Param = 137.833328f, NameText = "DH_RunesPageNameText273", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn273" });
            AddRow(new Row { Index = 274, Param = 138.333328f, NameText = "DH_RunesPageNameText274", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn274" });
            AddRow(new Row { Index = 275, Param = 138.833328f, NameText = "DH_RunesPageNameText275", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn275" });
            AddRow(new Row { Index = 276, Param = 139.333328f, NameText = "DH_RunesPageNameText276", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn276" });
            AddRow(new Row { Index = 277, Param = 139.833328f, NameText = "DH_RunesPageNameText277", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn277" });
            AddRow(new Row { Index = 278, Param = 140.333328f, NameText = "DH_RunesPageNameText278", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn278" });
            AddRow(new Row { Index = 279, Param = 140.833328f, NameText = "DH_RunesPageNameText279", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn279" });
            AddRow(new Row { Index = 280, Param = 141.333328f, NameText = "DH_RunesPageNameText280", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn280" });
            AddRow(new Row { Index = 281, Param = 141.833328f, NameText = "DH_RunesPageNameText281", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn281" });
            AddRow(new Row { Index = 282, Param = 142.333328f, NameText = "DH_RunesPageNameText282", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn282" });
            AddRow(new Row { Index = 283, Param = 142.833328f, NameText = "DH_RunesPageNameText283", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn283" });
            AddRow(new Row { Index = 284, Param = 143.333328f, NameText = "DH_RunesPageNameText284", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn284" });
            AddRow(new Row { Index = 285, Param = 143.833328f, NameText = "DH_RunesPageNameText285", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn285" });
            AddRow(new Row { Index = 286, Param = 144.333328f, NameText = "DH_RunesPageNameText286", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn286" });
            AddRow(new Row { Index = 287, Param = 144.833328f, NameText = "DH_RunesPageNameText287", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn287" });
            AddRow(new Row { Index = 288, Param = 145.333328f, NameText = "DH_RunesPageNameText288", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn288" });
            AddRow(new Row { Index = 289, Param = 145.833328f, NameText = "DH_RunesPageNameText289", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn289" });
            AddRow(new Row { Index = 290, Param = 146.333328f, NameText = "DH_RunesPageNameText290", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn290" });
            AddRow(new Row { Index = 291, Param = 146.833328f, NameText = "DH_RunesPageNameText291", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn291" });
            AddRow(new Row { Index = 292, Param = 147.333328f, NameText = "DH_RunesPageNameText292", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn292" });
            AddRow(new Row { Index = 293, Param = 147.833328f, NameText = "DH_RunesPageNameText293", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn293" });
            AddRow(new Row { Index = 294, Param = 148.333328f, NameText = "DH_RunesPageNameText294", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn294" });
            AddRow(new Row { Index = 295, Param = 148.833328f, NameText = "DH_RunesPageNameText295", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn295" });
            AddRow(new Row { Index = 296, Param = 149.333328f, NameText = "DH_RunesPageNameText296", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn296" });
            AddRow(new Row { Index = 297, Param = 149.833328f, NameText = "DH_RunesPageNameText297", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn297" });
            AddRow(new Row { Index = 298, Param = 150.333328f, NameText = "DH_RunesPageNameText298", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn298" });
            AddRow(new Row { Index = 299, Param = 150.833328f, NameText = "DH_RunesPageNameText299", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn299" });
            AddRow(new Row { Index = 300, Param = 151.333328f, NameText = "DH_RunesPageNameText300", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn300" });
            AddRow(new Row { Index = 301, Param = 151.833328f, NameText = "DH_RunesPageNameText301", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn301" });
            AddRow(new Row { Index = 302, Param = 152.333328f, NameText = "DH_RunesPageNameText302", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn302" });
            AddRow(new Row { Index = 303, Param = 152.833328f, NameText = "DH_RunesPageNameText303", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn303" });
            AddRow(new Row { Index = 304, Param = 153.333328f, NameText = "DH_RunesPageNameText304", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn304" });
            AddRow(new Row { Index = 305, Param = 153.833328f, NameText = "DH_RunesPageNameText305", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn305" });
            AddRow(new Row { Index = 306, Param = 154.333328f, NameText = "DH_RunesPageNameText306", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn306" });
            AddRow(new Row { Index = 307, Param = 154.833328f, NameText = "DH_RunesPageNameText307", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn307" });
            AddRow(new Row { Index = 308, Param = 155.333328f, NameText = "DH_RunesPageNameText308", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn308" });
            AddRow(new Row { Index = 309, Param = 155.833328f, NameText = "DH_RunesPageNameText309", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn309" });
            AddRow(new Row { Index = 310, Param = 156.333328f, NameText = "DH_RunesPageNameText310", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn310" });
            AddRow(new Row { Index = 311, Param = 156.833328f, NameText = "DH_RunesPageNameText311", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn311" });
            AddRow(new Row { Index = 312, Param = 157.333328f, NameText = "DH_RunesPageNameText312", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn312" });
            AddRow(new Row { Index = 313, Param = 157.833328f, NameText = "DH_RunesPageNameText313", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn313" });
            AddRow(new Row { Index = 314, Param = 158.333328f, NameText = "DH_RunesPageNameText314", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn314" });
            AddRow(new Row { Index = 315, Param = 158.833328f, NameText = "DH_RunesPageNameText315", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn315" });
            AddRow(new Row { Index = 316, Param = 159.333328f, NameText = "DH_RunesPageNameText316", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn316" });
            AddRow(new Row { Index = 317, Param = 159.833328f, NameText = "DH_RunesPageNameText317", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn317" });
            AddRow(new Row { Index = 318, Param = 160.333328f, NameText = "DH_RunesPageNameText318", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn318" });
            AddRow(new Row { Index = 319, Param = 160.833328f, NameText = "DH_RunesPageNameText319", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn319" });
            AddRow(new Row { Index = 320, Param = 161.333328f, NameText = "DH_RunesPageNameText320", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn320" });
            AddRow(new Row { Index = 321, Param = 161.833328f, NameText = "DH_RunesPageNameText321", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn321" });
            AddRow(new Row { Index = 322, Param = 162.333328f, NameText = "DH_RunesPageNameText322", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn322" });
            AddRow(new Row { Index = 323, Param = 162.833328f, NameText = "DH_RunesPageNameText323", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn323" });
            AddRow(new Row { Index = 324, Param = 163.333328f, NameText = "DH_RunesPageNameText324", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn324" });
            AddRow(new Row { Index = 325, Param = 163.833328f, NameText = "DH_RunesPageNameText325", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn325" });
            AddRow(new Row { Index = 326, Param = 164.333328f, NameText = "DH_RunesPageNameText326", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn326" });
            AddRow(new Row { Index = 327, Param = 164.833328f, NameText = "DH_RunesPageNameText327", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn327" });
            AddRow(new Row { Index = 328, Param = 165.333328f, NameText = "DH_RunesPageNameText328", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn328" });
            AddRow(new Row { Index = 329, Param = 165.833328f, NameText = "DH_RunesPageNameText329", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn329" });
            AddRow(new Row { Index = 330, Param = 166.333328f, NameText = "DH_RunesPageNameText330", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn330" });
            AddRow(new Row { Index = 331, Param = 166.833328f, NameText = "DH_RunesPageNameText331", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn331" });
            AddRow(new Row { Index = 332, Param = 167.333328f, NameText = "DH_RunesPageNameText332", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn332" });
            AddRow(new Row { Index = 333, Param = 167.833328f, NameText = "DH_RunesPageNameText333", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn333" });
            AddRow(new Row { Index = 334, Param = 168.333328f, NameText = "DH_RunesPageNameText334", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn334" });
            AddRow(new Row { Index = 335, Param = 168.833328f, NameText = "DH_RunesPageNameText335", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn335" });
            AddRow(new Row { Index = 336, Param = 169.333328f, NameText = "DH_RunesPageNameText336", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn336" });
            AddRow(new Row { Index = 337, Param = 169.833328f, NameText = "DH_RunesPageNameText337", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn337" });
            AddRow(new Row { Index = 338, Param = 170.333328f, NameText = "DH_RunesPageNameText338", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn338" });
            AddRow(new Row { Index = 339, Param = 170.833328f, NameText = "DH_RunesPageNameText339", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn339" });
            AddRow(new Row { Index = 340, Param = 171.333328f, NameText = "DH_RunesPageNameText340", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn340" });
            AddRow(new Row { Index = 341, Param = 171.833328f, NameText = "DH_RunesPageNameText341", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn341" });
            AddRow(new Row { Index = 342, Param = 172.333328f, NameText = "DH_RunesPageNameText342", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn342" });
            AddRow(new Row { Index = 343, Param = 172.833328f, NameText = "DH_RunesPageNameText343", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn343" });
            AddRow(new Row { Index = 344, Param = 173.333328f, NameText = "DH_RunesPageNameText344", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn344" });
            AddRow(new Row { Index = 345, Param = 173.833328f, NameText = "DH_RunesPageNameText345", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn345" });
            AddRow(new Row { Index = 346, Param = 174.333328f, NameText = "DH_RunesPageNameText346", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn346" });
            AddRow(new Row { Index = 348, Param = 175.333328f, NameText = "DH_RunesPageNameText348", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn348" });
            AddRow(new Row { Index = 349, Param = 175.833328f, NameText = "DH_RunesPageNameText349", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn349" });
            AddRow(new Row { Index = 350, Param = 176.333328f, NameText = "DH_RunesPageNameText350", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn350" });
            AddRow(new Row { Index = 351, Param = 176.833328f, NameText = "DH_RunesPageNameText351", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn351" });
            AddRow(new Row { Index = 352, Param = 177.333328f, NameText = "DH_RunesPageNameText352", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn352" });
            AddRow(new Row { Index = 353, Param = 177.833328f, NameText = "DH_RunesPageNameText353", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn353" });
            AddRow(new Row { Index = 354, Param = 178.333328f, NameText = "DH_RunesPageNameText354", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn354" });
            AddRow(new Row { Index = 355, Param = 178.833328f, NameText = "DH_RunesPageNameText355", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn355" });
            AddRow(new Row { Index = 356, Param = 179.333328f, NameText = "DH_RunesPageNameText356", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn356" });
            AddRow(new Row { Index = 357, Param = 179.833328f, NameText = "DH_RunesPageNameText357", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn357" });
            AddRow(new Row { Index = 358, Param = 180.333328f, NameText = "DH_RunesPageNameText358", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn358" });
            AddRow(new Row { Index = 359, Param = 180.833328f, NameText = "DH_RunesPageNameText359", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn359" });
            AddRow(new Row { Index = 360, Param = 181.333328f, NameText = "DH_RunesPageNameText360", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn360" });
            AddRow(new Row { Index = 361, Param = 181.833328f, NameText = "DH_RunesPageNameText361", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn361" });
            AddRow(new Row { Index = 362, Param = 182.333328f, NameText = "DH_RunesPageNameText362", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn362" });
            AddRow(new Row { Index = 363, Param = 182.833328f, NameText = "DH_RunesPageNameText363", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn363" });
            AddRow(new Row { Index = 364, Param = 183.333328f, NameText = "DH_RunesPageNameText364", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn364" });
            AddRow(new Row { Index = 365, Param = 183.833328f, NameText = "DH_RunesPageNameText365", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn365" });
            AddRow(new Row { Index = 366, Param = 184.333328f, NameText = "DH_RunesPageNameText366", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn366" });
            AddRow(new Row { Index = 367, Param = 184.833328f, NameText = "DH_RunesPageNameText367", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn367" });
            AddRow(new Row { Index = 368, Param = 185.333328f, NameText = "DH_RunesPageNameText368", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn368" });
            AddRow(new Row { Index = 370, Param = 186.333328f, NameText = "DH_RunesPageNameText370", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn370" });
            AddRow(new Row { Index = 371, Param = 186.833328f, NameText = "DH_RunesPageNameText371", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn371" });
            AddRow(new Row { Index = 372, Param = 187.333328f, NameText = "DH_RunesPageNameText372", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn372" });
            AddRow(new Row { Index = 373, Param = 187.833328f, NameText = "DH_RunesPageNameText373", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn373" });
            AddRow(new Row { Index = 374, Param = 188.333328f, NameText = "DH_RunesPageNameText374", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn374" });
            AddRow(new Row { Index = 375, Param = 188.833328f, NameText = "DH_RunesPageNameText375", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn375" });
            AddRow(new Row { Index = 376, Param = 189.333328f, NameText = "DH_RunesPageNameText376", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn376" });
            AddRow(new Row { Index = 377, Param = 189.833328f, NameText = "DH_RunesPageNameText377", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn377" });
            AddRow(new Row { Index = 379, Param = 190.833328f, NameText = "DH_RunesPageNameText379", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn379" });
            AddRow(new Row { Index = 380, Param = 191.333328f, NameText = "DH_RunesPageNameText380", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn380" });
            AddRow(new Row { Index = 381, Param = 191.833328f, NameText = "DH_RunesPageNameText381", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn381" });
            AddRow(new Row { Index = 382, Param = 192.333328f, NameText = "DH_RunesPageNameText382", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn382" });
            AddRow(new Row { Index = 383, Param = 192.833328f, NameText = "DH_RunesPageNameText383", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn383" });
            AddRow(new Row { Index = 384, Param = 193.333328f, NameText = "DH_RunesPageNameText384", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn384" });
            AddRow(new Row { Index = 385, Param = 193.833328f, NameText = "DH_RunesPageNameText385", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn385" });
            AddRow(new Row { Index = 386, Param = 194.333328f, NameText = "DH_RunesPageNameText386", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn386" });
            AddRow(new Row { Index = 387, Param = 194.833328f, NameText = "DH_RunesPageNameText387", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn387" });
            AddRow(new Row { Index = 388, Param = 195.333328f, NameText = "DH_RunesPageNameText388", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn388" });
            AddRow(new Row { Index = 389, Param = 195.833328f, NameText = "DH_RunesPageNameText389", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn389" });
            AddRow(new Row { Index = 390, Param = 196.333328f, NameText = "DH_RunesPageNameText390", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn390" });
            AddRow(new Row { Index = 391, Param = 196.833328f, NameText = "DH_RunesPageNameText391", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn391" });
            AddRow(new Row { Index = 392, Param = 197.333328f, NameText = "DH_RunesPageNameText392", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn392" });
            AddRow(new Row { Index = 393, Param = 197.833328f, NameText = "DH_RunesPageNameText393", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn393" });
            AddRow(new Row { Index = 394, Param = 198.333328f, NameText = "DH_RunesPageNameText394", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn394" });
            AddRow(new Row { Index = 395, Param = 198.833328f, NameText = "DH_RunesPageNameText395", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn395" });
            AddRow(new Row { Index = 397, Param = 199.833328f, NameText = "DH_RunesPageNameText397", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn397" });
            AddRow(new Row { Index = 398, Param = 200.333328f, NameText = "DH_RunesPageNameText398", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn398" });
            AddRow(new Row { Index = 399, Param = 200.833328f, NameText = "DH_RunesPageNameText399", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn399" });
            AddRow(new Row { Index = 400, Param = 201.333328f, NameText = "DH_RunesPageNameText400", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn400" });
            AddRow(new Row { Index = 401, Param = 201.833328f, NameText = "DH_RunesPageNameText401", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn401" });
            AddRow(new Row { Index = 402, Param = 202.333328f, NameText = "DH_RunesPageNameText402", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn402" });
            AddRow(new Row { Index = 403, Param = 202.833328f, NameText = "DH_RunesPageNameText403", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn403" });
            AddRow(new Row { Index = 404, Param = 203.333328f, NameText = "DH_RunesPageNameText404", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn404" });
            AddRow(new Row { Index = 405, Param = 203.833328f, NameText = "DH_RunesPageNameText405", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn405" });
            AddRow(new Row { Index = 406, Param = 204.333328f, NameText = "DH_RunesPageNameText406", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn406" });
            AddRow(new Row { Index = 407, Param = 204.833328f, NameText = "DH_RunesPageNameText407", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn407" });
            AddRow(new Row { Index = 408, Param = 205.333328f, NameText = "DH_RunesPageNameText408", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn408" });
            AddRow(new Row { Index = 409, Param = 205.833328f, NameText = "DH_RunesPageNameText409", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn409" });
            AddRow(new Row { Index = 410, Param = 206.333328f, NameText = "DH_RunesPageNameText410", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn410" });
            AddRow(new Row { Index = 412, Param = 207.333328f, NameText = "DH_RunesPageNameText412", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn412" });
            AddRow(new Row { Index = 413, Param = 207.833328f, NameText = "DH_RunesPageNameText413", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn413" });
            AddRow(new Row { Index = 414, Param = 208.333328f, NameText = "DH_RunesPageNameText414", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn414" });
            AddRow(new Row { Index = 415, Param = 208.833328f, NameText = "DH_RunesPageNameText415", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn415" });
            AddRow(new Row { Index = 416, Param = 209.333328f, NameText = "DH_RunesPageNameText416", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn416" });
            AddRow(new Row { Index = 417, Param = 209.833328f, NameText = "DH_RunesPageNameText417", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn417" });
            AddRow(new Row { Index = 418, Param = 210.333328f, NameText = "DH_RunesPageNameText418", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn418" });
            AddRow(new Row { Index = 419, Param = 210.833328f, NameText = "DH_RunesPageNameText419", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn419" });
            AddRow(new Row { Index = 420, Param = 211.333328f, NameText = "DH_RunesPageNameText420", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn420" });
            AddRow(new Row { Index = 421, Param = 211.833328f, NameText = "DH_RunesPageNameText421", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn421" });
            AddRow(new Row { Index = 422, Param = 212.333328f, NameText = "DH_RunesPageNameText422", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn422" });
            AddRow(new Row { Index = 423, Param = 212.833328f, NameText = "DH_RunesPageNameText423", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn423" });
            AddRow(new Row { Index = 424, Param = 213.333328f, NameText = "DH_RunesPageNameText424", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn424" });
            AddRow(new Row { Index = 425, Param = 213.833328f, NameText = "DH_RunesPageNameText425", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn425" });
            AddRow(new Row { Index = 427, Param = 214.833328f, NameText = "DH_RunesPageNameText427", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn427" });
            AddRow(new Row { Index = 428, Param = 215.333328f, NameText = "DH_RunesPageNameText428", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn428" });
            AddRow(new Row { Index = 429, Param = 215.833328f, NameText = "DH_RunesPageNameText429", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn429" });
            AddRow(new Row { Index = 430, Param = 216.333328f, NameText = "DH_RunesPageNameText430", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn430" });
            AddRow(new Row { Index = 431, Param = 216.833328f, NameText = "DH_RunesPageNameText431", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn431" });
            AddRow(new Row { Index = 432, Param = 217.333328f, NameText = "DH_RunesPageNameText432", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn432" });
            AddRow(new Row { Index = 433, Param = 217.833328f, NameText = "DH_RunesPageNameText433", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn433" });
            AddRow(new Row { Index = 434, Param = 218.333328f, NameText = "DH_RunesPageNameText434", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn434" });
            AddRow(new Row { Index = 435, Param = 218.833328f, NameText = "DH_RunesPageNameText435", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn435" });
            AddRow(new Row { Index = 436, Param = 219.333328f, NameText = "DH_RunesPageNameText436", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn436" });
            AddRow(new Row { Index = 437, Param = 219.833328f, NameText = "DH_RunesPageNameText437", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn437" });
            AddRow(new Row { Index = 438, Param = 220.333328f, NameText = "DH_RunesPageNameText438", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn438" });
            AddRow(new Row { Index = 439, Param = 220.833328f, NameText = "DH_RunesPageNameText439", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn439" });
            AddRow(new Row { Index = 440, Param = 221.333328f, NameText = "DH_RunesPageNameText440", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn440" });
            AddRow(new Row { Index = 441, Param = 221.833328f, NameText = "DH_RunesPageNameText441", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn441" });
            AddRow(new Row { Index = 442, Param = 222.333328f, NameText = "DH_RunesPageNameText442", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn442" });
            AddRow(new Row { Index = 443, Param = 222.833328f, NameText = "DH_RunesPageNameText443", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn443" });
            AddRow(new Row { Index = 445, Param = 223.833328f, NameText = "DH_RunesPageNameText445", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn445" });
            AddRow(new Row { Index = 446, Param = 224.333328f, NameText = "DH_RunesPageNameText446", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn446" });
            AddRow(new Row { Index = 447, Param = 224.833328f, NameText = "DH_RunesPageNameText447", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn447" });
            AddRow(new Row { Index = 448, Param = 225.333328f, NameText = "DH_RunesPageNameText448", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn448" });
            AddRow(new Row { Index = 449, Param = 225.833328f, NameText = "DH_RunesPageNameText449", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn449" });
            AddRow(new Row { Index = 450, Param = 226.333328f, NameText = "DH_RunesPageNameText450", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn450" });
            AddRow(new Row { Index = 451, Param = 226.833328f, NameText = "DH_RunesPageNameText451", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn451" });
            AddRow(new Row { Index = 452, Param = 227.333328f, NameText = "DH_RunesPageNameText452", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn452" });
            AddRow(new Row { Index = 453, Param = 227.833328f, NameText = "DH_RunesPageNameText453", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn453" });
            AddRow(new Row { Index = 454, Param = 228.333328f, NameText = "DH_RunesPageNameText454", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn454" });
            AddRow(new Row { Index = 455, Param = 228.833328f, NameText = "DH_RunesPageNameText455", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn455" });
            AddRow(new Row { Index = 456, Param = 229.333328f, NameText = "DH_RunesPageNameText456", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn456" });
            AddRow(new Row { Index = 457, Param = 229.833328f, NameText = "DH_RunesPageNameText457", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn457" });
            AddRow(new Row { Index = 458, Param = 230.333328f, NameText = "DH_RunesPageNameText458", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn458" });
            AddRow(new Row { Index = 459, Param = 230.833328f, NameText = "DH_RunesPageNameText459", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn459" });
            AddRow(new Row { Index = 460, Param = 231.333328f, NameText = "DH_RunesPageNameText460", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn460" });
            AddRow(new Row { Index = 461, Param = 231.833328f, NameText = "DH_RunesPageNameText461", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn461" });
            AddRow(new Row { Index = 463, Param = 232.833328f, NameText = "DH_RunesPageNameText463", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn463" });
            AddRow(new Row { Index = 464, Param = 233.333328f, NameText = "DH_RunesPageNameText464", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn464" });
            AddRow(new Row { Index = 465, Param = 233.833328f, NameText = "DH_RunesPageNameText465", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn465" });
            AddRow(new Row { Index = 466, Param = 234.333328f, NameText = "DH_RunesPageNameText466", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn466" });
            AddRow(new Row { Index = 467, Param = 234.833328f, NameText = "DH_RunesPageNameText467", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn467" });
            AddRow(new Row { Index = 468, Param = 235.333328f, NameText = "DH_RunesPageNameText468", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn468" });
            AddRow(new Row { Index = 469, Param = 235.833328f, NameText = "DH_RunesPageNameText469", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn469" });
            AddRow(new Row { Index = 470, Param = 236.333328f, NameText = "DH_RunesPageNameText470", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn470" });
            AddRow(new Row { Index = 471, Param = 236.833328f, NameText = "DH_RunesPageNameText471", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn471" });
            AddRow(new Row { Index = 472, Param = 237.333328f, NameText = "DH_RunesPageNameText472", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn472" });
            AddRow(new Row { Index = 473, Param = 237.833328f, NameText = "DH_RunesPageNameText473", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn473" });
            AddRow(new Row { Index = 474, Param = 238.333328f, NameText = "DH_RunesPageNameText474", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn474" });
            AddRow(new Row { Index = 475, Param = 238.833328f, NameText = "DH_RunesPageNameText475", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn475" });
            AddRow(new Row { Index = 476, Param = 239.333328f, NameText = "DH_RunesPageNameText476", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn476" });
            AddRow(new Row { Index = 478, Param = 240.333328f, NameText = "DH_RunesPageNameText478", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn478" });
            AddRow(new Row { Index = 479, Param = 240.833328f, NameText = "DH_RunesPageNameText479", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn479" });
            AddRow(new Row { Index = 480, Param = 241.333328f, NameText = "DH_RunesPageNameText480", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn480" });
            AddRow(new Row { Index = 481, Param = 241.833328f, NameText = "DH_RunesPageNameText481", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn481" });
            AddRow(new Row { Index = 482, Param = 242.333328f, NameText = "DH_RunesPageNameText482", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn482" });
            AddRow(new Row { Index = 483, Param = 242.833328f, NameText = "DH_RunesPageNameText483", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn483" });
            AddRow(new Row { Index = 484, Param = 243.333328f, NameText = "DH_RunesPageNameText484", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn484" });
            AddRow(new Row { Index = 485, Param = 243.833328f, NameText = "DH_RunesPageNameText485", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn485" });
            AddRow(new Row { Index = 486, Param = 244.333328f, NameText = "DH_RunesPageNameText486", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn486" });
            AddRow(new Row { Index = 487, Param = 244.833328f, NameText = "DH_RunesPageNameText487", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn487" });
            AddRow(new Row { Index = 488, Param = 245.333328f, NameText = "DH_RunesPageNameText488", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn488" });
            AddRow(new Row { Index = 489, Param = 245.833328f, NameText = "DH_RunesPageNameText489", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn489" });
            AddRow(new Row { Index = 490, Param = 246.333328f, NameText = "DH_RunesPageNameText490", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn490" });
            AddRow(new Row { Index = 491, Param = 246.833328f, NameText = "DH_RunesPageNameText491", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn491" });
            AddRow(new Row { Index = 492, Param = 247.333328f, NameText = "DH_RunesPageNameText492", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn492" });
            AddRow(new Row { Index = 493, Param = 247.833328f, NameText = "DH_RunesPageNameText493", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn493" });
            AddRow(new Row { Index = 494, Param = 248.333328f, NameText = "DH_RunesPageNameText494", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn494" });
            AddRow(new Row { Index = 496, Param = 249.333328f, NameText = "DH_RunesPageNameText496", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn496" });
            AddRow(new Row { Index = 497, Param = 249.833328f, NameText = "DH_RunesPageNameText497", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn497" });
            AddRow(new Row { Index = 498, Param = 250.333328f, NameText = "DH_RunesPageNameText498", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn498" });
            AddRow(new Row { Index = 499, Param = 250.833328f, NameText = "DH_RunesPageNameText499", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn499" });
            AddRow(new Row { Index = 500, Param = 251.333328f, NameText = "DH_RunesPageNameText500", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn500" });
            AddRow(new Row { Index = 501, Param = 251.833328f, NameText = "DH_RunesPageNameText501", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn501" });
            AddRow(new Row { Index = 502, Param = 252.333328f, NameText = "DH_RunesPageNameText502", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn502" });
            AddRow(new Row { Index = 503, Param = 252.833328f, NameText = "DH_RunesPageNameText503", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn503" });
            AddRow(new Row { Index = 504, Param = 253.333328f, NameText = "DH_RunesPageNameText504", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn504" });
            AddRow(new Row { Index = 505, Param = 253.833328f, NameText = "DH_RunesPageNameText505", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn505" });
            AddRow(new Row { Index = 506, Param = 254.333328f, NameText = "DH_RunesPageNameText506", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn506" });
            AddRow(new Row { Index = 507, Param = 254.833328f, NameText = "DH_RunesPageNameText507", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn507" });
            AddRow(new Row { Index = 508, Param = 255.333328f, NameText = "DH_RunesPageNameText508", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn508" });
            AddRow(new Row { Index = 509, Param = 255.833328f, NameText = "DH_RunesPageNameText509", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn509" });
            AddRow(new Row { Index = 510, Param = 256.333344f, NameText = "DH_RunesPageNameText510", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn510" });
            AddRow(new Row { Index = 511, Param = 256.833344f, NameText = "DH_RunesPageNameText511", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn511" });
            AddRow(new Row { Index = 512, Param = 257.333344f, NameText = "DH_RunesPageNameText512", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn512" });
            AddRow(new Row { Index = 513, Param = 257.833344f, NameText = "DH_RunesPageNameText513", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn513" });
            AddRow(new Row { Index = 514, Param = 258.333344f, NameText = "DH_RunesPageNameText514", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn514" });
            AddRow(new Row { Index = 515, Param = 258.833344f, NameText = "DH_RunesPageNameText515", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn515" });
            AddRow(new Row { Index = 517, Param = 259.833344f, NameText = "DH_RunesPageNameText517", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn517" });
            AddRow(new Row { Index = 518, Param = 260.333344f, NameText = "DH_RunesPageNameText518", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn518" });
            AddRow(new Row { Index = 519, Param = 260.833344f, NameText = "DH_RunesPageNameText519", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn519" });
            AddRow(new Row { Index = 520, Param = 261.333344f, NameText = "DH_RunesPageNameText520", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn520" });
            AddRow(new Row { Index = 521, Param = 261.833344f, NameText = "DH_RunesPageNameText521", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn521" });
            AddRow(new Row { Index = 522, Param = 262.333344f, NameText = "DH_RunesPageNameText522", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn522" });
            AddRow(new Row { Index = 523, Param = 262.833344f, NameText = "DH_RunesPageNameText523", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn523" });
            AddRow(new Row { Index = 524, Param = 263.333344f, NameText = "DH_RunesPageNameText524", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn524" });
            AddRow(new Row { Index = 525, Param = 263.833344f, NameText = "DH_RunesPageNameText525", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn525" });
            AddRow(new Row { Index = 526, Param = 264.333344f, NameText = "DH_RunesPageNameText526", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn526" });
            AddRow(new Row { Index = 527, Param = 264.833344f, NameText = "DH_RunesPageNameText527", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn527" });
            AddRow(new Row { Index = 528, Param = 265.333344f, NameText = "DH_RunesPageNameText528", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn528" });
            AddRow(new Row { Index = 529, Param = 265.833344f, NameText = "DH_RunesPageNameText529", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn529" });
            AddRow(new Row { Index = 530, Param = 266.333344f, NameText = "DH_RunesPageNameText530", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn530" });
            AddRow(new Row { Index = 531, Param = 266.833344f, NameText = "DH_RunesPageNameText531", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn531" });
            AddRow(new Row { Index = 532, Param = 267.333344f, NameText = "DH_RunesPageNameText532", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn532" });
            AddRow(new Row { Index = 533, Param = 267.833344f, NameText = "DH_RunesPageNameText533", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn533" });
            AddRow(new Row { Index = 535, Param = 268.833344f, NameText = "DH_RunesPageNameText535", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn535" });
            AddRow(new Row { Index = 536, Param = 269.333344f, NameText = "DH_RunesPageNameText536", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn536" });
            AddRow(new Row { Index = 537, Param = 269.833344f, NameText = "DH_RunesPageNameText537", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn537" });
            AddRow(new Row { Index = 538, Param = 270.333344f, NameText = "DH_RunesPageNameText538", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn538" });
            AddRow(new Row { Index = 539, Param = 270.833344f, NameText = "DH_RunesPageNameText539", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn539" });
            AddRow(new Row { Index = 540, Param = 271.333344f, NameText = "DH_RunesPageNameText540", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn540" });
            AddRow(new Row { Index = 541, Param = 271.833344f, NameText = "DH_RunesPageNameText541", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn541" });
            AddRow(new Row { Index = 542, Param = 272.333344f, NameText = "DH_RunesPageNameText542", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn542" });
            AddRow(new Row { Index = 543, Param = 272.833344f, NameText = "DH_RunesPageNameText543", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn543" });
            AddRow(new Row { Index = 544, Param = 273.333344f, NameText = "DH_RunesPageNameText544", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn544" });
            AddRow(new Row { Index = 545, Param = 273.833344f, NameText = "DH_RunesPageNameText545", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn545" });
            AddRow(new Row { Index = 546, Param = 274.333344f, NameText = "DH_RunesPageNameText546", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn546" });
            AddRow(new Row { Index = 547, Param = 274.833344f, NameText = "DH_RunesPageNameText547", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn547" });
            AddRow(new Row { Index = 548, Param = 275.333344f, NameText = "DH_RunesPageNameText548", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn548" });
            AddRow(new Row { Index = 549, Param = 275.833344f, NameText = "DH_RunesPageNameText549", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn549" });
            AddRow(new Row { Index = 550, Param = 276.333344f, NameText = "DH_RunesPageNameText550", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn550" });
            AddRow(new Row { Index = 551, Param = 276.833344f, NameText = "DH_RunesPageNameText551", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn551" });
            AddRow(new Row { Index = 553, Param = 277.833344f, NameText = "DH_RunesPageNameText553", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn553" });
            AddRow(new Row { Index = 554, Param = 278.333344f, NameText = "DH_RunesPageNameText554", NamePic = "D3Atlas35/35_bb_DH6_YeQianDiAn554" });
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
