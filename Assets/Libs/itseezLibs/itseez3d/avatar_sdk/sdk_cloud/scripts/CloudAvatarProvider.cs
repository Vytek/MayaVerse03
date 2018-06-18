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
using System.Linq;
using UnityEngine;
using ItSeez3D.AvatarSdk.Core;
using System.IO;

namespace ItSeez3D.AvatarSdk.Cloud
{
	/// <summary>
	/// Implementation of the IAvatarProvider for cloud version of the Avatar SDK.
	/// </summary>
	public class CloudAvatarProvider : IAvatarProvider
	{
		private Connection connection = new Connection();

		//cached data
		private Dictionary<string, AvatarHaircutData[]> haircutsDataCache = new Dictionary<string, AvatarHaircutData[]>();

		#region Constructor
		public CloudAvatarProvider()
		{
			UseCache = true;
		}
		#endregion

		#region IAvatarProvider
		/// <summary>
		/// Performs authorization on the server
		/// </summary>
		public AsyncRequest InitializeAsync()
		{
			// Obtain auth token asynchronously. This code will also create PlayerUID and
			// store it in persistent storage. Auth token and PlayerUID are required all further HTTP requests.
			return connection.AuthorizeAsync();
		}

		/// <summary>
		/// Initializes avatar and uploads photo to the server.
		/// </summary>
		public AsyncRequest<string> InitializeAvatarAsync(byte[] photoBytes)
		{
			var request = new AsyncRequest<string>(AvatarSdkMgr.Str(Strings.InitializingAvatar));
			AvatarSdkMgr.SpawnCoroutine(InitializeAvatarFunc("test_avatar", "test_description", false, photoBytes, request));
			return request;
		}

		/// <summary>
		/// Waits while the avatar is being calulated. Calculations start automatically after the photo was loaded to the server.
		/// </summary>
		public AsyncRequest StartAndAwaitAvatarCalculationAsync(string avatarCode)
		{
			var request = new AsyncRequest(AvatarSdkMgr.Str(Strings.GeneratingAvatar));
			AvatarSdkMgr.SpawnCoroutine(StartAndAwaitAvatarCalculationFunc(avatarCode, request));
			return request;
		}

		/// <summary>
		/// Downloads avatar files and stores them on disk.
		/// </summary>
		/// <param name="avatarCode">Avatar code</param>
		/// <param name="withHaircutPointClouds">If True, haircut point clouds will be downloaded.</param>
		/// <param name="withBlendshapes">If true, blendshapes will be downloaded.</param>
		/// <returns></returns>
		public AsyncRequest MoveAvatarModelToLocalStorageAsync(string avatarCode, bool withHaircutPointClouds, bool withBlendshapes)
		{
			var request = new AsyncRequest<AvatarData>(AvatarSdkMgr.Str(Strings.DownloadingAvatar));
			AvatarSdkMgr.SpawnCoroutine(MoveAvatarModelToLocalStorage(avatarCode, withHaircutPointClouds, withBlendshapes, request));
			return request;
		}

		/// <summary>
		/// Creates TexturedMesh of the head for a given avatar.
		/// If required files (mesh and texture) don't exist on disk, it downloads them from the cloud.
		/// </summary>
		/// <param name="avatarCode">code of the loaded avatar</param>
		/// <param name="withBlendshapes">blendshapes will be added to mesh</param>
		public AsyncRequest<TexturedMesh> GetHeadMeshAsync(string avatarCode, bool withBlendshapes, int detailsLevel = 0)
		{
			var request = new AsyncRequest<TexturedMesh>(AvatarSdkMgr.Str(Strings.GettingHeadMesh));
			AvatarSdkMgr.SpawnCoroutine(GetHeadMeshFunc(avatarCode, withBlendshapes, detailsLevel, request));
			return request;
		}

		/// <summary>
		/// Returns identities of all haircuts available for the avatar
		/// </summary>
		public AsyncRequest<string[]> GetHaircutsIdAsync(string avatarCode)
		{
			var request = new AsyncRequest<string[]>(AvatarSdkMgr.Str(Strings.GettingAvailableHaircuts));
			AvatarSdkMgr.SpawnCoroutine(GetHaircutsIdFunc(avatarCode, request));
			return request;
		}

		/// <summary>
		/// Creates TexturedMesh of the haircut.
		/// If any of the required files doesn't exist it downloads them from the cloud and saves on the disk.
		/// </summary>
		/// <param name="avatarCode">Avatar code</param>
		/// <param name="haircutName">Haircut identity</param>
		public AsyncRequest<TexturedMesh> GetHaircutMeshAsync(string avatarCode, string haircutId)
		{
			var request = new AsyncRequest<TexturedMesh>(AvatarSdkMgr.Str(Strings.GettingHaircutMesh));
			AvatarSdkMgr.SpawnCoroutine(GetHaircutMeshFunc(avatarCode, haircutId, request));
			return request;
		}

		/// <summary>
		/// Requests from the server identities of the latest "maxItems" avatars.
		/// </summary>
		public AsyncRequest<string[]> GetAllAvatarsAsync(int maxItems)
		{
			var request = new AsyncRequest<string[]>(AvatarSdkMgr.Str(Strings.GettingAvatarList));
			AvatarSdkMgr.SpawnCoroutine(GetAllAvatarsFunc(maxItems, request));
			return request;
		}

		/// <summary>
		/// Requests server to delete all data permanently and deletes local avatar files.
		/// </summary>
		public AsyncRequest DeleteAvatarAsync(string avatarCode)
		{
			var request = new AsyncRequest(AvatarSdkMgr.Str(Strings.DeletingAvatarFiles));
			AvatarSdkMgr.SpawnCoroutine(DeleteAvatarFunc(avatarCode, request));
			return request;
		}
		#endregion

		#region IDisposable
		/// <summary>
		/// Empty method in Cloud version
		/// </summary>
		public virtual void Dispose() { }
		#endregion

		#region public methods
		/// <summary>
		/// Get the created connection instance.
		/// </summary>
		public Connection Connection { get { return connection; } }

		/// <summary>
		/// To avoid redundant requests to the server, some type of data may be cached.
		/// This property determinates whether the data cache is enabled. Default value in True.
		/// </summary>
		public bool UseCache { get; set; }

		/// <summary>
		/// Initializes avatar and uploads photo to the server.
		/// </summary>
		/// <param name="name">Name of the avatar</param>
		/// <param name="description">Description of the avatar</param>
		/// <param name="forcePowerOfTwoTexture">In case of true, generated texture resolution will be power of 2</param>
		/// <param name="photoBytes">Photo bytes (jpg or png encoded).</param>
		/// <returns></returns>
		public AsyncRequest<string> InitializeAvatarAsync(string name, string description, bool forcePowerOfTwoTexture, byte[] photoBytes)
		{
			var request = new AsyncRequest<string>(AvatarSdkMgr.Str(Strings.GeneratingAvatar));
			AvatarSdkMgr.SpawnCoroutine(InitializeAvatarFunc(name, description, forcePowerOfTwoTexture, photoBytes, request));
			return request;
		}

		/// <summary>
		/// Download all avatar files, unzip and save to disk.
		/// </summary>
		/// <param name="connection">Connection session.</param>
		/// <param name="avatar">Avatar to download.</param>
		/// <param name="withHaircutPointClouds">If set to true, download all haircut point clouds too.</param>
		/// <param name="withBlendshapes">If set to true, download blendshapes too.</param>
		public AsyncRequest DownloadAndSaveAvatarModelAsync(AvatarData avatar, bool withHaircutPointClouds, bool withBlendshapes)
		{
			var request = new AsyncRequest<AvatarData>(AvatarSdkMgr.Str(Strings.DownloadingAvatar));
			AvatarSdkMgr.SpawnCoroutine(DownloadAndSaveAvatarModel(avatar, withHaircutPointClouds, withBlendshapes, request));
			return request;
		}

		/// <summary>
		/// Get haircut info
		/// </summary>
		/// <param name="avatarCode">Avatar code</param>
		/// <param name="haircutId">Haircut identity</param>
		public AsyncRequest<AvatarHaircutData> GetHaircutDataAsync(string avatarCode, string haircutId)
		{
			var request = new AsyncRequest<AvatarHaircutData>(AvatarSdkMgr.Str(Strings.GettingHaircutInfo));
			AvatarSdkMgr.SpawnCoroutine(GetHaircutDataFunc(avatarCode, haircutId, request));
			return request;
		}

		/// <summary>
		/// Download haircut mesh and texture and save them to disk
		/// </summary>
		public AsyncRequest DownloadAndSaveHaircutMeshAsync(AvatarHaircutData haircutData)
		{
			var request = new AsyncRequest(AvatarSdkMgr.Str(Strings.GettingHaircutMesh));
			AvatarSdkMgr.SpawnCoroutine(DownloadAndSaveHaircutMeshFunc(haircutData, request));
			return request;
		}

		/// <summary>
		/// Download haircut points and save them to disk
		/// </summary>
		public AsyncRequest DownloadAndSaveHaircutPointsAsync(string avatarCode, AvatarHaircutData haircutData)
		{
			var request = new AsyncRequest(AvatarSdkMgr.Str(Strings.GettingHaircutPointCloud));
			AvatarSdkMgr.SpawnCoroutine(DownloadAndSaveHaircutPointsFunc(avatarCode, haircutData, request));
			return request;
		}

		/// <summary>
		/// Process blendshapes slightly differently compared to other zips (for compatibility reasons).
		/// Blendshapes are unzipped not just in avatar directory, but in their own personal folder.
		/// </summary>
		/// <param name="blendshapesZip">Full path to blendshapes zip archive.</param>
		/// <param name="avatarCode">Avatar identifier to determine the correct unzip location.</param>
		public AsyncRequest<string> UnzipBlendshapesAsync(string blendshapesZip, string avatarCode)
		{
			var blendshapesDir = AvatarSdkMgr.Storage().GetAvatarSubdirectory(avatarCode, AvatarSubdirectory.BLENDSHAPES);
			return CoreTools.UnzipFileAsync(blendshapesZip, blendshapesDir);
		}
		#endregion

		#region private methods

		/// <summary>
		/// InitializeAvatarAsync implementation
		/// </summary>
		private IEnumerator InitializeAvatarFunc(string name, string description, bool forcePowerOfTwoTexture, byte[] photoBytes, AsyncRequest<string> request)
		{
			// uploading photo and registering new avatar on the server
			var createAvatar = connection.CreateAvatarWithPhotoAsync(name, description, photoBytes, forcePowerOfTwoTexture);

			// Wait until async request is completed (without blocking the main thread).
			// Instead of using AwaitSubrequest we could just use `yield return createAvatar;`
			// AwaitSubrequest is a helper function that allows to track progress on composite
			// requests automatically. It also provides info for the caller about current subrequest
			// (and it's progress) and propagetes error from subrequest to the parent request.
			// finalProgress is a value between 0 and 1, a desired progress of parent request when given
			// subrequest is completed.
			yield return request.AwaitSubrequest(createAvatar, finalProgress: 0.99f);

			// must check whether request was successful before proceeding
			if (request.IsError)
				yield break;

			string avatarCode = createAvatar.Result.code;

			// save photo for later use
			var savePhoto = CoreTools.SaveAvatarFileAsync(photoBytes, avatarCode, AvatarFile.PHOTO);
			yield return request.AwaitSubrequest(savePhoto, finalProgress: 1.0f);

			// again, must check for the error, there's no point in proceeding otherwise
			if (request.IsError)
				yield break;

			request.Result = avatarCode;
			request.IsDone = true;
		}

		/// <summary>
		/// StartAndAwaitAvatarCalculationAsync implementation
		/// </summary>
		private IEnumerator StartAndAwaitAvatarCalculationFunc(string avatarCode, AsyncRequest request)
		{
			var avatarRequest = connection.GetAvatarAsync(avatarCode);
			yield return  request.AwaitSubrequest(avatarRequest, 0.01f);
			if (request.IsError)
				yield break;

			var awaitCalculations = connection.AwaitAvatarCalculationsAsync(avatarRequest.Result);
			yield return request.AwaitSubrequest(awaitCalculations, finalProgress: 1.0f);
			if (request.IsError)
				yield break;

			if (Strings.BadFinalStates.Contains(awaitCalculations.Result.status))
			{
				request.SetError(string.Format("Avatar {0} calculation finished with status: {1}", awaitCalculations.Result.code, awaitCalculations.Result.status));
				yield break;
			}

			request.IsDone = true;
		}

		/// <summary>
		/// MoveToLocalStorageAvatarModelAsync implementation
		/// </summary>
		private IEnumerator MoveAvatarModelToLocalStorage(string avatarCode, bool withHaircutPointClouds, bool withBlendshapes, AsyncRequest request)
		{
			var avatarRequest = connection.GetAvatarAsync(avatarCode);
			yield return avatarRequest.Await();
			if (avatarRequest.IsError)
			{
				request.SetError(avatarRequest.ErrorMessage);
				yield break;
			}

			yield return DownloadAndSaveAvatarModel(avatarRequest.Result, withHaircutPointClouds, withBlendshapes, request);
		}


		/// <summary>
		/// DownloadAndSaveAvatarModelAsync implementation.
		/// </summary>
		private IEnumerator DownloadAndSaveAvatarModel(AvatarData avatar, bool withHaircutPointClouds, bool withBlendshapes, AsyncRequest request)
		{
			// By initializing multiple requests at the same time (without yield between them) we're
			// starting them in parallel. In this particular case we're downloading multiple files at the same time,
			// which is usually a bit faster than sequential download.
			var meshZip = connection.DownloadMeshZipAsync(avatar);
			var textureRequest = connection.DownloadTextureBytesAsync(avatar);

			var download = new List<AsyncRequest> { meshZip, textureRequest };
			AsyncWebRequest<byte[]> allHaircutPointCloudsZip = null, blendshapesZip = null;

#if BLENDSHAPES_IN_PLY_OR_FBX
			// just a sample of how to get blendshapes in a different format

			AsyncWebRequest<byte[]> blendshapesZipFbx = null, blendshapesZipPly = null;
#endif

			if (withHaircutPointClouds)
			{
				allHaircutPointCloudsZip = connection.DownloadAllHaircutPointCloudsZipAsync(avatar);
				download.Add(allHaircutPointCloudsZip);
			}

			if (withBlendshapes)
			{
				blendshapesZip = connection.DownloadBlendshapesZipAsync(avatar);
				download.Add(blendshapesZip);

#if BLENDSHAPES_IN_PLY_OR_FBX
				// just a sample of how to get blendshapes in a different format

				blendshapesZipFbx = connection.DownloadBlendshapesZipAsync (avatar, BlendshapesFormat.FBX);
				download.Add (blendshapesZipFbx);

				blendshapesZipPly = connection.DownloadBlendshapesZipAsync (avatar, BlendshapesFormat.PLY);
				download.Add (blendshapesZipPly);
#endif
			}

			// continue execution when all requests finish
			yield return request.AwaitSubrequests(0.9f, download.ToArray());
			// return if any of the requests failed
			if (request.IsError)
				yield break;

			// save all the results to disk, also in parallel
			var saveMeshZip = CoreTools.SaveAvatarFileAsync(meshZip.Result, avatar.code, AvatarFile.MESH_ZIP);
			var saveTexture = CoreTools.SaveAvatarFileAsync(textureRequest.Result, avatar.code, AvatarFile.TEXTURE);

			var save = new List<AsyncRequest>() { saveMeshZip, saveTexture };
			AsyncRequest<string> saveHaircutPointsZip = null, saveBlendshapesZip = null;
			if (allHaircutPointCloudsZip != null)
			{
				saveHaircutPointsZip = CoreTools.SaveAvatarFileAsync(allHaircutPointCloudsZip.Result, avatar.code, AvatarFile.ALL_HAIRCUT_POINTS_ZIP);
				save.Add(saveHaircutPointsZip);
			}

			if (blendshapesZip != null)
			{
				saveBlendshapesZip = CoreTools.SaveAvatarFileAsync(blendshapesZip.Result, avatar.code, AvatarFile.BLENDSHAPES_ZIP);
				save.Add(saveBlendshapesZip);
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

			yield return request.AwaitSubrequests(0.95f, save.ToArray());
			if (request.IsError)
				yield break;

			var unzipMesh = CoreTools.UnzipFileAsync(saveMeshZip.Result);

			var unzip = new List<AsyncRequest>() { unzipMesh };
			AsyncRequest<string> unzipHaircutPoints = null, unzipBlendshapes = null;
			if (saveHaircutPointsZip != null)
			{
				unzipHaircutPoints = CoreTools.UnzipFileAsync(saveHaircutPointsZip.Result);
				unzip.Add(unzipHaircutPoints);
			}
			if (saveBlendshapesZip != null)
			{
				unzipBlendshapes = UnzipBlendshapesAsync(saveBlendshapesZip.Result, avatar.code);
				unzip.Add(unzipBlendshapes);
			}

			yield return request.AwaitSubrequests(0.99f, unzip.ToArray());
			if (request.IsError)
				yield break;

			// delete all .zip files we don't need anymore
			try
			{
				foreach (var fileToDelete in new AvatarFile[] { AvatarFile.MESH_ZIP, AvatarFile.ALL_HAIRCUT_POINTS_ZIP, AvatarFile.BLENDSHAPES_ZIP })
					CoreTools.DeleteAvatarFile(avatar.code, fileToDelete);
			}
			catch (Exception ex)
			{
				// error here is not critical, we can just ignore it
				Debug.LogException(ex);
			}
			
			request.IsDone = true;
		}

		/// <summary>
		/// GetHaircutDataAsync implementation
		/// </summary>
		private IEnumerator GetHaircutDataFunc(string avatarCode, string haircutId, AsyncRequest<AvatarHaircutData> request)
		{
			bool takeFromCache = UseCache && haircutsDataCache.ContainsKey(avatarCode);
			if (takeFromCache)
				request.Result = haircutsDataCache[avatarCode].FirstOrDefault(h => string.Compare(h.identity, haircutId) == 0);
			else
			{
				// get AvatarData firstly.
				// If you would like to make multiple requests for getting haircut data, it is better to get AvatarData only once and store it somewhere
				var avatarRequest = connection.GetAvatarAsync(avatarCode);
				yield return request.AwaitSubrequest(avatarRequest, 0.45f);
				if (request.IsError)
					yield break;

				var haircutInfoRequest = connection.GetHaircutsAsync(avatarRequest.Result);
				yield return request.AwaitSubrequest(haircutInfoRequest, 0.9f);
				if (request.IsError)
					yield break;

				if (UseCache)
					haircutsDataCache.Add(avatarCode, haircutInfoRequest.Result);

				AvatarHaircutData haircutData = haircutInfoRequest.Result.FirstOrDefault(h => string.Compare(h.identity, haircutId) == 0);
				if (haircutData == null)
				{
					Debug.LogErrorFormat("There is no {0} haircut for avatar with code: {1}", haircutId, avatarCode);
					yield break;
				}
				request.Result = haircutData;
			}

			request.IsDone = true;
		}

		/// <summary>
		/// GetHaircutsIdAsync implementation
		/// </summary>
		private IEnumerator GetHaircutsIdFunc(string avatarCode, AsyncRequest<string[]> request)
		{
			bool takeFromCache = UseCache && haircutsDataCache.ContainsKey(avatarCode);
			if (takeFromCache)
				request.Result = haircutsDataCache[avatarCode].Select(h => h.identity).ToArray();
			else
			{
				var avatarRequest = connection.GetAvatarAsync(avatarCode);
				yield return request.AwaitSubrequest(avatarRequest, 0.45f);
				if (request.IsError)
					yield break;

				var haircutInfoRequest = connection.GetHaircutsAsync(avatarRequest.Result);
				yield return request.AwaitSubrequest(haircutInfoRequest, 0.9f);
				if (request.IsError)
					yield break;

				request.Result = haircutInfoRequest.Result.Select(h => h.identity).ToArray();

				if (UseCache)
					haircutsDataCache.Add(avatarCode, haircutInfoRequest.Result);
			}

			request.IsDone = true;
		}

		/// <summary>
		/// DownloadAndSaveHaircutMeshAsync implementation
		/// </summary>
		private IEnumerator DownloadAndSaveHaircutMeshFunc(AvatarHaircutData haircutData, AsyncRequest request)
		{
			Debug.LogFormat("Downloading haircut mesh, texture and points simultaneously...");
			var haircutMeshRequest = connection.DownloadHaircutMeshZipAsync(haircutData);
			var haircutTextureRequest = connection.DownloadHaircutTextureBytesAsync(haircutData);
			yield return request.AwaitSubrequests(0.8f, haircutMeshRequest, haircutTextureRequest);
			if (request.IsError)
				yield break;

			Debug.LogFormat("Saving haircut mesh and texture to disk...");
			var saveHaircutMeshRequest = CoreTools.SaveHaircutFileAsync(haircutMeshRequest.Result, haircutData.identity, HaircutFile.HAIRCUT_MESH_ZIP);
			var saveHaircutTextureRequest = CoreTools.SaveHaircutFileAsync(haircutTextureRequest.Result, haircutData.identity, HaircutFile.HAIRCUT_TEXTURE);
			yield return request.AwaitSubrequests(0.9f, saveHaircutMeshRequest, saveHaircutTextureRequest);
			if (request.IsError)
				yield break;

			Debug.LogFormat("Unzip haircut mesh...");
			var unzipMeshRequest = CoreTools.UnzipFileAsync(saveHaircutMeshRequest.Result);
			yield return request.AwaitSubrequest(unzipMeshRequest, 1.0f);
			if (request.IsError)
				yield break;

			request.IsDone = true;
		}

		/// <summary>
		/// DownloadAndSaveHaircutPointsAsync implementation
		/// </summary>
		private IEnumerator DownloadAndSaveHaircutPointsFunc(string avatarCode, AvatarHaircutData haircutData, AsyncRequest request)
		{
			var haircutPointsRequest = connection.DownloadHaircutPointCloudZipAsync(haircutData);
			yield return request.AwaitSubrequest(haircutPointsRequest, 0.9f);
			if (request.IsError)
				yield break;

			var saveHaircutPointsRequest = CoreTools.SaveAvatarHaircutFileAsync(haircutPointsRequest.Result, avatarCode, haircutData.identity, AvatarFile.HAIRCUT_POINT_CLOUD_ZIP);
			yield return request.AwaitSubrequest(saveHaircutPointsRequest, 0.95f);
			if (request.IsError)
				yield break;

			var unzipPointsRequest = CoreTools.UnzipFileAsync(saveHaircutPointsRequest.Result);
			yield return request.AwaitSubrequest(unzipPointsRequest, 1.0f);
			if (request.IsError)
				yield break;

			request.IsDone = true;
		}

		/// <summary>
		/// GetHaircutMeshAsync implementation
		/// </summary>
		private IEnumerator GetHaircutMeshFunc(string avatarCode, string haircutId, AsyncRequest<TexturedMesh> request)
		{
			DateTime startTime = DateTime.Now;
			// In order to display the haircut in a scene correctly we need three things: mesh, texture, and coordinates of
			// vertices adjusted specifically for our avatar (this is called "haircut point cloud"). We need this because
			// algorithms automatically adjust haircuts for each model to provide better fitness.
			// Haircut texture and mesh (number of points and mesh topology) are equal for all avatars, but "point cloud"
			// should be downloaded separately for each model. 
			// If mesh and texture are not cached yet, lets download and save them.
			string haircutMeshFilename = AvatarSdkMgr.Storage().GetHaircutFilename(haircutId, HaircutFile.HAIRCUT_MESH_PLY);
			string haircutTextureFilename = AvatarSdkMgr.Storage().GetHaircutFilename(haircutId, HaircutFile.HAIRCUT_TEXTURE);
			string haircutPointCloudFilename = AvatarSdkMgr.Storage().GetAvatarHaircutFilename(avatarCode, haircutId, AvatarFile.HAIRCUT_POINT_CLOUD_PLY);

			bool existMeshFiles = File.Exists(haircutMeshFilename) && File.Exists(haircutTextureFilename);
			bool existPointcloud = File.Exists(haircutPointCloudFilename);
			if (!existMeshFiles || !existPointcloud)
			{
				var haircutDataRequest = GetHaircutDataAsync(avatarCode, haircutId);
				yield return request.AwaitSubrequest(haircutDataRequest, 0.05f);
				if (request.IsError)
					yield break;
				
				List<AsyncRequest> downloadRequests = new List<AsyncRequest>();
				if (!existMeshFiles)
					downloadRequests.Add(DownloadAndSaveHaircutMeshAsync(haircutDataRequest.Result));
				if (!existPointcloud)
					downloadRequests.Add(DownloadAndSaveHaircutPointsAsync(avatarCode, haircutDataRequest.Result));

				yield return request.AwaitSubrequests(0.9f, downloadRequests.ToArray());
				if (request.IsError)
					yield break;
			}

			var loadHaircutRequest = CoreTools.LoadHaircutFromDiskAsync(avatarCode, haircutId);
			yield return request.AwaitSubrequest(loadHaircutRequest, 1.0f);
			if (request.IsError)
				yield break;

			request.IsDone = true;
			request.Result = loadHaircutRequest.Result;
		}

		/// <summary>
		/// GetHeadMeshAsync implementation
		/// </summary>
		private IEnumerator GetHeadMeshFunc(string avatarCode, bool withBlendshapes, int detailsLevel, AsyncRequest<TexturedMesh> request)
		{
			string meshFilename = AvatarSdkMgr.Storage().GetAvatarFilename(avatarCode, AvatarFile.MESH_PLY);
			string textureFilename = AvatarSdkMgr.Storage().GetAvatarFilename(avatarCode, AvatarFile.TEXTURE);
			//If there are no required files, will download them.
			if (!File.Exists(meshFilename) || !File.Exists(textureFilename))
			{
				var avatarRequest = connection.GetAvatarAsync(avatarCode);
				yield return request.AwaitSubrequest(avatarRequest, 0.05f);
				if (request.IsError)
					yield break;

				var downloadRequest = DownloadAndSaveAvatarModelAsync(avatarRequest.Result, false, withBlendshapes);
				yield return request.AwaitSubrequest(downloadRequest, 0.95f);
				if (request.IsError)
					yield break;
			}

			//if there are no blendshapes, will download them
			var blendshapesDir = AvatarSdkMgr.Storage().GetAvatarSubdirectory(avatarCode, AvatarSubdirectory.BLENDSHAPES);
			bool blendshapesExist = Directory.GetFiles(blendshapesDir).Length > 0;
			if (withBlendshapes && !blendshapesExist)
			{
				var avatarRequest = connection.GetAvatarAsync(avatarCode);
				yield return request.AwaitSubrequest(avatarRequest, 0.05f);
				if (request.IsError)
					yield break;

				var downloadBlendshapes = connection.DownloadBlendshapesZipAsync(avatarRequest.Result, BlendshapesFormat.BIN, detailsLevel);
				yield return request.AwaitSubrequest(downloadBlendshapes, 0.8f);
				if (request.IsError)
					yield break;

				var saveBlendshapesZip = CoreTools.SaveAvatarFileAsync(downloadBlendshapes.Result, avatarCode, AvatarFile.BLENDSHAPES_ZIP);
				yield return request.AwaitSubrequest(saveBlendshapesZip, 0.9f);
				if (request.IsError)
					yield break;

				var unzipBlendshapes = UnzipBlendshapesAsync(saveBlendshapesZip.Result, avatarCode);
				yield return request.AwaitSubrequest(unzipBlendshapes, 0.95f);
				if (request.IsError)
					yield break;

				CoreTools.DeleteAvatarFile(avatarCode, AvatarFile.BLENDSHAPES_ZIP);
			}

			// At this point all avatar files are already saved to disk. Let's load the files to Unity.
			var loadAvatarHeadRequest = CoreTools.LoadAvatarHeadFromDiskAsync(avatarCode, withBlendshapes, detailsLevel);
			yield return request.AwaitSubrequest(loadAvatarHeadRequest, 1.0f);
			if (request.IsError)
				yield break;

			request.Result = loadAvatarHeadRequest.Result;
			request.IsDone = true;
		}

		/// <summary>
		/// GetAllAvatarsAsync implementation
		/// </summary>
		private IEnumerator GetAllAvatarsFunc(int maxItems, AsyncRequest<string[]> request)
		{
			var avatarsRequest = connection.GetAvatarsAsync(maxItems);
			yield return avatarsRequest;
			if (avatarsRequest.IsError)
			{
				request.SetError(avatarsRequest.ErrorMessage);
				yield break;
			}

			var avatarsData = avatarsRequest.Result.OrderBy(av => DateTime.Parse(av.created_on)).Reverse().ToArray();
			request.Result = avatarsData.Select(a => a.code).ToArray();
			request.IsDone = true;
		}

		/// <summary>
		/// DeleteAvatarAsync implementation
		/// </summary>
		private IEnumerator DeleteAvatarFunc(string avatarCode, AsyncRequest request)
		{
			var avatarRequest = connection.GetAvatarAsync(avatarCode);
			yield return avatarRequest;
			if (avatarRequest.IsError)
			{
				request.SetError(avatarRequest.ErrorMessage);
				yield break;
			}

			var deleteRequest = connection.DeleteAvatarAsync(avatarRequest.Result);
			yield return request.AwaitSubrequest(deleteRequest, 0.5f);
			if (request.IsError)
				yield break;

			CoreTools.DeleteAvatarFiles(avatarCode);

			request.IsDone = true;
		}
		#endregion
	}
}
