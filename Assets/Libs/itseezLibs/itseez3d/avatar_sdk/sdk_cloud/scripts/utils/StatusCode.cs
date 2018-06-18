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

namespace ItSeez3D.AvatarSdk.Cloud
{
	/// <summary>
	/// Helper class for HTTP status codes.
	/// </summary>
	public class StatusCode
	{
		/// <summary>
		/// Codes that can be returned by the Avatar Web API.
		/// </summary>
		public enum Code : long
		{
			UNKNOWN = -1,

			OK = 200,
			CREATED = 201,
			ACCEPTED = 202,

			BAD_REQUEST = 400,
			UNAUTHORIZED = 401,
			PAYMENT_REQUIRED = 402,
			FORBIDDEN = 403,
			NOT_FOUND = 404,
			TOO_MANY_REQUESTS_THROTTLING = 429,

			SERVER_ERROR = 500,
			BAD_GATEWAY = 502,
			TIMEOUT = 504,
		}

		public StatusCode ()
		{
			Value = (long)Code.UNKNOWN;
		}

		public StatusCode (long value)
		{
			Value = value;
		}

		public long Value { get; set; }

		public bool IsGood {
			get { return (Value >= 200 && Value < 400) || Value == (long)Code.UNKNOWN; }
		}

		public bool IsBad { get { return !IsGood; } }

		public override string ToString ()
		{
			if (Value == (long)Code.UNKNOWN)
				return string.Empty;
			else if (Enum.IsDefined (typeof(Code), Value))
				return string.Format ("{0} ({1})", Value, ((Code)Value).ToString ());
			else
				return Value.ToString ();
		}
	}
}