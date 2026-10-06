using System.Collections.Generic;
using System.Globalization;
using BepInEx.Configuration;

namespace ConfigManager
{
    public static class I18n
    {
        public static string CurrentLanguage { get; set; } = "en";

        private static readonly Dictionary<string, Dictionary<string, string>> Translations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "en", new Dictionary<string, string>
                {
                    { "MainMenuToggle", "Main Menu Toggle" },
                    { "MainMenuToggleDesc", "The toggle for the Config Manager menu" },
                    { "AutoSave", "Auto-save settings" },
                    { "AutoSaveDesc", "Automatically save settings after changing them? This will mean the undo feature will be unavailable." },
                    { "AutoShowMainMenu", "Auto-show Main Menu" },
                    { "AutoShowMainMenuDesc", "Automatically show the main menu after the game starts." },
                    { "DisplayConfigType", "Display Config Type" },
                    { "DisplayConfigTypeDesc", "Show configuration type in the setting title bar." },
                    { "StartupDelay", "Startup Delay" },
                    { "StartupDelayDesc", "Delays the core startup process. Adjust it if you experience issues." },
                    { "DisableEventSystemOverride", "Disable EventSystem Override" },
                    { "DisableEventSystemOverrideDesc", "Disables the overriding of the EventSystem from the game, if you experience issues with UI Input." },
                    { "Advanced", "Advanced" },
                    { "Undo", "Undo" },
                    { "Default", "Default" },
                    { "Rebind", "Rebind" },
                    { "Confirm", "Confirm" },
                    { "Cancel", "Cancel" },
                    { "Clear", "Clear" },
                    { "Copy", "Copy" },
                    { "Copied", "Copied" },
                    { "PressAKey", "Press a key..." },
                    { "NotSet", "<notset>" },
                    { "ExpandToEdit", "▲ Expand to edit" },
                    { "ClickToHide", "▼ Click to hide" },
                    { "ShowAdvancedSettings", "Show Advanced Settings" },
                    { "Search", "Search..." },
                    { "SavePreferences", "Save Preferences" },
                    { "PluginName", "BepInExConfigManager" },
                    { "ToggleHint", "<color=#524c5a>[ {0} ] Show/Hide</color>" }
                }
            },
            {
                "zh", new Dictionary<string, string>
                {
                    { "MainMenuToggle", "显示主菜单" },
                    { "MainMenuToggleDesc", "绑定一个热键用来呼出模组配置管理器菜单" },
                    { "AutoSave", "自动保存设置" },
                    { "AutoSaveDesc", "更改后自动保存设置，这意味着撤消功能将不可用。" },
                    { "AutoShowMainMenu", "自动显示主菜单" },
                    { "AutoShowMainMenuDesc", "游戏启动后自动显示主菜单" },
                    { "DisplayConfigType", "显示设置类型" },
                    { "DisplayConfigTypeDesc", "在设置选项标题栏显示设置类型。" },
                    { "StartupDelay", "延迟启动" },
                    { "StartupDelayDesc", "延迟核心启动过程。 如果您遇到问题，请进行调整。" },
                    { "DisableEventSystemOverride", "禁用事件系统覆盖" },
                    { "DisableEventSystemOverrideDesc", "如果您遇到 UI 输入问题，请禁用游戏中的事件系统覆盖。" },
                    { "Advanced", "高级" },
                    { "Undo", "撤销" },
                    { "Default", "默认值" },
                    { "Rebind", "重新绑定" },
                    { "Confirm", "确认" },
                    { "Cancel", "取消" },
                    { "Clear", "清除" },
                    { "Copy", "复制" },
                    { "Copied", "已复制" },
                    { "PressAKey", "按下一个键..." },
                    { "NotSet", "<未设置>" },
                    { "ExpandToEdit", "▲ 展开以编辑" },
                    { "ClickToHide", "▼ 点击以隐藏" },
                    { "ShowAdvancedSettings", "显示高级设置" },
                    { "Search", "搜索..." },
                    { "SavePreferences", "保存配置" },
                    { "PluginName", "模组配置管理器" },
                    { "ToggleHint", "<color=#524c5a>[ {0} 键 ] 显示/隐藏</color>" }
                }
            },
            {
                "ru", new Dictionary<string, string>
                {
                    { "MainMenuToggle", "Переключатель главного меню" },
                    { "MainMenuToggleDesc", "Переключатель для меню менеджера конфигураций" },
                    { "AutoSave", "Автосохранение настроек" },
                    { "AutoSaveDesc", "Автоматически сохранять настройки после их изменения? Это сделает функцию отмены недоступной." },
                    { "AutoShowMainMenu", "Автопоказ главного меню" },
                    { "AutoShowMainMenuDesc", "Автоматически показывать главное меню после запуска игры." },
                    { "DisplayConfigType", "Отображать тип конфигурации" },
                    { "DisplayConfigTypeDesc", "Показывать тип конфигурации в строке заголовка настройки." },
                    { "StartupDelay", "Задержка запуска" },
                    { "StartupDelayDesc", "Задерживает основной процесс запуска. Настройте, если возникают проблемы." },
                    { "DisableEventSystemOverride", "Отключить переопределение EventSystem" },
                    { "DisableEventSystemOverrideDesc", "Отключает переопределение EventSystem игры. Используйте, если возникают проблемы с вводом в интерфейсе." },
                    { "Advanced", "Расширенные" },
                    { "Undo", "Отменить" },
                    { "Default", "По умолчанию" },
                    { "Rebind", "Переназначить" },
                    { "Confirm", "Подтвердить" },
                    { "Cancel", "Отмена" },
                    { "Clear", "Очистить" },
                    { "Copy", "Копировать" },
                    { "Copied", "Скопировано" },
                    { "PressAKey", "Нажмите клавишу..." },
                    { "NotSet", "<не задано>" },
                    { "ExpandToEdit", "▲ Развернуть для редактирования" },
                    { "ClickToHide", "▼ Нажмите, чтобы скрыть" },
                    { "ShowAdvancedSettings", "Показать расширенные настройки" },
                    { "Search", "Поиск..." },
                    { "SavePreferences", "Сохранить настройки" },
                    { "PluginName", "BepInExConfigManager" },
                    { "ToggleHint", "<color=#524c5a>[ {0} ] Показать/Скрыть</color>" }
                }
            }
        };

        static I18n()
        {
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
            if (Translations.ContainsKey(culture))
            {
                CurrentLanguage = culture;
            }
            else
            {
                CurrentLanguage = "en";
            }
        }

        public static string Translate(string key, params object[] args)
        {
            if (Translations.TryGetValue(CurrentLanguage, out var langDict) && langDict.TryGetValue(key, out var text))
            {
                if (args != null && args.Length > 0)
                {
                    return string.Format(text, args);
                }
                return text;
            }
            
            // Fallback to English
            if (Translations["en"].TryGetValue(key, out var fallbackText))
            {
                 if (args != null && args.Length > 0)
                 {
                     return string.Format(fallbackText, args);
                 }
                 return fallbackText;
            }

            return key;
        }

        public static string T(string key, params object[] args) => Translate(key, args);
    }
}
