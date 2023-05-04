﻿using System;
using System.Linq;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;

using UnityEngine;

namespace CGCustomWaves
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony harmony;

        internal static ConfigEntry<int> ConfigCustomWave;
        internal static WaveSetter CustomWaveSetter;
        internal static Sprite CheatIcon;
        
        private void Awake()
        {
            Logger.LogInfo($"Loading Plugin {PluginInfo.PLUGIN_GUID}...");

            Plugin.Log = base.Logger;
            Plugin.Log.LogInfo("Created Global Logger");

            ConfigCustomWave = Config.Bind("General", "CGCustomWaves", 1, "The value of the Custom Wave Setter");
            Plugin.Log.LogInfo("Setup Config");

            byte[] imageBytes = Convert.FromBase64String(CheatIconStr);
            Texture2D tex = new Texture2D(32, 32);
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(imageBytes);

            CheatIcon = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

            harmony = new Harmony("CGCustomWaves");
            harmony.PatchAll();

            Plugin.Log.LogInfo($"Applied {harmony.GetPatchedMethods().Count()} Patches");

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }

        internal static string CGSceneName = "9240e656c89994d44b21940f65ab57da";

        // Dont worry about it :)
        internal static string CheatIconStr = "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Ly9ucy5hZG9iZS5jb20vdGlmZi8xLjAvIgogICAgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIgogICB4bXBNTTpEb2N1bWVudElEPSJnaW1wOmRvY2lkOmdpbXA6MzM1NzQ5YzMtNTdmNi00NDJhLTg5M2ItNTRlYzliZTE2ZjNlIgogICB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjM1YmZjZGI5LWU3NWMtNGMxOS1hMWI4LWI5ODE0OTI4MTAxZCIKICAgeG1wTU06T3JpZ2luYWxEb2N1bWVudElEPSJ4bXAuZGlkOjc3YmFkYTNiLTMwMzUtNDBmZi1iMTA3LTA2N2Y0YzNlYjUyMSIKICAgZGM6Rm9ybWF0PSJpbWFnZS9wbmciCiAgIEdJTVA6QVBJPSIyLjAiCiAgIEdJTVA6UGxhdGZvcm09IkxpbnV4IgogICBHSU1QOlRpbWVTdGFtcD0iMTY1NjM0NDE5MzM4MDExNSIKICAgR0lNUDpWZXJzaW9uPSIyLjEwLjMyIgogICB0aWZmOk9yaWVudGF0aW9uPSIxIgogICB4bXA6Q3JlYXRvclRvb2w9IkdJTVAgMi4xMCIKICAgeG1wOk1ldGFkYXRhRGF0ZT0iMjAyMjowNjoyN1QxNjozNjozMSswMTowMCIKICAgeG1wOk1vZGlmeURhdGU9IjIwMjI6MDY6MjdUMTY6MzY6MzErMDE6MDAiPgogICA8eG1wTU06SGlzdG9yeT4KICAgIDxyZGY6U2VxPgogICAgIDxyZGY6bGkKICAgICAgc3RFdnQ6YWN0aW9uPSJzYXZlZCIKICAgICAgc3RFdnQ6Y2hhbmdlZD0iLyIKICAgICAgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDplODgwOGVkZi03MjBmLTQ3YjUtYTE1Mi00NWFhNDYwNDBjNTYiCiAgICAgIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkdpbXAgMi4xMCAoTGludXgpIgogICAgICBzdEV2dDp3aGVuPSIyMDIyLTA2LTI3VDE2OjM2OjMzKzAxOjAwIi8+CiAgICA8L3JkZjpTZXE+CiAgIDwveG1wTU06SGlzdG9yeT4KICA8L3JkZjpEZXNjcmlwdGlvbj4KIDwvcmRmOlJERj4KPC94OnhtcG1ldGE+CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAKPD94cGFja2V0IGVuZD0idyI/PpmfW8wAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAIFSURBVHja7dxbjoMwDADAsuL+V2Z/FqldAeXhgJPMfKI+XDB2EiivFwAAAAAAAAAAAAAAUKuh9BdM0zR9fOEwDNHvufv183uWth8+ADv2R0ljzdm7dgDm7U/v3Br82AV9G1s685deV3MVONM+m0+Ao3337nZQW8JpAcYA9Zz5W2f/8CeqchgD1DKP/XfQo6ZnGX+bFoAKsOcMmbc/UQn2fGemgeJjCbC2uuacNAhECyizJoAKQGvTwMhKsjQGOVplrASiAtRcCZ6cQn6rPk1cDNrasdHldu3ztlYLv8XQ2lRVC9AC+nSm1J+5nS37oHG8Y0df6aeld9Ce+J46SC4G0cYYYOta/db2O+/iORofAAAAAABUImydO/IOmqj196iYssUTGZOrgZ2TAFpAjpIWVepKx5QtnqvtQAXQAtACgkvamZJ05XNLxJQtnlKfqwJoAfQs7LbwqwsT7+/PsICTLZ5SMakAWgASAAmABMAs4Lz3UWmWv1NliynjPlIBtAB6lv5y8EewBR7Q0FI8Z1qLCqAFoAUEj3CzlDd3B6kASABuaQGRth7wKJ7YmFQALQAJgARAAiAB6M3h+wFanhK1PG1UAZAABLSAPWWv1P/e7owpWzylYlIBtAB65gkhlcdzdXahAmgBaAHBI9wnS1qJmDwnEC0AAAAAAAAAAAAAAAAAAEjhF7ccNMtwLDCzAAAAAElFTkSuQmCC";
    }
}
