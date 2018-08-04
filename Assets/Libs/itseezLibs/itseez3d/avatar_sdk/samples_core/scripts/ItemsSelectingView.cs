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
using UnityEngine;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// This script displays items collection by creating Toggle control for each element and allows to select any of these items 
	/// </summary>
	public class ItemsSelectingView : MonoBehaviour
	{
		// Panel on which items will be displayed
		public GameObject itemsPanel;

		// View containg all controls
		public GameObject itemsView;

		// Toggle prefab that will be instantiated
		public Toggle togglePrefab;

		// Toggle group if only one selected item allowed
		public ToggleGroup toggleGroup;

		// Items selected by default
		protected List<string> defaultSelectedItems = null;

		// Close action callback
		protected Action<List<string>> closeCallback = null;

		// List of instantiated toggles
		protected List<Toggle> toggles = new List<Toggle>();

		// True if view is shown and active
		protected bool isShown = false;

		/// <summary>
		/// Creates toggle for each item
		/// </summary>
		/// <param name="items"></param>
		public virtual void InitItems(List<string> items)
		{
			foreach (Toggle t in toggles)
				Destroy(t.gameObject);
			toggles.Clear();

			foreach (string item in items)
			{
				Toggle toggle = Instantiate<Toggle>(togglePrefab);
				toggle.isOn = false;
				toggle.transform.localScale = itemsPanel.transform.lossyScale;
				toggle.gameObject.transform.SetParent(itemsPanel.transform);
				ToggleId toggleId = toggle.gameObject.GetComponentInChildren<ToggleId>();
				toggleId.Id = item;
				toggle.group = toggleGroup;

				toggles.Add(toggle);
			}
		}

		/// <summary>
		/// Shows list view
		/// </summary>
		public virtual void Show(List<string> selectedItems, Action<List<string>> closeAction)
		{
			itemsView.SetActive(true);

			defaultSelectedItems = selectedItems;
			closeCallback = closeAction;

			foreach(Toggle t in toggles)
			{
				string id = t.GetComponentInChildren<ToggleId>().Id;
				t.isOn = selectedItems.Contains(id);
			}

			isShown = true;
		}

		/// <summary>
		/// Returns selected items
		/// </summary>
		public List<string> CurrentSelection
		{
			get
			{
				List<string> selectedItems = new List<string>();
				foreach(Toggle t in toggles)
				{
					if (t.isOn)
						selectedItems.Add(t.GetComponentInChildren<ToggleId>().Id);
				}
				return selectedItems;
			}
		}

		/// <summary>
		/// Done button click handler. Closes view with the current selection.
		/// </summary>
		public void OnDoneClick()
		{
			itemsView.SetActive(false);
			if (closeCallback != null)
				closeCallback(CurrentSelection);
			isShown = false;
		}

		/// <summary>
		/// Cancel button click handler. Closes view with default selection.
		/// </summary>
		public void OnCancelClick()
		{
			itemsView.SetActive(false);
			if (closeCallback != null)
				closeCallback(defaultSelectedItems);
			isShown = false;
		}

		/// <summary>
		/// Default button click handler. Resets selection to default.
		/// </summary>
		public void OnSelectDefaultClick()
		{
			foreach(Toggle t in toggles)
			{
				string id = t.GetComponentInChildren<ToggleId>().Id;
				t.isOn = defaultSelectedItems.Contains(id);
			}
		}

		/// <summary>
		/// SelectAll button click handler. Selects all elements. If all elements are already selected, turns them off.
		/// </summary>
		public void OnSelectAllClick()
		{
			bool isAllChecked = toggles.TrueForAll(t => t.isOn);
			foreach (Toggle t in toggles)
				t.isOn = !isAllChecked;
		}
	}
}
