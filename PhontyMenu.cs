using BepInEx.Configuration;
using MTM101BaldAPI.OptionsAPI;
using UnityEngine;

namespace PhontyPlus
{
    internal class PhontyMenu : MonoBehaviour
    {
        public static ConfigEntry<bool> nonLethalConfig;
        public static ConfigEntry<int> timeLeftUntilMad;
        public static ConfigEntry<bool> guaranteeSpawn;

        private MenuToggle nonlethalToggle;
        private MenuToggle guaranteeSpawnToggle;

        private void Initialize(OptionsMenu __instance)
        {
            nonlethalToggle = CustomOptionsCore.CreateToggleButton(__instance,
                new Vector2(20f, 0), "NonLethal",
                nonLethalConfig.Value,
                "If enabled, Phonty will deaf the player instead of ending the game"
            );
            nonlethalToggle.transform.SetParent(transform, false);
            nonlethalToggle.hotspot.GetComponent<StandardMenuButton>().OnPress.AddListener(() => nonLethalConfig.Value = nonlethalToggle.Value);

            guaranteeSpawnToggle = CustomOptionsCore.CreateToggleButton(__instance,
                new Vector2(20f, -25f), "GuaranteeSpawn",
                guaranteeSpawn.Value,
                "If enabled, Phonty spawn weight will be increased by a thousand"
            );
            guaranteeSpawnToggle.transform.SetParent(transform, false);
            guaranteeSpawnToggle.hotspot.GetComponent<StandardMenuButton>().OnPress.AddListener(() => guaranteeSpawn.Value = guaranteeSpawnToggle.Value);
        }

        internal static void OnMenuInitialize(OptionsMenu __instance)
        {
            var obj = CustomOptionsCore.CreateNewCategory(__instance, "Phonty Phonograph");
            var phontyMenu = obj.AddComponent<PhontyMenu>();
            phontyMenu.Initialize(__instance);
        }
        /// <summary>
        /// Triggered when launching the mod, mostly to setup BepInEx config bindings.
        /// </summary>
        internal static void Setup()
        {
            nonLethalConfig = Mod.Instance.Config.Bind("Phonty", "NonLethal", false, "Enabling this will replace Phonty's ability to kill the player with deaf.");
            timeLeftUntilMad = Mod.Instance.Config.Bind("Phonty", "WindUpTime", 180, "Amount of seconds until Phonty will become mad if not wound up.");
            guaranteeSpawn = Mod.Instance.Config.Bind("Phonty", "GuaranteeSpawn", false, "Enabling this will make sure that Phonty will ALWAYS spawn. Used to check if Phonty actually works.");
        }
    }
}
