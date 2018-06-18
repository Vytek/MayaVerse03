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
using System.IO;
using System.Threading;
using ItSeez3D.AvatarSdk.Core;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Cloud
{
	/// <summary>
	/// This class is a collection of static methods that you'll need when you work with avatars.
	/// There are two groups of methods:
	/// 1) Low-level API methods that don't require a connection to server (e.g. load avatar files to/from disk).
	/// 2) High-level API methods, basically composite requests consisting of multiple low-level requests that may
	/// or may not require a connection. For example, DownloadAndSaveAvatarModelAsync uses multiple lower-level
	/// requests to download avatar files, unzip them and save to disk.
	/// You're encouraged to use High-level API functions when you're fine with the pipeline implemented within them.
	/// If you need finer control over the implementation of a particular task, you should consider these functions
	/// as samples and create your own methods based on SDK implementation in a separate class/file
	/// (e.g. by using different order of requests, more/less parallel requests, saving more results along the way, etc.)
	/// It is not recommended to modify this file, you'll have difficulties upgrading to new SDK version.
	/// 
	/// PLEASE NOTE! THIS CLASS IS DEPRECATED AND WILL BE REMOVED IN FUTURE VERSIONS (SOON).
	/// Use AvatarProvider High-Level API instead.
	/// 
	/// </summary>
	[Obsolete("Use CloudAvatarProvider instead")]
	public static class CloudTools
	{
		#region Helpers specific for "cloud" SDK

		/// <summary>
		/// Process blendshapes slightly differently compared to other zips (for compatibility reasons).
		/// Blendshapes are unzipped not just in avatar directory, but in their own personal folder.
		/// </summary>
		/// <param name="blendshapesZip">Full path to blendshapes zip archive.</param>
		/// <param name="avatarId">Avatar identifier to determine the correct unzip location.</param>
		public static AsyncRequest<string> UnzipBlendshapes (string blendshapesZip, string avatarId)
		{
			Debug.LogWarning("This method is obsolete. Use CloudAvatarProvider instead.");
			var blendshapesDir = AvatarSdkMgr.Storage ().GetAvatarSubdirectory (avatarId, AvatarSubdirectory.BLENDSHAPES);
			return CoreTools.UnzipFileAsync (blendshapesZip, blendshapesDir);
		}

		#endregion

		#region Higher-level API (composite requests)

		/// <summary>
		/// GenerateAndSaveAvatarAsync implementation.
		/// </summary>
		private static IEnumerator GenerateAndSaveAvatarFunc (
			Connection connection,
			string name,
			string description,
			byte[] photoBytes,
			bool withHaircutPointClouds,
			bool withBlendshapes,
			bool forcePowerOfTwoTexture,
			AsyncRequest<AvatarData> request
		)
		{
			// uploading photo and registering new avatar on the server
			var createAvatar = connection.CreateAvatarWithPhotoAsync (name, description, photoBytes, forcePowerOfTwoTexture);

			// Wait until async request is completed (without blocking the main thread).
			// Instead of using AwaitSubrequest we could just use `yield return createAvatar;`
			// AwaitSubrequest is a helper function that allows to track progress on composite
			// requests automatically. It also provides info for the caller about current subrequest
			// (and it's progress) and propagetes error from subrequest to the parent request.
			// finalProgress is a value between 0 and 1, a desired progress of parent request when given
			// subrequest is completed.
			yield return request.AwaitSubrequest (createAvatar, finalProgress: 0.19f);

			// must check whether request was successful before proceeding
			if (request.IsError)
				yield break;

			// Result field contains, well, result of the request. In this case it's an AvatarData object.
			AvatarData avatar = createAvatar.Result;

			// save photo for later use
			var savePhoto = CoreTools.SaveAvatarFileAsync (photoBytes, avatar.code, AvatarFile.PHOTO);
			yield return request.AwaitSubrequest (savePhoto, finalProgress: 0.2f);

			// again, must check for the error, there's no point in proceeding otherwise
			if (request.IsError)
				yield break;

			// Server starts calculating 3D shape and texture after photo has been uploaded.
			// Now we must wait until calculations finish.
			var awaitCalculations = connection.AwaitAvatarCalculationsAsync (avatar);
			yield return request.AwaitSubrequest (awaitCalculations, finalProgress: 0.95f);
			if (request.IsError)
				yield break;

			// calculations finished, update avatar info from the latest result
			avatar = awaitCalculations.Result;

			// download, save and unzip all files
			var downloadAndSave = DownloadAndSaveAvatarModelAsync (connection, avatar, withHaircutPointClouds, withBlendshapes);
			yield return request.AwaitSubrequest (downloadAndSave, finalProgress: 1);
			if (request.IsError)
				yield break;

			// At this point we have all avatar files stored in the filesystem ready for being loaded and displayed.
			// Our job is considered done here.
			request.Result = avatar;
			request.IsDone = true;
		}

		/// <summary>
		/// Upload photo to server, wait until avatar is calculated, download and save the results.
		/// If this function does not suit the requirements of your application precisely, feel free to use it as
		/// a sample and implement your own version in a separate class (for finer control over details).
		/// </summary>
		/// <param name="connection">Connection session.</param>
		/// <param name="name">Name of avatar.</param>
		/// <param name="description">Description of avatar.</param>
		/// <param name="photoBytes">Photo bytes (jpg or png encoded).</param>
		/// <param name="withHaircutPointClouds">If set to true then download all haircut point clouds too.</param>
		/// <param name="withBlendshapes">If set to true then download blendshapes too.</param>
		/// <param name="forcePowerOfTwoTexture">In case of true, generated texture resolution will be power of 2</param>
		public static AsyncRequest<AvatarData> GenerateAndSaveAvatarAsync (
			Connection connection,
			string name,
			string description,
			byte[] photoBytes,
			bool withHaircutPointClouds,
			bool withBlendshapes,
			bool forcePowerOfTwoTexture = false
		)
		{
			Debug.LogWarning("This method is obsolete. Use CloudAvatarProvider instead.");
			var request = new AsyncRequest<AvatarData> (AvatarSdkMgr.Str (Strings.GeneratingAvatar));
			AvatarSdkMgr.SpawnCoroutine (GenerateAndSaveAvatarFunc (
				connection, name, description, photoBytes, withHaircutPointClouds, withBlendshapes, forcePowerOfTwoTexture, request
			));
			return request;
		}

		/// <summary>
		/// DownloadAndSaveAvatarModelAsync implementation.
		/// </summary>
		private static IEnumerator DownloadAndSaveAvatarModel (
			Connection connection,
			AvatarData avatar,
			bool withHaircutPointClouds,
			bool withBlendshapes,
			AsyncRequest<AvatarData> request
		)
		{
			// By initializing multiple requests at the same time (without yield between them) we're
			// starting them in parallel. In this particular case we're downloading multiple files at the same time,
			// which is usually a bit faster than sequential download.
			var meshZip = connection.DownloadMeshZipAsync (avatar);
			var textureRequest = connection.DownloadTextureBytesAsync (avatar);

			var download = new List<AsyncRequest> { meshZip, textureRequest };
			AsyncWebRequest<byte[]> allHaircutPointCloudsZip = null, blendshapesZip = null;

			#if BLENDSHAPES_IN_PLY_OR_FBX
			// just a sample of how to get blendshapes in a different format

			AsyncWebRequest<byte[]> blendshapesZipFbx = null, blendshapesZipPly = null;
			#endif

			if (withHaircutPointClouds) {
				allHaircutPointCloudsZip = connection.DownloadAllHaircutPointCloudsZipAsync (avatar);
				download.Add (allHaircutPointCloudsZip);
			}

			if (withBlendshapes) {
				blendshapesZip = connection.DownloadBlendshapesZipAsync (avatar);
				download.Add (blendshapesZip);

				#if BLENDSHAPES_IN_PLY_OR_FBX
				// just a sample of how to get blendshapes in a different format

				blendshapesZipFbx = connection.DownloadBlendshapesZipAsync (avatar, BlendshapesFormat.FBX);
				download.Add (blendshapesZipFbx);

				blendshapesZipPly = connection.DownloadBlendshapesZipAsync (avatar, BlendshapesFormat.PLY);
				download.Add (blendshapesZipPly);
				#endif
			}

			// continue execution when all requests finish
			yield return request.AwaitSubrequests (0.9f, download.ToArray ());
			// return if any of the requests failed
			if (request.IsError)
				yield break;

			// save all the results to disk, also in parallel
			var saveMeshZip = CoreTools.SaveAvatarFileAsync (meshZip.Result, avatar.code, AvatarFile.MESH_ZIP);
			var saveTexture = CoreTools.SaveAvatarFileAsync (textureRequest.Result, avatar.code, AvatarFile.TEXTURE);

			var save = new List<AsyncRequest> () { saveMeshZip, saveTexture };
			AsyncRequest<string> saveHaircutPointsZip = null, saveBlendshapesZip = null;
			if (allHaircutPointCloudsZip != null) {
				saveHaircutPointsZip = CoreTools.SaveAvatarFileAsync (allHaircutPointCloudsZip.Result, avatar.code, AvatarFile.ALL_HAIRCUT_POINTS_ZIP);
				save.Add (saveHaircutPointsZip);
			}

			if (blendshapesZip != null) {
				saveBlendshapesZip = CoreTools.SaveAvatarFileAsync (blendshapesZip.Result, avatar.code, AvatarFile.BLENDSHAPES_ZIP);
				save.Add (saveBlendshapesZip);
			}

			#if BLENDSHAPES_IN_PLY_OR_FBX
			// just a sample of how to get blendshapes in a different format

			if (blendshapesZipFbx != null) {
				var saveBlendshapesZipFbx = CoreTools.SaveAvatarFileAsync (blendshapesZipFbx.Result, avatar.code, AvatarFile.BLENDSHAPES_FBX_ZIP);
				save.Add (saveBlendshapesZipFbx);
			}

			if (blendshapesZipPly != null) {
				var saveBlendshapesZipPly = CoreTools.SaveAvatarFileAsync (blendshapesZipPly.Result, avatar.code, AvatarFile.BLENDSHAPES_PLY_ZIP);
				save.Add (saveBlendshapesZipPly);
			}
			#endif

			yield return request.AwaitSubrequests (0.95f, save.ToArray ());
			if (request.IsError)
				yield break;

			var unzipMesh = CoreTools.UnzipFileAsync (saveMeshZip.Result);

			var unzip = new List<AsyncRequest> () { unzipMesh };
			AsyncRequest<string> unzipHaircutPoints = null, unzipBlendshapes = null;
			if (saveHaircutPointsZip != null) {
				unzipHaircutPoints = CoreTools.UnzipFileAsync (saveHaircutPointsZip.Result);
				unzip.Add (unzipHaircutPoints);
			}
			if (saveBlendshapesZip != null) {
				unzipBlendshapes = UnzipBlendshapes (saveBlendshapesZip.Result, avatar.code);
				unzip.Add (unzipBlendshapes);
			}

			yield return request.AwaitSubrequests (0.99f, unzip.ToArray ());
			if (request.IsError)
				yield break;

			// delete all .zip files we don't need anymore
			try {
				foreach (var fileToDelete in new AvatarFile[] { AvatarFile.MESH_ZIP, AvatarFile.ALL_HAIRCUT_POINTS_ZIP, AvatarFile.BLENDSHAPES_ZIP})
					CoreTools.DeleteAvatarFile (avatar.code, fileToDelete);
			} catch (Exception ex) {
				// error here is not critical, we can just ignore it
				Debug.LogException (ex);
			}

			request.Result = avatar;
			request.IsDone = true;
		}

		/// <summary>
		/// Download all avatar files, unzip and save to disk.
		/// </summary>
		/// <param name="connection">Connection session.</param>
		/// <param name="avatar">Avatar to download.</param>
		/// <param name="withHaircutPointClouds">If set to true, download all haircut point clouds too.</param>
		/// <param name="withBlendshapes">If set to true, download blendshapes too.</param>
		public static AsyncRequest<AvatarData> DownloadAndSaveAvatarModelAsync (
			Connection connection, AvatarData avatar, bool withHaircutPointClouds, bool withBlendshapes
		)
		{
			Debug.LogWarning("This method is obsolete. Use CloudAvatarProvider instead.");
			var request = new AsyncRequest<AvatarData> (AvatarSdkMgr.Str (Strings.DownloadingAvatar));
			AvatarSdkMgr.SpawnCoroutine (DownloadAndSaveAvatarModel (connection, avatar, withHaircutPointClouds, withBlendshapes, request));
			return request;
		}

		#endregion
	}
}