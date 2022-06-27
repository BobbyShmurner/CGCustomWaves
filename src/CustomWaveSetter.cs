using System.Reflection;
using UnityEngine;

namespace CustomWave {
	class CustomWaveSetter : MonoBehaviour {
		public GameObject buttonSuccess;
		public GameObject buttonFail;

		public WaveSetter customButton;

		public int changeValue;

		ControllerPointer pointer;

		void Awake()
		{
			if (!TryGetComponent<ControllerPointer>(out pointer))
			{
				pointer = base.gameObject.AddComponent<ControllerPointer>();
			}
			pointer.OnPressed.AddListener(OnPointerClick);
		}

		void OnPointerClick() {
			WaveMenu wm = customButton.GetComponentInParent<WaveMenu>();

			customButton.wave = Mathf.Max(1, customButton.wave + changeValue);
			if (!CyberGrindWaveOverride.GetActive()) {
				int highestWave = (int)typeof(WaveMenu).GetField("highestWave", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(wm);
				customButton.wave = Mathf.Min(Mathf.Min(customButton.wave * 2, highestWave - (highestWave % 10)), 50) / 2;
			}

			wm.SetCurrentWave(customButton.wave);

			typeof(WaveSetter).GetMethod("Prepare", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(customButton, new Object[]{});

			Object.Instantiate(buttonSuccess);
		}
	}
}