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
	public class BlendshapeReader
	{
		private Vector3[] reusableBuffer = null;
		private int[] indexMap;

		public BlendshapeReader(int[] _indexMap)
		{
			indexMap = _indexMap;
		}

		public Vector3[] ReadVerticesDeltas (string blendshapeFilename, bool leftHandedCoordinates = true)
		{
			var buffer = File.ReadAllBytes (blendshapeFilename);

			Vector3[] deltas, finalDeltas;
			unsafe {
				int vecSize = sizeof (Vector3);
				int numDeltas = buffer.Length / vecSize;

				if (reusableBuffer == null || reusableBuffer.Length != numDeltas) {
					Debug.LogFormat ("Allocate reusable blendshape buffer for {0}", blendshapeFilename);
					reusableBuffer = new Vector3[numDeltas];
				}
				deltas = reusableBuffer;

				fixed (byte* bytePtr = &buffer[0]) {
					for (int i = 0; i < numDeltas; ++i) {
						float* ptr = (float*)(bytePtr + i * vecSize);
						deltas[i].x = -(*ptr);
						deltas[i].y = *(ptr + 1);
						deltas[i].z = *(ptr + 2);
					}
				}
			}

			if (!leftHandedCoordinates)
				for (int i = 0; i < deltas.Length; ++i)
					deltas[i].x *= -1;

			finalDeltas = new Vector3[indexMap.Length];
			Debug.Assert (finalDeltas.Length >= deltas.Length, "indexMap array has incorrect length");

			for (int i = 0; i < finalDeltas.Length; ++i)
				finalDeltas[i] = deltas [indexMap [i]];

			return finalDeltas;
		}
	}
}

