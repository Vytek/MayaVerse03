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
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// This class is used to provide different implementations of interfaces for Cloud and Offline versions of SDK 
	/// </summary>
	public class IoCContainer
	{
		SdkType sdkType = SdkType.Cloud;

		Dictionary<Type, string> cloudImplementations = new Dictionary<Type, string>()
		{
			{ typeof(IAvatarProvider), "ItSeez3D.AvatarSdk.Cloud.CloudAvatarProvider" },
			{ typeof(IMeshConverter), "ItSeez3D.AvatarSdk.Core.CoreMeshConverter" }
		};

		Dictionary<Type, string> offlineImplementations = new Dictionary<Type, string>()
		{
			{ typeof(IAvatarProvider), "ItSeez3D.AvatarSdk.Offline.OfflineAvatarProvider" },
			{ typeof(IMeshConverter), "ItSeez3D.AvatarSdk.Offline.OfflineMeshConverter" }
		};

		public IoCContainer(SdkType sdkType)
		{
			this.sdkType = sdkType;
		}

		public T Create<T>()
		{
			Type type = typeof(T);
			var currentImplementations = GetCurrentImplementations();
			if (currentImplementations.ContainsKey(type))
			{
				string className =  currentImplementations[type];
				Assembly assembly = Assembly.GetExecutingAssembly();
				Type implType = assembly.GetType(className);
				if (implType == null)
				{
					Debug.LogErrorFormat("Unable to create instance of: {0}", implType);
					return default(T);
				}
				return (T)Activator.CreateInstance(implType);
			}
			else
			{
				Debug.LogErrorFormat("There is no implementation for: {0}", type);
				return default(T);
			}
		}

		private Dictionary<Type, string> GetCurrentImplementations()
		{
			if (sdkType == SdkType.Offline)
				return offlineImplementations;
			else
				return cloudImplementations;
		}
	}
}
