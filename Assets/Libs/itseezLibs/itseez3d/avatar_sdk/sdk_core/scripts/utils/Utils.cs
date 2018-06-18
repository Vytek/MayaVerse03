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
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Little helpers that have no other place to go.
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Currently native plugins (compiled from C and C++) do not support non-ASCII file paths.
		/// This is to be fixed in the future.
		/// </summary>
		public static bool HasNonAscii (string s)
		{
			return s.Any (c => c > 127);
		}

		/// <summary>
		/// In editor display the actual window, otherwise just log a warning.
		/// </summary>
		public static void DisplayWarning (string title, string msg)
		{
			Debug.LogFormat ("{0}: {1}", title, msg);
			#if UNITY_EDITOR
			EditorUtility.DisplayDialog (title, msg, "Ok");
			#endif
		}

		/// <summary>
		/// C# should have this overload out of the box, honestly.
		/// </summary>
		public static string CombinePaths (params string[] tokens)
		{
			return tokens.Aggregate (Path.Combine);
		}

		/// <summary>
		/// Creates the directory inside "Assets" folder if necessary.
		/// </summary>
		/// <param name="tokens">Tokens.</param>
		#if UNITY_EDITOR
		public static string EnsureEditorDirectoryExists (params string[] tokens)
		{
			List<string> existingPath = new List<string> { "Assets" };
			for (int i = 0; i < tokens.Length; ++i) {
				var prevPathStr = string.Join ("/", existingPath.ToArray ());
				existingPath.Add (tokens [i]);
				var existingPathStr = string.Join ("/", existingPath.ToArray ());
				if (!AssetDatabase.IsValidFolder (existingPathStr))
					AssetDatabase.CreateFolder (prevPathStr, tokens [i]);
				AssetDatabase.SaveAssets ();
			}
			AssetDatabase.Refresh ();
			return string.Join ("/", existingPath.ToArray ());
		}
		#endif

		/// <summary>
		/// Return true if we're currently in editor and not playing the game.
		/// </summary>
		public static bool IsDesignTime ()
		{
			#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying)
				return true;
			#endif

			return false;
		}

		public static GameObject FindSubobjectByName (GameObject obj, string name)
		{
			foreach (var trans in obj.GetComponentsInChildren<Transform>())
				if (trans.name == obj.name)
					return trans.gameObject;

			return null;
		}
	}
}

