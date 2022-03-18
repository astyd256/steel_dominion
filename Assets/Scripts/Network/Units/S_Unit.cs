using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;

namespace Mirror
{
    public class S_Unit : NetworkBehaviour
    {
        [SerializeField]
        private Canvas canvasUI = null;
        [SerializeField]
        private Slider healthBar = null;
        [SerializeField]
        private NavMeshAgent agent = null;
        [SerializeField]
        private Transform AttackSpherePoint = null;

        private float maxHealth = 0f;
        private float health = 0f;

        private int maxDamage = 2;
        private int minDamage = 1;


        private float distTotarget;

        private enum State
        {
            Chase,
            Attack,
            Idle
        }

        State unitState = State.Idle;

        private int Teamid = 0;
        private GameObject target = null;

        private bool chase = false;

        private S_NetworkManagerSteel gameroom;

        private S_NetworkManagerSteel GameRoom
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
        }

        [Server]
        public void StartBehaviour()
        {
            CalcDistances();

            ShowHealth();
            //this.transform.LookAt(target.transform.position);
            //this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

            if(target != null) unitState = State.Chase;
        }

        [ServerCallback]

        private void Update()
        {
            if (unitState == State.Idle) return;

            if (target != null)
            {
                if (unitState == State.Attack) return;

                if (distTotarget < 2f && unitState != State.Attack)
                {
                    Debug.Log("Trying to attack!");
                    this.transform.LookAt(target.transform.position);
                    this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

                    agent.isStopped = true;
                    unitState = State.Attack;

                    Collider[] colliders = Physics.OverlapSphere(AttackSpherePoint.position, 10f);

                    foreach (var hitCollider in colliders)
                    {
                        if(hitCollider.gameObject == target)
                        {
                            Debug.Log("Damage to " + target.name);

                            System.Random rand = new System.Random();

                            int dmg = rand.Next(minDamage ,maxDamage);

                            target.GetComponent<S_Unit>().CalcDamage(dmg);
                            break;
                        }
                    }
                    unitState = State.Idle;
                    this.CallWithDelay(ResetState, 2f);

                    return;
                }
                else if (unitState == State.Chase)
                {
                    
                    agent.SetDestination(target.transform.position);
                    distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
                    return;
                }
            }
        }

        [Server]
        public void ResetState()
        {
            Debug.Log("Reseting");
            CalcDistances();

            if (target != null) unitState = State.Chase;
        }

        [ClientRpc]
        public void ShowHealth()
        {
            canvasUI.gameObject.SetActive(true);
        }

        [Server]
        public void CalcDistances()
        {
            Debug.Log("Calc distance");

            target = null;
            float minDistance = 1000000;
            List<GameObject> unitlists = new List<GameObject>();

            if (Teamid == 0) unitlists = GameRoom.GetBattlePlayerUnits(1).ToList();
            else unitlists = GameRoom.GetBattlePlayerUnits(0).ToList();

            foreach (GameObject unit in unitlists)
            {
                Debug.Log("checking unit");
                float dist = Vector3.Distance(this.gameObject.transform.position, unit.transform.position);
                if (dist < minDistance)
                {
                    
                    minDistance = dist;
                    distTotarget = minDistance;
                    target = unit;
                    Debug.Log("checking unit = new " + target.name + " Dis = " + distTotarget);
                    agent.isStopped = false;
                }
            }
        }

        [Server]
        public void CalcDamage(float dmg)
        {
            health = health - dmg;
            if (health < 0) health = 0;

            //Dead
            if (health <= 0)
            {
                Debug.Log("Died");
                GameRoom.RemoveBattleUnit(Teamid, this.gameObject);
                Destroy(this.gameObject);
            }

            SetHealthBarValue(health / maxHealth);
        }

        [ClientRpc]
        public void SetHealthBarValue(float newVal)
        {
            healthBar.value = newVal;
        }
    }
}
