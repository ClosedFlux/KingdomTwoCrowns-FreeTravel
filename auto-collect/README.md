# 市民自动收钱 0.1

《王国：两位君主》电脑版独立模组，自动把当前岛市民可上交的金币转入君主钱包，免去来回收钱。

**Auto Collect 0.1 — automatically transfers existing taxable citizen coins to the single monarch.**

这是自动收钱组件的首次公开版本，标签为 `auto-collect-v0.1`，与自由换岛及菜单汉化版本分别编号。DLL 使用三段版本 `0.1.0`。内容对应本机验证通过的修正版；本机此前开发编号 0.1.1 不作为公开发布编号。

## 功能

- 收取当前王国平民、工人、弓箭手、农民实际持有的可上交金币。
- 保留原版纳税底金，不产生额外金币，不收宝石、银行余额或地面散落金币。
- 排除骑士、枪兵、狂战士以及正在原版交钱的市民。
- 仅单人；本地双人、在线联机、暂停及加载/换岛时停止收取。
- 中文设置：启用开关、收取间隔（默认 2 秒，范围 1–30 秒）、日志开关。
- 直接复制当前王国的市民集合后转账，避免原生遍历器失效。
- 检查整数上限、扣款和入账；转移异常时尝试回滚，并停止本次运行的自动收取。

## 依赖

- Windows IL2CPP 版游戏和兼容的 BepInEx 6。
- **[Unlimited Wallet 1.1.1](https://www.nexusmods.com/kingdomtwocrowns/mods/45)** 必须先安装，插件 GUID 为 `com.codex.kingdom.unlimitedwallet`。
- 游戏内修改设置需要 BepInExConfigManager。测试环境绑定 F1，其他环境按设置管理器自己的快捷键打开。

安装包只含自动收钱 DLL，不附带游戏文件、加载器、第三方无限钱包或设置管理器。无限钱包由原作者单独发布，本组件不修改其文件。

## 安装和使用

1. 保存退出游戏，备份存档。
2. 确认上述依赖已正常运行。
3. 下载本版本安装 ZIP，将 `BepInEx` 文件夹合并到游戏目录。
4. 最终路径：`BepInEx/plugins/KingdomMod.AutoCollect/KingdomMod.AutoCollect.dll`。
5. 进入单人战役即可自动收钱；F1 → **市民自动收钱**可修改设置。

配置：`BepInEx/config/KingdomMod.AutoCollect.cfg`。
日志：`BepInEx/LogOutput.log`。实际收取时记录岛屿、人数、金额及钱包变化，每 15 秒输出筛选统计。异常始终记录。

停用：保存退出后，将 `KingdomMod.AutoCollect` 文件夹移出 `plugins`。不会自动恢复存档或撤销已经收取的金币。

## 验证

测试环境：Steam Windows64，游戏 2.4.2，Unity 6 / IL2CPP，BepInEx 6 be.753，Unlimited Wallet 1.1.1。

- 16 项规则检查通过：保留底金、金额守恒、重复收取、整数上限、入账失败回滚、单人/联机/双人/暂停保护。
- 隔离编译与加载通过。
- 游戏日志实际转移 15 + 1 枚金币，钱包 42 → 57 → 58，无收钱错误。
- 玩家确认：界面到账、F1 关闭后停止、购买正常扣款。
- 其他游戏版本、全部 DLC、保存重载和换岛后的连续收取未专项验证；双人和联机不启用。

## 源码构建

需要 PowerShell 7、.NET SDK 和本地游戏引用（先启动一次 BepInEx 生成 interop）。仓库不提供游戏程序集。

```powershell
./scripts/test.ps1
./scripts/build.ps1 -GameDirectory "D:\SteamLibrary\steamapps\common\Kingdom Two Crowns"
./scripts/install.ps1 -GameDirectory "D:\SteamLibrary\steamapps\common\Kingdom Two Crowns"
```

构建只输出 `dist/KingdomMod.AutoCollect`；安装脚本要求游戏退出，先备份并逐文件校验存档，再安装 DLL。

反馈问题时请提供游戏版本、单人状态和自动收钱相关日志；无需上传完整存档。
