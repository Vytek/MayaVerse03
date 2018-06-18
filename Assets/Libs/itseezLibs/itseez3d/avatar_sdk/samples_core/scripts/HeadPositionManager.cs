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

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class HeadPositionManager : MonoBehaviour
	{
		public GameObject positionControlsPanel;

		public delegate void PositionChangedHandler (Dictionary<PositionType, PositionControl> controls);

		public event PositionChangedHandler PositionChanged;

		private Dictionary<PositionType, PositionControl> positionControlsDict = new Dictionary<PositionType, PositionControl> ();

		void Start ()
		{
			if (positionControlsPanel == null) {
				Debug.LogError ("Please set position controls for body attachment");
				return;
			}

			foreach (var positionControl in positionControlsPanel.GetComponentsInChildren<PositionControl>()) {
				positionControl.ValueChanged += PositionValueChanged;
				positionControlsDict [positionControl.positionType] = positionControl;
			}
		}

		private void PositionValueChanged ()
		{
			if (PositionChanged != null)
				PositionChanged (positionControlsDict);
		}

		public void AttachHeadToBody (GameObject avatarHeadObject)
		{
		}
	}
}