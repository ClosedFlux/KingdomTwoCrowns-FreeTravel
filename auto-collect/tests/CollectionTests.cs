public static class CollectionTests
{
    private static int count;
    private static void Check(bool condition, string message) { if (!condition) throw new System.Exception(message); count++; }
    public static int Run()
    {
        Check(KingdomMod.AutoCollect.CollectionRules.Available(12, 2, 10) == 10, "Must retain citizen reserve");
        Check(KingdomMod.AutoCollect.CollectionRules.Available(1, 2, 10) == 0, "Below reserve");
        Check(KingdomMod.AutoCollect.CollectionRules.Available(-1, 0, 10) == 0, "Invalid source");
        Check(KingdomMod.AutoCollect.CollectionRules.Available(8, -2, 10) == 8, "Negative reserve treated as zero");
        Check(KingdomMod.AutoCollect.CollectionRules.Available(8, 0, int.MaxValue - 2) == 2, "Int32 overflow prevented");
        Check(KingdomMod.AutoCollect.CollectionRules.CanRun(true, false, true, 1, false), "Single player allowed");
        Check(!KingdomMod.AutoCollect.CollectionRules.CanRun(false, false, true, 1, false), "Disabled");
        Check(!KingdomMod.AutoCollect.CollectionRules.CanRun(true, true, true, 1, false), "Online blocked");
        Check(!KingdomMod.AutoCollect.CollectionRules.CanRun(true, false, false, 1, false), "Travel/loading blocked");
        Check(!KingdomMod.AutoCollect.CollectionRules.CanRun(true, false, true, 2, false), "Local coop blocked");
        Check(!KingdomMod.AutoCollect.CollectionRules.CanRun(true, false, true, 1, true), "Pause blocked");
        Check(!KingdomMod.AutoCollect.CollectionRules.CanRun(true, false, true, 1, false, true), "Coop with one enabled player blocked");
        int source = 12, destination = 10;
        var moved = KingdomMod.AutoCollect.CollectionRules.Transfer(() => source, v => source = v, () => destination, v => destination = v, 2);
        Check(moved == 10 && source == 2 && destination == 20, "Transfer must conserve total coins");
        moved = KingdomMod.AutoCollect.CollectionRules.Transfer(() => source, v => source = v, () => destination, v => destination = v, 2);
        Check(moved == 0 && destination == 20, "Repeated sweep must not duplicate coins");
        source = 12; destination = 10;
        try { KingdomMod.AutoCollect.CollectionRules.Transfer(() => source, v => source = v, () => destination, v => { if (v == 20) throw new System.Exception("native write failed"); destination = v; }, 2); }
        catch (System.Exception) { }
        Check(source == 12 && destination == 10, "Failed credit must restore debit");
        source = 12; destination = 10;
        try { KingdomMod.AutoCollect.CollectionRules.Transfer(() => source, v => source = v, () => destination, v => destination = System.Math.Min(v, 15), 2); }
        catch (System.Exception) { }
        Check(source == 12 && destination == 10, "Clamped credit must restore both wallets");
        return count;
    }
}
