using System;
namespace KingdomMod.AutoCollect
{
    /// <summary>Single-player collection and coin conservation rules.</summary>
    public static class CollectionRules
    {
        public static int Available(int coins, int reserve, int destination)
        {
            if (coins <= 0 || destination < 0) { return 0; }
            return (int)Math.Min(Math.Max(0L, (long)coins - Math.Max(0, reserve)), (long)int.MaxValue - destination);
        }
        public static bool CanRun(bool enabled, bool online, bool playing, int players, bool paused, bool coop = false)
        {
            return enabled && !coop && !online && playing && players == 1 && !paused;
        }
        public static int Transfer(Func<int> readSource, Action<int> writeSource, Func<int> readDestination, Action<int> writeDestination, int reserve)
        {
            int source = readSource();
            int destination = readDestination();
            int amount = Available(source, reserve, destination);
            if (amount == 0) { return 0; }
            try
            {
                writeSource(source - amount);
                if (readSource() != source - amount) { throw new InvalidOperationException("市民钱包扣款未完成"); }
                writeDestination(destination + amount);
                if (readDestination() != destination + amount) { throw new InvalidOperationException("君主钱包入账未完成"); }
                return amount;
            }
            catch (Exception transferError)
            {
                // No asynchronous calls: restore both snapshots before allowing any further collection.
                Exception rollbackError = null;
                try { writeDestination(destination); }
                catch (Exception error) { rollbackError = error; }
                try { writeSource(source); }
                catch (Exception error) { rollbackError = error; }
                if (rollbackError != null || readSource() != source || readDestination() != destination)
                {
                    throw new InvalidOperationException("自动收钱回滚失败，请退出且不要覆盖此前备份", rollbackError ?? transferError);
                }
                throw new InvalidOperationException("收钱失败，金币已恢复", transferError);
            }
        }
    }
}
