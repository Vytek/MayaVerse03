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
using System.Collections;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	public class AsyncUtils
	{
		/// <summary>
		/// Await multiple AsyncRequests with a single line of code :)
		/// </summary>
		public static IEnumerator AwaitAll (params AsyncRequest[] requests)
		{
			bool allDone;
			do {
				allDone = true;
				foreach (var request in requests)
					if (!request.IsDone)
						allDone = false;
				yield return null;
			} while (!allDone);
		}
	}
}

