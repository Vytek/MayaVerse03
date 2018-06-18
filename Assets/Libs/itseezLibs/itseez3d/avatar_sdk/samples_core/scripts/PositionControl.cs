/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ItSeez3D.AvatarSdkSamples.Core
{
	public enum PositionType
	{
		SCALE,
		AXIS_X,
		AXIS_Y,
		AXIS_Z,
		YAW,
		PITCH,
		ROLL,
	}

	public class PositionControl : MonoBehaviour
	{
		public PositionType positionType;
		public Slider slider;

		private float initialValue;

		public delegate void PositionValueHandler ();

		public event PositionValueHandler ValueChanged;

		public float Value { get { return slider.value; } }

		public float MinValue { get { return slider.minValue; } }

		public float MaxValue { get { return slider.maxValue; } }

		void Start ()
		{
			if (slider != null)
				initialValue = slider.value;
			ResetValue ();
		}

		public void OnValueChanged (float value)
		{
			if (ValueChanged != null)
				ValueChanged ();
		}

		public void ResetValue ()
		{
			if (slider != null) {
				slider.value = initialValue;
				if (ValueChanged != null)
					ValueChanged ();
			}
		}
	}
}