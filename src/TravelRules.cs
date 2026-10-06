namespace KingdomMod.FreeTravel
{
    public static class TravelRules
    {
        /// <summary>Native boat travel waits for an already sailing boat to leave the island.</summary>
        public static bool UseBoatGroup(bool hasBoat, string boatState)
        {
            return hasBoat && boatState == "Sailing";
        }

        public static string Validate(int targetIsland, int currentLand, int maximumIslands, bool online, bool playing, bool canSave, bool busy)
        {
            if (online) return "第一版仅支持单人和本地双人。请退出在线联机后使用。";
            if (busy) return "正在换岛，请等待完成。";
            if (!playing) return "请进入战役并回到正常游玩状态后操作。";
            if (!canSave) return "当前不能保存，请等待保存或动画结束后重试。";
            if (targetIsland < 1 || targetIsland > maximumIslands) return "目标岛屿超出当前战役范围。";
            if (targetIsland == currentLand + 1) return "你已经在目标岛屿。";
            return "";
        }
    }
}
