/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using System;
using System.Collections.Generic;
using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdkSamples.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class HaircutRecoloring : MonoBehaviour
	{
		#region UI

		public GameObject colorPickerPanel;

		#endregion

		public ColorPicker colorPicker;

		private AvatarViewer avatarViewer = null;

		private Color averageColor = Color.clear;

		public Color CurrentColor { get; private set; }

		public Vector4 CurrentTint { get; private set; }

		void Start ()
		{
			if (colorPicker == null)
				Debug.LogWarning ("Color picker is not set!");
			else
				colorPicker.SetOnValueChangeCallback (OnColorChange);

			avatarViewer = GetComponent<AvatarViewer>();

			if (avatarViewer == null) {
				Debug.LogWarning ("Avatar viewer reference is null");
				return;
			}

			avatarViewer.displayedHaircutChanged += OnHaircutChanged;
			avatarViewer.shaderTypeChanged += OnShaderChanged;
		}

		void OnDestroy ()
		{
			if (avatarViewer != null)
				avatarViewer.displayedHaircutChanged -= OnHaircutChanged;
			Debug.LogFormat ("Haircut recolorer destroyed");
		}

		private void CalculateHaircutParameters ()
		{
			var haircutObject = avatarViewer.HaircutObject;
			if (haircutObject == null)
				return;

			var hairMeshRenderer = haircutObject.GetComponent<MeshRenderer> ();
			averageColor = CoreTools.CalculateAverageColor (hairMeshRenderer.material.mainTexture as Texture2D);
			Debug.LogFormat ("Haircut average color: {0}", averageColor.ToString ());
		}

		public void ResetTint ()
		{
			colorPicker.Color = averageColor;
		}

		private bool EnableRecoloring ()
		{
			var haircutObject = avatarViewer.HaircutObject;
			return haircutObject != null;
		}

		private void UpdateRecoloring ()
		{
			bool enable = EnableRecoloring ();
			CalculateHaircutParameters ();
			ResetTint ();
			colorPickerPanel.SetActive (enable);
		}

		private void OnHaircutChanged (string newHaircutId)
		{
			UpdateRecoloring ();
		}

		private void OnShaderChanged (bool isUnlit)
		{
			UpdateRecoloring ();
		}

		private void OnColorChange (Color color)
		{
			var haircutObject = avatarViewer.HaircutObject;
			if (haircutObject == null)
				return;

			var hairMeshRenderer = haircutObject.GetComponent<MeshRenderer> ();

			CurrentColor = color;
			CurrentTint = CoreTools.CalculateTint (color, averageColor);
			hairMeshRenderer.material.SetVector ("_ColorTarget", color);
			hairMeshRenderer.material.SetVector ("_ColorTint", CurrentTint);
			hairMeshRenderer.material.SetFloat ("_TintCoeff", 0.8f);
		}
	}
}
