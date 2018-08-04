/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

/// <summary>
/// Fixes link errors in iOS build.
/// Courtesy to: http://answers.unity3d.com/questions/982417/how-to-specify-objc-in-other-linker-flag-in-xcode.html
/// </summary>
public static class IosBuildHelper
{
	[PostProcessBuild (700)]
	public static void OnPostProcessBuild (BuildTarget target, string pathToBuiltProject)
	{
		if (target != BuildTarget.iOS)
			return;
		Debug.Log ("Modifying link settings for iOS...");
		string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
		PBXProject proj = new PBXProject ();
		proj.ReadFromString (File.ReadAllText (projPath));
		string targetGUID = proj.TargetGuidByName ("Unity-iPhone");
		proj.SetBuildProperty (targetGUID, "IPHONEOS_DEPLOYMENT_TARGET", "8.0");
		File.WriteAllText (projPath, proj.WriteToString ());
		Debug.Log ("Linkage settings modified!");

		var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
		var plist = new PlistDocument();
		plist.ReadFromFile(plistPath);
		PlistElementDict rootDict = plist.root;
		rootDict.SetString("NSCameraUsageDescription", "Used for taking selfies");
		File.WriteAllText(plistPath, plist.WriteToString());
	}
}
#endif