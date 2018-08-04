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
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

namespace ItSeez3D.AvatarSdk.Core
{
	public static class CoreTools
	{
		#region Version

		/// <summary>
		/// Current version of an SDK. Used for update checks in the editor.
		/// </summary>
		public static Version SdkVersion { get { return new Version (1, 5, 0); } }

		#endregion

		#region Save avatar files

		/// <summary>
		/// Some of the files involved in avatar generation (e.g. textures) may be large. This function helps to
		/// work around this by saving file in a separate thread, thus not blocking the main thread.
		/// </summary>
		/// <param name="bytes">Binary file content.</param>
		/// <param name="path">Full absolute path.</param>
		public static AsyncRequest<string> SaveFileAsync (byte[] bytes, string path)
		{
			var request = new AsyncRequestThreaded<string> (() => {
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes (path, bytes);
				return path;
			});
			request.State = AvatarSdkMgr.Str (Strings.SavingFiles);
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		/// <summary>
		/// Helper method that automatically generates full path to file from file type and avatar id, and then calls
		/// SaveFileAsync.
		/// </summary>
		/// <param name="bytes">Binary file content.</param>
		/// <param name="code">Avatar code.</param>
		/// <param name="file">Avatar file type.</param>
		public static AsyncRequest<string> SaveAvatarFileAsync (byte[] bytes, string code, AvatarFile file)
		{
			try {
				var filename = AvatarSdkMgr.Storage ().GetAvatarFilename (code, file);
				return SaveFileAsync (bytes, filename);
			} catch (Exception ex) {
				Debug.LogException (ex);
				var request = new AsyncRequest<string> ("");
				request.SetError (string.Format ("Could not save {0}, reason: {1}", file, ex.Message));
				return request;
			}
		}

		/// <summary>
		/// Same as SaveAvatarFileAsync, but for haircut points, because they are unique for each avatar and should be stored in avatar folder.
		/// </summary>
		/// <param name="bytes">Binary file content.</param>
		/// <param name="code">Avatar unique code.</param>
		/// <param name="haircutId">Unique ID of a haircut.</param>
		public static AsyncRequest<string> SaveAvatarHaircutPointCloudZipFileAsync (
			byte[] bytes,
			string code,
			string haircutId
		)
		{
			try {
				var filename = AvatarSdkMgr.Storage ().GetAvatarHaircutPointCloudZipFilename(code, haircutId);
				return SaveFileAsync (bytes, filename);
			} catch (Exception ex) {
				Debug.LogException (ex);
				var request = new AsyncRequest<string> ("Saving file");
				request.SetError (string.Format ("Could not save point cloud zip, reason: {0}", ex.Message));
				return request;
			}
		}

		/// <summary>
		/// Same as SaveAvatarFileAsync, but for haircut files. Haircut meshes and textures can be shared between
		/// avatars and thus only one copy of each mesh/texture is stored in a separate directory.
		/// </summary>
		/// <param name="bytes">Binary file content.</param>
		/// <param name="haircutId">Unique ID of a haircut.</param>
		/// <param name="file">Kind of file.</param>
		public static AsyncRequest<string> SaveHaircutFileAsync (
			byte[] bytes,
			string haircutId,
			HaircutFile file
		)
		{
			try {
				var filename = AvatarSdkMgr.Storage ().GetHaircutFilename (haircutId, file);
				return SaveFileAsync (bytes, filename);
			} catch (Exception ex) {
				Debug.LogException (ex);
				var request = new AsyncRequest<string> ("Saving file");
				request.SetError (string.Format ("Could not save {0}, reason: {1}", file, ex.Message));
				return request;
			}
		}

		#endregion

		#region Load avatar files

		/// <summary>
		/// Just like SaveFileAsync, loads file asynchronously in a separate thread.
		/// </summary>
		/// <param name="path">Absolute path to file.</param>
		public static AsyncRequest<byte[]> LoadFileAsync (string path)
		{
			var request = new AsyncRequestThreaded<byte[]> (() => File.ReadAllBytes (path));
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		/// <summary>
		/// Read text file asynchronously
		/// </summary>
		public static AsyncRequest<string> ReadFileAsync(string path)
		{
			var request = new AsyncRequestThreaded<string>(() => File.ReadAllText(path));
			AvatarSdkMgr.SpawnCoroutine(request.Await());
			return request;
		}

		/// <summary>
		/// Loads the avatar file asynchronously.
		/// </summary>
		/// <param name="code">Avatar unique code.</param>
		/// <param name="file">File type (e.g. head texture).</param>
		public static AsyncRequest<byte[]> LoadAvatarFileAsync (string code, AvatarFile file)
		{
			try {
				var filename = AvatarSdkMgr.Storage ().GetAvatarFilename (code, file);
				return LoadFileAsync (filename);
			} catch (Exception ex) {
				Debug.LogException (ex);
				var request = new AsyncRequest<byte[]> ();
				request.SetError (string.Format ("Could not load {0}, reason: {1}", file, ex.Message));
				return request;
			}
		}

		/// <summary>
		/// Loads the avatar haircut points file asynchronously.
		/// </summary>
		/// <param name="code">Avatar unique code.</param>
		/// <param name="haircutId">Unique ID of a haircut.</param>
		public static AsyncRequest<byte[]> LoadAvatarHaircutPointcloudFileAsync (string code, string haircutId)
		{
			try {
				var filename = AvatarSdkMgr.Storage ().GetAvatarHaircutPointCloudFilename(code, haircutId);
				return LoadFileAsync (filename);
			} catch (Exception ex) {
				Debug.LogException (ex);
				var request = new AsyncRequest<byte[]> ();
				request.SetError (string.Format ("Could not load haircut {0} point cloud, reason: {1}", haircutId, ex.Message));
				return request;
			}
		}

		/// <summary>
		/// Loads the haircut file asynchronously.
		/// </summary>
		/// <param name="haircutId">Unique ID of a haircut.</param>
		/// <param name="file">File type (e.g. haircut texture).</param>
		public static AsyncRequest<byte[]> LoadHaircutFileAsync (string haircutId, HaircutFile file)
		{
			try {
				var filename = AvatarSdkMgr.Storage ().GetHaircutFilename (haircutId, file);
				return LoadFileAsync (filename);
			} catch (Exception ex) {
				Debug.LogException (ex);
				var request = new AsyncRequest<byte[]> ();
				request.SetError (string.Format ("Could not load {0}, reason: {1}", file, ex.Message));
				return request;
			}
		}

		/// <summary>
		/// See LoadMeshDataFromDiskAsync.
		/// </summary>
		private static IEnumerator LoadMeshDataFromDisk (string avatarId, AsyncRequest<MeshData> request)
		{
			var meshBytesRequest = LoadAvatarFileAsync (avatarId, AvatarFile.MESH_PLY);
			yield return request.AwaitSubrequest (meshBytesRequest, finalProgress: 0.5f);
			if (request.IsError)
				yield break;

			var parsePlyTimer = new MeasureTime ("Parse ply");
			var parsePlyRequest = PlyToMeshDataAsync (meshBytesRequest.Result);
			yield return request.AwaitSubrequest (parsePlyRequest, finalProgress: 1);
			if (request.IsError)
				yield break;
			parsePlyTimer.Stop ();

			request.Result = parsePlyRequest.Result;
			request.IsDone = true;
		}

		/// <summary>
		/// Loads the mesh data and converts from .ply format into Unity format.
		/// </summary>
		public static AsyncRequest<MeshData> LoadMeshDataFromDiskAsync (string avatarId)
		{
			var request = new AsyncRequest <MeshData> (AvatarSdkMgr.Str (Strings.LoadingFiles));
			AvatarSdkMgr.SpawnCoroutine (LoadMeshDataFromDisk (avatarId, request));
			return request;
		}

		/// <summary>
		/// Loads a mesh with the given level of details. 
		/// It takes faces and UV-coordinates from the template model, points coordinates from the avatar's model and merges them into a single model.
		/// </summary>
		private static IEnumerator LoadDetailedMeshDataFromDisk(string avatarId, int detailsLevel, AsyncRequest<MeshData> request)
		{
			if (detailsLevel < 0)
			{
				Debug.LogWarningFormat("Invalid details level parameter: {0}. Will be used value 0 (highest resolution).", detailsLevel);
				detailsLevel = 0;
			}

			if (detailsLevel > 4)
			{
				Debug.LogWarningFormat("Invalid details level parameter: {0}. Will be used value 3 (lowest resolution).", detailsLevel);
				detailsLevel = 4;
			}

			if (detailsLevel == 0)
			{
				yield return LoadMeshDataFromDisk(avatarId, request);
			}
			else
			{
				var meshBytesRequest = LoadAvatarFileAsync(avatarId, AvatarFile.MESH_PLY);
				yield return request.AwaitSubrequest(meshBytesRequest, finalProgress: 0.3f);
				if (request.IsError)
					yield break;

				string headTemplateFileName = string.Format("template_heads/head_lod_{0}", detailsLevel);
				var headTemplateRequest = Resources.LoadAsync(headTemplateFileName);
				yield return headTemplateRequest;
				TextAsset templateHeadAsset = headTemplateRequest.asset as TextAsset;
				if (templateHeadAsset == null)
				{
					Debug.LogError("Unable to load template head!");
					yield break;
				}

				var meshRequest = PlyToMeshDataAsync(templateHeadAsset.bytes);
				var pointsRequest = PlyToPointsAsync(meshBytesRequest.Result);
				yield return request.AwaitSubrequests(0.95f, meshRequest, pointsRequest);

				request.Result = ReplacePointCoords(meshRequest.Result, pointsRequest.Result);
				request.IsDone = true;
			}
		}

		/// <summary>
		/// Loads the mesh with the given details level and converts from .ply format into Unity format.
		/// </summary>
		public static AsyncRequest<MeshData> LoadDetailedMeshDataFromDiskAsync(string avatarId, int detailsLevel)
		{
			var request = new AsyncRequest<MeshData>(AvatarSdkMgr.Str(Strings.LoadingFiles));
			AvatarSdkMgr.SpawnCoroutine(LoadDetailedMeshDataFromDisk(avatarId, detailsLevel, request));
			return request;
		}

		/// <summary>
		/// LoadAvatarHeadFromDiskAsync implementation.
		/// </summary>
		private static IEnumerator LoadAvatarHeadFromDisk (
			string avatarId,
			bool withBlendshapes,
			int detailsLevel,
			AsyncRequest<TexturedMesh> request
		)
		{
			// loading two files simultaneously
			var meshDataRequest = LoadDetailedMeshDataFromDiskAsync(avatarId, detailsLevel);
			var textureBytesRequest = LoadAvatarFileAsync (avatarId, AvatarFile.TEXTURE);

			yield return request.AwaitSubrequests (0.6f, meshDataRequest, textureBytesRequest);
			if (request.IsError)
				yield break;

			MeshData meshData = meshDataRequest.Result;

			var parseTextureTimer = new MeasureTime ("Parse texture data");
			// at this point we have all data we need to generate a textured mesh
			var texturedMesh = new TexturedMesh {
				mesh = CreateMeshFromMeshData (meshData, "HeadMesh"),
				texture = new Texture2D (0, 0)
			};

			// This actually blocks the main thread for a few frames, which is bad for VR.
			// To optimize: load jpg/png texture in C++ code in a separate thread and only SetPixels here in Unity. Should be faster.
			texturedMesh.texture.LoadImage (textureBytesRequest.Result);
			parseTextureTimer.Stop ();

			if (withBlendshapes)
			{
				// adding blendshapes...
				using (new MeasureTime ("Add blendshapes")) {
					var addBlendshapesRequest = AddBlendshapesAsync (avatarId, texturedMesh.mesh, meshData.indexMap);
					yield return request.AwaitSubrequest (addBlendshapesRequest, 1.0f);
					if (addBlendshapesRequest.IsError)
						Debug.LogError ("Could not add blendshapes!");
				}
			}

			request.Result = texturedMesh;
			request.IsDone = true;
		}

		/// <summary>
		/// Loads the avatar head files from disk into TexturedMesh object (parses .ply file too).
		/// </summary>
		/// <param name="avatarCode">Avatar code</param>
		/// <param name="withBlendshapes">If True, blendshapes will be loaded and added to mesh.</param>
		/// <param name="detailsLevel">Indicates polygons count in mesh. 0 - highest resolution, 3 - lowest resolution.</param>
		public static AsyncRequest<TexturedMesh> LoadAvatarHeadFromDiskAsync (string avatarCode, bool withBlendshapes, int detailsLevel)
		{
			var request = new AsyncRequest <TexturedMesh> (AvatarSdkMgr.Str (Strings.LoadingAvatar));
			AvatarSdkMgr.SpawnCoroutine(LoadAvatarHeadFromDisk(avatarCode, withBlendshapes, detailsLevel, request));
			return request;
		}

		/// <summary>
		/// LoadHaircutFromDiskAsync implementation.
		/// </summary>
		private static IEnumerator LoadHaircutFromDiskFunc (
			string avatarCode, string haircutId, AsyncRequest<TexturedMesh> request
		)
		{
			var loadingTime = Time.realtimeSinceStartup;

			// start three async request in parallel
			var haircutTexture = LoadHaircutFileAsync(haircutId, HaircutFile.HAIRCUT_TEXTURE);
			var haircutMesh = LoadHaircutFileAsync(haircutId, HaircutFile.HAIRCUT_MESH_PLY);
			var haircutPoints = LoadAvatarHaircutPointcloudFileAsync(avatarCode, haircutId);

			// wait until mesh and points load
			yield return request.AwaitSubrequests (0.4f, haircutMesh, haircutPoints);
			if (request.IsError)
				yield break;

			// we can start another two subrequests, now parsing the ply files
			var parseHaircutPly = PlyToMeshDataAsync (haircutMesh.Result);
			var parseHaircutPoints = PlyToPointsAsync (haircutPoints.Result);

			// await everything else we need for the haircut
			yield return request.AwaitSubrequests (0.95f, parseHaircutPly, parseHaircutPoints, haircutTexture);
			if (request.IsError)
				yield break;

			// now we have all data we need to generate a textured mesh
			var haircutMeshData = ReplacePointCoords (parseHaircutPly.Result, parseHaircutPoints.Result);

			var texturedMesh = new TexturedMesh ();
			texturedMesh.mesh = CreateMeshFromMeshData (haircutMeshData, "HaircutMesh");
			texturedMesh.texture = new Texture2D (0, 0);
			texturedMesh.texture.LoadImage (haircutTexture.Result);

			request.Result = texturedMesh;
			request.IsDone = true;

			Debug.LogFormat ("Took {0} seconds to load a haircut", Time.realtimeSinceStartup - loadingTime);
		}

		/// <summary>
		/// Loads the avatar haircut files from disk into TexturedMesh object (parses .ply files too).
		/// </summary>
		/// <returns>Async request which gives complete haircut TexturedMesh object eventually.</returns>
		public static AsyncRequest<TexturedMesh> LoadHaircutFromDiskAsync (string avatarCode, string haircutId)
		{
			var request = new AsyncRequest <TexturedMesh> (AvatarSdkMgr.Str (Strings.LoadingHaircut));
			AvatarSdkMgr.SpawnCoroutine (LoadHaircutFromDiskFunc (avatarCode, haircutId, request));
			return request;
		}

		#endregion

		#region Delete avatar files

		/// <summary>
		/// Delete entire avatar directory.
		/// </summary>
		public static void DeleteAvatarFiles (string avatarCode)
		{
			var path = AvatarSdkMgr.Storage ().GetAvatarDirectory (avatarCode);
			Directory.Delete (path, true);
		}

		/// <summary>
		/// Delete particular avatar file by type (e.g. zip mesh file after unzip).
		/// </summary>
		public static void DeleteAvatarFile (string avatarCode, AvatarFile file)
		{
			var path = AvatarSdkMgr.Storage ().GetAvatarFilename (avatarCode, file);
			File.Delete (path);
		}

		#endregion

		#region Zip utils

		/// <summary>
		/// Unzips the file asynchronously.
		/// </summary>
		/// <param name="path">Absolute path to zip file.</param>
		/// <param name="location">Unzip location. If null, then files will be unzipped in the location of .zip file.</param>
		public static AsyncRequest<string> UnzipFileAsync (string path, string location = null)
		{
			if (string.IsNullOrEmpty (location))
				location = Path.GetDirectoryName (path);

			AsyncRequest<string> request = null;
			Func<string> unzipFunc = () => {
				ZipUtils.Unzip (path, location);
				return location;
			};

			// unzip asynchronously in a separate thread
			request = new AsyncRequestThreaded<string> (() => unzipFunc (), AvatarSdkMgr.Str (Strings.UnzippingFile));
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		#endregion

		#region Ply/mesh utils

		/// <summary>
		/// Parsing .ply data asynchronously into Unity mesh data (vertices, triangles, etc.)
		/// </summary>
		/// <param name="plyBytes">Binary content of .ply file.</param>
		public static AsyncRequest<MeshData> PlyToMeshDataAsync (byte[] plyBytes)
		{
			var request = new AsyncRequestThreaded<MeshData> (() => {
				var meshData = new MeshData ();
				PlyReader.ReadMeshDataFromPly (
					plyBytes,
					out meshData.vertices,
					out meshData.triangles,
					out meshData.uv,
					out meshData.indexMap
				);
				return meshData;
			}, AvatarSdkMgr.Str (Strings.ParsingMeshData));
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		/// <summary>
		/// Parsing .ply-encoded 3D points (e.g. "haircut point cloud").
		/// </summary>
		public static AsyncRequest<Vector3[]> PlyToPointsAsync (byte[] plyBytes)
		{
			var request = new AsyncRequestThreaded <Vector3[]> (() => {
				Vector3[] points;
				PlyReader.ReadPointCloudFromPly (plyBytes, out points);
				return points;
			}, AvatarSdkMgr.Str (Strings.ParsingPoints));
			AvatarSdkMgr.SpawnCoroutine (request.Await ());
			return request;
		}

		/// <summary>
		/// Create Unity Mesh object from MeshData. Must be called from main thread!
		/// </summary>
		/// <returns>Unity Mesh object.</returns>
		/// <param name="meshData">Data (presumably parsed from ply).</param>
		/// <param name="meshName">Name of mesh object.</param>
		public static Mesh CreateMeshFromMeshData (MeshData meshData, string meshName)
		{
			Mesh mesh = new Mesh ();
			mesh.name = meshName;
			mesh.vertices = meshData.vertices;
			mesh.triangles = meshData.triangles;
			mesh.uv = meshData.uv;
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
			ImproveNormals (mesh, meshData.indexMap);
			return mesh;
		}

		/// <summary>
		/// Replace 3D point coordinates of a mesh with "coords", keeping mesh topology the same.
		/// Useful for reusing haircut meshes.
		/// </summary>
		/// <returns>Mesh data with replaced coordinates.</returns>
		/// <param name="meshData">Original mesh data.</param>
		/// <param name="coords">New 3D coordinates.</param>
		public static MeshData ReplacePointCoords (MeshData meshData, Vector3[] coords)
		{
			var vertices = new Vector3[meshData.vertices.Length];
			for (int i = 0; i < vertices.Length; ++i)
				vertices [i] = coords [meshData.indexMap [i]];
			meshData.vertices = vertices;
			return meshData;
		}

		/// <summary>
		/// Initially duplicated vertices have different normals.
		/// We have to solve it by setting average normal to avoid seams on a mesh.
		/// </summary>
		private static void ImproveNormals (Mesh mesh, int[] indexMap)
		{
			var vertices = mesh.vertices;
			var originalNormals = mesh.normals;

			Vector3[] normals = new Vector3[originalNormals.Length];
			bool[] normalSetFlag = new bool[originalNormals.Length]; 
			for (int i = 0; i < vertices.Length; i++) {
				if (indexMap [i] != i) {
					var n1 = originalNormals [i];
					var n2 = originalNormals [indexMap [i]];
					var n = (n1 + n2).normalized;
					normals [i] = n;
					normals [indexMap [i]] = n;
					normalSetFlag [i] = true;
					normalSetFlag [indexMap [i]] = true;
				} else if (!normalSetFlag [i]) {
					normals [i] = originalNormals [i];
				}
			}
			mesh.normals = normals;
		}

		#endregion

		#region Blendshapes

		/// <summary>
		/// Read blendshapes from the avatar directory and add them to 3D head mesh.
		/// </summary>
		private static IEnumerator AddBlendshapes (string avatarId, Mesh mesh, int[] indexMap, AsyncRequest<Mesh> request)
		{
			var blendshapesDirs = AvatarSdkMgr.Storage ().GetAvatarBlendshapesDirs(avatarId);

			var loadBlendshapesRequest = new AsyncRequestThreaded<Dictionary<string, Vector3[]>> ((r) => {
				var timer = new MeasureTime ("Read all blendshapes");
				var blendshapes = new Dictionary<string, Vector3[]> ();
				List<string> blendshapeFiles = new List<string>();
				foreach (string dir in blendshapesDirs)
					blendshapeFiles.AddRange(Directory.GetFiles(dir));
				var blendshapeReader = new BlendshapeReader (indexMap);

				for (int i = 0; i < blendshapeFiles.Count; ++i) {
					var blendshapePath = blendshapeFiles [i];
					var filename = Path.GetFileName (blendshapePath);

					// crude parsing of filenames
					if (!filename.EndsWith (".bin"))
						continue;
					var tokens = filename.Split (new []{ ".bin" }, StringSplitOptions.None);
					if (tokens.Length != 2)
						continue;

					var blendshapeName = tokens [0];
					blendshapes [blendshapeName] = blendshapeReader.ReadVerticesDeltas (blendshapePath);
					r.Progress = (float)i / blendshapeFiles.Count;
				}

				timer.Stop ();
				return blendshapes;
			}, AvatarSdkMgr.Str (Strings.ParsingBlendshapes));

			yield return request.AwaitSubrequest (loadBlendshapesRequest, finalProgress: 0.9f);
			if (request.IsError)
				yield break;

			var addBlendshapesTimer = DateTime.Now;
			float targetFps = 30.0f;

			int numBlendshapes = 0, loadedSinceLastPause = 0;
			var blendshapesDict = loadBlendshapesRequest.Result;
			foreach (var blendshape in blendshapesDict) {
				mesh.AddBlendShapeFrame (blendshape.Key, 100.0f, blendshape.Value, null, null);
				++numBlendshapes;
				++loadedSinceLastPause;

				if ((DateTime.Now - addBlendshapesTimer).TotalMilliseconds > 1000.0f / targetFps && loadedSinceLastPause >= 5) {
					// Debug.LogFormat ("Pause after {0} blendshapes to avoid blocking the main thread", numBlendshapes);
					yield return null;
					addBlendshapesTimer = DateTime.Now;
					loadedSinceLastPause = 0;
				}
			}

			request.Result = mesh;
			request.IsDone = true;
		}

		/// <summary>
		/// Read blendshapes from the avatar directory and add them to 3D head mesh.
		/// </summary>
		public static AsyncRequest<Mesh> AddBlendshapesAsync (string avatarId, Mesh mesh, int[] indexMap)
		{
			var request = new AsyncRequest<Mesh> (AvatarSdkMgr.Str (Strings.LoadingAnimations));
			AvatarSdkMgr.SpawnCoroutine (AddBlendshapes (avatarId, mesh, indexMap, request));
			return request;
		}

		#endregion Blendshapes

		#region Recoloring

		/// <summary>
		/// Average color across the haircut texture. We ignore the pixels with full transparency.
		/// </summary>
		/// <returns>The average color.</returns>
		/// <param name="texture">Unity texture.</param>
		public static Color CalculateAverageColor (Texture2D texture)
		{
			var w = texture.width;
			var h = texture.height;

			var pixels = texture.GetPixels ();

			var avgChannels = new double[3];
			Array.Clear (avgChannels, 0, avgChannels.Length);

			int numNonTransparentPixels = 0;
			float minAlphaThreshold = 0.1f;
			for (int i = 0; i < h; ++i)
				for (int j = 0; j < w; ++j) {
					var idx = i * w + j;
					if (pixels [idx].a < minAlphaThreshold)
						continue;

					++numNonTransparentPixels;
					avgChannels [0] += pixels [idx].r;
					avgChannels [1] += pixels [idx].g;
					avgChannels [2] += pixels [idx].b;
				}

			for (int ch = 0; ch < 3; ++ch) {
				avgChannels [ch] /= (double)numNonTransparentPixels;
				avgChannels [ch] = Math.Max (avgChannels [ch], 0.15);
			}

			return new Color ((float)avgChannels [0], (float)avgChannels [1], (float)avgChannels [2]);
		}

		/// <summary>
		/// Calculate what tint to apply given selected color and an average color of the texture.
		/// </summary>
		/// <returns>The tint color.</returns>
		/// <param name="targetColor">Target color.</param>
		/// <param name="avgColor">Average color.</param>
		public static Vector4 CalculateTint (Color targetColor, Color avgColor)
		{
			var tint = Vector4.zero;
			tint [0] = targetColor.r - avgColor.r;
			tint [1] = targetColor.g - avgColor.g;
			tint [2] = targetColor.b - avgColor.b;
			tint [3] = 0;  // alpha does not matter
			return tint;
		}

		#endregion

		#region Utils

		/// <summary>
		/// Checks whether the current platform is supported.
		/// </summary>
		/// <returns>True if platform is supported</returns>
		/// <param name="errorMessage">Outpur error message in case platform is not suppported.</param>
		public static bool IsPlatformSupported(SdkType sdkType, out string errorMessage)
		{
			bool isSupported = false;
			var runtimePlatform = Application.platform;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			isSupported = true;
#endif
#if UNITY_WEBGL
			if (sdkType == SdkType.Cloud)
				isSupported = true;
#endif

			if (!isSupported)
			{
				var msg = "'Offline' avatar generation is not supported for the current platform.\n";
				msg += "Your platform is: {0}\nList of supported platforms:\n{1}\n";
				msg += "\nPlease switch to one of the supported platforms in File -> Build Settings -> Switch platform\n";
				msg += "or use avatar generation in the Cloud (try samples from 'samples_cloud' folder).\n";
				msg += "We are planning to support offline avatar generation on more platforms in future versions,\n";
				msg += "please stay tuned and you won't miss the update!";

				var supportedPlatforms = new RuntimePlatform[] {
					RuntimePlatform.WindowsEditor,
					RuntimePlatform.WindowsPlayer,
					RuntimePlatform.Android,
					RuntimePlatform.IPhonePlayer,
					RuntimePlatform.OSXEditor,
					RuntimePlatform.OSXPlayer
				};

				var listOfSupported = string.Join("\n", supportedPlatforms.Select(p => p.ToString()).ToArray());
				msg = string.Format(msg, runtimePlatform.ToString(), listOfSupported);
				errorMessage = msg;
				return false;
			}

			var bitness = IntPtr.Size * 8;
			var platformIsWindows = runtimePlatform == RuntimePlatform.WindowsEditor || runtimePlatform == RuntimePlatform.WindowsPlayer;
			if (platformIsWindows && bitness != 64)
			{
				var msg = "'Offline' avatar generation for Windows currently works only in 64-bit version.\n";
				msg += "Please try to switch to x86_64 architecture in File -> Build Settings";
				errorMessage = msg;
				return false;
			}

			// exception not thrown, everything is fine!
			Debug.LogFormat("Platform is supported!");
			errorMessage = string.Empty;
			return true;
		}
		#endregion

		#region Avatar calculation params
		/// <summary>
		/// Prepares JSON with parameters required for avatar calculation.
		/// </summary>
		public static string GetAvatarCalculationParamsJson(AvatarResources avatarResources)
		{
			JSONObject resourcesJson = new JSONObject();

			if (avatarResources != null)
			{
				if (!IsListNullOrEmpty(avatarResources.blendshapes))
					resourcesJson["blendshapes"] = ListToJsonNode(avatarResources.blendshapes);

				if (!IsListNullOrEmpty(avatarResources.haircuts))
					resourcesJson["haircuts"] = ListToJsonNode(avatarResources.haircuts);
			}

			return resourcesJson.ToString(4);
		}

		/// <summary>
		/// Converts list with resources to JSONNode
		/// </summary>
		private static JSONNode ListToJsonNode(List<string> list)
		{
			Dictionary<string, JSONArray> groups = new Dictionary<string, JSONArray>();

			foreach(string item in list)
			{
				string[] subItems = item.Split(new char[] { '\\', '/' });
				if (subItems.Length == 2)
				{
					if (!groups.ContainsKey(subItems[0]))
						groups.Add(subItems[0], new JSONArray());
					groups[subItems[0]][""] = subItems[1];
				}
				else
					Debug.LogErrorFormat("Invalid resource name: {0}", item);
			}

			JSONObject baseNode = new JSONObject();
			foreach(var group in groups)
			{
				baseNode[group.Key] = group.Value;
			}

			return baseNode;
		}

		/// <summary>
		/// Checks if list is null or empty
		/// </summary>
		private static bool IsListNullOrEmpty(List<string> list)
		{
			return list == null || list.Count == 0;
		}
		#endregion

		#region haircuts naming
		/// <summary>
		/// Since SDK version 1.5.0 all haircuts ids have new format.
		/// To provide backward compatibility for avatars created by previous version of SDK, we need to distinguish them.
		/// This method allows to check if the haircut id is from the previos version or not.
		/// </summary>
		public static bool IsHaircutIdInOldFormat(string haircutId)
		{
			return haircutId.LastIndexOfAny(new char[] { '\\', '/' }) == -1;
		}

		/// <summary>
		/// Converts haircut id to new format if it is required
		/// </summary>
		public static string ConvertHaircutIdToNewFormat(string haircutId)
		{
			if (IsHaircutIdInOldFormat(haircutId))
				return string.Format("base/{0}", haircutId);

			return haircutId;
		}
		#endregion

		#region Export functionality

		/// <summary>
		/// Converts avatar mesh from ply to obj format
		/// </summary>
		public static void AvatarPlyToObj(string avatarId, AvatarFile avatarMesh, AvatarFile avatarTexture, string objFile)
		{
			var plyFile = AvatarSdkMgr.Storage().GetAvatarFilename(avatarId, avatarMesh);
			var srcTextureFile = AvatarSdkMgr.Storage().GetAvatarFilename(avatarId, avatarTexture);
			var dstTextureFile = Path.Combine(Path.GetDirectoryName(objFile), Path.GetFileNameWithoutExtension(objFile) + ".jpg");

			var returnCode = CreateMeshConverter().ConvertPlyModelToObj(plyFile, null, objFile, dstTextureFile);
			if (returnCode != 0)
			{
				Debug.LogErrorFormat("Unable convert avatar to obj. Error code: {0}", returnCode);
				return;
			}

			if (File.Exists(dstTextureFile))
				File.Delete(dstTextureFile);
			File.Copy(srcTextureFile, dstTextureFile);
		}

		/// <summary>
		/// Converts current haircut mesh from ply to obj format and saves recolored texture.
		/// </summary>
		public static void HaircutPlyToObj(string avatarId, string haircutId, string objFile, Color color, Vector4 tint)
		{
			var filenames = AvatarSdkMgr.Storage();
			var pointCloudPlyFile = filenames.GetAvatarHaircutPointCloudFilename(avatarId, haircutId);
			var haircutPlyFile = filenames.GetHaircutFilename(haircutId, HaircutFile.HAIRCUT_MESH_PLY);
			var srcHaircutTextureFile = filenames.GetHaircutFilename(haircutId, HaircutFile.HAIRCUT_TEXTURE);
			var dstHaircutTextureFile = Path.Combine(Path.GetDirectoryName(objFile), Path.GetFileNameWithoutExtension(objFile) + ".png");

			var returnCode = CreateMeshConverter().ConvertPlyModelToObj(pointCloudPlyFile, haircutPlyFile, objFile, dstHaircutTextureFile);
			if (returnCode != 0)
			{
				Debug.LogErrorFormat("Unable convert haircut to obj. Error code: {0}", returnCode);
				return;
			}

			RecolorAndSaveTexture(srcHaircutTextureFile, dstHaircutTextureFile, color, tint);
		}

		public static void ExportAvatarAsFbx(string avatarId, string fbxFile)
		{
			var plyFile = AvatarSdkMgr.Storage().GetAvatarFilename(avatarId, AvatarFile.MESH_PLY);
			var blendshapeDir = AvatarSdkMgr.Storage().GetAvatarSubdirectory(avatarId, AvatarSubdirectory.BLENDSHAPES);

			// copy texture to destination dir
			var srcTextureFile = AvatarSdkMgr.Storage().GetAvatarFilename(avatarId, AvatarFile.TEXTURE);
			var dstTextureFile = Path.Combine(Path.GetDirectoryName(fbxFile), Path.GetFileNameWithoutExtension(fbxFile) + ".jpg");
			if (File.Exists(dstTextureFile))
				File.Delete(dstTextureFile);
			File.Copy(srcTextureFile, dstTextureFile);

			var returnCode = CreateMeshConverter().ExportFbxWithBlendshapes(plyFile, dstTextureFile, blendshapeDir, fbxFile);
			if (returnCode != 0)
				Debug.LogErrorFormat("Unable export avatar to fbx. Error code: {0}", returnCode);
		}

		/// <summary>
		/// Converts current haircut mesh from ply to fbx format and saves recolored texture.
		/// </summary>
		public static void HaircutPlyToFbx(string avatarId, string haircutId, string fbxFile, Color color, Vector4 tint)
		{
			var filenames = AvatarSdkMgr.Storage();
			var pointCloudPlyFile = filenames.GetAvatarHaircutPointCloudFilename(avatarId, haircutId);
			var haircutPlyFile = filenames.GetHaircutFilename(haircutId, HaircutFile.HAIRCUT_MESH_PLY);
			var srcHaircutTextureFile = filenames.GetHaircutFilename(haircutId, HaircutFile.HAIRCUT_TEXTURE);
			var dstHaircutTextureFile = Path.Combine(Path.GetDirectoryName(fbxFile), Path.GetFileNameWithoutExtension(fbxFile) + ".png");

			var returnCode = CreateMeshConverter().СonvertPlyModelToFbx(pointCloudPlyFile, haircutPlyFile, fbxFile, dstHaircutTextureFile);
			if (returnCode != 0)
			{
				Debug.LogErrorFormat("Unable export haircut to fbx. Error code: {0}", returnCode);
				return;
			}

			RecolorAndSaveTexture(srcHaircutTextureFile, dstHaircutTextureFile, color, tint);
		}

		/// <summary>
		/// Saves recolored haircut texture.
		/// </summary>
		private static void RecolorAndSaveTexture(string srcTextureFile, string dstTextureFile, Color color, Vector4 tint)
		{
			byte[] bytes = File.ReadAllBytes(srcTextureFile);
			Texture2D texture = new Texture2D(2, 2);
			texture.LoadImage(bytes);
			Color[] pixels = texture.GetPixels();
			float threshold = 0.2f, tintCoeff = 0.8f;  // should be the same as in the shader
			for (int i = 0; i < pixels.Length; ++i)
			{
				Color tinted = pixels[i] + tintCoeff * new Color(tint.x, tint.y, tint.z);
				float maxTargetChannel = Math.Max(color.r, Math.Max(color.g, color.b));
				if (maxTargetChannel < threshold)
				{
					float darkeningCoeff = Math.Min(0.85f, (threshold - maxTargetChannel) / threshold);
					tinted = (1.0f - darkeningCoeff) * tinted + darkeningCoeff * (color * pixels[i]);
				}
				pixels[i].r = tinted.r;
				pixels[i].g = tinted.g;
				pixels[i].b = tinted.b;
			}
			texture.SetPixels(pixels);

			bytes = texture.EncodeToPNG();
			File.WriteAllBytes(dstTextureFile, bytes);
		}

		private static IMeshConverter CreateMeshConverter()
		{
			IMeshConverter meshConverter = AvatarSdkMgr.IoCContainer.Create<IMeshConverter>();
			if (meshConverter == null)
				Debug.LogError("Unable to create mesh converter");
			return meshConverter;
		}

		#endregion
	}
}