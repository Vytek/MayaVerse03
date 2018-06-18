/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

var fileBrowser = 
{
	fileBrowserInit: function(objectNamePtr, callbackFuncNamePtr)
	{
		var objectName = Pointer_stringify(objectNamePtr);
		var funcName = Pointer_stringify(callbackFuncNamePtr);

		var fileuploader = document.getElementById('fileuploader');
		if (!fileuploader)
		{
			fileuploader = document.createElement('input');
			fileuploader.setAttribute('style','display:none;');
			fileuploader.setAttribute('type', 'file');
			fileuploader.setAttribute('accept', 'image/*');
			fileuploader.setAttribute('id', 'fileuploader');
			document.getElementsByTagName('body')[0].appendChild(fileuploader);

			fileuploader.onchange = function(e) 
			{
				var files = e.target.files;
				for (var i = 0, f; f = files[i]; i++) 
				{
					SendMessage(objectName, funcName, URL.createObjectURL(f));
				}
			};
		}
		document.addEventListener('click', function() 
		{
			var fileuploader = document.getElementById('fileuploader');
			if (fileuploader && fileuploader.getAttribute('class') == 'focused') 
			{
				fileuploader.setAttribute('class', '');
				fileuploader.click();
			}
		});
	},

	fileBrowserSetFocus: function()
	{
		var fileuploader = document.getElementById('fileuploader');
		if (fileuploader)
			fileuploader.setAttribute('class', 'focused');
	}
};

mergeInto(LibraryManager.library, fileBrowser);