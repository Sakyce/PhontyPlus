using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhontyPlus
{
	[HarmonyPatch(typeof(WarningScreen), nameof(WarningScreen.Advance))]
	[HarmonyPriority(Priority.Last)]
    public static class PreventWarningScreenAdvancePatch
	{
		public static bool Prefix(WarningScreen __instance) => !WarningScreenPatch.active;
	}
	[HarmonyPatch(typeof(WarningScreen), nameof(WarningScreen.Start))]
	[HarmonyPriority(Priority.Last)]
	public static class WarningScreenPatch
	{
		public static string forceText = null;
		public static bool active = false;
		public static bool Prefix(WarningScreen __instance)
		{
			string path = AssetLoader.GetModPath(Mod.Instance);
			Debug.Log(path != null ? path : "NO PATH ?");
			if (Directory.Exists(path) && forceText == null) { 
				return true; 
			}
			Debug.Log(forceText);

			WarningScreenPatch.active = true;
			__instance.textBox.text = forceText ?? "Your <color=blue>Phonty</color> installation seems to be <color=red>broken</color> and you should <color=yellow>fix it by reading the instructions provided with the mod!</color>\nThe dlls and dependencies are correctly installed but not the assets! The assets folder should be inside BALDI_DATA/StreamingAssets/Modded\n\n<alpha=#AA>PRESS ALT + F4 TO CLOSE THIS GAME";
			return false;
		}
		public static void Show(string text)
		{
            forceText = text;
            SceneManager.LoadSceneAsync("Warnings").completed += (operation) => {
				active = true;
			};
		}
	}
}