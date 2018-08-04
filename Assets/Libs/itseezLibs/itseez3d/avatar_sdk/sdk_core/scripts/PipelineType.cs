/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

namespace ItSeez3D.AvatarSdk.Core
{
	public enum PipelineType
	{
		/// <summary>
		/// Standard pipeline (bald head with the neck, supports different haircuts, supports blendshapes).
		/// </summary>
		FACE,

		/// <summary>
		/// Pipeline that generates head with hair and shoulders (supports blendshapes)
		/// </summary>
		HEAD
	}

	public static class PipelineTypeExtensions
	{
		public static string GetPipelineTypeName(this PipelineType pipelineType)
		{
			switch (pipelineType)
			{
				case PipelineType.HEAD:
					return "head";

				case PipelineType.FACE:
				default:
					return "animated_face";
			}
		}
	}
}
