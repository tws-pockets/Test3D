using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
namespace BlobWars {
	public class Tower : HealthObject {
		// Soldier Prefabs that are used to spawn soldiers
		public GameObject Fighter, Ranged, Artillery;
		[SyncVar]
		public string uid;
		// Maximum Number of Soldiers
		public int maxSoldiers = 2;
		// Current Number of Soldiers
		[SyncVar]
		private int numSoldiers = 0;
		// Variable for position
		[SyncVar]
		public Vector3 syncPos;
		// Variable for rotation
		[SyncVar] 
		public Quaternion syncRot;

		void Start () {
			currentHealth = maxHealth;
			// Setup SyncPosition 
			syncPos = transform.position;
			// Create 'unique' name for tower
			uid = "Player" + GetComponent<NetworkIdentity>().netId;
			gameObject.transform.name = uid;
			Camera serverCam = GameObject.Find ("Server Camera").GetComponent<Camera>();
			Camera clientCam = GameObject.Find ("Client Camera").GetComponent<Camera>();
			// Handle Cameras for host
			if (isServer && isClient) {
				if (serverCam != null) {
					serverCam.enabled = true;
				}
				if (clientCam != null) {
					clientCam.enabled = false;
				}
			} else { // for client
				if (serverCam != null) {
					serverCam.enabled = false;
				}
				if (clientCam != null) {
					clientCam.enabled = true;
				}
			}
		}
		
		
		// Update is called once per frame
		void FixedUpdate () {
			// If you click on your tower, a new unit spawns
			// No Position synchronization necessary
			if (Input.GetMouseButtonDown(0)) {
				if(hitMe()) {
					if (isLocalPlayer){
						spawnSoldier();
					}
				}
			}
		}
		// Client call to spawn a soldier on the server
		[Client]
		void spawnSoldier() {
			if (numSoldiers < maxSoldiers) {
				// Get unique name for blob
				string blobName = uid + "." + 0;
				for (int i = 0; i < maxSoldiers*5; i++) {
					blobName = uid + "." + i;
					if (GameObject.Find (blobName) == null) {
						break;
					}
				}
				// Server call with towername, blob id and spawn position
				CmdSpawnSoldier (uid, blobName, transform.position);
			}
		}
		// Spawns a soldier on the server
		[Command]
		void CmdSpawnSoldier(string towerName, string blobName, Vector3 location) {
			// Create, name and spawn the object on the server
			GameObject blob = (GameObject)Instantiate (Fighter, location, Quaternion.identity);
			blob.GetComponent<Blob> ().towerName = towerName;
			numSoldiers++;
			Debug.Log ("Spawning " + blobName + " of tower " + towerName + " at " + location);
			NetworkServer.Spawn (blob);
			
		}
		// In case a blob changes it's destination, the tower is used to 
		// inform the Server about the changes
		[Client]
		public void TransmitDestination (string blobName, Vector3 destination) {
			CmdTransmitDestination (blobName, destination);
		}
		// Server gets changed object an set different location
		[Command]
		void CmdTransmitDestination(string blobName, Vector3 destination) {
			GameObject.Find (blobName).GetComponent<Blob> ().MoveTo (destination);
		}
		// Did that raycast hit me?
		bool hitMe () {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				if (hit.transform.CompareTag("Player")) {
					if (hit.transform.GetComponent<NetworkIdentity>().isLocalPlayer) {
						return true;
					}
				}
			}
			return false;
			
		}
	}
}
