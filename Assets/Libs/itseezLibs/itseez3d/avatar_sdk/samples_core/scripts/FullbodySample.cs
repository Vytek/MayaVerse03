/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using ItSeez3D.AvatarSdk.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class FullbodySample : GettingStartedSample
	{
		public BodyAttachment[] bodyAttachments;

		protected override void Start()
		{
			base.Start();

			var headPositionManager = gameObject.GetComponentInChildren<HeadPositionManager>();
			headPositionManager.PositionChanged += (Dictionary<PositionType, PositionControl> controls) => {
				foreach (var bodyAttachment in bodyAttachments)
					bodyAttachment.ChangePosition(controls);
			};
		}

		protected override void DisplayHead(TexturedMesh headMesh, TexturedMesh haircutMesh)
		{
			if (pipelineType != PipelineType.FACE)
			{
				Debug.LogErrorFormat("Avatar from the {0} can't be used in Fullbody sample!", pipelineType);
				return;
			}

			// create parent avatar object in a scene, attach a script to it to allow rotation by mouse
			var avatarObject = new GameObject("ItSeez3D Avatar");

			// create head object in the scene
			{
				Debug.LogFormat("Generating Unity mesh object for head...");
				var meshObject = new GameObject("HeadObject");
				var meshRenderer = meshObject.AddComponent<SkinnedMeshRenderer>();
				meshRenderer.sharedMesh = headMesh.mesh;
				var material = new Material(Shader.Find("AvatarUnlitShader"));
				material.mainTexture = headMesh.texture;
				meshRenderer.material = material;
				meshObject.transform.SetParent(avatarObject.transform);
			}

			// create haircut object in the scene
			{
				var meshObject = new GameObject("HaircutObject");
				var meshRenderer = meshObject.AddComponent<SkinnedMeshRenderer>();
				meshRenderer.sharedMesh = haircutMesh.mesh;
				var material = new Material(Shader.Find("AvatarUnlitHairShader"));
				material.mainTexture = haircutMesh.texture;
				meshRenderer.material = material;
				meshObject.transform.SetParent(avatarObject.transform);
			}

			if (bodyAttachments == null || bodyAttachments.Length <= 0)
			{
				Debug.LogError("No body attachments specified!");
				return;
			}

			foreach (var bodyAttachment in bodyAttachments)
				bodyAttachment.AttachHeadToBody(GameObject.Instantiate(avatarObject));

			GameObject.Destroy(avatarObject);
		}
	}
}
