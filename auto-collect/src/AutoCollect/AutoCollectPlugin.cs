using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace KingdomMod.AutoCollect
{
    /// <summary>Transfers existing taxable citizen coins to the single active monarch.</summary>
    [BepInPlugin("KingdomMod.AutoCollect", "市民自动收钱", "0.1.0")]
    [BepInProcess("KingdomTwoCrowns.exe")]
    [BepInDependency("com.codex.kingdom.unlimitedwallet", BepInDependency.DependencyFlags.HardDependency)]
    public class AutoCollectPlugin : BasePlugin
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<float> Interval;
        internal static ConfigEntry<bool> CollectionLog;
        internal static ManualLogSource Logger;
        public override void Load()
        {
            Logger = Log;
            Enabled = Config.Bind("自动收钱", "启用自动收钱", true, "仅单人。自动收取当前岛市民可上交的金币；保留原版纳税保留金额。需要已安装无限钱包。双人、联机、暂停和换岛时停止收取。");
            Interval = Config.Bind("自动收钱", "收取间隔（秒）", 2f,
                new ConfigDescription("每次收取之间的游戏时间，修改后立即生效。", new AcceptableValueRange<float>(1f, 30f)));
            CollectionLog = Config.Bind("日志", "记录收钱日志", true, "每次实际收取记录人数、金币数、岛屿与君主钱包变化；异常始终记录，并停止自动收钱。日志位于 BepInEx/LogOutput.log。");
            AddComponent<AutoCollectController>();
            Log.LogInfo("市民自动收钱 0.1 已加载：仅单人，保留市民纳税底金，支持无限钱包。");
        }
    }

    public class AutoCollectController : MonoBehaviour
    {
        private float _nextCollection;
        private bool _faulted;
        private float _nextDiagnostic;
        public AutoCollectController(IntPtr pointer) : base(pointer) { }
        private void Update()
        {
            if (_faulted || !AutoCollectPlugin.Enabled.Value || Time.time < _nextCollection) { return; }
            _nextCollection = Time.time + AutoCollectPlugin.Interval.Value;
            try
            {
                var game = Managers.Inst?.game;
                var kingdom = Managers.Inst?.kingdom;
                if (kingdom == null || game == null) { return; }
                int count = 0;
                Player player = null;
                foreach (var activePlayer in kingdom._activePlayers)
                {
                    if (activePlayer != null && activePlayer.isActiveAndEnabled)
                    {
                        count++;
                        player = activePlayer;
                    }
                }
                if (!CollectionRules.CanRun(AutoCollectPlugin.Enabled.Value,
                    NetworkBigBoss.IsOnline || ProgramDirector.IsClient,
                    game.state == Game.State.Playing, count, Time.timeScale <= 0, Managers.COOP_ENABLED || Managers.TABLET_COOP_ENABLED)) { return; }
                var destination = player?.wallet;
                if (destination == null || !destination.CanGrabCurrency(CurrencyType.Coins)) { return; }
                int before = destination.Coins;
                long total = 0;
                int citizens = 0;
                var registry = kingdom._characters;
                if (registry == null) { return; }
                var snapshot = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Character>(registry.Count);
                registry.CopyTo(snapshot);
                int eligible = 0;
                int wallets = 0;
                long held = 0;
                foreach (var character in snapshot)
                {
                    if (character == null || !character.isActiveAndEnabled || character.inert || character.grabbed) { continue; }
                    var go = character.gameObject;
                    if (go.GetComponent<Knight>() != null || go.GetComponent<Berserker>() != null || go.GetComponent<Pikeman>() != null) { continue; }
                    if (go.GetComponent<Peasant>() == null && go.GetComponent<Worker>() == null &&
                        go.GetComponent<Archer>() == null && go.GetComponent<Farmer>() == null) { continue; }
                    eligible++;
                    var source = character.wallet;
                    if (source == null || source.Pointer == destination.Pointer || source._playerRef != null ||
                        source._payingTaxes || Wallet.DebugDisableTaxes || Wallet.InfiniteMoney) { continue; }
                    wallets++;
                    held += source.Coins;
                    int moved = CollectionRules.Transfer(() => source.Coins, value => source.SetCurrency(CurrencyType.Coins, value),
                        () => destination.Coins, value => destination.SetCurrency(CurrencyType.Coins, value), source.payTaxesAbove);
                    if (moved > 0) { total += moved; citizens++; }
                }
                if (AutoCollectPlugin.CollectionLog.Value && Time.unscaledTime >= _nextDiagnostic)
                {
                    _nextDiagnostic = Time.unscaledTime + 15;
                    AutoCollectPlugin.Logger.LogInfo($"收钱检查：市民 {snapshot.Length}，职业符合 {eligible}，可收钱包 {wallets}，钱包持币 {held}，本次入账 {total}。");
                }
                if (total > 0 && AutoCollectPlugin.CollectionLog.Value)
                {
                    AutoCollectPlugin.Logger.LogInfo($"自动收钱：第 {game.currentLand + 1} 岛，{citizens} 位市民，{total} 枚金币；钱包 {before} → {destination.Coins}。");
                }
            }
            catch (Exception error)
            {
                _faulted = true;
                AutoCollectPlugin.Logger.LogError($"自动收钱遇到异常，已停止本次运行的自动收取，请保留日志：{error}");
            }
        }
    }
}
