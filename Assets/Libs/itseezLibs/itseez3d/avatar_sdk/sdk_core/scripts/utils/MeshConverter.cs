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
using System.Runtime.InteropServices;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	public interface IMeshConverter
	{
		bool IsObjConvertEnabled { get; }

		bool IsFBXExportEnabled { get; }

		int ConvertPlyModelToObj(string plyModelFile, string templateModelFile, string objModelFile, string textureFile);

		int СonvertPlyModelToFbx(string plyModelFile, string templateModelFile, string fbxModelFile, string textureFile);

		int ExportFbxWithBlendshapes(string plyModelFile, string texturePath, string binaryBlendshapesDir, string outputFbxPath);
	}

	public class CoreMeshConverter : IMeshConverter
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX 
		[DllImport(DllHelperCore.dll)]
		protected static extern int convertPlyModelToObj(string plyModelFile, string templateModelFile, string objModelFile, string textureFile);

		public int ConvertPlyModelToObj(string plyModelFile, string templateModelFile, string objModelFile, string textureFile)
		{
			return convertPlyModelToObj(plyModelFile, templateModelFile, objModelFile, textureFile);
		}

		public bool IsObjConvertEnabled { get { return true; } }
#else
		public virtual int ConvertPlyModelToObj(string plyModelFile, string templateModelFile, string objModelFile, string textureFile)
		{
			Debug.LogError("Method not implemented!");
			return -1;
		}

		public virtual bool IsObjConvertEnabled { get { return false; } }
#endif

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || (UNITY_STANDALONE_OSX && !UNITY_EDITOR_OSX)
		[DllImport(DllHelperCore.dll)]
		protected static extern int convertPlyModelToFbx(string plyModelFile, string templateModelFile, string fbxModelFile, string textureFile);

		[DllImport(DllHelperCore.dll)]
		protected static extern int exportFbxWithBlendshapes(string plyModelFile, string texturePath, string binaryBlendshapesDir, string outputFbxPath);

		public int СonvertPlyModelToFbx(string plyModelFile, string templateModelFile, string fbxModelFile, string textureFile)
		{
			return convertPlyModelToFbx(plyModelFile, templateModelFile, fbxModelFile, textureFile);
		}

		public int ExportFbxWithBlendshapes(string plyModelFile, string texturePath, string binaryBlendshapesDir, string outputFbxPath)
		{
			return exportFbxWithBlendshapes(plyModelFile, texturePath, binaryBlendshapesDir, outputFbxPath);
		}

		public bool IsFBXExportEnabled { get { return true; } }
#else
		public int СonvertPlyModelToFbx(string plyModelFile, string templateModelFile, string fbxModelFile, string textureFile)
		{
		Debug.LogError("Method not implemented!");
		return -1;
		}

		public int ExportFbxWithBlendshapes(string plyModelFile, string texturePath, string binaryBlendshapesDir, string outputFbxPath)
		{
		Debug.LogError("Method not implemented!");
		return -1;
		}

		public virtual bool IsFBXExportEnabled { get { return false; } }
#endif
	}
}
