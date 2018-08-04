/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/
using System.Collections;

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core.Editor
{
	/// <summary>
	/// Provides a way to create async tasks in the editor.
	/// </summary>
	public static class EditorAsync
	{
		public class EditorAsyncTask
		{
			public Func<bool> IsDone { get; private set; }

			public Action OnCompleted { get; private set; }

			public EditorAsyncTask (Func<bool> isDone, Action onCompleted)
			{
				IsDone = isDone;
				OnCompleted = onCompleted;
			}
		}

		private static readonly List<EditorAsyncTask> tasks = new List<EditorAsyncTask> ();

		public static void ProcessTask (EditorAsyncTask task)
		{
			if (tasks.Count == 0)
				EditorApplication.update += Process;
			tasks.Add (task);
		}

		private static void Process ()
		{
			for (int i = tasks.Count - 1; i >= 0; --i) {
				try {
					var task = tasks [i];
					if (!task.IsDone ())
						continue;

					tasks.RemoveAt (i);
					task.OnCompleted ();
				} catch (Exception ex) {
					Debug.LogException (ex);
				}
			}

			if (tasks.Count == 0)
				EditorApplication.update -= Process;
		}
	}

	/// <summary>
	/// Emulates coroutines in editor. Very simple implementation, does not support nested coroutines.
	/// </summary>
	public class EditorCoroutineManager
	{
		public class EditorCoroutine
		{
			public Stack<object> yields = new Stack<object> ();

			public EditorCoroutine (IEnumerator routine)
			{
				yields.Push (routine);
			}
		}

		private EditorCoroutine currentCoroutine;
		private Queue<EditorCoroutine> waitingCoroutines = new Queue<EditorCoroutine>();
		private object syncMutex = new object();

		private static EditorCoroutineManager instance = null;

		private static EditorCoroutineManager Instance {
			get {
				if (instance == null)
					instance = new EditorCoroutineManager ();
				return instance;
			}
		}

		public static EditorCoroutine Start (IEnumerator routine)
		{
			return Instance.StartCoroutine (routine);
		}

		private EditorCoroutine StartCoroutine (IEnumerator routine)
		{
			lock(syncMutex)
			{
				if (currentCoroutine == null)
				{
					currentCoroutine = new EditorCoroutine(routine);
					EditorApplication.update += Update;
					return currentCoroutine;
				}
				else
				{
					EditorCoroutine coroutine = new EditorCoroutine(routine);
					waitingCoroutines.Enqueue(coroutine);
					return coroutine;
				}
			}
		}

		private void StopCoroutine ()
		{
			lock(syncMutex)
			{
				if (waitingCoroutines.Count > 0)
				{
					currentCoroutine = waitingCoroutines.Dequeue();
				}
				else
				{
					EditorApplication.update -= Update;
					currentCoroutine = null;
				}
			}
		}

		private void Update ()
		{
			try {
				if (currentCoroutine.yields.Count == 0) {
					StopCoroutine ();
					return;
				}

				var currentYieldObject = currentCoroutine.yields.Peek ();
				if (currentYieldObject == null)
					throw new Exception ("Current yield is null!");

				if (!(currentYieldObject is IEnumerator))
					throw new Exception ("Unsupported type of yield object! Only IEnumerators are supported!");

				var currentYield = currentYieldObject as IEnumerator;
				var nextYield = currentYield.Current;

				if (nextYield == null)
				{
					if (!currentYield.MoveNext ())
						currentCoroutine.yields.Pop ();
				}
				else if (nextYield is AsyncOperation)
				{
					AsyncOperation op = nextYield as AsyncOperation;
					if (op.isDone)
					{
						if (!currentYield.MoveNext())
							currentCoroutine.yields.Pop();
					}
				}
				else if (nextYield is IEnumerator)
				{
					// nested yield
					if ((nextYield as IEnumerator).MoveNext ())
						currentCoroutine.yields.Push (nextYield);
					else
					{
						// finished the nested yield, return back to the current one
						if (!currentYield.MoveNext ())
							currentCoroutine.yields.Pop ();
					}
				}
				else
				{
					throw new Exception (string.Format ("Unsupported nested yield type: {0}", nextYield.GetType ().ToString ()));
				}
			} catch (Exception ex) {
				Debug.LogException (ex);
				StopCoroutine ();
			}
		}
	}
}
#endif