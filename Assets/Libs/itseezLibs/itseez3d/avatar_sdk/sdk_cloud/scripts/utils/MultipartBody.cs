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
using System.Text;
using UnityEngine.Networking;

namespace ItSeez3D.AvatarSdk.Cloud
{
	/// <summary>
	/// Unity 5.5.0 (and probably earlier versions) have a weird bug in default multipart form data
	/// implementation, which causes incorrect boundaries between data fields. To work around this bug the
	/// multipart request body is constructed manually by this class.
	/// </summary>
	public class MultipartBody : IDisposable
	{
		private byte[] boundary;
		private string boundaryString, separator;
		private MemoryStream stream;
		private BinaryWriter requestBody;

		public MultipartBody ()
		{
			boundary = UnityWebRequest.GenerateBoundary ();
			boundaryString = Encoding.UTF8.GetString (boundary);
			separator = string.Format ("\r\n--{0}\r\n", boundaryString);

			stream = new MemoryStream ();
			requestBody = new BinaryWriter (stream);
		}

		private void WriteStr (string s)
		{
			requestBody.Write (Encoding.UTF8.GetBytes (s));
		}

		public string Boundary { get { return boundaryString; } }

		public void WriteTextField (string name, string value)
		{
			WriteStr (separator);
			WriteStr (string.Format ("Content-Disposition: form-data; name=\"{0}\"\r\n", name));
			WriteStr ("Content-Type: text/plain; encoding=utf-8\r\n\r\n");
			WriteStr (value);
		}

		public void WriteFileField (string name, string filename, byte[] data)
		{
			WriteStr (separator);
			WriteStr (string.Format ("Content-Disposition: file; name=\"{0}\"; filename=\"{1}\"\r\n", name, filename));
			WriteStr ("Content-Type: application/octet-stream\r\n\r\n");
			requestBody.Write (data);
		}

		public void WriteFooter ()
		{
			WriteStr (separator);
			WriteStr ("--\r\n");
		}

		public byte[] GetRequestBodyData ()
		{
			return stream.ToArray ();
		}

		#region IDisposable implementation

		public void Dispose ()
		{
			requestBody.Close ();
			stream.Close ();
		}

		#endregion
	}
}

