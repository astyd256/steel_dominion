using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Mirror
{
    public class S_Unit : NetworkBehaviour
    {
        [SerializeField]
        private Canvas canvasUI = null;
        [SerializeField]
        private Slider healthBar = null;
        [SerializeField]
        protected NavMeshAgent agent = null;
        [SerializeField]
        protected Transform AttackSpherePoint = null;

        private float maxHealth = 0f;
        private float health = 0f;

        protected int maxDamage = 2;
        protected int minDamage = 1;


        protected float distTotarget;

        protected enum State
        {
            Chase,
            Attack,
            Idle,
            AttackAfterPause,
            PreAttack,
            TargetLooking
        }
        [SerializeField]
        protected State unitState = State.Idle;

        protected int Teamid = 0;
        [SerializeField]
        protected GameObject target = null;

        private S_NetworkManagerSteel gameroom;

        protected S_NetworkManagerSteel GameRoom
        {
            get
            {
                if (gameroom != null) { return gameroom; }
                return gameroom = NetworkManager.singleton as S_NetworkManagerSteel;
            }
        }

        [Server]
        public void SetData(int teamid, int maxhealth, int miDamage, int maDamage)
        {
            maxHealth = maxhealth;
            health = maxHealth;
            Teamid = teamid;

            minDamage = miDamage;
            maxDamage = maDamage;
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
            //this.transform.LookAt(target.transform.position);
            //this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

            if(target != null) unitState = State.Chase;
        }

        [ServerCallback]

        private void Update()
        {
            //if (unitState == State.Idle) return;

            //if (target != null)
            //{
            //    if (unitState == State.Attack) return;

            //    if (distTotarget < 2f && unitState != State.Attack)
            //    {
            //        Debug.Log("Trying to attack!");
            //        this.transform.LookAt(target.transform.position);
            //        this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

            //        agent.isStopped = true;
            //        unitState = State.Attack;

            //        Collider[] colliders = Physics.OverlapSphere(AttackSpherePoint.position, 10f);

            //        foreach (var hitCollider in colliders)
            //        {
            //            if(hitCollider.gameObject == target)
            //            {
            //                Debug.Log("Damage to " + target.name);

            //                System.Random rand = new System.Random();

            //                int dmg = rand.Next(minDamage ,maxDamage);

            //                target.GetComponent<S_Unit>().CalcDamage(dmg);
            //                break;
            //            }
            //        }
            //        unitState = State.Idle;
            //        this.CallWithDelay(ResetState, 2f);

            //        return;
            //    }
            //    else if (unitState == State.Chase)
            //    {
                    
            //        agent.SetDestination(target.transform.position);
            //        distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
            //        return;
            //    }
            //}
        }

        [Server]
        public virtual void ResetState()
        {
         //   Debug.Log("Reseting");
            CalcDistances();

            if (target != null) unitState = State.Chase;
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
            //Debug.Log("Calc distance");

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
                    distTotarget = minDistance;
                    target = unit;
                 //   Debug.Log("checking unit = new " + target.name + " Dis = " + distTotarget);
                    agent.isStopped = false;
                }
            }
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
