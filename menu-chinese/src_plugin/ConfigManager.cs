using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if MONO
using BepInEx.Unity.Mono;
#endif
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ConfigManager.UI;
using HarmonyLib;
using UnityEngine;
using UniverseLib.Input;
#if INTEROP
using Il2CppInterop.Runtime.Injection;
#endif

namespace ConfigManager
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ConfigManager
#if MONO
        : BaseUnityPlugin
#else
        : BepInEx.Unity.IL2CPP.BasePlugin
#endif
    {
        public const string GUID = "com.sinai.BepInExConfigManager";
        public const string NAME = "BepInExConfigManager";
        public const string AUTHOR = "Sinai";
        public const string VERSION = "1.3.2";

        public static ConfigManager Instance { get; private set; }

        public static ManualLogSource LogSource =>
#if MONO
            Instance.Logger;
#else
            Instance.Log;
#endif

        internal static Harmony Harmony { get; } = new(GUID);

        // Internal config
        internal const string CTG_ID = "BepInExConfigManager";
        internal static string CTG = "Settings";
        internal static ConfigEntry<KeyCode> Main_Menu_Toggle;
        internal static ConfigEntry<bool> Auto_Show_Main_Menu;
        internal static ConfigEntry<bool> Auto_Save_Configs;
        internal static ConfigEntry<bool> Display_Config_Type;
        internal static ConfigEntry<float> Startup_Delay;
        internal static ConfigEntry<bool> Disable_EventSystem_Override;

#if MONO
        internal void Awake()
        {
            Instance = this;
            Init();
        }

        internal void Update()
        {
            DoUpdate();
        }
#else
        public override void Load()
        {
            Instance = this;

            ClassInjector.RegisterTypeInIl2Cpp<ManagerBehaviour>();
            GameObject obj = new("ManagerBehaviour");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            obj.AddComponent<ManagerBehaviour>();
            Init();
        }

        public class ManagerBehaviour : MonoBehaviour
        {
            public ManagerBehaviour(IntPtr ptr) : base(ptr) { }

            internal void Update()
            {
                DoUpdate();
            }
        }
#endif

        const string IL2CPP_LIBS_PATH =
#if UNHOLLOWER
            "unhollowed"
#else
            "interop"
#endif
            ;

        public static void Init()
        {
            InitConfig();
            LogSource.LogInfo("中文菜单已启用（本地修订 1.3.2）；现有消息、警告和错误继续写入 BepInEx 日志。");

            UniverseLib.Universe.Init(Startup_Delay.Value, LateInit, LogHandler, new()
            {
                Disable_EventSystem_Override = Disable_EventSystem_Override.Value,
                Force_Unlock_Mouse = true,
                Unhollowed_Modules_Folder = Path.Combine(Paths.BepInExRootPath, IL2CPP_LIBS_PATH)
            });
        }

        private static void LateInit()
        {
            UIManager.Init();
        }

        public static void DoUpdate()
        {
            if (UIManager.uiBase == null)
                return;

            if (InputManager.GetKeyDown(Main_Menu_Toggle.Value)
                && !InputManager.GetKey(KeyCode.LeftControl) && !InputManager.GetKey(KeyCode.RightControl)
                && !InputManager.GetKey(KeyCode.LeftShift) && !InputManager.GetKey(KeyCode.RightShift)
                && !InputManager.GetKey(KeyCode.LeftAlt) && !InputManager.GetKey(KeyCode.RightAlt))
                UIManager.ShowMenu = !UIManager.ShowMenu;
        }

        public static void InitConfig()
        {
            Main_Menu_Toggle = Instance.Config.Bind(new ConfigDefinition(CTG, I18n.T("MainMenuToggle")),
                KeyCode.F5,
                new ConfigDescription(I18n.T("MainMenuToggleDesc")));
            Main_Menu_Toggle.SettingChanged += Main_Menu_Toggle_SettingChanged;

            Auto_Show_Main_Menu = Instance.Config.Bind(new ConfigDefinition(CTG, I18n.T("AutoShowMainMenu")),
                true, new ConfigDescription(I18n.T("AutoShowMainMenuDesc")));

            Auto_Save_Configs = Instance.Config.Bind(new ConfigDefinition(CTG, I18n.T("AutoSave")),
                true,
                new ConfigDescription(I18n.T("AutoSaveDesc"), null, "Advanced"));
            Auto_Save_Configs.SettingChanged += Auto_Save_Configs_SettingChanged;

            Display_Config_Type = Instance.Config.Bind(new ConfigDefinition(CTG, I18n.T("DisplayConfigType")),
                false,
                new ConfigDescription(I18n.T("DisplayConfigTypeDesc"), null, "Advanced"));
            Display_Config_Type.SettingChanged += Display_Config_Type_SettingChanged;

            //UI_Scale = ConfigManagerPlugin.Instance.Config.Bind(new ConfigDefinition(CTG, "UI Scale"),
            //    1f,
            //    new ConfigDescription("The scale of the UI elements", new AcceptableValueRange<float>(0.75f, 1.25f)));
            //
            //UI_Scale.SettingChanged += UiScale_SettingChanged;

            Startup_Delay = Instance.Config.Bind(CTG, I18n.T("StartupDelay"), 1f,
                new ConfigDescription(I18n.T("StartupDelayDesc"), null, "Advanced"));

            Disable_EventSystem_Override = Instance.Config.Bind(CTG, I18n.T("DisableEventSystemOverride"), false,
                new ConfigDescription(I18n.T("DisableEventSystemOverrideDesc"), null, "Advanced"));
            Disable_EventSystem_Override.SettingChanged += Disable_EventSystem_Override_SettingChanged;

            // InitTest();
        }

        private static void Main_Menu_Toggle_SettingChanged(object sender, EventArgs e)
        {
            UIManager.Instance?.SetToggleKeyLabelText(string.Format(I18n.T("ToggleHint"), Main_Menu_Toggle.Value));
        }

        private static void Display_Config_Type_SettingChanged(object sender, EventArgs e)
        {
            foreach (var pair in UIManager.ConfigFiles)
            {
                foreach (var valueEntry in pair.Value.Entries)
                {
                    var config = valueEntry.Cached;
                    config.mainLabel.text = MenuChinese.Text(config.RefConfig.Definition.Key);
                    if (Display_Config_Type.Value)
                        config.mainLabel.text += $" <i>({MenuChinese.SettingType(config.RefConfig.SettingType)})</i>";
                    if (config.IsAdvanced)
                        config.mainLabel.text += $" <i>(<color=#da2c43>{I18n.T("Advanced")}</color>)</i>";
                }
            }
        }

        private static void Disable_EventSystem_Override_SettingChanged(object sender, EventArgs e)
        {
            bool val = (bool)(e as SettingChangedEventArgs).ChangedSetting.BoxedValue;
            UniverseLib.Config.ConfigManager.Disable_EventSystem_Override = val;
        }

        private static void Auto_Save_Configs_SettingChanged(object sender, EventArgs e)
        {
            bool val = (bool)(e as SettingChangedEventArgs).ChangedSetting.BoxedValue;
            UIManager.saveButton.Component.gameObject.SetActive(!val);
        }

        //private static void UiScale_SettingChanged(object sender, EventArgs e)
        //{
        //    float scale = (float)(e as SettingChangedEventArgs).ChangedSetting.BoxedValue;
        //    ConfigUIManager.CanvasRoot.GetComponent<Canvas>().scaleFactor = scale;
        //}

        private static void LogHandler(string log, LogType logType)
        {
            switch (logType)
            {
                case LogType.Log:
                    LogSource.LogMessage(log);
                    return;
                case LogType.Warning:
                case LogType.Assert:
                    LogSource.LogWarning(log);
                    return;
                case LogType.Error:
                case LogType.Exception:
                    LogSource.LogError(log);
                    return;
            }
        }

        ////  ~~~~~~~~~~~~~~~~ TEST CONFIG ~~~~~~~~~~~~~~~~

        //static void InitTest()
        //{
        //    TomlTypeConverter.AddConverter(typeof(TestConfigClass), new TypeConverter()
        //    {
        //        ConvertToObject = (string s, Type t) =>
        //        {
        //            var split = s.Split(',');
        //            return new TestConfigClass() { myInt1 = int.Parse(split[0]), myInt2 = int.Parse(split[1]) };
        //        },
        //        ConvertToString = (object o, Type t) =>
        //        {
        //            var x = (TestConfigClass)o;
        //            return $"{x.myInt1},{x.myInt2}";
        //        }
        //    });
        //    TomlTypeConverter.AddConverter(typeof(Color), new TypeConverter()
        //    {
        //        ConvertToObject = (string s, Type t) =>
        //        {
        //            var split = s.Split(',');
        //            var c = new CultureInfo("en-US");
        //            return new Color()
        //            {
        //                r = float.Parse(split[0], c),
        //                g = float.Parse(split[1], c),
        //                b = float.Parse(split[2], c),
        //                a = float.Parse(split[3], c)
        //            };
        //        },
        //        ConvertToString = (object o, Type t) =>
        //        {
        //            var x = (Color)o;
        //            return string.Format(new CultureInfo("en-US"), "{0},{1},{2},{3}",
        //                x.r, x.g, x.b, x.a);
        //        }
        //    });

        //    string ctg1 = "Category One";
        //    string ctg2 = "Category Two";
        //    var file = ConfigManagerPlugin.Instance.Config;

        //    file.Bind(new ConfigDefinition(ctg1, "Advanced setting 1"), true, new ConfigDescription("", null, "Advanced"));
        //    file.Bind(new ConfigDefinition(ctg1, "Advanced setting 2"), true, new ConfigDescription("", null, 
        //        new ConfigurationManagerAttributes() { IsAdvanced = true }));

        //    file.Bind(new ConfigDefinition(ctg1, "ValueList"), "One", new ConfigDescription("",
        //        new AcceptableValueList<string>("One", "Two", "Three", "Four")));

        //    file.Bind(new ConfigDefinition(ctg1, "This is a setting name"), true, new ConfigDescription("This is a description\r\nwith a new line"));
        //    file.Bind(new ConfigDefinition(ctg1, "Take a byte"), (byte)0xD, new ConfigDescription("Bytes have a max value of 255"));
        //    file.Bind(new ConfigDefinition(ctg1, "Int slider"), 32, new ConfigDescription("You can use sliders for any number type",
        //        new AcceptableValueRange<int>(0, 100)));
        //    file.Bind(new ConfigDefinition(ctg1, "Float slider"), 666.6f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1000f)));
        //    file.Bind(new ConfigDefinition(ctg1, "Key binding"), KeyCode.Dollar, new ConfigDescription("Keybinds have a special rebind helper"));

        //    file.Bind(new ConfigDefinition(ctg2, "Enum dropdown"), CameraClearFlags.SolidColor, new ConfigDescription("Enums use a dropdown"));
        //    file.Bind(new ConfigDefinition(ctg2, "Color picker"), Color.magenta, new ConfigDescription("Colors use a special color picker"));
        //    file.Bind(new ConfigDefinition(ctg2, "Multiline input"), "Hello,\r\nworld!", new ConfigDescription("Strings use a multi-line input field"));
        //    //file.Bind(new ConfigDefinition(ctg2, "Float structs"), Vector3.up, new ConfigDescription("Float-structs use an editor like this"));
        //    file.Bind(new ConfigDefinition(ctg2, "Flag toggles"), BindingFlags.Public, new ConfigDescription("Enums with [Flags] attribute use a multi-toggle"));
        //    file.Bind(new ConfigDefinition(ctg2, "Custom type"), new TestConfigClass() { myInt1 = 25, myInt2 = 50 },
        //        new ConfigDescription("Custom types are supported with a basic Toml input, if a Converter was registered to TypeConverter."));
        //}

        //public struct TestConfigClass
        //{
        //    public int myInt1;
        //    public int myInt2;
        //}

        ////  ~~~~~~~~~~~~~~~~ END TEST CONFIG ~~~~~~~~~~~~~~~~
    }
}