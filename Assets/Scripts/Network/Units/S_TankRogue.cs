using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Mirror
{
    public class S_TankRogue : S_Unit
    {

        [SerializeField]
        private Transform towerRotateObject = null;

        [SerializeField]
        private float fireRate = 1f;

        private float curFireTimer = 0f;

        [SerializeField]
        private float turningRate = 30;



        void Start()
        {
            towerRotateObject.rotation = Quaternion.Euler(-90, 0, 90);
            unitState = State.Idle;
        }

        [ClientRpc]
        public void RotateTower(Quaternion to)
        {
            towerRotateObject.rotation = Quaternion.RotateTowards(towerRotateObject.rotation, to, turningRate * Time.deltaTime);
        }

        [Server]
        public void ServerUpdate()
        {
            if (unitState == State.Idle) return;

            if (curFireTimer > 0f) curFireTimer -= Time.deltaTime;

            if (target != null)
            {
                if (unitState == State.AttackAfterPause) return;

                if (distTotarget < 40f && unitState != State.Attack)
                {
                    var lookPos = target.transform.position - transform.position;
                    Quaternion rotation = Quaternion.LookRotation(lookPos);

                    rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

                    towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);



                    //float angle = Quaternion.Angle(towerRotateObject.rotation, target.rotation);
                    // if ()
                    //  Debug.Log("Trying to attack! Rot = " + towerRotateObject.rotation.eulerAngles);
                    //  this.transform.LookAt(target.transform.position);
                    //  this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

                    agent.isStopped = true;
                    unitState = State.Attack;



                    //Collider[] colliders = Physics.OverlapSphere(AttackSpherePoint.position, 10f);

                    //foreach (var hitCollider in colliders)
                    //{
                    //    if (hitCollider.gameObject == target)
                    //    {
                    //        Debug.Log("Damage to " + target.name);

                    //        System.Random rand = new System.Random();

                    //        int dmg = rand.Next(minDamage, maxDamage);

                    //        target.GetComponent<S_Unit>().CalcDamage(dmg);
                    //        break;
                    //    }
                    //}
                    // unitState = State.Idle;
                    // this.CallWithDelay(ResetState, 2f);

                    return;
                }
                else if (unitState == State.Attack)
                {
                    var lookPos = target.transform.position - transform.position;
                    Quaternion rotation = Quaternion.LookRotation(lookPos);

                    rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

                    towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);

                    //float angle = Quaternion.Angle(towerRotateObject.rotation, target.rotation);
                    // if ()
                    //   Debug.Log("Trying to attack state! Rot = " + towerRotateObject.rotation.eulerAngles);
                    //  this.transform.LookAt(target.transform.position);
                    //  this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

                    agent.isStopped = true;
                    //unitState = State.Attack;
                }
                else if (unitState == State.Chase)
                {

                    agent.SetDestination(target.transform.position);
                    distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
                    return;
                }
            }
        }

        [Client]
        public void ClientUpdate()
        {
            if (unitState == State.Idle) return;

            if (target != null)
            {
                if (unitState == State.TargetLooking)
                {
                    var lookPos = target.transform.position - transform.position;
                    Quaternion rotation = Quaternion.LookRotation(lookPos);

                    rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

                    towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);

                    // dirToFace.LookAt(target.transform.position);

                    // towerRotateObject.eulerAngles = new Vector3(-90, 0, dirToFace.rotation.z+90f);

                    // towerRotateObject.eulerAngles = new Vector3(0, dirToFace.eulerAngles.y,0);



                    // this.transform.LookAt(target.transform.position);
                    //        this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

                    //towerRotateObject.transform.eulerAngles = dirToFace;// new Vector3(-90, 0, dirToFace.z);
                }
            }
        }

        private void Update()
        {
            if (isServer) ServerUpdate();
            else if (isClient) ClientUpdate();
        }

        [Server]
        public override void CalcDistances()
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
            Debug.Log("Send target to client");
            ClientGetTarget(target);
        }

        [ClientRpc]
        public void ClientGetTarget(GameObject newTarget)
        {
            Debug.Log("New target = " + newTarget.name);
            target = newTarget;
            unitState = State.TargetLooking;
            //Change state for client
        }

    }
}
