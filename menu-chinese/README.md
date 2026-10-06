# 模组菜单完整汉化 · MenuChinese 1.3.2

《王国：两位君主》Windows IL2CPP 环境使用的 BepInExConfigManager 本地中文修订版。这个版本是**菜单组件 1.3.2**，不代表 FreeTravel 换岛模组的版本；FreeTravel 当前仍是 v0.1.1。

基于 [sinai-dev/BepInExConfigManager](https://github.com/sinai-dev/BepInExConfigManager) 和 [abevol 的维护分支](https://github.com/abevol/BepInExConfigManager)，维护分支基线提交为 `c77288236512de6f943c62e868f00919b685f1ad`。本地修改于 2026-10-06，保留上游署名，按附带的 GPL-3.0 许可提供完整修改源码。这不是上游官方 1.3.2 发行版。

## 汉化内容

- 菜单标题、模组名称、配置分组、选项名称和说明。
- 界面样式、地图名称、地图标记样式和存档附加数据，共 177 个地图相关显示词条。
- `MapOffset`、`ZoomScale`、探索左右边界、时间、天数，以及动态战役／岛屿分组。
- 布尔值、日志级别、颜色通道、设置类型和语言下拉选项。
- 搜索支持中文显示名称／说明，同时保留原配置键搜索。

汉化只改变显示文字。配置键、保存值、枚举解析、文件名、路径、字体资源名、快捷键和用户填写的字符串内容保留原数据。

## 日志

已有消息、警告与错误日志，沿用 BepInEx 日志系统。启用文件日志后可查看游戏根目录下 `BepInEx/LogOutput.log`。本机该功能已经启用；菜单加载时会记录“中文菜单已启用（本地修订 1.3.2）”。

## 安装与升级

此包用于**已安装兼容 BepInEx 6 IL2CPP、BepInExConfigManager 和 UniverseLib 的环境**。本机使用 Unity 6 兼容加载环境及原有配置管理器 patcher。压缩包只替换菜单 DLL，不包含或升级加载器、UniverseLib、patcher、游戏程序集、字体、其他模组或存档。

1. 保存并退出游戏，备份配置和原菜单 DLL。
2. 下载菜单汉化安装包，把 `BepInEx` 文件夹合并到游戏根目录。
3. 覆盖下列文件，不要在其他目录同时放置另一份菜单 DLL：

```text
BepInEx/plugins/BepInExConfigManager/BepInExConfigManager.Il2Cpp.CoreCLR.dll
```

4. 启动游戏，打开模组设置面板。本机绑定 F1；其他环境以自己的设置为准。
5. 原配置与热键继续使用，不需要删除配置文件。

回退时退出游戏，把备份的原 DLL 放回同一路径。停止使用汉化组件不会修改游戏存档。

## 源码构建与检查

需要 PowerShell 7、.NET SDK，以及已生成互操作引用的本地游戏环境：

```powershell
./scripts/test.ps1
./scripts/test.ps1 -GameDirectory "D:\SteamLibrary\steamapps\common\Kingdom Two Crowns"
./scripts/build.ps1 -GameDirectory "D:\SteamLibrary\steamapps\common\Kingdom Two Crowns"
```

不提供 GameDirectory 时仅检查翻译规则；提供路径后还会检查该环境配置中是否存在未覆盖的分组、字段和说明。构建输出到 `dist`，不会自动安装。

源码保留上游项目文件供参考；随附 build.ps1 直接使用本机游戏依赖编译，无需运行上游 csproj。

## 验证范围

本机 Windows Steam 游戏 2.4.2 / Unity 6 / IL2CPP：覆盖检查、完整编译和安装哈希通过，启动加载成功，检查时日志 Error 为 0；菜单、地图和加载器配置键／值在重启后保持一致。用户此前截图确认菜单主体已中文化；本次补齐字段后的全部页面视觉和交互尚未逐页确认。

其他版本、其他游戏、所有 DLC 与新安装的第三方模组不保证完整覆盖。新模组带来的未知文字会保留原显示，不修改它们的配置。

## 文件范围

- `src_plugin/`：本组件对应源码，包含上游与本地修改。
- `scripts/`：本地构建与翻译检查。
- `LICENSE`：上游 GPL-3.0。
- `docs/CHANGELOG.md`：本地汉化修订记录。
