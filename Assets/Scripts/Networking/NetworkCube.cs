using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NetworkCube : MonoBehaviour {

    //The ID of the client that owns this object (so we can check if it's us updating)
    public ushort objectID;
    public bool DEBUG = true;

    Vector3 lastPosition = Vector3.zero;
    Vector3 nextPosition = Vector3.zero;
    Quaternion lastRotation = Quaternion.identity;
    Quaternion nextRotation = Quaternion.identity;
    Vector3 lastScale;

    // Use this for initialization
    void Start () {
        NetworkManager.OnReceiveMessageFromGameObjectUpdate += NetworkManager_OnReceiveMessageFromGameObjectUpdate;
        //Initialize
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

	/// <summary>
	/// Thises the will be executed on the main thread.
	/// </summary>
	/// <returns>The will be executed on the main thread.</returns>
    public IEnumerator ThisWillBeExecutedOnTheMainThread()
    {
        Debug.Log("This is executed from the main thread");
        //transform.position = new Vector3(newMessage.GameObjectPos.x, newMessage.GameObjectPos.y, newMessage.GameObjectPos.z);
        lastPosition = nextPosition;
        //https://twitter.com/arturonereu/status/1042083997399101441
        transform.position = nextPosition;
		//Added rotation
        lastRotation = nextRotation;
        transform.rotation = nextRotation;
        yield return null;
    }

	/// <summary>
	/// Networks the manager on receive message from game object update.
	/// </summary>
	/// <returns>The manager on receive message from game object update.</returns>
	/// <param name="newMessage">New message.</param>
    void NetworkManager_OnReceiveMessageFromGameObjectUpdate (NetworkManager.ReceiveMessageFromGameObject newMessage)
    {
        Debug.Log ("Raise event in GameObject");
        Debug.Log (newMessage.MessageType);
        Debug.Log (newMessage.GameObjectID);
        Debug.Log (newMessage.GameObjectPos);
        Debug.Log (newMessage.GameObjectRot);

        //Update pos and rot
        if (newMessage.GameObjectID == objectID)
        {
            nextPosition = new Vector3(newMessage.GameObjectPos.x, newMessage.GameObjectPos.y, newMessage.GameObjectPos.z);
            nextRotation = new Quaternion(newMessage.GameObjectRot.x, newMessage.GameObjectRot.y, newMessage.GameObjectRot.z, newMessage.GameObjectRot.w);
            UnityMainThreadDispatcher.Instance().Enqueue(ThisWillBeExecutedOnTheMainThread());
        }
    }
    
    // FixedUpdate is NOT called once per frame
    void FixedUpdate () {
        if ((Vector3.Distance(transform.position, lastPosition) > 0.05) || (Quaternion.Angle(transform.rotation, lastRotation) > 0.3))
        {
			NetworkManager.instance.SendMessage(NetworkManager.SendType.SENDTOOTHER, NetworkManager.PacketId.OBJECT_MOVE, this.objectID, String.Empty, true ,transform.position, transform.rotation);
            //Update stuff
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
    }
}
