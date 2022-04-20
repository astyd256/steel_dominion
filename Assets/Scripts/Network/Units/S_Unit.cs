using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Mirror
{
    public class S_Unit : NetworkBehaviour
    {
        protected bool isAlive = false;

        //Components refs
        private Canvas canvasUI = null;
        private Slider healthBar = null;
        protected NavMeshAgent agent = null;
        protected Rigidbody unitRB = null;
        
        //Unit stats
        protected int Teamid = 0;
        private float maxHealth = 0f;
        private float health = 0f;
        protected int maxDamage = 2;
        protected int minDamage = 1;
        protected SO_UnitItemData.UnitType _unitSize = SO_UnitItemData.UnitType.small;

        //Target info
        [SerializeField] protected GameObject target = null;
        public Action<Transform> _targetChanged;
        public Action _behaviourStarting;

        //Unit movement stats
        protected NavMeshPath path;

        protected float forwardAmount = 0;
        protected float turnAmount = 0;

        
        protected float speed;
        [Header("Movement settings")]
        [SerializeField] protected float speedMax = 70f;
        [SerializeField] protected float speedMin = -50f;

        [SerializeField] protected float acceleration = 30f;
        [SerializeField] protected float brakeSpeed = 100f;
        [SerializeField] protected float reverseSpeed = 30f;
        [SerializeField] protected float idleSlowdown = 10f;

        protected float turnSpeed;
        [SerializeField] protected float turnSpeedMax = 300f;
        [SerializeField] protected float turnSpeedAcceleration = 300f;
        [SerializeField] protected float turnIdleSlowdown = 500f;

        //Unit state
        protected enum State
        {
            Moving,
            Idle,
            Hovering
        }
        protected State unitState = State.Idle;

        //Network object info
        private S_NetworkManagerSteel gameroom;

        protected S_NetworkManagerSteel GameRoom
        {
            get
            {
                if (gameroom != null) { return gameroom; }
                return gameroom = NetworkManager.singleton as S_NetworkManagerSteel;
            }
        }

        public S_NetworkManagerSteel GetGameRoom()
        {
            return GameRoom;
        }

        public SO_UnitItemData.UnitType GetUnitType()
        {
            return _unitSize;
        }

        [Server]
        public void SetData(int teamid, int maxhealth, int miDamage, int maDamage, SO_UnitItemData.UnitType type)
        {
            maxHealth = maxhealth;
            health = maxHealth;
            Teamid = teamid;

            minDamage = miDamage;
            maxDamage = maDamage;

            _unitSize = type;

            ClientSetData(teamid);
        }

        public int GetTeam()
        {
            return Teamid;
        }

        [ClientRpc]
        public void ClientSetData(int teamid)
        {
            Teamid = teamid;
        }

        [Server]
        public virtual void StartBehaviour()
        {
            CalcDistances();

            ShowHealth(Teamid);

            if(_behaviourStarting != null) _behaviourStarting.Invoke();

            isAlive = true;
            //this.transform.LookAt(target.transform.position);
            //this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

            //if(target != null) unitState = State.Chase;
        }

        public override void OnStartServer()
        {        
            agent = this.GetComponent<NavMeshAgent>();
            unitRB = this.GetComponent<Rigidbody>();
        }

        public override void OnStartClient()
        {
            canvasUI = this.transform.Find("Canvas").GetComponent<Canvas>();
            healthBar = canvasUI.transform.Find("HPSlider").GetComponent<Slider>();
            this.GetComponent<Rigidbody>().isKinematic = true;
        }

        [ClientRpc]
        public void ShowHealth(int teamId)
        {
            canvasUI.gameObject.SetActive(true);

            GameObject[] findedPlayers;

            findedPlayers =  GameObject.FindGameObjectsWithTag("Player");

            foreach(GameObject player in findedPlayers)
                if((teamId == player.GetComponent<S_GamePlayer>().netId - 1) && !player.GetComponent<S_GamePlayer>().hasAuthority)
                    healthBar.fillRect.GetComponent<Image>().color = Color.red;
        }

        [Server]
        public virtual void CalcDistances()
        {
            GameObject _oldTarget = (target == null) ? null : target;
            target = null;

            float minDistance = 1000000;
            List<GameObject> unitlists = new List<GameObject>();

            if (Teamid == 0) unitlists = GameRoom.GetBattlePlayerUnits(1).ToList();
            else unitlists = GameRoom.GetBattlePlayerUnits(0).ToList();

            foreach (GameObject unit in unitlists)
            {
                float dist = Vector3.Distance(this.gameObject.transform.position, unit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    target = unit;
                }
            }
            
            if (target != _oldTarget && _targetChanged != null && target != null) _targetChanged.Invoke(target.transform);
        }

        [ServerCallback]
        public void CalcDamage(float dmg)
        {
            health = health - dmg;
            if (health < 0) health = 0;

            //Dead
            if (health <= 0)
            {
               // Debug.Log("Died");
                GameRoom.RemoveBattleUnit(Teamid, this.gameObject);
                //Destroy(this.gameObject);
            }

            SetHealthBarValue(health / maxHealth, Mathf.FloorToInt(dmg));
        }

        [ClientRpc]
        public void SetHealthBarValue(float newVal, int damage)
        {
            healthBar.value = newVal;
            S_DamageText.Create(this.transform.position, damage);
        }
    }
}
