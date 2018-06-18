/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/
using ItSeez3D.AvatarSdk.Core;

namespace ItSeez3D.AvatarSdk.Cloud
{
	/// <summary>
	/// For web requests we can track progress for either upload or download, not both.
	/// </summary>
	public enum TrackProgress
	{
		UPLOAD,
		DOWNLOAD,
	}

	/// <summary>
	/// Async request for web requests.
	/// </summary>
	public class AsyncWebRequest<DataType> : AsyncRequest<DataType>
	{
		private TrackProgress progressTracking = TrackProgress.DOWNLOAD;
		private StatusCode statusCode = new StatusCode ();

		/// <summary>
		/// Initializes a new instance of the <see cref="ItSeez3D.AsyncWebRequest`1"/> class.
		/// </summary>
		/// <param name="initialState">Initial value to be returned by State property. Usually a name of the
		/// operation, e.g. "Downloading file".</param>
		/// <param name="progressTracking">Set to UPLOAD if you're uploading file to server, DOWNLOAD otherwise.</param>
		public AsyncWebRequest (string initialState = "", TrackProgress progressTracking = TrackProgress.DOWNLOAD)
			: base (initialState)
		{
			this.progressTracking = progressTracking;
		}

		/// <summary>
		/// Type of progress tracking.
		/// </summary>
		public TrackProgress ProgressTracking { get { return progressTracking; } }

		/// <summary>
		/// HTTP status code (e.g. 200 or 404).
		/// </summary>
		public StatusCode Status {
			get { return statusCode; }
			set { statusCode = value; }
		}

		public ulong BytesUploaded { get; set; }

		public ulong BytesDownloaded { get; set; }

		/// <summary>
		/// True if error is set or if HTTP status is "bad".
		/// </summary>
		public override bool IsError {
			get {
				bool statusBad = Status != null && Status.IsBad;
				return base.IsError || statusBad;
			}
		}

		/// <summary>
		/// Detailed error message with the status code.
		/// </summary>
		public override string ErrorMessage {
			get {
				if (Status.Value == (long)StatusCode.Code.UNKNOWN || Status.IsGood)
					return base.ErrorMessage;
				else {
					var msg = string.Format ("{0}. Status: {1}", base.ErrorMessage, Status);
					return msg;
				}
			}
		}
	}

	/// <summary>
	/// Async request for web requests where we don't need the result.
	/// </summary>
	public class AsyncWebRequest : AsyncWebRequest<object>
	{
		public AsyncWebRequest (string initialState = "", TrackProgress progressTracking = TrackProgress.DOWNLOAD)
			: base (initialState, progressTracking)
		{
		}
	}
}