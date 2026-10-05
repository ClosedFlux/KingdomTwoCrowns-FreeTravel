# v0.1.0 验证记录

日期：2026-10-05。

- Windows Steam 版游戏 2.4.2，Unity 6，IL2CPP，现有 BepInEx 加载环境。
- 构建与 9 项换岛规则检查通过。
- 模组和控制器在游戏中加载成功。
- 第 4 岛 → 第 2 岛请求使用 None 随行组，没有船也成功到达。
- 第 2 岛 → 第 4 岛成功返回，玩家确认原有建筑与岛屿进度保留。
- 两次完成日志中的游戏当前岛与战役当前岛一致。

日志摘录仅包含功能结果，编号为游戏内部从零开始的编号：

```text
Travel requested: from=3, target=1, group=None, canSave=True
Travel completed: gameLand=1, campaignLand=1
Travel requested: from=1, target=3, group=None, canSave=True
Travel completed: gameLand=3, campaignLand=3
```

代码检查和加载结果不替代其他战役或多人场景的实机验证。未解锁岛屿、本地双人和队伍保留没有单独完成实测。
