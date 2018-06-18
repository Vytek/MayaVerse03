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
using ICSharpCode.SharpZipLib.Zip;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Tiny utility class that wraps SharpZipLib functionality.
	/// </summary>
	public static class ZipUtils
	{
		static ZipUtils()
		{
			ICSharpCode.SharpZipLib.Zip.ZipConstants.DefaultCodePage = 0;
		}

		public static void Unzip(string zipFilePath, string location)
		{
			using (var s = new ZipInputStream(File.OpenRead(zipFilePath)))
				Unzip(s, location);
		}

		public static void Unzip(byte[] bytes, string location)
		{
			using (var s = new ZipInputStream(new MemoryStream(bytes)))
				Unzip(s, location);
		}

		public static void Unzip(ZipInputStream s, string location)
		{
			ZipEntry theEntry;
			while ((theEntry = s.GetNextEntry()) != null)
			{
				string directoryName = Path.GetDirectoryName(theEntry.Name);
				if (directoryName.Length > 0)
					Directory.CreateDirectory(Path.Combine(location, directoryName));

				string fileName = Path.GetFileName(theEntry.Name);
				if (string.IsNullOrEmpty(fileName))
					continue;

				using (FileStream streamWriter = File.Create(Path.Combine(location, theEntry.Name)))
				{
					int size = 2048;
					byte[] data = new byte[size];
					while (true)
					{
						size = s.Read(data, 0, data.Length);
						if (size <= 0)
							break;

						streamWriter.Write(data, 0, size);
					}
				}
			}
		}
	}
}