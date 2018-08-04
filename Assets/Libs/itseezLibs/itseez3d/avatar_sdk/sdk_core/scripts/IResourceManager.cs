/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	public interface IResourceManager 
	{
		/// <summary>
		/// Returns requested resources for pipeline.
		/// There are three sets of resources.
		/// ALL_PUBLIC - public resources that available for all customers. This set doesn't include custom resources!
		/// DEFAULT - default set of public resources
		/// CUSTOM - unique custom resources that available only for your 
		/// </summary>
		AsyncRequest<AvatarResources> GetResourcesAsync(AvatarResourcesSubset resourcesSubset, PipelineType pipelineType);
	}

	/// <summary>
	/// Base class that contains common methods for OfflineResourceManager and CloudResourceManager
	/// </summary>
	public abstract class ResourceManager : IResourceManager
	{
		protected const string BLENDSHAPES_KEY = "blendshapes";
		protected const string HAIRCUTS_KEY = "haircuts";

		public abstract AsyncRequest<AvatarResources> GetResourcesAsync(AvatarResourcesSubset resourcesSubset, PipelineType pipelineType);

		/// <summary>
		/// Parses JSON to AvatarResources
		/// </summary>
		protected AvatarResources GetResourcesFromJson(string json)
		{
			AvatarResources avatarResources = AvatarResources.Empty;
			var rootNode = JSON.Parse(json);
			if (rootNode != null)
			{
				var blendshapesRootNode = FindNodeByName(rootNode, BLENDSHAPES_KEY);
				if (blendshapesRootNode != null)
					avatarResources.blendshapes = JsonNodeToResourceList(blendshapesRootNode);

				var haircutsRootNode = FindNodeByName(rootNode, HAIRCUTS_KEY);
				if (haircutsRootNode != null)
					avatarResources.haircuts = JsonNodeToResourceList(haircutsRootNode);

			}
			return avatarResources;
		}

		/// <summary>
		/// Parses JSON node to list of resources
		/// </summary>
		protected List<string> JsonNodeToResourceList(JSONNode node)
		{
			List<string> resourcesList = new List<string>();
			foreach (var tag in node.Keys)
			{
				var resourceArray = node[tag.Value];
				foreach (var resource in resourceArray)
					resourcesList.Add(string.Format("{0}/{1}", tag.Value, resource.Value.ToString().Replace("\"", "")));
			}
			return resourcesList;
		}

		/// <summary>
		/// Recursive finds Node with the given name in JSON
		/// </summary>
		protected JSONNode FindNodeByName(JSONNode rootNode, string name)
		{
			if (rootNode == null)
				return null;

			var node = rootNode[name];
			if (node != null)
				return node;

			foreach (JSONNode childNode in rootNode.Children)
			{
				node = FindNodeByName(childNode, name);
				if (node != null)
					return node;
			}

			return null;
		}
	}
}
