using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;

using System.Collections;
using Photon.Pun.Demo.PunBasics;


namespace Com.DV.Multiplayer
{
    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    public class Temporal : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
    {

        #region Public Fields
        [Tooltip("The current Health of our player")]
        public float Health = 100f;
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;
        
        [SerializeField]
        public float velocidadMovimiento = 5.0f;
        [SerializeField]
        public float velocidadRotacion = 200.0f;
        [SerializeField]
        public Animator animator;
        [SerializeField]
        public float x,y;

        /*[SerializeField]
        public GameObject bullet;*/
        /*[SerializeField]
        public Transform spawnPoint;

        [SerializeField]
        public float shotForce = 15000;
        [SerializeField]
        public float shotRate = 0.05f;

        [SerializeField]
        public float shotRateTime = 0;*/
        #endregion

        #region Private Fields 
        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        private GameObject beams;
        //True, when the user is firing
        bool IsFiring;
        bool levelCompleted = false;
        #endregion


        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //Debug.LogError("OnPhotonSerializeView  PLAYER");

            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
                stream.SendNext(levelCompleted);
            }
            else
            {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
                this.levelCompleted = (bool)stream.ReceiveNext();
            }
        }

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            if (beams == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }

            #if UNITY54ORNEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            #endif

            if (PlayerUiPrefab != null)
            {
                Debug.LogError("Start", this);

                GameObject _uiGo =  Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {

            /*if (photonView.IsMine)
            {
                ProcessInputs ();
            }*/

            // trigger Beams active state
            if (beams != null && IsFiring != beams.activeInHierarchy)
            {
                beams.SetActive(IsFiring);
            }

            if (photonView.IsMine)
            {
                ProcessInputs();
                
                if (Health <= 0f)
                {
                    GameManager.Instance.LeaveRoom();//error 254
                }
            }

            /*if(levelCompleted){
                LoadLevel();
            }*/
        }

        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player.
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if ((other.gameObject.tag == "HandAtack"))
            {
                Health -= 0.01f;
                return;
            }
            /*if (!other.name.Contains("Beam"))
            {
                return;
            }*/
            if ((other.gameObject.tag == "Bullet"))
            {
                return;
            }
            //Health -= 0.1f;
        }
        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are touching the player
        /// </summary>
        /// <param name="other">Other.</param>
        void OnTriggerStay(Collider other)
        {
            // we dont' do anything if we are not the local player.
            if (!photonView.IsMine)
            {
                return;
            }
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            /*if ((other.gameObject.tag == "EnemyAtack"))
            {
                return;
            }*/
            /*if (!other.name.Contains("Beam"))
            {
                return;
            }*/
           // Debug.LogError("ATACKING"+(other.gameObject.tag == "HandAtack"));
            if ((other.gameObject.tag == "HandAtack"))
            {
                Health -= 0.01f;
                return;
            }
            if ((other.gameObject.tag == "Bullet"))
            {
                return;
            }
            // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
            //Health -= 0.1f * Time.deltaTime;
        }

        #if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
        #endif

        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
                Debug.LogError("CalledOnLevelWasLoaded", this);

            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }

            GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        #if UNITY54ORNEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable ();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endif
        
        #endregion

        #region Custom
        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        void ProcessInputs()
        {
            x = Input.GetAxis("Horizontal");
            y = Input.GetAxis("Vertical");

            transform.Rotate(0, x*Time.deltaTime*velocidadRotacion,0);
            transform.Translate(0, 0, y*Time.deltaTime*velocidadMovimiento);
            animator.SetFloat("VelX", x);
            animator.SetFloat("VelY", y);
            if (Input.GetButtonDown("Fire1"))
            {
           // Debug.Log("Firing");

                if (!IsFiring)
                {
                    IsFiring = true;
                }
            }
            if (Input.GetButtonUp("Fire1"))
            {
            //Debug.Log("NOT Firing");

                if (IsFiring)
                {
                    IsFiring = false;
                }
            }
            /*if(Input.GetKey(KeyCode.F))
            {
                if(Time.time > shotRateTime)
                {
                    ShootBullet();
                }
            }*/
            
        }

        public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
        {
            //Debug.Log("Is this mine?... "+info.ToString());
            object[] instantiationData = info.photonView.InstantiationData;
        }

        void LoadLevel()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
                return;
            }
            string currentLevelName = PhotonNetwork.CurrentRoom.Name;
            int level = int.Parse(currentLevelName.Split(' ')[1])+1;
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", level);
            PhotonNetwork.LoadLevel("Room for " + level);
        }

        #endregion

        #region Private Methods
        #if UNITY54ORNEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
        #endif
        #endregion
    }
}
