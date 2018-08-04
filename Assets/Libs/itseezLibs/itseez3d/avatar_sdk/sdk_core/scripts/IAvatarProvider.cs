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
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	/// High-level interface that provides uniform code to work with avatars for both Cloud and Offline SDK.
	public interface IAvatarProvider : IDisposable
	{
		/// <summary>
		/// Performs SDK initialization.
		/// </summary>
		AsyncRequest InitializeAsync();

		/// <summary>
		/// Initializes avatar and prepares for calculation.
		/// </summary>
		/// <param name="photoBytes">Photo bytes (jpg or png encoded).</param>
		/// <param name="name">Name of the avatar</param>
		/// <param name="description">Description of the avatar</param>
		/// <param name="pipeline">Calculation pipeline to use, see PipelineType</param>
		/// <param name="avatarResources">Resources that will be generated for this avatar</param>
		/// <returns>Avatar code</returns>
		AsyncRequest<string> InitializeAvatarAsync(byte[] photoBytes, string name, string description, PipelineType pipeline = PipelineType.FACE,
			AvatarResources avatarResources = null);

		/// <summary>
		/// Starts and waits while avatar is being calculated.
		/// </summary>
		AsyncRequest StartAndAwaitAvatarCalculationAsync(string avatarCode);

		/// <summary>
		/// Moves files from the server to local storage if it is required.
		/// </summary>
		AsyncRequest MoveAvatarModelToLocalStorageAsync(string avatarCode, bool withHaircutPointClouds, bool withBlendshapes);

		/// <summary>
		/// Makes TexturedMesh with generated head.
		/// </summary>
		/// <param name="detailsLevel">Level of mesh details in range [0..3]</param>
		AsyncRequest<TexturedMesh> GetHeadMeshAsync(string avatarCode, bool withBlendshapes, int detailsLevel = 0);

		/// <summary>
		/// Returns list with haircut identities available for this avatar.
		/// </summary>
		AsyncRequest<string[]> GetHaircutsIdAsync(string avatarCode);

		/// <summary>
		/// Makes TexturedMesh with haircut.
		/// </summary>
		AsyncRequest<TexturedMesh> GetHaircutMeshAsync(string avatarCode, string haircutId);

		/// <summary>
		/// Returns haircut preview image as bytes array
		/// </summary>
		AsyncRequest<byte[]> GetHaircutPreviewAsync(string avatarCode, string haircutId);

		/// <summary>
		/// Returns list of avatars identities created by the current user.
		/// </summary>
		AsyncRequest<string[]> GetAllAvatarsAsync(int maxItems);

		/// <summary>
		/// Deletes avatar.
		/// </summary>
		AsyncRequest DeleteAvatarAsync(string avatarCode);

		/// <summary>
		/// Returns resource manager
		/// </summary>
		IResourceManager ResourceManager { get; }
	}
}
