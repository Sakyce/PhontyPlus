using HarmonyLib;
using UnityEngine;
using UnityEngine.Audio;

namespace PhontyPlus.patches
{
    [HarmonyPatch(typeof(CoreGameManager), nameof(CoreGameManager.ReturnToMenu))]
    internal class RestoreAudioOnReturnToMenu
    {
        internal static bool Prefix(CoreGameManager __instance)
        {
            AudioListener.volume = 1f;
            Mod.assetManager.Get<AudioMixer>("Mixer").SetFloat("EchoWetMix", 0f);
            return true;
        }
    }
    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.RestartLevel))]
    internal class RestoreAudioOnRestart
    {
        internal static bool Prefix(BaseGameManager __instance)
        {
            AudioListener.volume = 1f;
            Mod.assetManager.Get<AudioMixer>("Mixer").SetFloat("EchoWetMix", 0f);
            return true;
        }
    }
    [HarmonyPatch(typeof(BaseGameManager), nameof(BaseGameManager.LoadNextLevel))]
    internal class RestoreAudioOnNextLevel
    {
        internal static bool Prefix(BaseGameManager __instance)
        {
            AudioListener.volume = 1f;
            Mod.assetManager.Get<AudioMixer>("Mixer").SetFloat("EchoWetMix", 0f);
            return true;
        }
    }

}
//AudioListener.volume = 0.01f;
//Mod.assetManager.Get<AudioMixer>("Mixer").SetFloat("EchoWetMix", 1f);
