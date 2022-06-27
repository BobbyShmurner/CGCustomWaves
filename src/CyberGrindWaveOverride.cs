using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace CustomWave
{
	public class CyberGrindWaveOverride : ICheat
	{
		private static CyberGrindWaveOverride _lastInstance;

		private bool active;

		public string LongName => "Wave Override";

		public string Identifier => "customwave.wave-override";

		public string ButtonEnabledOverride { get; }

		public string ButtonDisabledOverride { get; }

		public string Icon => "wave-override";

		public bool IsActive => active;

		public bool DefaultState { get; }

		public StatePersistenceMode PersistenceMode => StatePersistenceMode.Persistent;

		public void Enable()
		{
			active = true;
			_lastInstance = this;

			RefreshWaveSetters();
		}

		public void Disable()
		{
			active = false;
			RefreshWaveSetters();
		}

		public void Update() {}

		// Im using a function here because its easier to call using a transpiler
		public static bool GetActive() {
			if (_lastInstance != null)
			{
				return _lastInstance.active;
			}
			return false;
		}

		private void RefreshWaveSetters() {
			WaveMenu wm = GameObject.FindObjectOfType<WaveMenu>();
			if (wm == null) {
				Plugin.Log.LogWarning("Failed to find a Wave Menu, Wave Setters have not been refreshed");
				return;
			}

			typeof(WaveMenu).GetMethod("GetHighestWave", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(wm, new System.Object[]{});
			wm.SetCurrentWave((int)typeof(WaveMenu).GetField("currentWave", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wm));

			foreach (WaveSetter setter in wm.setters) {
				typeof(WaveSetter).GetMethod("Prepare", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(setter, new System.Object[]{});
			}
		}
	}
}
