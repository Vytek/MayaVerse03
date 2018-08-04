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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using SimpleJSON;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Predefined subdirectories inside avatar folder.
	/// </summary>
	public enum AvatarSubdirectory
	{
		BLENDSHAPES,
		OBJ_EXPORT,
		FBX_EXPORT,
	}

	/// <summary>
	/// "Types" of files encountered during avatar generation and loading.
	/// </summary>
	public enum AvatarFile
	{
		PHOTO,
		THUMBNAIL,
		MESH_PLY,
		MESH_ZIP,
		TEXTURE,
		HAIRCUT_POINT_CLOUD_PLY,
		HAIRCUT_POINT_CLOUD_ZIP,
		ALL_HAIRCUT_POINTS_ZIP,
		HAIRCUT_LIST, //Obsolete. Will be removed in next releases. 
		HAIRCUTS_JSON,
		BLEDNSHAPES_JSON,
		BLENDSHAPES_ZIP,
		BLENDSHAPES_FBX_ZIP,
		BLENDSHAPES_PLY_ZIP,
		PARAMETERS_JSON,
		PIPELINE_INFO
	}

	/// <summary>
	/// "Types" of files encountered during haircut loading.
	/// </summary>
	public enum HaircutFile
	{
		HAIRCUT_MESH_PLY,
		HAIRCUT_MESH_ZIP,
		HAIRCUT_TEXTURE,
		HAIRCUT_PREVIEW
	}

	/// <summary>
	/// SDK uses this interface to interact with the filesystem, e.g. save/load files and metadata.
	/// By default SDK will use DefaultPersistentStorage implementation. If your application stores files differently
	/// you can implement this interface and pass instance of your implementation to AvatarSdkMgr.Init() - this
	/// will override the default behavior. Probably the best way to implement IPersistentStorage is to derive from
	/// DefaultPersistentStorage.
	/// </summary>
	public abstract class IPersistentStorage
	{
		public abstract Dictionary<AvatarSubdirectory, string> AvatarSubdirectories { get; }

		public abstract Dictionary<AvatarFile, string> AvatarFilenames { get; }

		public abstract Dictionary<HaircutFile, string> HaircutFilenames { get; }

		public abstract string EnsureDirectoryExists (string d);

		public abstract string GetDataDirectory ();

		public abstract string GetResourcesDirectory ();

		public abstract string GetAvatarsDirectory ();

		public abstract string GetHaircutsDirectory ();

		public abstract string GetAvatarDirectory (string avatarCode);

		public abstract string GetAvatarSubdirectory (string avatarCode, AvatarSubdirectory dir);

		public abstract string GetAvatarFilename (string avatarCode, AvatarFile file);

		public abstract string GetHaircutFilename (string haircutId, HaircutFile file);

		public abstract Dictionary<string, string> GetAvatarHaircutsFilenames(string avatarCode);

		public abstract string GetAvatarHaircutPointCloudFilename (string avatarCode, string haircutId);

		public abstract string GetAvatarHaircutPointCloudZipFilename(string avatarCode, string haircutId);

		public abstract List<string> GetAvatarBlendshapesDirs(string avatarCode);

		public abstract void StorePlayerUID (string identifier, string uid);

		public abstract string LoadPlayerUID (string identifier);
	}

	/// <summary>
	/// Default implementation of IPersistentStorage.
	/// </summary>
	public class DefaultPersistentStorage : IPersistentStorage
	{
		#region data members

		private Dictionary<AvatarSubdirectory, string> avatarSubdirectories = new Dictionary<AvatarSubdirectory, string> () {
			{ AvatarSubdirectory.BLENDSHAPES, "blendshapes" },
			{ AvatarSubdirectory.OBJ_EXPORT, "obj" },
			{ AvatarSubdirectory.FBX_EXPORT, "fbx" },
		};

		private Dictionary<AvatarFile, string> avatarFiles = new Dictionary<AvatarFile, string> () {
			{ AvatarFile.PHOTO, "photo.jpg" },
			{ AvatarFile.THUMBNAIL, "thumbnail.jpg" },
			{ AvatarFile.MESH_PLY, "model.ply" },  // corresponds to file name inside zip
			{ AvatarFile.MESH_ZIP, "model.zip" },
			{ AvatarFile.TEXTURE, "model.jpg" },
			{ AvatarFile.HAIRCUT_POINT_CLOUD_PLY, "cloud_{0}.ply" },
			{ AvatarFile.HAIRCUT_POINT_CLOUD_ZIP, "{0}_points.zip" },
			{ AvatarFile.ALL_HAIRCUT_POINTS_ZIP, "all_haircut_points.zip" },
			// "haircut_list.txt" isn't generated any more. Use "haircuts.json" instead of it.
			{ AvatarFile.HAIRCUT_LIST, "haircut_list.txt" },
			{ AvatarFile.HAIRCUTS_JSON, "haircuts.json" },
			{ AvatarFile.BLEDNSHAPES_JSON, "blendshapes.json" },
			{ AvatarFile.BLENDSHAPES_ZIP, "blendshapes.zip" },
			{ AvatarFile.BLENDSHAPES_FBX_ZIP, "blendshapes_fbx.zip" },
			{ AvatarFile.BLENDSHAPES_PLY_ZIP, "blendshapes_ply.zip" },
			{ AvatarFile.PARAMETERS_JSON, "parameters.json"},
			{ AvatarFile.PIPELINE_INFO, "pipeline.txt"},
		};

		private Dictionary<HaircutFile, string> haircutFiles = new Dictionary<HaircutFile, string> () {
			{ HaircutFile.HAIRCUT_MESH_PLY, "{0}.ply" },  // corresponds to file name inside zip
			{ HaircutFile.HAIRCUT_MESH_ZIP, "{0}_model.zip" },
			{ HaircutFile.HAIRCUT_TEXTURE, "{0}_model.png" },
			{ HaircutFile.HAIRCUT_PREVIEW, "{0}_preview.png"},
		};

		private string dataRoot = string.Empty;

		#endregion

		#region implemented abstract members of IPersistentStorage

		public override Dictionary<AvatarSubdirectory, string> AvatarSubdirectories { get { return avatarSubdirectories; } }

		public override Dictionary<AvatarFile, string> AvatarFilenames { get { return avatarFiles; } }

		public override Dictionary<HaircutFile, string> HaircutFilenames { get { return haircutFiles; } }

		public override string EnsureDirectoryExists (string d)
		{
			if (!Directory.Exists (d))
				Directory.CreateDirectory (d);
			return d;
		}

		/// <summary>
		/// Native plugins do not currently support non-ASCII file paths. Therefore we must choose
		/// location that only contains ASCII characters in its path and is read-write accessible.
		/// This function will try different options before giving up.
		/// </summary>
		public override string GetDataDirectory ()
		{
			if (string.IsNullOrEmpty (dataRoot)) {
				var options = new string[] {
					Application.persistentDataPath,
					#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
					Utils.CombinePaths (Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData), "avatar_sdk"),
					Utils.CombinePaths ("C:\\", "avatar_sdk_data"),
					#endif
					Utils.CombinePaths (Application.dataPath, "..", "avatar_sdk"),
				};

				for (int i = 0; i < options.Length; ++i) {
					Debug.LogFormat ("Trying {0} as data root...", options [i]);
					if (Utils.HasNonAscii (options [i])) {
						Debug.LogWarningFormat ("Data path \"{0}\" contains non-ASCII characters, trying next option...", options [i]);
						continue;
					}

					try {
						// make sanity checks to make sure we actually have read-write access to the directory
						EnsureDirectoryExists (options [i]);
						var testFilePath = Path.Combine (options [i], "test.file");
						File.WriteAllText (testFilePath, "test");
						File.ReadAllText (testFilePath);
						File.Delete (testFilePath);
					} catch (Exception ex) {
						Debug.LogException (ex);
						Debug.LogWarningFormat ("Could not access {0}, trying next option...", options [i]);
						continue;
					}

					dataRoot = options [i];
					break;
				}
			}

			if (string.IsNullOrEmpty (dataRoot))
				throw new Exception ("Could not find directory for persistent data! See log for details.");

			return EnsureDirectoryExists (dataRoot);
		}

		public override string GetResourcesDirectory ()
		{
			return EnsureDirectoryExists (Path.Combine (GetDataDirectory (), "resources"));
		}

		public override string GetAvatarsDirectory ()
		{
			return EnsureDirectoryExists (Path.Combine (GetDataDirectory (), "avatars"));
		}

		public override string GetHaircutsDirectory ()
		{
			return EnsureDirectoryExists (Path.Combine (GetDataDirectory (), "haircuts"));
		}

		public override string GetAvatarDirectory (string avatarCode)
		{
			return EnsureDirectoryExists (Path.Combine (GetAvatarsDirectory (), avatarCode));
		}

		public override string GetAvatarSubdirectory (string avatarCode, AvatarSubdirectory dir)
		{
			return EnsureDirectoryExists (Path.Combine (GetAvatarDirectory (avatarCode), AvatarSubdirectories [dir]));
		}

		public override string GetAvatarFilename (string avatarCode, AvatarFile file)
		{
			return Path.Combine (GetAvatarDirectory (avatarCode), AvatarFilenames [file]);
		}

		public override string GetHaircutFilename (string haircutId, HaircutFile file)
		{
			haircutId = CoreTools.ConvertHaircutIdToNewFormat(haircutId);
			var filename = string.Format (HaircutFilenames [file], haircutId);
			return Path.Combine (GetHaircutsDirectory (), filename);
		}

		public override string GetAvatarHaircutPointCloudFilename(string avatarCode, string haircutId)
		{
			// Get haircut pointcloud file path from the haircuts_list file.
			// If there is no such file or it doesn't contain requested haircut, use default naming
			string pointCloudFilename = string.Format(AvatarFilenames[AvatarFile.HAIRCUT_POINT_CLOUD_PLY], haircutId);

			var haircutsWithFilenames = GetAvatarHaircutsFilenames(avatarCode);
			if (haircutsWithFilenames.ContainsKey(haircutId))
				pointCloudFilename = haircutsWithFilenames[haircutId];

			return Path.Combine(GetAvatarDirectory(avatarCode), pointCloudFilename);
		}

		private string PlayerUIDFilename (string identifier)
		{
			var filename = string.Format ("player_uid_{0}.dat", identifier);
			var path = Path.Combine (GetDataDirectory (), filename);
			return path;
		}

		public override void StorePlayerUID (string identifier, string uid)
		{
			try {
				Debug.LogFormat ("Storing player UID: {0}", uid);
				var uidText = Convert.ToBase64String (UTF8Encoding.UTF8.GetBytes (uid));
				var path = PlayerUIDFilename (identifier);
				File.WriteAllText (path, uidText);
			} catch (Exception ex) {
				Debug.LogErrorFormat ("Could not store player UID in a file, msg: {0}", ex.Message);
			}
		}

		public override string LoadPlayerUID (string identifier)
		{
			try {
				var path = PlayerUIDFilename (identifier);
				if (!File.Exists (path))
					return null;
				return UTF8Encoding.UTF8.GetString (Convert.FromBase64String (File.ReadAllText (path)));
			} catch (Exception ex) {
				Debug.LogWarningFormat ("Could not read player_uid from file: {0}", ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Reads haircuts' ids and pointcloud filenames from the haircuts list file 
		/// </summary>
		public override Dictionary<string, string> GetAvatarHaircutsFilenames(string avatarCode)
		{
			Dictionary<string, string> haircuts = new Dictionary<string, string>();
			try
			{
				string haircutsListFilename = GetAvatarFilename(avatarCode, AvatarFile.HAIRCUT_LIST);
				string haircutsJsonFilename = GetAvatarFilename(avatarCode, AvatarFile.HAIRCUTS_JSON);

				if (File.Exists(haircutsListFilename))
				{
					// this avatar was generated by version 1.4.0 or earlier
					string[] haircutListContent = File.ReadAllLines(haircutsListFilename);
					foreach (string line in haircutListContent)
					{
						string haircutId = line;
						haircuts.Add(haircutId, string.Format(AvatarFilenames[AvatarFile.HAIRCUT_POINT_CLOUD_PLY], haircutId));
					}
				}
				else if (File.Exists(haircutsJsonFilename))
				{
					var jsonContent = JSON.Parse(File.ReadAllText(haircutsJsonFilename));
					foreach (var haircutNameJson in jsonContent.Keys)
					{
						string haircutId = haircutNameJson.Value.ToString().Replace("\"", "");
						var haircutPathJson = jsonContent[haircutId];
						haircuts.Add(haircutId, haircutPathJson.ToString().Replace("\"", ""));
					}
				}
			}
			catch (Exception exc)
			{
				Debug.LogErrorFormat("Unable to read haircuts json file: {0}", exc);
			}
			return haircuts;
		}

		public override string GetAvatarHaircutPointCloudZipFilename(string avatarCode, string haircutId)
		{
			string zipFilename = string.Format(AvatarFilenames[AvatarFile.HAIRCUT_POINT_CLOUD_ZIP], haircutId);
			return Path.Combine(GetAvatarDirectory(avatarCode), zipFilename);
		}

		public override List<string> GetAvatarBlendshapesDirs(string avatarCode)
		{
			List<string> blendshapesDirs = new List<string>();
			try
			{
				string blendshapesJsonFilename = GetAvatarFilename(avatarCode, AvatarFile.BLEDNSHAPES_JSON);
				if (File.Exists(blendshapesJsonFilename))
				{
					var jsonContent = JSON.Parse(File.ReadAllText(blendshapesJsonFilename));
					foreach (var blendshapesNameJson in jsonContent.Keys)
					{
						string blendshapesId = blendshapesNameJson.Value.ToString().Replace("\"", "");
						var blendshapesPathJson = jsonContent[blendshapesId];
						blendshapesDirs.Add(Path.Combine(GetAvatarDirectory(avatarCode), blendshapesPathJson.ToString().Replace("\"", "")));
					}
				}
				else
					blendshapesDirs.Add(GetAvatarSubdirectory(avatarCode, AvatarSubdirectory.BLENDSHAPES));
			}
			catch (Exception exc)
			{
				Debug.LogErrorFormat("Unable to read blendshapes json file: {0}", exc);
			}
			return blendshapesDirs;
		}

		#endregion
	}
}

