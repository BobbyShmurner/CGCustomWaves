using HarmonyLib;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CGCustomWaves
{
	// Sets up the custom ui in the wave menu
	[HarmonyPatch(typeof(HudOpenEffect), "Awake", new Type[]{})]
	class WaveUISetup {
		public static void Postfix(HudOpenEffect __instance) {
			if (__instance.gameObject.name != "Waves") return;

			Transform waveUI = __instance.transform.GetChild(0);
			
			RectTransform titleRect = waveUI.Find("Title").GetComponent<RectTransform>();
			titleRect.anchoredPosition = new Vector2(titleRect.anchoredPosition.x, 0);
			titleRect.GetComponent<TextMeshProUGUI>().fontSize = 24;

			RectTransform textRect = waveUI.Find("Text").GetComponent<RectTransform>();
			textRect.anchoredPosition = new Vector2(textRect.anchoredPosition.x, -50);
			textRect.GetComponent<TextMeshProUGUI>().text = "SELECT STARTING WAVE:<size=16><color=orange>\n\n(Use the Wave Override Cheat to play any wave)</color></size>";

			RectTransform customWave = GameObject.Instantiate(waveUI.Find("0").gameObject, waveUI).GetComponent<RectTransform>();
			customWave.anchoredPosition = new Vector2(0, 25);
			Plugin.CustomWaveSetter = customWave.GetComponent<WaveSetter>();
			Plugin.CustomWaveSetter.wave = Plugin.ConfigCustomWave.Value;

			// Value Buttons

			RectTransform increase = GameObject.Instantiate(waveUI.Find("0").gameObject, waveUI).GetComponent<RectTransform>();

			CustomWaveSetter increaseSetter = increase.gameObject.AddComponent<CustomWaveSetter>();
			increaseSetter.customButton = customWave.GetComponent<WaveSetter>();
			increaseSetter.buttonSuccess = (GameObject)typeof(WaveSetter).GetField("buttonSuccess", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(increase.GetComponent<WaveSetter>());
			increaseSetter.buttonFail = (GameObject)typeof(WaveSetter).GetField("buttonFail", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(increase.GetComponent<WaveSetter>());
			increaseSetter.changeValue = 1;

			GameObject.DestroyImmediate(increase.GetComponent<WaveSetter>());

			increase.anchoredPosition = new Vector2(65, 25);
			increase.localScale = Vector3.one;
			increase.sizeDelta = new Vector2(30, 40);
			increase.GetComponentInChildren<TextMeshProUGUI>().text = ">";

			RectTransform increaseGreater = GameObject.Instantiate(increase.gameObject, waveUI).GetComponent<RectTransform>();
			increaseGreater.GetComponent<CustomWaveSetter>().changeValue = 10;
			increaseGreater.localScale = Vector3.one;
			increaseGreater.anchoredPosition = new Vector2(100, 25);
			increaseGreater.sizeDelta = new Vector2(40, 40);
			increaseGreater.GetComponentInChildren<TextMeshProUGUI>().text = ">>";

			RectTransform decrease = GameObject.Instantiate(increase.gameObject, waveUI).GetComponent<RectTransform>();
			decrease.GetComponent<CustomWaveSetter>().changeValue = -1;
			decrease.localScale = Vector3.one;
			decrease.anchoredPosition = new Vector2(-65, 25);
			decrease.sizeDelta = new Vector2(30, 40);
			decrease.GetComponentInChildren<TextMeshProUGUI>().text = "<";

			RectTransform decreaseGreater = GameObject.Instantiate(increase.gameObject, waveUI).GetComponent<RectTransform>();
			decreaseGreater.GetComponent<CustomWaveSetter>().changeValue = -10;
			decreaseGreater.localScale = Vector3.one;
			decreaseGreater.anchoredPosition = new Vector2(-100, 25);
			decreaseGreater.sizeDelta = new Vector2(40, 40);
			decreaseGreater.GetComponentInChildren<TextMeshProUGUI>().text = "<<";
		}
	}

	// Fixes the fixes colour not being set correctly for custom waves
	[HarmonyPatch(typeof(EndlessGrid), "NextWave", new Type[]{})]
	public static class GridColorPatch {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			// Replaces "switch (currentWave)" with "switch (currentWave - currentWave % 5)"

			var codes = new List<CodeInstruction>(instructions);

			for (var i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode != OpCodes.Callvirt || (MethodInfo)codes[i].operand != typeof(GoreZone).GetMethod("ResetGibs", BindingFlags.Public | BindingFlags.Instance)) continue;

				codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(i + 4, new CodeInstruction(OpCodes.Ldfld, typeof(EndlessGrid).GetField("currentWave", BindingFlags.Public | BindingFlags.Instance)));
				codes.Insert(i + 5, new CodeInstruction(OpCodes.Ldc_I4_5));
				codes.Insert(i + 6, new CodeInstruction(OpCodes.Rem));
				codes.Insert(i + 7, new CodeInstruction(OpCodes.Sub));

				break;
			}

			for (var i = 0; i < codes.Count; i++) {
				if (codes[i].opcode != OpCodes.Ldc_I4_S || (sbyte)codes[i].operand != 25) continue;

				codes[i + 1].opcode = OpCodes.Bge;

				break;
			}

			return codes.AsEnumerable();
		}
	}

	// Returns unselected if cheat is enabled, instead of returning locked
	// Correctly checks the highest wave
	[HarmonyPatch(typeof(WaveMenu), "CheckWaveAvailability", new Type[]{typeof(WaveSetter)})]
	public static class CheckWaveAvailabilityPatch {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var codes = new List<CodeInstruction>(instructions);

			for (var i = 0; i < codes.Count; i++)
			{
				// Returns unselected if cheat is enabled, instead of returning locked
				if (codes[i].opcode == OpCodes.Blt) {
					Label unselectedLabel = generator.DefineLabel();
					Label lockedLabel = (Label)codes[i].operand;

					codes[i + 1].labels.Add(unselectedLabel);

					codes[i].opcode = OpCodes.Bge;
					codes[i].operand = unselectedLabel;

					codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(CyberGrindWaveOverride).GetMethod("GetActive", BindingFlags.Public | BindingFlags.Static)));
					codes.Insert(i + 2, new CodeInstruction(OpCodes.Brfalse, lockedLabel));
				}

				// Correctly checks the highest wave
				if (codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == typeof(WaveMenu).GetField("highestWave", BindingFlags.NonPublic | BindingFlags.Instance) && codes[i + 1].opcode == OpCodes.Ldarg_1) {
					codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
					codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldfld, typeof(WaveMenu).GetField("highestWave", BindingFlags.NonPublic | BindingFlags.Instance)));
					codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldc_I4, 10));
					codes.Insert(i + 4, new CodeInstruction(OpCodes.Rem));
					codes.Insert(i + 5, new CodeInstruction(OpCodes.Sub));
				}
			}

			return codes.AsEnumerable();
		}
	}

	// Doesnt return if the cheat is enabled
	// Correctly checks the highest wave
	[HarmonyPatch(typeof(WaveMenu), "SetCurrentWave", new Type[]{typeof(int)})]
	public static class SetCurrentWavePatch {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var codes = new List<CodeInstruction>(instructions);

			for (var i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode != OpCodes.Ldfld || (FieldInfo)codes[i].operand != typeof(WaveMenu).GetField("highestWave", BindingFlags.NonPublic | BindingFlags.Instance)) continue;

				codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldfld, typeof(WaveMenu).GetField("highestWave", BindingFlags.NonPublic | BindingFlags.Instance)));
				codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldc_I4, 10));
				codes.Insert(i + 4, new CodeInstruction(OpCodes.Rem));
				codes.Insert(i + 5, new CodeInstruction(OpCodes.Sub));

				break;
			}

			// Doesnt return if the cheat is enabled
			for (var i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode != OpCodes.Ble) continue;

				Label continueLabel = (Label)codes[i].operand;

				codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(CyberGrindWaveOverride).GetMethod("GetActive", BindingFlags.Public | BindingFlags.Static)));
				codes.Insert(i + 2, new CodeInstruction(OpCodes.Brtrue, continueLabel));

				break;
			}

			return codes.AsEnumerable();
		}
	}

	// Doesn't set the wave to 0 if the cheat is enabled
	// Correctly checks the highest wave
	[HarmonyPatch(typeof(WaveMenu), "GetHighestWave", new Type[]{})]
	public static class GetHighestWavePatch {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var codes = new List<CodeInstruction>(instructions);

			// Correctly checks the highest wave
			for (var i = 0; i < codes.Count; i++) {
				if (codes[i].opcode != OpCodes.Ldfld || (FieldInfo)codes[i].operand != typeof(WaveMenu).GetField("highestWave", BindingFlags.NonPublic | BindingFlags.Instance)) continue;

				codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldfld, typeof(WaveMenu).GetField("highestWave", BindingFlags.NonPublic | BindingFlags.Instance)));
				codes.Insert(i + 3, new CodeInstruction(OpCodes.Ldc_I4, 10));
				codes.Insert(i + 4, new CodeInstruction(OpCodes.Rem));
				codes.Insert(i + 5, new CodeInstruction(OpCodes.Sub));

				break;
			}

			// Doesn't set the wave to 0 if the cheat is enabled
			for (var i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode != OpCodes.Blt) continue;

				Label setTo0Label = (Label)codes[i].operand;
				Label setToHighestLabel = generator.DefineLabel();

				codes[i + 1].labels.Add(setToHighestLabel);

				codes[i].opcode = OpCodes.Bge;
				codes[i].operand = setToHighestLabel;

				codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, typeof(CyberGrindWaveOverride).GetMethod("GetActive", BindingFlags.Public | BindingFlags.Static)));
				codes.Insert(i + 2, new CodeInstruction(OpCodes.Brfalse, setTo0Label));

				break;
			}

			return codes.AsEnumerable();
		}
	}

	// Registers the Wave Override cheat
	[HarmonyPatch(typeof(CheatsManager), "Start", new Type[]{})]
	public static class RegisterCheat {
		public static void Prefix(CheatsManager __instance) {
			if (SceneManager.GetActiveScene().name != Plugin.CGSceneName) return;

			__instance.RebuildIcons();
			__instance.RegisterCheat(new CyberGrindWaveOverride(), "cyber grind");
		}
	}

	// Set's the button state to Unselected if not selected or locked
	[HarmonyPatch(typeof(WaveSetter), "Prepare", new Type[]{})]
	public static class WSPreparePatch {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			var codes = new List<CodeInstruction>(instructions);

			for (var i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode != OpCodes.Bne_Un) continue;

				Label lockedLabel = generator.DefineLabel();
				codes[i + 1].labels.Add(lockedLabel);

				codes[i].opcode = OpCodes.Beq;
				codes[i].operand = lockedLabel;

				codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
				codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, typeof(WaveSetter).GetMethod("Unselected", BindingFlags.Public | BindingFlags.Instance)));
				codes.Insert(i + 3, new CodeInstruction(OpCodes.Ret));
			}

			return codes.AsEnumerable();
		}
	}

	[HarmonyPatch(typeof(CheatsManager), "RebuildIcons", new Type[]{})]
	class CheatIconPatch {
		public static void Postfix(ref Dictionary<string, Sprite> ___spriteIcons) {
			___spriteIcons.Add("wave-override", Plugin.CheatIcon);
		}
	}

	// The next 3 patches just update the button states for the WaveSetters
	// Unselect all other Wave Setters when selected
	[HarmonyPatch(typeof(WaveSetter), "Selected", new Type[]{})]
	class WSSelectedPatch {
		public static void Postfix(ref ButtonState ___state, WaveMenu ___wm, WaveSetter __instance) {
			___state = ___wm.CheckWaveAvailability(__instance);
			if (__instance == Plugin.CustomWaveSetter) Plugin.ConfigCustomWave.Value = __instance.wave;

			foreach (WaveSetter ws in ___wm.setters) {
				if (ws != __instance) ws.Unselected();
			}
		}
	}

	
	[HarmonyPatch(typeof(WaveSetter), "Unselected", new Type[]{})]
	class WSUnselectedPatch {
		public static void Postfix(ref ButtonState ___state, WaveMenu ___wm, WaveSetter __instance) {
			___state = ___wm.CheckWaveAvailability(__instance);
		}
	}

	
	[HarmonyPatch(typeof(WaveSetter), "Locked", new Type[]{})]
	class WSLockedPatch {
		public static void Postfix(ref ButtonState ___state, WaveMenu ___wm, WaveSetter __instance) {
			___state = ___wm.CheckWaveAvailability(__instance);
		}
	}
}
