using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace KingdomMod.FreeTravel
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, "自由换岛 FreeTravel", MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("KingdomTwoCrowns.exe")]
    public class FreeTravelPlugin : BasePlugin
    {
        internal static ConfigEntry<int> TargetIsland;
        internal static ConfigEntry<bool> Execute;
        internal static ConfigEntry<string> Status;
        internal static ManualLogSource Logger;
        internal static bool Pending;

        public override void Load()
        {
            Logger = Log;
            Config.SaveOnConfigSet = true;
            TargetIsland = Config.Bind("换岛", "目标岛屿", 1,
                new ConfigDescription("岛屿从 1 开始编号。可选择未解锁岛屿；超出当前战役范围会拒绝执行。", new AcceptableValueRange<int>(1, 32)));
            Execute = Config.Bind("换岛", "执行换岛（勾选一次）", false,
                "先选择目标岛屿，再勾选执行。自动复位。调用游戏原有保存和换岛流程，不需要造船或到码头。第一版仅支持单人／本地双人。");
            Status = Config.Bind("换岛", "状态说明", "等待进入战役", "显示当前岛屿、战役范围或执行结果。");
            Execute.Value = false;
            Execute.SettingChanged += OnExecuteChanged;
            AddComponent<FreeTravelController>();
            Log.LogInfo("FreeTravel loaded: select an island in F1, then tick the execute setting.");
        }

        private static void OnExecuteChanged(object sender, EventArgs args)
        {
            if (Execute.Value) Pending = true;
        }

        internal static void SetStatus(string message)
        {
            if (Status.Value != message) Status.Value = message;
        }
    }

    public class FreeTravelController : MonoBehaviour
    {
        private bool _busy;
        private int _expectedLand = -1;
        private float _startedAt;
        private float _nextStatusAt;
        private float _nextDiagnosticAt;

        public FreeTravelController(IntPtr pointer) : base(pointer) { }

        private void Update()
        {
            if (FreeTravelPlugin.Pending)
            {
                FreeTravelPlugin.Pending = false;
                FreeTravelPlugin.Execute.Value = false;
                TryTravel();
            }

            if (Time.unscaledTime < _nextStatusAt) return;
            _nextStatusAt = Time.unscaledTime + 1;
            var game = Managers.Inst?.game;
            var campaign = CampaignSaveData.current;
            if (game == null || campaign == null) return;
            if (_busy && Time.unscaledTime >= _nextDiagnosticAt)
            {
                _nextDiagnosticAt = Time.unscaledTime + 5;
                var boat = Managers.Inst.kingdom?.boat;
                FreeTravelPlugin.Logger.LogInfo($"Travel progress: elapsed={Time.unscaledTime - _startedAt:0.0}, state={game.state}, gameLand={game.currentLand}, campaignLand={campaign.CurrentLand}, expected={_expectedLand}, timeScale={Time.timeScale}, blocked={game.blockStateProgression}, boat={(boat != null ? boat.state.ToString() : "none")}");
                if (Time.unscaledTime - _startedAt > 30 && game.state != Game.State.Playing)
                {
                    FreeTravelPlugin.SetStatus($"换岛仍在等待：{game.state}；请关闭 F1，若仍卡住请反馈。不要重复执行换岛。");
                }
            }
            if (_busy && game.state == Game.State.Playing)
            {
                if (game.currentLand == _expectedLand && campaign.CurrentLand == _expectedLand && Time.unscaledTime - _startedAt > 1)
                {
                    _busy = false;
                    string message = $"已到达第 {game.currentLand + 1} 岛；当前战役共 {campaign.MaxIslands} 岛。";
                    FreeTravelPlugin.SetStatus(message);
                    FreeTravelPlugin.Logger.LogInfo($"Travel completed: gameLand={game.currentLand}, campaignLand={campaign.CurrentLand}");
                }
                else if (Time.unscaledTime - _startedAt > 15)
                {
                    _busy = false;
                    FreeTravelPlugin.SetStatus("换岛未完成或已取消，请确认游戏提示后重试。");
                    FreeTravelPlugin.Logger.LogWarning("Travel did not reach the requested island.");
                }
            }
            if (!_busy && FreeTravelPlugin.Status.Value == "等待进入战役")
            {
                FreeTravelPlugin.SetStatus($"当前第 {game.currentLand + 1} 岛；当前战役共 {campaign.MaxIslands} 岛。选择目标并勾选执行。");
            }
        }

        private void TryTravel()
        {
            var game = Managers.Inst?.game;
            var campaign = CampaignSaveData.current;
            int target = FreeTravelPlugin.TargetIsland.Value;
            string error = TravelRules.Validate(target, game?.currentLand ?? -1, campaign?.MaxIslands ?? 0,
                NetworkBigBoss.IsOnline || ProgramDirector.IsClient, game != null && game.state == Game.State.Playing,
                game != null && game.CanSave, _busy);
            if (error.Length > 0)
            {
                FreeTravelPlugin.SetStatus(error);
                FreeTravelPlugin.Logger.LogWarning(error);
                return;
            }
            try
            {
                _busy = true;
                _expectedLand = target - 1;
                _startedAt = Time.unscaledTime;
                _nextDiagnosticAt = 0;
                FreeTravelPlugin.SetStatus($"正在前往第 {target} 岛，请等待游戏保存和加载完成。" );
                var boat = Managers.Inst.kingdom.boat;
                string boatState = boat != null ? boat.state.ToString() : "none";
                var group = TravelRules.UseBoatGroup(boat != null, boatState) ? SailAwayGroup.Boats : SailAwayGroup.None;
                FreeTravelPlugin.Logger.LogInfo($"Travel requested: from={game.currentLand}, target={_expectedLand}, group={group}, boat={boatState}, canSave={game.CanSave}");
                game.SailAway(group, new Il2CppSystem.Nullable<int>(_expectedLand));
            }
            catch (Exception exception)
            {
                _busy = false;
                FreeTravelPlugin.SetStatus("换岛调用失败，请保留日志并反馈。");
                FreeTravelPlugin.Logger.LogError(exception);
            }
        }
    }
}
