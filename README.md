# 自由换岛 · Kingdom Two Crowns FreeTravel

《王国：两位君主》电脑版的独立 BepInEx 模组。在模组设置面板选择目标岛屿，即可调用游戏原有流程换岛，无需造船或走到码头。

**Free island travel for Kingdom Two Crowns on Windows. Select a destination in the mod configuration panel and travel without building a boat.**

当前版本：**v0.1.1 测试版**（修复未就绪船只导致的换岛卡住）。

本仓库还提供独立的 **菜单汉化 1.3.2**：[下载菜单汉化版](https://github.com/ClosedFlux/KingdomTwoCrowns-FreeTravel/releases/tag/menu-zh-v1.3.2) · [源码、安装和许可说明](menu-chinese/README.md)。菜单组件使用 `menu-zh-v1.3.2` 标签，与 FreeTravel 的 `v0.1.1` 分开编号和打包；仅需要自由换岛时继续下载下面的 FreeTravel 安装包。

## 下载

[下载 v0.1.1 安装包](https://github.com/ClosedFlux/KingdomTwoCrowns-FreeTravel/releases/tag/v0.1.1) · [历史 v0.1.0](https://github.com/ClosedFlux/KingdomTwoCrowns-FreeTravel/releases/tag/v0.1.0) · [版本说明](CHANGELOG.md) · [反馈问题](https://github.com/ClosedFlux/KingdomTwoCrowns-FreeTravel/issues)

v0.1.1 是新的独立版本，保留 v0.1.0 下载。旧版用户保存退出游戏后，用新版覆盖同一路径的 DLL 即可；不要在其他目录同时保留旧版 DLL，无需清空存档或配置。

## 功能

- 按岛屿编号选择目标，编号从 **1** 开始。
- 允许请求前往未解锁岛屿，越界编号会拒绝执行。
- 使用游戏原有 `Game.SailAway` 接口，不通过手工改写存档切换岛屿。
- 执行开关只触发一次，随后自动复位，避免启动时重复换岛。
- 检查游玩状态、是否允许保存、重复请求和同岛请求。
- 加载完成后，同时核对游戏和战役的当前岛屿编号。
- 第一版开放单人与本地双人，在线联机会拒绝执行。

## 已验证与适用范围

| 项目 | 状态 |
| --- | --- |
| Windows Steam 版 2.4.2、Unity 6、IL2CPP | 本机编译与加载通过 |
| 换岛条件与船只选择检查 | 9 + 5 项通过 |
| 第 4 岛 → 已访问的第 5 岛，船处于 Pushing | v0.1.1 修复后实机成功，玩家确认 |
| 第 4 岛 → 第 2 岛 → 第 4 岛 | 实际游戏往返成功 |
| 没有船时换岛 | 实际游戏成功 |
| 返回原岛后的建筑与进度 | 玩家确认保留 |
| 未解锁岛屿 | 代码允许请求，尚未专项实测 |
| 本地双人 | 代码开放，尚未专项实测 |
| 随行人员 | 仅已航行船使用 Boats，其余使用 None；队伍完整性尚未专项实测 |
| 在线联机、其他游戏版本与 DLC | 当前没有通过兼容性验证 |

随行组不是整个岛上的军队集合。本模组没有额外收集未登船人员的逻辑。未航行船使用 None 路径，不携带随船队伍，也不会强制修改船的状态。

## 安装

需要 Windows IL2CPP 版游戏、**兼容游戏版本的 BepInEx 6 IL2CPP 加载器**及 **BepInExConfigManager** 设置面板。Unity 6 环境须使用相应兼容版本或补丁。

1. 保存并退出游戏，备份游戏存档。
2. 确认 BepInEx 和设置面板已正常工作，并已启动过一次游戏生成互操作引用。
3. 从 Releases 下载 `KingdomTwoCrowns-FreeTravel-v0.1.1.zip`。
4. 将压缩包中的 `BepInEx` 文件夹合并到游戏根目录。
5. 确认最终路径为：

```text
Kingdom Two Crowns/
├─ KingdomTwoCrowns.exe
└─ BepInEx/
   └─ plugins/
      └─ KingdomMod.FreeTravel/
         └─ KingdomMod.FreeTravel.dll
```

安装包只包含此模组，加载器、游戏文件及其他模组需另行准备。

## 使用

1. 进入战役，回到正常游玩状态。
2. 打开模组设置面板。本项目测试环境绑定为 **F1**；设置管理器原始默认键是 **F5**，可在其设置里修改。
3. 找到 **自由换岛 FreeTravel → 换岛**。
4. 设置 **目标岛屿**。
5. 勾选 **执行换岛（勾选一次）**，关闭设置面板，等待游戏保存与加载完成。
6. 在 **状态说明** 中查看执行结果。

目标输入范围为 1–32，但只有当前战役实际存在的岛屿编号才会执行。例如五岛战役不会接受第六岛。

## 编译与检查

需要 PowerShell 7 和 .NET SDK。本机使用 .NET SDK 9 构建，引用来自游戏目录的运行时、BepInEx 与互操作程序集；仓库不包含游戏程序集。

```powershell
./scripts/test.ps1
./scripts/build.ps1 -GameDirectory "D:\SteamLibrary\steamapps\common\Kingdom Two Crowns"
```

构建输出位于 `dist/KingdomMod.FreeTravel/`，不会自动写入游戏。

如从源码构建后安装，可使用带存档备份与哈希校验的脚本：

```powershell
./scripts/install.ps1 -GameDirectory "D:\SteamLibrary\steamapps\common\Kingdom Two Crowns"
```

GitHub Actions 检查纯换岛规则；完整模组构建需要本地游戏引用，实机验证需在游戏内进行。

## 停用与存档备份

保存退出游戏后，将 `BepInEx/plugins/KingdomMod.FreeTravel/KingdomMod.FreeTravel.dll` 移出 `plugins`，或运行：

```powershell
./scripts/disable.ps1 -GameDirectory "D:\SteamLibrary\steamapps\common\Kingdom Two Crowns"
```

源码安装脚本会将原存档复制到仓库本地 `backups/FreeTravel-日期时间/OriginalSaves/` 并逐文件校验。停用不会恢复或覆盖存档；需要回退进度时，请先备份当前存档再处理。

## 问题反馈

请在 Issue 中说明游戏版本、战役或 DLC、单人／本地双人／在线模式、原岛与目标岛编号，以及设置面板的状态说明。可附上与 FreeTravel 有关的日志片段；不需要上传完整存档。

## 致谢

- [BepInEx](https://github.com/BepInEx/BepInEx)：模组加载与 IL2CPP 支持。
- [BepInExConfigManager](https://github.com/sinai-dev/BepInExConfigManager)：游戏内设置面板。
- [abevol/KingdomMod](https://github.com/abevol/KingdomMod)：现有模组环境与开发参考。

这是独立的社区模组，不是游戏官方产品。
