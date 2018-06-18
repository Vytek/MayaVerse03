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
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class BodyAttachment : MonoBehaviour
	{
		public GameObject headPosition, headBone, neckBone, body;
		public Matrix4x4 headBindPose, neckBindPose;

		[HideInInspector]
		public int headBoneIdx = -1, neckBoneIdx = -1;

		private GameObject generatedHeadObject = null;

		public void RebuildBindpose ()
		{
			if (headBone != null && neckBone != null && headPosition != null) {
				headBindPose = headBone.transform.worldToLocalMatrix * headPosition.transform.localToWorldMatrix;
				neckBindPose = neckBone.transform.worldToLocalMatrix * headPosition.transform.localToWorldMatrix;
			} else
				Debug.LogErrorFormat ("Please initialize all the fields of BodyAttachment object!");
		}

		public void AttachHeadToBody (GameObject avatarHeadObject, string headObjectName = "HeadObject")
		{
			// delete all existing heads (if any)
			foreach (var obj in transform.parent.GetComponentsInChildren<Transform> ())
				if (obj.name == avatarHeadObject.name)
					GameObject.Destroy (obj.gameObject);

			if (body == null) {
				Debug.LogError ("Please specify main body mesh object");
				return;
			}

			var bodyMeshRenderer = body.GetComponentInChildren<SkinnedMeshRenderer> ();
			if (bodyMeshRenderer == null) {
				Debug.LogError ("Body does not contain skinned mesh renderer component");
				return;
			}

			var bodyBones = bodyMeshRenderer.bones;
			for (int i = 0; i < bodyBones.Length; ++i) {
				var bone = bodyBones [i];
				if (bone.name == headBone.name) {
					Debug.LogFormat ("Head bone name: {0}, idx: {1}", bone.name, i);
					headBoneIdx = i;
				}
				if (bone.name == neckBone.name) {
					Debug.LogFormat ("Neck bone name: {0}, idx: {1}", bone.name, i);
					neckBoneIdx = i;
				}
			}

			foreach (var avatarComponentTransform in avatarHeadObject.GetComponentsInChildren<Transform>()) {
				var avatarComponent = avatarComponentTransform.gameObject;

				var meshRenderer = avatarComponent.GetComponentInChildren<SkinnedMeshRenderer> ();
				if (meshRenderer == null) {
					Debug.LogError ("Could not find head skinned mesh renderer");
					return;
				}

				var originalHeadMesh = meshRenderer.sharedMesh;

				var mesh = new Mesh ();
				mesh.name = originalHeadMesh.name;
				mesh.vertices = originalHeadMesh.vertices;
				mesh.normals = originalHeadMesh.normals;
				mesh.uv = originalHeadMesh.uv;
				mesh.triangles = originalHeadMesh.triangles;
				mesh.RecalculateBounds ();

				//copy blendshapes
				for (int i = 0; i < originalHeadMesh.blendShapeCount; i++)
				{
					int vertexCount = originalHeadMesh.vertexCount;
					Vector3[] deltaVertices = new Vector3[vertexCount], deltaNormals = new Vector3[vertexCount], deltaTangents = new Vector3[vertexCount];
					originalHeadMesh.GetBlendShapeFrameVertices(i, 0, deltaVertices, deltaNormals, deltaTangents);
					mesh.AddBlendShapeFrame(originalHeadMesh.GetBlendShapeName(i), 100.0f, deltaVertices, deltaNormals, deltaTangents);
				}

				// extend the bottom edge loop of the neck downwards
				var minY = mesh.bounds.min.y;
				var borderEdgeLoops = TopologyUtils.GetBorderEdgeLoops (mesh);

				// find the lowest border edge loop, this is the model boundary at the bottom of the neck
				var vertices = mesh.vertices;

				if (avatarComponent.name == headObjectName) {
					var lowestBorderEdgeLoop = borderEdgeLoops [0];
					var lowestVertexY = vertices [lowestBorderEdgeLoop [0].v1].y;
					foreach (var borderEdgeLoop in borderEdgeLoops) {
						foreach (var edge in borderEdgeLoop) {
							float y = Math.Min (vertices [edge.v1].y, vertices [edge.v2].y);
							if (y < lowestVertexY) {
								lowestVertexY = y;
								lowestBorderEdgeLoop = borderEdgeLoop;
							}
						}
					}

					// shift all vertices of neck boundary downwards
					foreach (var edge in lowestBorderEdgeLoop) {
						vertices [edge.v1].y = minY;
						vertices [edge.v2].y = minY;
					}
					mesh.vertices = vertices;
					mesh.RecalculateBounds ();
				}

				// attach the mesh to head & neck bones
				var meshBoneWeights = new BoneWeight[mesh.vertices.Length];
				minY = mesh.bounds.min.y;
				var maxY = mesh.bounds.max.y;
				var height = maxY - minY;

				var neckBoneInfluence = 0.125f;  // bottom % of the vertices influenced by the neck bone

				float eps = 1e-5f;
				if (neckBoneIdx < 0)
					neckBoneInfluence = -eps;  // ignore neck bone if it's not specified

				if (avatarComponent.name != headObjectName)
					neckBoneInfluence = -eps;  // accessories and haircuts are not influenced by the neck

				var maxNeckBoneAttachY = minY + neckBoneInfluence * height;
				Debug.Assert (maxNeckBoneAttachY < mesh.bounds.max.y);

				for (int i = 0; i < meshBoneWeights.Length; ++i) {
					var heightFraction = (vertices [i].y - minY) / height;

					float headBoneWeight = 1, neckBoneWeight = 0;
					if (neckBoneInfluence > eps) {
						// linear interpolation between two weights, depending on the y-coordinate of the vertex in the original mesh
						headBoneWeight = (1.0f / neckBoneInfluence) * heightFraction;
						headBoneWeight = Math.Min (1.0f, headBoneWeight);
						headBoneWeight = Math.Max (0.0f, headBoneWeight);

						neckBoneWeight = -(1.0f / neckBoneInfluence) * heightFraction + 1.0f;
						neckBoneWeight = Math.Min (1.0f, neckBoneWeight);
						neckBoneWeight = Math.Max (0.0f, neckBoneWeight);
					}

					// let's make bone with higher weight the 1st bone
					if (neckBoneWeight > headBoneWeight) {
						meshBoneWeights [i].boneIndex0 = neckBoneIdx;
						meshBoneWeights [i].weight0 = neckBoneWeight;

						if (headBoneWeight > 0) {
							meshBoneWeights [i].boneIndex1 = headBoneIdx;
							meshBoneWeights [i].weight1 = headBoneWeight;
						}
					} else {
						meshBoneWeights [i].boneIndex0 = headBoneIdx;
						meshBoneWeights [i].weight0 = headBoneWeight;

						if (neckBoneWeight > 0) {
							meshBoneWeights [i].boneIndex1 = neckBoneIdx;
							meshBoneWeights [i].weight1 = neckBoneWeight;
						}
					}
				}
				mesh.boneWeights = meshBoneWeights;

				var bindposes = bodyMeshRenderer.sharedMesh.bindposes;
				bindposes [headBoneIdx] = headBindPose;
				bindposes [neckBoneIdx] = neckBindPose;
				mesh.bindposes = bindposes;

				var headBones = new Transform[bodyMeshRenderer.bones.Length];
				for (int i = 0; i < bodyMeshRenderer.bones.Length; ++i) {
					var bone = bodyMeshRenderer.bones [i];
					headBones [i] = bone;
				}

				meshRenderer.bones = headBones;
				meshRenderer.sharedMesh = mesh;
				meshRenderer.quality = SkinQuality.Auto;
				meshRenderer.rootBone = bodyMeshRenderer.rootBone;
			}

			avatarHeadObject.transform.SetParent (transform.parent);
			generatedHeadObject = avatarHeadObject;
		}

		public void ChangePosition (Dictionary<PositionType, PositionControl> positionControlsDict)
		{
			if (generatedHeadObject == null)
				return;

			var scaleControl = positionControlsDict [PositionType.SCALE];
			var scale = scaleControl.Value;
			var scaleVector = new Vector3 (scale, scale, scale);

			var x = positionControlsDict [PositionType.AXIS_X].Value;
			var y = positionControlsDict [PositionType.AXIS_Y].Value;
			var z = positionControlsDict [PositionType.AXIS_Z].Value;
			var translationVector = new Vector3 (x, y, z);

			var yaw = positionControlsDict [PositionType.YAW].Value;
			var pitch = positionControlsDict [PositionType.PITCH].Value;
			var roll = positionControlsDict [PositionType.ROLL].Value;
			var rotationQuaternion = Quaternion.Euler (new Vector3 (pitch, yaw, roll));

			foreach (var renderer in generatedHeadObject.GetComponentsInChildren<SkinnedMeshRenderer>()) {
				var mesh = renderer.sharedMesh;
				var bindposes = mesh.bindposes;

				var trsMatrix = Matrix4x4.TRS (translationVector, rotationQuaternion, scaleVector);
				var newHeadBindPose = headBindPose * trsMatrix;
				var newNeckBindPose = neckBindPose * trsMatrix;

				bindposes [headBoneIdx] = newHeadBindPose;
				bindposes [neckBoneIdx] = newNeckBindPose;
				mesh.bindposes = bindposes;
				mesh.RecalculateBounds ();
			}
		}
	}

	#if UNITY_EDITOR
	[CustomEditor (typeof(BodyAttachment))]
	public class BodyAttachmentEditor : Editor
	{
		public override void OnInspectorGUI ()
		{
			DrawDefaultInspector ();
			var bodyAttachment = (BodyAttachment)target;
			if (GUILayout.Button ("Rebuild Bindpose"))
				bodyAttachment.RebuildBindpose ();
		}
	}
	#endif
}

