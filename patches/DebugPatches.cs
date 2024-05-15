namespace PhontyPlus.patches
{
#if DEBUG

    [HarmonyPatch(typeof(EnvironmentController), nameof(EnvironmentController.NPCSpawner))]
    public static class InstantMap
    {
        static void Postfix(EnvironmentController __instance, ref IEnumerator __result)
        {
            void postfixAction() {
                foreach (var npc in __instance.npcs)
                {
                    __instance.map.AddArrow(npc.transform, Color.red);
                }
            }

            void prefixAction() {
                __instance.map.CompleteMap();
            }

            __result = new SimpleEnumerator() { enumerator = __result, postfixAction = postfixAction, prefixAction = prefixAction }.GetEnumerator();
        }
    }

    [HarmonyPatch(typeof(HappyBaldi), nameof(HappyBaldi.Activate))]
    public static class NoHappyBaldi
    {
        static void Prefix(HappyBaldi __instance)
        {
            Singleton<MusicManager>.Instance.StopMidi();
            Singleton<BaseGameManager>.Instance.BeginSpoopMode();
            __instance.ec.SpawnNPCs();
            __instance.ec.GetBaldi().Despawn();
            __instance.activated = true;
            __instance.ec.StartEventTimers();
            __instance.sprite.enabled = false;
            GameObject.Destroy(__instance.gameObject);
        }
    }
#endif
}
