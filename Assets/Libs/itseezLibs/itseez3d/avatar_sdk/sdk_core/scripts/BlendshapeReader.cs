/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using UnityEngine;
using System.IO;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Read blendshapes from a simple binary format.
	/// </summary>
	public static class BlendshapeReader
	{
		public static Vector3[] ReadVerticesDeltas (string blendshapeFilename, int[] indexMap, bool leftHandedCoordinates = true)
		{
			Vector3[] deltas, finalDeltas;

			using (var fs = File.OpenRead (blendshapeFilename)) {
				long arraySize = fs.Length / (sizeof(float) * 3);
				deltas = new Vector3[arraySize];
				using (var br = new BinaryReader (fs)) {
					for (long vIdx = 0; vIdx < arraySize; ++vIdx) {
						deltas [vIdx].x = -br.ReadSingle ();
						deltas [vIdx].y = br.ReadSingle ();
						deltas [vIdx].z = br.ReadSingle ();
						if (!leftHandedCoordinates)
							deltas [vIdx].x *= -1.0f;
					}
				}
			}

			finalDeltas = new Vector3[indexMap.Length];
			Debug.Assert (finalDeltas.Length >= deltas.Length, "indexMap array has incorrect length");

			for (int i = 0; i < finalDeltas.Length; ++i) {
				Debug.AssertFormat (indexMap [i] < deltas.Length, "Original vertex index is too big: {0}", indexMap [i]);
				finalDeltas [i] = deltas [indexMap [i]];
			}

			return finalDeltas;
		}
	}
}

