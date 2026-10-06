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

# v0.1.1 验证记录

日期：2026-10-06。环境同上。

- 9 项换岛条件检查和 5 项船状态选择检查通过，完整编译成功。
- 修正前：四岛前往五岛，游戏为 SailingAway、船为 Pushing，时间缩放为 1，未阻止状态推进，但始终未完成。
- 修正后：未航行船选择 None；四岛成功到达此前访问过的五岛，游戏与战役编号一致。玩家确认“解决了”。
- 实机使用的修正版插件标识仍为 0.1.0；发布包将插件版本标识改为 0.1.1，功能源代码与实测修正版相同。

```text
Travel requested: from=3, target=4, group=None, boat=Pushing, canSave=True
Travel completed: gameLand=4, campaignLand=4
```

未强制改变船状态，也未手工改写存档。WaitingForPlayer、WaitingForPassengers 和 Sailing 路径通过规则检查，未分别实机专项验证；此前未验证的多人、未解锁岛屿、队伍保留等限制仍适用。
