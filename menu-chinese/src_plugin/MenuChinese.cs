using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ConfigManager
{
    /// <summary>Display-only translations; configuration definitions and values remain unchanged.</summary>
    public static class MenuChinese
    {
        private static readonly Dictionary<string, string> Labels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "BepInExConfigManager", "模组配置管理器" },
            { "KingdomMod.OverlayMap", "地图与统计" },
            { "KingdomMod.StaminaBar", "坐骑状态条" },
            { "KingdomMod.SharedLib", "模组共享库" },
            { "自由换岛 FreeTravel", "自由换岛" },
            { "BepInEx", "BepInEx 加载器" },
            { "Settings", "菜单设置" },
            { "Global", "全局设置" },
            { "Language", "语言" },
            { "MarkerStyleFile", "地图标记样式文件" },
            { "GuiUpdatesPerSecond", "界面每秒刷新次数" },
            { "system", "跟随系统" },
            { "zh-CN", "简体中文" },
            { "en-US", "英语（美国）" },
            { "Language to use", "选择地图界面语言；跟随系统会使用系统语言。" },
            { "Increase to be more accurate, decrease to reduce performance impact", "提高刷新次数可提升准确性，降低可减少性能开销。" },
            { "Caching", "缓存" },
            { "Detours", "函数挂钩" },
            { "Harmony.Logger", "Harmony 日志" },
            { "IL2CPP", "IL2CPP 互操作" },
            { "Logging", "日志设置" },
            { "Logging.Console", "控制台日志" },
            { "Logging.Disk", "文件日志" },
            { "Preloader", "预加载器" },
            { "EnableAssemblyCache", "启用程序集缓存" },
            { "DetourProviderType", "函数挂钩后端" },
            { "LogChannels", "日志通道" },
            { "UpdateInteropAssemblies", "自动更新互操作程序集" },
            { "UnityBaseLibrariesSource", "Unity 基础库来源" },
            { "UnhollowerDeobfuscationRegex", "反混淆正则表达式" },
            { "ScanMethodRefs", "扫描方法引用" },
            { "DumpDummyAssemblies", "导出占位程序集" },
            { "IL2CPPInteropAssembliesPath", "互操作程序集目录" },
            { "PreloadIL2CPPInteropAssemblies", "预载互操作程序集" },
            { "GlobalMetadataPath", "全局元数据路径" },
            { "UnityLogListening", "监听游戏引擎日志" },
            { "Enabled", "启用" },
            { "PreventClose", "禁止关闭控制台" },
            { "ShiftJisEncoding", "使用 Shift-JIS 编码" },
            { "StandardOutType", "标准输出方式" },
            { "LogLevels", "日志级别" },
            { "AppendLog", "启动时追加日志" },
            { "InstantFlushing", "立即写入日志" },
            { "ConcurrentFileLimit", "同时写入日志文件上限" },
            { "WriteUnityLog", "写入游戏引擎日志" },
            { "HarmonyBackend", "Harmony 后端" },
            { "DumpAssemblies", "导出已修补程序集" },
            { "LoadDumpedAssemblies", "加载已导出程序集" },
            { "BreakBeforeLoadAssemblies", "加载程序集前暂停调试" },
            { "empty", "空字符串" },
            { "null", "空值" },
            { "Enable/disable assembly metadata cache\nEnabling this will speed up discovery of plugins and patchers by caching the metadata of all types BepInEx discovers.", "启用或禁用程序集元数据缓存。启用后会缓存类型信息，加快模组与补丁发现速度。" },
            { "The native provider to use for managed detours", "为托管函数挂钩选择原生后端。" },
            { "Specifies which Harmony log channels to listen to.\nNOTE: IL channel dumps the whole patch methods, use only when needed!", "选择需要监听的 Harmony 日志通道。IL 通道会导出完整补丁方法，仅在需要排错时开启。" },
            { "Enables showing unity log messages in the BepInEx logging system.", "将游戏引擎消息传入 BepInEx 日志系统。" },
            { "Enables showing a console for log output.", "显示用于日志输出的控制台窗口。" },
            { "If enabled, will prevent closing the console (either by deleting the close button or in other platform-specific way).", "启用后禁止关闭日志控制台。" },
            { "If true, console is set to the Shift-JIS encoding, otherwise UTF-8 encoding.", "启用后控制台使用 Shift-JIS 编码，关闭时使用 UTF-8。" },
            { "Which log levels to show in the console output.", "选择控制台输出的日志级别。" },
            { "Appends to the log file instead of overwriting, on game startup.", "游戏启动时追加到已有日志文件，关闭时覆盖上一轮日志。" },
            { "Enables writing log messages to disk.", "启用将日志写入游戏目录下的 BepInEx/LogOutput.log。" },
            { "Only displays the specified log levels in the disk log output.", "选择写入文件的日志级别。" },
            { "If true, instantly writes any received log entries to disk.\nThis incurs a major performance hit if a lot of log messages are being written, however it is really useful for debugging crashes.", "立即把日志写入磁盘。大量日志会增加性能开销，但有助于排查崩溃。" },
            { "The maximum amount of concurrent log files that will be written to disk.\nAs one log file is used per open game instance, you may find it necessary to increase this limit when debugging multiple instances at the same time.", "同时写入磁盘的日志文件上限。每个游戏实例使用一个文件，多实例排错时可适当提高。" },
            { "Include unity log messages in log file output.", "把游戏引擎日志一并写入日志文件。" },
            { "If enabled, BepInEx will save patched assemblies into BepInEx/DumpedAssemblies.\nThis can be used by developers to inspect and debug preloader patchers.", "将已修补程序集保存到 BepInEx/DumpedAssemblies，供开发者检查和调试预加载补丁。" },
            { "If enabled, BepInEx will load patched assemblies from BepInEx/DumpedAssemblies instead of memory.\nIf set to true, will override DumpAssemblies.", "从 BepInEx/DumpedAssemblies 加载已修补程序集；启用时覆盖导出程序集选项。" },
            { "If enabled, BepInEx will call Debugger.Break() once before loading patched assemblies.\nThis can be used to be able to load patched assemblies into debuggers like dnSpy.", "加载已修补程序集前暂停一次，方便使用 dnSpy 等调试器。" }
        };

        private static readonly Dictionary<string, string> CoreDescriptions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "UpdateInteropAssemblies", "游戏或加载器更新后自动生成 IL2CPP 互操作程序集。关闭后不会自动更新引用。" },
            { "UnityBaseLibrariesSource", "Unity 基础库压缩包来源，可使用 {VERSION} 占位符。也可填写本地压缩包文件名。" },
            { "UnhollowerDeobfuscationRegex", "用于识别和重命名混淆类型、成员的正则表达式。" },
            { "ScanMethodRefs", "扫描交叉引用以识别无效方法并生成调用数量属性。" },
            { "DumpDummyAssemblies", "保存 IL2CPP 导出的占位程序集到 BepInEx/dummy。" },
            { "IL2CPPInteropAssembliesPath", "互操作程序集存放路径，支持 {BepInEx} 和 {ProcessName} 占位符。" },
            { "PreloadIL2CPPInteropAssemblies", "加载模组前预载互操作程序集，部分模组需要此选项。" },
            { "GlobalMetadataPath", "IL2CPP 元数据文件路径，支持 {BepInEx}、{ProcessName}、{GameDataPath} 占位符。" },
            { "StandardOutType", "选择标准输出重定向方式。Auto 自动选择；ConsoleOut 优先控制台输出；StandardOut 优先标准输出。" },
            { "LoadDumpedAssemblies", "从 BepInEx/DumpedAssemblies 加载已修补程序集，以便使用调试器。启用时覆盖导出程序集选项。" },
            { "BreakBeforeLoadAssemblies", "加载已修补程序集前暂停一次，方便使用 dnSpy 等调试器安装断点。" },
            { "HarmonyBackend", "选择 Harmony 修补使用的 MonoMod 后端。auto 自动选择；其他选项主要供开发者调试。" }
        };

        public static string Text(string text)
        {
            if (text == null) { return null; }
            if (Labels.TryGetValue(text.Replace("\r\n", "\n").Trim(), out var translated)) { return translated; }
            if (MenuTerms.Labels.TryGetValue(text, out translated)) { return translated; }
            var archive = Regex.Match(text, @"^island-v\d+-c(\d+)-l(\d+)$");
            if (archive.Success && int.TryParse(archive.Groups[1].Value, out var campaign) && int.TryParse(archive.Groups[2].Value, out var land))
            {
                return string.Format(CultureInfo.InvariantCulture, "战役 {0} · 第 {1} 岛", (long)campaign + 1, (long)land + 1);
            }
            if (text.StartsWith("GuiStyle.", StringComparison.Ordinal)) { return "界面样式（" + Text(text.Substring(9)) + "）"; }
            if (text.StartsWith("Language.", StringComparison.Ordinal)) { return "地图名称（" + Text(text.Substring(9)) + "）"; }
            var parts = text.Split('.');
            if (parts.Length > 1)
            {
                var words = new List<string>();
                foreach (var part in parts)
                {
                    if (!MenuTerms.Labels.TryGetValue(part, out var word)) { return text; }
                    words.Add(word);
                }
                return string.Join(" · ", words);
            }
            return text;
        }

        public static string Description(string section, string key, string original)
        {
            if (section == "IL2CPP" || section == "Preloader" || (section == "Logging.Console" && key == "StandardOutType"))
            {
                if (CoreDescriptions.TryGetValue(key, out var translated)) { return translated; }
            }
            return Text(original);
        }

        public static string Bool(bool value) { return value ? "开启" : "关闭"; }

        private static readonly Dictionary<string, string> Options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "None", "无" }, { "Default", "默认" }, { "Auto", "自动" }, { "auto", "自动" },
            { "Fatal", "致命错误" }, { "Error", "错误" }, { "Warning", "警告" }, { "Warn", "警告" },
            { "Message", "消息" }, { "Info", "信息" }, { "Debug", "调试" }, { "All", "全部" },
            { "IL", "中间语言" }, { "ConsoleOut", "控制台输出" }, { "StandardOut", "标准输出" },
            { "Dobby", "Dobby 后端" }, { "Funchook", "Funchook 后端" },
            { "dynamicmethod", "动态方法" }, { "methodbuilder", "方法构建器" }, { "cecil", "Cecil 后端" }
        };

        public static string Option(string value)
        {
            if (value == null) { return null; }
            var parts = value.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                if (!Options.TryGetValue(parts[i].Trim(), out var translated)) { return Text(value); }
                parts[i] = translated;
            }
            return string.Join("、", parts);
        }

        public static string ColorChannel(string name)
        {
            switch (name.ToUpperInvariant())
            {
                case "R": return "红";
                case "G": return "绿";
                case "B": return "蓝";
                case "A": return "透明度";
                default: return name;
            }
        }

        public static string SettingType(Type type)
        {
            if (type == typeof(bool)) { return "开关"; }
            if (type == typeof(string)) { return "文字"; }
            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) { return "小数"; }
            if (type.IsPrimitive) { return "整数"; }
            if (type.Name == "KeyCode" || type.Name == "Key") { return "按键"; }
            if (type.IsEnum) { return "选项"; }
            if (type.Name == "Color" || type.Name == "Color32") { return "颜色"; }
            if (type.Name.StartsWith("Vector", StringComparison.Ordinal)) { return "向量"; }
            if (type.Name == "Quaternion") { return "旋转"; }
            return "自定义数据";
        }
    }
}
