using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;

namespace Com.DV.Multiplayer{

    public class Enemy : MonoBehaviourPunCallbacks, IPunObservable
    {
        public float Health = 25;
        public Transform target;
        public NavMeshAgent agent;
        private Animator animator;
        /*public float speed = 20f;
        public Rigidbody rigidbody;*/

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //Debug.LogError("OnPhotonSerializeView  ENEMY");

            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                //stream.SendNext(IsFiring);
            //Debug.LogError("HEADL SENDED"+Health);
                stream.SendNext(Health);
            }
            else
            {
                // Network player, receive data
                //this.IsFiring = (bool)stream.ReceiveNext();
                        //Debug.LogError("HEADL RECIBED"+this.Health);
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion
        // Start is called before the first frame update
        void Start()
        {
            //Debug.LogWarning("TARGET"+agent.isStopped , this);
            animator = GetComponent<Animator>();
            //enemyMesh = GetComponent<NavMeshAgent>();
              //  Debug.LogWarning(this.gameObject.name, this);

            /*PhotonNetwork.Instantiate(this.gameObject.name, new Vector3(0f, 0.504f, -0.3826895f), Quaternion.identity, 0);*/
            /*foreach (var player in PhotonNetwork.PlayerList) {if (player.isMasterClient) {

            }}*/
        }

        private void Awake()
        {
            target = GameObject.FindWithTag("PlayerArnold").transform;
            //DontDestroyOnLoad(this.gameObject);
            //Debug.LogWarning("TARGET"+agent.isStopped , this);

            agent = GetComponent<NavMeshAgent>();
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        }
        // Update is called once per frame
        void Update()
        {
           
           // Debug.LogError(agent.velocity.magnitude)

            if(agent.velocity.magnitude==0){
                animator.SetBool("isStopped", true);
            }else{
                animator.SetBool("isStopped", false);
            }
            agent.ResetPath();
            agent.SetDestination(target.position); 
            if (Health <= 0f)
            {
                Destroy(gameObject);
            }
        }

        /*public void FixedUpdate() {
            Vector3 position = Vector3.MoveTowards(transform.position, target.position, speed*Time.fixedDeltaTime);
            rigidbody.MovePosition(position);
            transform.LookAt(target);
        }*/

        void OnTriggerEnter(Collider other)
        {
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if ((other.gameObject.tag == "Bullet"))
            {
                //Debug.LogWarning("HEALT"+Health, this);

                Health -= 1f;
                return;
            }
        }
        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are touching the player
        /// </summary>
        /// <param name="other">Other.</param>
        void OnTriggerStay(Collider other)
        {
            
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if ((other.gameObject.tag == "Bullet"))
            {
                Health -= 1f * Time.deltaTime;
                return;
            }
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            
            // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
        }
    }


}
