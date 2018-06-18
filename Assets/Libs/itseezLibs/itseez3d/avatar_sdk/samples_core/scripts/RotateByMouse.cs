/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// This behavior is added to avatar heads in samples to allow rotation around y-axis by mouse.
	/// Does not work well on mobile, but okay for a sample.
	/// </summary>
	public class RotateByMouse : MonoBehaviour
	{
		private Vector2 lastPosition;

		void Update ()
		{
			if (EventSystem.current.IsPointerOverGameObject () || IsPointerOverUIObject())
				return;

			#if !UNITY_WEBGL
			if (Input.touchSupported)
			{
				if (Input.touches.Length != 1)
					return;

				Touch t = Input.touches[0];
				if (t.phase == TouchPhase.Moved)
				{
					Vector2 delta = t.position - lastPosition;
					transform.Rotate(Vector3.up, -0.5f * delta.x);
				}
				lastPosition = t.position;
			}
			else
			#endif
			{
				if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
				{
					var dx = Input.GetAxis("Mouse X");
					transform.Rotate(Vector3.up, -dx * 5);
				}
			}
		}

		private bool IsPointerOverUIObject()
		{
			PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
			{
				position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
			};
			List<RaycastResult> results = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
			return results.Count > 0;
		}
	}
}