using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.Input;
using UniverseLib;
using UniverseLib.UI.Models;
using UniverseLib.Utility;
using HarmonyLib;
using UniverseLib.UI.Panels;
using System.IO;
using System.Threading;
using UniverseLib.UI.Widgets;
#if MONO
using BepInEx.Unity.Mono;
#endif
#if CPP
using BepInEx.Unity.IL2CPP;
#endif

namespace ConfigManager.UI
{
    public class ConfigFileInfo
    {
        public ConfigFile RefConfigFile;
        public BepInPlugin Meta;

        internal bool isCompletelyHidden;
        internal ButtonRef listButton;
        internal GameObject navbarGroupObj;
        internal GameObject contentObj;
        internal ButtonRef currentNavButton;
        internal Dictionary<string, ButtonRef> navButtons;
        internal Dictionary<string, GameObject> navSections;

        internal IEnumerable<GameObject> HiddenEntries
            => Entries.Where(it => it.IsHidden).Select(it => it.content);

        internal List<EntryInfo> Entries { get; set; } = new();
    }

    internal class EntryInfo
    {
        public EntryInfo(CachedConfigEntry cached) { Cached = cached; }
        public CachedConfigEntry Cached { get; }
        public ConfigEntryBase RefEntry;
        public bool IsHidden { get; internal set; }

        internal GameObject content;
    }

    public class UIManager : PanelBase
    {
        public static UIManager Instance { get; internal set; }

        public static readonly Dictionary<string, ConfigFileInfo> ConfigFiles = new();
        static readonly Dictionary<ConfigEntryBase, CachedConfigEntry> configsToCached = new();
        static ConfigFileInfo currentCategory;
        private static string firstActiveCategory;

        static readonly HashSet<CachedConfigEntry> editingEntries = new();
        internal static ButtonRef saveButton;

        internal static UIBase uiBase;
        public override string Name => $"<b>{MenuChinese.Text(ConfigManager.NAME)}</b> <i>{ConfigManager.VERSION}</i>";
        public override int MinWidth => 750;
        public override int MinHeight => 750;
        public override Vector2 DefaultAnchorMin => new(0.2f, 0.02f);
        public override Vector2 DefaultAnchorMax => new(0.8f, 0.98f);

        public static string TitleBarTextTemplate = $"<b><color=#8b736b>{I18n.T("PluginName")}</color></b>";
        public static string ToggleKeyLabelTextTemplate = I18n.T("ToggleHint");

        

        public static bool ShowMenu
        {
            get => uiBase != null && uiBase.Enabled;
            set
            {
                if (uiBase == null || !uiBase.RootObject || uiBase.Enabled == value)
                    return;

                UniversalUI.SetUIActive(ConfigManager.GUID, value);
                Instance.SetActive(value);
                if (value && !string.IsNullOrEmpty(firstActiveCategory))
                {
                    SetActiveCategory(firstActiveCategory);
                    firstActiveCategory = "";
                }
            }
        }

        public static bool ShowHiddenConfigs { get; internal set; }

        internal static GameObject CategoryListContent;
        internal static GameObject ConfigNavbarContent;
        internal static GameObject ConfigEditorContent;
        internal static ScrollRect CategoryListScrollRect;
        internal static ScrollRect ConfigNavbarScrollRect;
        internal static ScrollRect ConfigEditorScrollRect;
        internal static AutoSliderScrollbar ConfigEditorScrollbar;

        internal static string Filter => currentFilter ?? "";
        private static string currentFilter;

        private static Color normalInactiveColor = new(0.38f, 0.34f, 0.34f);
        private static Color normalActiveColor = UnityHelpers.ToColor("c2b895");

        public UIManager(UIBase owner) : base(owner)
        {
            Instance = this;
        }

        internal static void Init()
        {
            uiBase = UniversalUI.RegisterUI(ConfigManager.GUID, null);

            CreateMenu();

            // Force refresh of anchors etc
            Canvas.ForceUpdateCanvases();

            ShowMenu = false;

            SetupCategories();

            if (ShowMenu == false && ConfigManager.Auto_Show_Main_Menu.Value)
                ShowMenu = true;
        }

        internal static void SetupCategories()
        {
#if CPP
            ConfigFile coreConfig = ConfigFile.CoreConfig;
#else
            ConfigFile coreConfig = (ConfigFile)AccessTools.Property(typeof(ConfigFile), "CoreConfig").GetValue(null, null);
#endif
            if (coreConfig != null)
                SetupCategory(coreConfig, null, new BepInPlugin("bepinex.core.config", "BepInEx", "1.0"), true, true);

            foreach (CachedConfigFile cachedConfig in Patcher.ConfigFiles)
                ProcessConfigFile(cachedConfig);

            Patcher.ConfigFileCreated += ProcessConfigFile;
            // Existing installed patcher does not expose ConfigFileDestroyed.
        }

        static void ProcessConfigFile(CachedConfigFile cachedConfig)
        {
            RuntimeHelper.StartCoroutine(DelayedProcess(cachedConfig));
        }

        static IEnumerator DelayedProcess(CachedConfigFile cachedConfig)
        {
            yield return null;

            SetupCategory(cachedConfig.configFile, null, cachedConfig.metadata, false);

            cachedConfig.configFile.SettingChanged += ConfigFile_SettingChanged;
        }

        private static void ConfigFile_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            try
            {
                if (sender is ConfigEntryBase entry && configsToCached.TryGetValue(entry, out var cached))
                    cached.OnSettingChanged(sender, e);
            }
            catch (Exception ex)
            {
                ConfigManager.LogSource.LogWarning(ex);
            }
        }

        static void OnConfigFileDestroyed(CachedConfigFile cachedConfig)
        {
            try
            {
                string guid = cachedConfig.metadata?.GUID
                    ?? Path.GetFileNameWithoutExtension(cachedConfig.configFile.ConfigFilePath);
                RemoveCategory(guid, cachedConfig.configFile);
            }
            catch (Exception ex)
            {
                ConfigManager.LogSource.LogWarning($"Exception removing config file: {ex}");
            }
        }

        internal static void RemoveCategory(string guid, ConfigFile configFile)
        {
            if (!ConfigFiles.TryGetValue(guid, out ConfigFileInfo info))
                return;

            // If this is the currently active category, unset it first
            if (currentCategory == info)
                UnsetActiveCategory();

            // Clear firstActiveCategory if it points to this one
            if (firstActiveCategory == guid)
                firstActiveCategory = null;

            // Unsubscribe SettingChanged event
            if (configFile != null)
                configFile.SettingChanged -= ConfigFile_SettingChanged;

            // Clean up editing entries and configsToCached
            foreach (EntryInfo entry in info.Entries)
            {
                editingEntries.Remove(entry.Cached);
                configsToCached.Remove(entry.RefEntry);
                entry.Cached.Destroy();
            }
            info.Entries.Clear();

            // Update save button state
            if (!editingEntries.Any() && saveButton?.Component != null)
                saveButton.Component.interactable = false;

            // Destroy UI GameObjects
            if (info.listButton?.Component?.gameObject)
                GameObject.Destroy(info.listButton.Component.gameObject);
            if (info.navbarGroupObj)
                GameObject.Destroy(info.navbarGroupObj);
            if (info.contentObj)
                GameObject.Destroy(info.contentObj);

            // Remove from dictionary
            ConfigFiles.Remove(guid);
        }

        internal static void SetupCategory(ConfigFile configFile, object plugin, BepInPlugin meta, bool isCoreConfig, bool forceAdvanced = false)
        {
            try
            {
                string GUID = meta?.GUID ?? Path.GetFileNameWithoutExtension(configFile.ConfigFilePath);
                string name = meta?.Name ?? GUID;
                string version = meta?.Version.ToString() ?? "1.0.0";

                if (ConfigFiles.ContainsKey(GUID))
                    return;

#if CPP
                BasePlugin basePlugin = plugin as BasePlugin;
#else
                BaseUnityPlugin basePlugin = plugin as BaseUnityPlugin;
#endif
                if (name is "BepInEx" or "UnityExplorer")
                    forceAdvanced = true;

                if (!forceAdvanced && basePlugin != null)
                {
                    Type type = basePlugin.GetType();
                    if (type.GetCustomAttributes(typeof(BrowsableAttribute), false)
                            .Cast<BrowsableAttribute>()
                            .Any(it => !it.Browsable))
                    {
                        forceAdvanced = true;
                    }
                }

                ConfigFileInfo info = new()
                {
                    RefConfigFile = configFile,
                    Meta = meta,
                };

                // List button

                ButtonRef btn = UIFactory.CreateButton(CategoryListContent, "BUTTON_" + GUID, $"{MenuChinese.Text(name)} <i><color=#3e3a44>v{version}</color></i>");
                btn.OnClick += () => { SetActiveCategory(GUID); };
                UIFactory.SetLayoutElement(btn.Component.gameObject, flexibleWidth: 9999, minHeight: 30, flexibleHeight: 0);

                RuntimeHelper.SetColorBlock(btn.Component, normalInactiveColor, new Color(0.6f, 0.55f, 0.45f),
                    new Color(0.20f, 0.18f, 0.15f));

                info.listButton = btn;

                // Navbar content

                GameObject navbarGroup = UIFactory.CreateHorizontalGroup(ConfigNavbarContent, "NAVBAR_GROUP_" + GUID,
                    true, true, true, true, 2, default, new Color(0.05f, 0.05f, 0.05f));

                // Editor content

                GameObject content = UIFactory.CreateVerticalGroup(ConfigEditorContent, "CATEGORY_" + GUID,
                    true, false, true, true, 4, default, new Color(0.05f, 0.05f, 0.05f));

                Dictionary<string, List<ConfigEntryBase>> dict = new()
                {
                    { "", new List<ConfigEntryBase>() } // make sure the null category is first.
                };

                // Iterate and prepare categories
                foreach (ConfigDefinition entry in configFile.Keys)
                {
                    string sec = entry.Section;
                    if (sec == null)
                        sec = "";

                    if (!dict.ContainsKey(sec))
                        dict.Add(sec, new List<ConfigEntryBase>());

                    dict[sec].Add(configFile[entry]);
                }

                // Create actual entry editors
                info.navButtons = new Dictionary<string, ButtonRef>();
                info.navSections = new Dictionary<string, GameObject>();
                foreach (KeyValuePair<string, List<ConfigEntryBase>> ctg in dict)
                {
                    var section = content;
                    if (!string.IsNullOrEmpty(ctg.Key))
                    {
                        // Config nav button
                        ButtonRef navButton = UIFactory.CreateButton(navbarGroup, "NAV_BUTTON_" + ctg.Key, MenuChinese.Text(ctg.Key));
                        navButton.OnClick += () => { OnNavButtonClicked(info, ctg.Key); };
                        UIFactory.SetLayoutElement(navButton.Component.gameObject, minWidth: 100, flexibleWidth: 0, minHeight: 30);
                        RuntimeHelper.SetColorBlock(navButton.Component, normalInactiveColor, new Color(0.6f, 0.55f, 0.45f),
                            new Color(0.20f, 0.18f, 0.15f));
                        info.navButtons.Add(ctg.Key, navButton);

                        // Config section
                        section = UIFactory.CreateVerticalGroup(content, "ConfigSection_" + ctg.Key,
                            true, false, true, true, 4, default, new Color(0.05f, 0.05f, 0.05f));
                        info.navSections.Add(ctg.Key, section);

                        // Config section title
                        GameObject bg = UIFactory.CreateHorizontalGroup(section, "TitleBG", true, true, true, true, 0, default,
                            new Color(0.07f, 0.07f, 0.07f));
                        Text title = UIFactory.CreateLabel(bg, $"Title_{ctg.Key}", MenuChinese.Text(ctg.Key), TextAnchor.MiddleCenter, default, true, 17);
                        UIFactory.SetLayoutElement(title.gameObject, minHeight: 30, minWidth: 200, flexibleWidth: 9999);
                    }

                    foreach (ConfigEntryBase configEntry in ctg.Value)
                    {
                        CachedConfigEntry cache = new(configEntry, section);

                        object[] tags = configEntry.Description?.Tags;
                        if (tags != null && tags.Any())
                        {
                            foreach (var tag in tags)
                            {
                                if (tag is string strTag)
                                {
                                    if (strTag == "Advanced")
                                        cache.IsAdvanced = true;
                                    else if (strTag == "ReadOnly")
                                        cache.ReadOnly = cache.HideDefaultButton = true;
                                    else if (strTag == "AllowCopy")
                                        cache.AllowCopy = true;
                                    else if (strTag == "HideDefaultButton")
                                        cache.HideDefaultButton = true;
                                }
                                else if ((tag.GetType().Name == "ConfigurationManagerAttributes") && tag is { } attributes)
                                {
                                    cache.IsAdvanced = (bool?)attributes.GetType().GetField("IsAdvanced")?.GetValue(attributes) == true;
                                    cache.ReadOnly = cache.HideDefaultButton = (bool?)attributes.GetType().GetField("ReadOnly")?.GetValue(attributes) == true;
                                    cache.AllowCopy = (bool?)attributes.GetType().GetField("AllowCopy")?.GetValue(attributes) == true;
                                    cache.HideDefaultButton = (bool?)attributes.GetType().GetField("HideDefaultButton")?.GetValue(attributes) == true;
                                }
                            }
                        }

                        if (forceAdvanced)
                            cache.IsAdvanced = true;

                        if (cache.ReadOnly)
                            cache.RefConfig.BoxedValue = cache.RefConfig.DefaultValue;

                        cache.Enable();

                        configsToCached.Add(configEntry, cache);

                        GameObject obj = cache.UIroot;

                        info.Entries.Add(new EntryInfo(cache)
                        {
                            RefEntry = configEntry,
                            content = obj,
                            IsHidden = cache.IsAdvanced
                        });
                    }
                }

                if (info.navButtons.Count > 0)
                    SetActiveNavButton(info, info.navButtons.First().Value.ButtonText.text);

                // hide buttons for completely-hidden categories.
                if (!info.Entries.Any(it => !it.IsHidden))
                {
                    btn.Component.gameObject.SetActive(false);
                    info.isCompletelyHidden = true;
                }

                content.SetActive(false);
                navbarGroup.SetActive(false);

                info.navbarGroupObj = navbarGroup;
                info.contentObj = content;

                ConfigFiles.Add(GUID, info);

                if (currentCategory == null
                    && string.IsNullOrEmpty(firstActiveCategory)
                    && name != ConfigManager.NAME
                    && name != "BepInEx"
                    && name != "UnityExplorer")
                {
                    if (ShowMenu)
                        SetActiveCategory(GUID);
                    else
                        firstActiveCategory = GUID;
                }
            }
            catch (Exception ex)
            {
                if (meta != null)
                    ConfigManager.LogSource.LogWarning($"Exception setting up category '{meta.GUID}'!\r\n{ex}");
                else
                {
                    string name;
                    try { name = Path.GetFileNameWithoutExtension(configFile.ConfigFilePath); }
                    catch { name = "UNKNOWN";  }

                    ConfigManager.LogSource.LogWarning($"Exception setting up category (no meta): {name}\n{ex}");
                }
            }
        }

        // called by UIManager.Init
        internal static void CreateMenu()
        {
            if (Instance != null)
            {
                ConfigManager.LogSource.LogWarning("An instance of ConfigurationEditor already exists, cannot create another!");
                return;
            }

            new UIManager(uiBase);
        }

        public static void OnEntryEdit(CachedConfigEntry entry)
        {
            if (!editingEntries.Contains(entry))
                editingEntries.Add(entry);

            if (!saveButton.Component.interactable)
                saveButton.Component.interactable = true;
        }

        public static void OnEntrySaveOrUndo(CachedConfigEntry entry)
        {
            if (editingEntries.Contains(entry))
                editingEntries.Remove(entry);

            if (!editingEntries.Any())
                saveButton.Component.interactable = false;
        }

        public static void SavePreferences()
        {
            foreach (ConfigFileInfo ctg in ConfigFiles.Values)
            {
                foreach (EntryInfo entry in ctg.Entries)
                    entry.RefEntry.BoxedValue = entry.Cached.EditedValue;

                ConfigFile file = ctg.RefConfigFile;
                if (!file.SaveOnConfigSet)
                    file.Save();
            }

            for (int i = editingEntries.Count - 1; i >= 0; i--)
                editingEntries.ElementAt(i).OnSaveOrUndo();

            editingEntries.Clear();
            saveButton.Component.interactable = false;
        }

        public static void SetHiddenConfigVisibility(bool show)
        {
            if (ShowHiddenConfigs == show)
                return;

            ShowHiddenConfigs = show;

            foreach (KeyValuePair<string, ConfigFileInfo> entry in ConfigFiles)
            {
                ConfigFileInfo info = entry.Value;

                if (info.isCompletelyHidden)
                    info.listButton.Component.gameObject.SetActive(ShowHiddenConfigs);
            }

            if (currentCategory != null && !ShowHiddenConfigs && currentCategory.isCompletelyHidden)
                UnsetActiveCategory();

            RefreshFilter();
        }

        public static void FilterConfigs(string search)
        {
            currentFilter = search.ToLower();
            RefreshFilter();
        }

        internal static void RefreshFilter()
        {
            if (currentCategory == null)
                return;

            foreach (EntryInfo entry in currentCategory.Entries)
            {
                bool val = (string.IsNullOrEmpty(currentFilter) 
                                || entry.RefEntry.Definition.Key.ToLower().Contains(currentFilter)
                                || MenuChinese.Text(entry.RefEntry.Definition.Key).ToLower().Contains(currentFilter)
                                || (MenuChinese.Description(entry.RefEntry.Definition.Section, entry.RefEntry.Definition.Key, entry.RefEntry.Description?.Description)?.ToLower().Contains(currentFilter) ?? false)
                                || (entry.RefEntry.Description?.Description?.Contains(currentFilter) ?? false))
                           && (!entry.IsHidden || ShowHiddenConfigs);

                entry.content.SetActive(val);
            }
        }

        public static void SetActiveCategory(string categoryIdentifier)
        {
            if (!ConfigFiles.ContainsKey(categoryIdentifier))
                return;

            UnsetActiveCategory();

            ConfigFileInfo info = ConfigFiles[categoryIdentifier];

            currentCategory = info;

            GameObject obj = info.contentObj;
            info.navbarGroupObj.SetActive(true);
            obj.SetActive(true);

            ButtonRef btn = info.listButton;
            RuntimeHelper.SetColorBlock(btn.Component, normalActiveColor);

            RefreshFilter();
        }

        internal static void UnsetActiveCategory()
        {
            if (currentCategory == null)
                return;

            RuntimeHelper.SetColorBlock(currentCategory.listButton.Component, normalInactiveColor);
            currentCategory.navbarGroupObj.SetActive(false);
            currentCategory.contentObj.SetActive(false);

            currentCategory = null;
        }

        public static void OnNavButtonClicked(ConfigFileInfo info, string configBlockKey)
        {
            SetActiveNavButton(info, configBlockKey);
            NavigateToConfigSection(info, configBlockKey);
        }

        public static void SetActiveNavButton(ConfigFileInfo info, string sectionKey)
        {
            if (!info.navButtons.ContainsKey(sectionKey))
                return;
            if (info.currentNavButton != null)
            {
                RuntimeHelper.SetColorBlock(info.currentNavButton.Component, normalInactiveColor);
                info.currentNavButton = null;
            }

            var navButton = info.navButtons[sectionKey];
            RuntimeHelper.SetColorBlock(navButton.Component, normalActiveColor);
            info.currentNavButton = navButton;
        }

        public static void NavigateToConfigSection(ConfigFileInfo info, string sectionKey)
        {
            if (!info.navSections.TryGetValue(sectionKey, out var section))
                return;

            ConfigEditorScrollRect.SnapToSection(section.GetComponent<RectTransform>());
        }

        public static void InitScrollRectListeners()
        {
            ConfigEditorScrollRect.onValueChanged.AddListener((Vector2 value) =>
            {
                if (currentCategory == null || currentCategory.navSections == null) return;
                foreach (var pair in currentCategory.navSections)
                {
                    var section = pair.Value;

                    if (ConfigEditorScrollRect.IsInSection(section.GetComponent<RectTransform>()))
                    {
                        SetActiveNavButton(currentCategory, pair.Key);
                        break;
                    }
                }
            });
        }

        public override void SetDefaultSizeAndPosition()
        {
            base.SetDefaultSizeAndPosition();
            this.Rect.localPosition = new Vector3(-this.Rect.rect.width / 2, this.Rect.rect.height / 2, 0);
        }

        protected override void ConstructPanelContent()
        {
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ContentRoot, true, false, true, true);

            ConstructTitleBar();

            ConstructSaveButton();

            ConstructToolbar();

            ConstructEditorViewport();
        }

        protected override void OnClosePanelClicked()
        {
            base.OnClosePanelClicked();

            ShowMenu = false;
        }

        public void SetTitleBarText(string title)
        {
            Text titleText = TitleBar.transform.GetChild(0).GetComponent<Text>();
            titleText.text = title;
        }

        public void SetToggleKeyLabelText(string title)
        {
            Text titleText = TitleBar.transform.GetChild(1).GetComponent<Text>();
            titleText.text = title;
        }

        private void ConstructTitleBar()
        {
            SetTitleBarText(TitleBarTextTemplate);

            Button closeButton = TitleBar.GetComponentInChildren<Button>();
            var closeHolder = closeButton.transform.parent.gameObject;
            var layout = closeHolder.GetComponent<LayoutElement>();
            layout.flexibleWidth = 0f;

            RuntimeHelper.SetColorBlock(closeButton, new(1, 0.2f, 0.2f), new(1, 0.6f, 0.6f), new(0.3f, 0.1f, 0.1f));

            Text hideText = closeButton.GetComponentInChildren<Text>();
            hideText.color = Color.white;
            hideText.resizeTextForBestFit = true;
            hideText.resizeTextMinSize = 8;
            hideText.resizeTextMaxSize = 14;

            Text labelText = UIFactory.CreateLabel(TitleBar, "ToggleKeyLabel", string.Format(ToggleKeyLabelTextTemplate,
                ConfigManager.Main_Menu_Toggle.Value), TextAnchor.MiddleRight);
            labelText.transform.SetSiblingIndex(1);
            UIFactory.SetLayoutElement(labelText.gameObject, minWidth: 120, minHeight: 25, flexibleHeight: 0, flexibleWidth: 9999);
        }

        private void ConstructSaveButton()
        {
            saveButton = UIFactory.CreateButton(ContentRoot, "SaveButton", I18n.T("SavePreferences"));
            saveButton.OnClick += SavePreferences;
            UIFactory.SetLayoutElement(saveButton.Component.gameObject, minHeight: 35, flexibleHeight: 0, flexibleWidth: 9999);
            RuntimeHelper.SetColorBlock(saveButton.Component, new Color(0.1f, 0.3f, 0.1f),
                new Color(0.2f, 0.5f, 0.2f), new Color(0.1f, 0.2f, 0.1f), new Color(0.2f, 0.2f, 0.2f));

            saveButton.Component.interactable = false;

            saveButton.Component.gameObject.SetActive(!ConfigManager.Auto_Save_Configs.Value);
        }

        private void ConstructToolbar()
        {
            GameObject toolbarGroup = UIFactory.CreateHorizontalGroup(ContentRoot, "Toolbar", false, false, true, true, 4, new Vector4(3, 3, 3, 3),
                new Color(0.1f, 0.1f, 0.1f));

            GameObject toggleObj = UIFactory.CreateToggle(toolbarGroup, "HiddenConfigsToggle", out Toggle toggle, out Text toggleText);
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((bool val) =>
            {
                SetHiddenConfigVisibility(val);
            });
            toggleText.text = I18n.T("ShowAdvancedSettings");
            UIFactory.SetLayoutElement(toggleObj, minWidth: 295, minHeight: 25, flexibleHeight: 0, flexibleWidth: 0);

            InputFieldRef inputField = UIFactory.CreateInputField(toolbarGroup, "FilterInput", I18n.T("Search"));
            UIFactory.SetLayoutElement(inputField.Component.gameObject, flexibleWidth: 9999, minHeight: 25);
            inputField.OnValueChanged += FilterConfigs;
        }

        private void ConstructEditorViewport()
        {
            GameObject horiGroup = UIFactory.CreateHorizontalGroup(ContentRoot, "Main", true, true, true, true, 2, default, new Color(0.08f, 0.08f, 0.08f));

            GameObject ctgList = UIFactory.CreateScrollView(horiGroup, "CategoryList", out GameObject ctgContent, out _, new Color(0.1f, 0.1f, 0.1f));
            CategoryListScrollRect = ctgList.GetComponent<ScrollRect>();
            CategoryListScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            UIFactory.SetLayoutElement(ctgList, minWidth: 300, flexibleWidth: 0);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ctgContent, spacing: 3);
            CategoryListContent = ctgContent;

            GameObject configGroup = UIFactory.CreateVerticalGroup(horiGroup, "Config",
                true, false, true, true, 2, default, new Color(0.05f, 0.05f, 0.05f));

            GameObject configNavbar = UIFactory.CreateScrollView(configGroup, "ConfigNavbar", out GameObject configNavbarContent, out AutoSliderScrollbar navScrollbar, new Color(0.1f, 0.1f, 0.1f));
            ConfigNavbarScrollRect = configNavbar.GetComponent<ScrollRect>();
            UIFactory.SetLayoutElement(configNavbar, flexibleWidth: 9999, minHeight: 32, flexibleHeight: 0);
            ConfigNavbarContent = configNavbarContent;

            GameObject editor = UIFactory.CreateScrollView(configGroup, "ConfigEditor", out GameObject editorContent, out AutoSliderScrollbar editorScrollbar, new Color(0.05f, 0.05f, 0.05f));
            ConfigEditorScrollbar = editorScrollbar;
            UIFactory.SetLayoutElement(editor, flexibleWidth: 9999);
            ConfigEditorContent = editorContent;
            ConfigEditorScrollRect = editor.GetComponent<ScrollRect>();
            InitScrollRectListeners();
        }
    }
}
