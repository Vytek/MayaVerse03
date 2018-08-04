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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Cloud
{
	/// <summary>
	/// Resource manager for Cloud SDK
	/// </summary>
	public class CloudResourceManager : ResourceManager
	{
		private Connection connection = null;

		// Cached resources
		private Dictionary<PipelineType, Dictionary<AvatarResourcesSubset, AvatarResources>> avatarResourcesCache = null;

		public CloudResourceManager(Connection connection)
		{
			this.connection = connection;
			avatarResourcesCache = new Dictionary<PipelineType, Dictionary<AvatarResourcesSubset, AvatarResources>>();
			avatarResourcesCache.Add(PipelineType.FACE, new Dictionary<AvatarResourcesSubset, AvatarResources>());
			avatarResourcesCache.Add(PipelineType.HEAD, new Dictionary<AvatarResourcesSubset, AvatarResources>());
		}

		public override AsyncRequest<AvatarResources> GetResourcesAsync(AvatarResourcesSubset resourcesSubset, PipelineType pipelineType)
		{
			var request = new AsyncRequest<AvatarResources>(AvatarSdkMgr.Str(Strings.GettingResourcesList));
			AvatarSdkMgr.SpawnCoroutine(GetResourcesFunc(resourcesSubset, pipelineType, request));
			return request;
		}

		private IEnumerator GetResourcesFunc(AvatarResourcesSubset resourcesSubset, PipelineType pipelineType, AsyncRequest<AvatarResources> request)
		{
			if (avatarResourcesCache[pipelineType].ContainsKey(resourcesSubset))
			{
				request.Result = avatarResourcesCache[pipelineType][resourcesSubset];
				request.IsDone = true;
			}
			else
			{
				var resourcesWebRequest = connection.GetResourcesAsync(pipelineType, resourcesSubset);
				yield return resourcesWebRequest;
				if (resourcesWebRequest.IsError)
				{
					Debug.LogError(resourcesWebRequest.ErrorMessage);
					request.SetError(resourcesWebRequest.ErrorMessage);
					yield break;
				}
				AvatarResources avatarResources = GetResourcesFromJson(resourcesWebRequest.Result);
				avatarResourcesCache[pipelineType].Add(resourcesSubset, avatarResources);
				request.IsDone = true;
				request.Result = avatarResources;
			}
		}
	}
}
