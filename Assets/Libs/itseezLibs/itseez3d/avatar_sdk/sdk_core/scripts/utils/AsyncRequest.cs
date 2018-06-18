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
using System.Collections;
using UnityEngine;
using System.Threading;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// The main return type for most SDK calls. Analogue of Unity's WWW and UnityWebRequest classes.
	/// Should be used inside coroutines. AsyncRequests allow chaining of multiple async calls in a single coroutine,
	/// much like the traditional sequential code. Class is derived from CustomYieldInstruction, which allows you
	/// to yield on it, and then check the return value or the error once the async operation is completed.
	/// See the samples for usage examples.
	/// </summary>
	public class AsyncRequest : CustomYieldInstruction
	{
		protected bool isDone = false;
		protected string error = string.Empty;
		protected object mutex = new object ();

		/// <summary>
		/// Initializes a new instance of the <see cref="ItSeez3D.AsyncRequest"/> class.
		/// </summary>
		/// <param name="initialState">Initial value to be returned by State property. Usually a name of the
		/// operation, e.g. "Downloading file".</param>
		public AsyncRequest (string initialState = "")
		{
			State = initialState;
		}

		/// <summary>
		/// Overridden property of CustomYieldInstruction. Allows you to yield on objects of this class inside
		/// coroutines.
		/// </summary>
		public override bool keepWaiting { get { return !IsDone; } }

		/// <summary>
		/// True if async operation is completed (with success or error).
		/// </summary>
		public bool IsDone {
			get {
				lock (mutex)
					return isDone || IsError;
			}
			set {
				lock (mutex) {
					isDone = value;
					if (isDone)
						Progress = 1.0f;
				}
			}
		}

		/// <summary>
		/// True if async request has encountered an error.
		/// </summary>
		public virtual bool IsError {
			get {
				lock (mutex)
					return !string.IsNullOrEmpty (error);
			}
		}

		/// <summary>
		/// Error message.
		/// </summary>
		public virtual string ErrorMessage { get { return error; } }

		/// <summary>
		/// Progress (from 0.0 to 1.0)
		/// </summary>
		public float Progress { get; set; }

		/// <summary>
		/// Progress in % (from 0 to 100).
		/// </summary>
		public float ProgressPercent { get { return Progress * 100; } }

		/// <summary>
		/// Set the error message. If non-empty also sets IsDone = true and IsError = true.
		/// </summary>
		public void SetError (string errorMsg)
		{
			lock (mutex)
				error = errorMsg;
		}

		/// <summary>
		/// Current "stage" of the async operation. Some requests may go through multiple stages. E.g. - waiting avatar
		/// from server goes through Queued, Computing, Completed stages.
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// Some async operations are composite, e.g. may be implemented as a sequence of async sub-operations.
		/// This property returns current sub-request or null if there's no such thing.
		/// The nesting level of subrequests can be arbitrary, but generally no bigger than three.
		/// You can iterate over subrequests to get detailed information about "main" request progress (see samples
		/// for usage examples).
		/// </summary>
		public AsyncRequest CurrentSubrequest { get; set; }

		public IEnumerator Await ()
		{
			yield return this;
		}

		/// <summary>
		/// Tiny wrapper around AwaitSubrequests for trivial case.
		/// </summary>
		public IEnumerator AwaitSubrequest (AsyncRequest request, float finalProgress)
		{
			return AwaitSubrequests (finalProgress, request);
		}

		/// <summary>
		/// Helper function that allows implementation of the "main" request as a sequence of async subrequests.
		/// Usage: yield return request.AwaitSubrequest(subrequest, finalProgress: 0.5f)
		/// </summary>
		/// <param name="subrequest">Subrequest - the async operation that is a part of "main" request.</param>
		/// <param name="finalProgress">Progress of main operation by the end of this sub-operation.</param>
		public IEnumerator AwaitSubrequests (float finalProgress, params AsyncRequest[] subrequests)
		{
			if (IsDone || IsError) {
				Debug.LogError ("Cannot start subrequest for request that is already finished!");
				yield break;
			}

			if (subrequests.Length == 0) {
				Debug.LogError ("Cannot await on empty list of subrequests");
				yield break;
			}

			if (finalProgress < Progress)
				Debug.LogWarningFormat (
					"Cannot not rollback progress from {0} to {1}, progress can only move forward",
					Progress, finalProgress
				);

			float initialProgress = Progress;
			float progressShare = Math.Max (0, finalProgress - initialProgress);
			float progressSharePerSubrequest = progressShare / subrequests.Length;
			while (!IsDone) {
				float newProgress = initialProgress;
				int numUnfinished = subrequests.Length;
				bool currentSubrequestSet = false;
				foreach (var subrequest in subrequests) {
					if (subrequest.IsError) {
						SetError (string.Format ("{0} failed, reason: {1}", State, subrequest.ErrorMessage));
						Debug.LogWarning (ErrorMessage);
						break;
					} else {
						newProgress += progressSharePerSubrequest * subrequest.Progress;
						if (newProgress > 1.0001f) {
							Debug.LogWarningFormat ("Progress for {0} is more than 1 ({1})!", State, newProgress);
							newProgress = 0.99f;
						}

						if (subrequest.IsDone)
							--numUnfinished;
						else if (!currentSubrequestSet) {
							CurrentSubrequest = subrequest;
							currentSubrequestSet = true;
						}
					}
				}

				if (!IsDone)
					Progress = newProgress;

				if (numUnfinished == 0)
					break;

				yield return null;
			}
		}
	}

	/// <summary>
	/// Async request with a defined result of DataType.
	/// </summary>
	public class AsyncRequest<DataType> : AsyncRequest
	{
		protected DataType data;

		public AsyncRequest (string initialState = "")
			: base (initialState)
		{
		}

		public DataType Result {
			get {
				lock (mutex) {
					if (IsError) {
						var errorMsg = string.Format ("Request finished with error: {0}. Result is unavailable.", ErrorMessage);
						Debug.LogError (errorMsg);
						throw new Exception (errorMsg);
					}

					if (!IsDone) {
						var errorMsg = string.Format ("Attempt to access result before request is completed!");
						Debug.LogError (errorMsg);
						throw new Exception (errorMsg);
					}

					return data;
				}
			}
			set {
				lock (mutex)
					data = value;
			}
		}
	}

	/// <summary>
	/// Simple extension to the AsyncRequest, allows waiting for jobs in background threads in
	/// coroutine manner.
	/// </summary>
	public class AsyncRequestThreaded<DataType> : AsyncRequest<DataType>
	{
		#if UNITY_WEBGL
		private Func<DataType> functionToExecute;
		#else
		private Thread thread;
		#endif
		private bool started = false;
		private Action<AsyncRequestThreaded<DataType>> onCompletedAction = null;

		public AsyncRequestThreaded (Func<DataType> func, string initialState = "", bool startImmediately = true)
			: base (initialState)
		{
			Init (func, startImmediately);
		}

		public AsyncRequestThreaded (Func<AsyncRequestThreaded<DataType>, DataType> func, string initialState = "", bool startImmediately = true)
			: base (initialState)
		{
			Init (() => func (this), startImmediately);
		}

		/// <summary>
		/// Call onCompletedAction before returning false.
		/// </summary>
		public override bool keepWaiting {
			get {
				if (IsDone) {
					if (onCompletedAction != null)
						onCompletedAction (this);
					return false;
				}
				return true;
			}
		}

		private void Execute (Func<DataType> func)
		{
			try {
				Result = func ();
				IsDone = true;
			} catch (Exception ex) {
				Debug.LogWarning (ex.Message);
				SetError (ex.Message);
			}
		}

		private void Init (Func<DataType> func, bool startImmediately)
		{
			#if UNITY_WEBGL
			functionToExecute = func;
			if (startImmediately) {
				Execute (functionToExecute);
				started = true;
			}
			#else
			thread = new Thread (() => {
				Execute (func);
			});
			if (startImmediately) {
				thread.Start ();
				started = true;
			}
			#endif
		}


		/// <summary>
		/// Set callback to be executed on main thread when request finishes.
		/// </summary>
		public void SetOnCompleted (Action<AsyncRequestThreaded<DataType>> action)
		{
			onCompletedAction = action;
		}

		public void StartThread ()
		{
			if (started) {
				Debug.Log ("Already started!");
			} else {
				#if UNITY_WEBGL
				Execute(functionToExecute);  // just execute synchronously
				#else
				thread.Start ();
				#endif
			}
		}
	}
}