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
        private Transform barrelRotateObject = null;

        [SerializeField]
        private Transform projectilePrefab = null;

        [SerializeField]
        private float fireRate = 2f;
        private float currentFireCooldown = 0;

        [SerializeField]
        private float pauseBetweenAttack = 5f;
        private float currentPauseBetweenAttack = 0;

        [SerializeField]
        private int shotsAmount = 5;
        private int currentShot = 0;

        [SerializeField]
        private float barrelRotateRate = 180f;

        [SerializeField]
        private float turningRate = 30f;

        [SerializeField]
        private Animation anim;

        private enum TankState
        {
            Idle,
            Shooting,
            Reloading
        }

        private TankState tankState;

        void Start()
        {
            //towerRotateObject.rotation = Quaternion.Euler(-90, 0, 90);
            unitState = State.Idle;
            tankState = TankState.Idle;
        }

        [ServerCallback]
        public void ServerUpdate()
        {
            if (unitState == State.Idle) return;


            if (tankState == TankState.Shooting)
            {
                currentFireCooldown -= Time.deltaTime;

                if (currentFireCooldown <= 0)
                {
                    MakeShot();
                }
            }
            else if (tankState == TankState.Reloading)
            {
                currentPauseBetweenAttack -= Time.deltaTime;

                if (currentPauseBetweenAttack <= 0)
                {
                    Debug.Log("Reload complete!");
                    tankState = TankState.Idle;
                    CalcDistances();
                }
            }

            if (target != null)
            {            
                if (unitState == State.Chase && distTotarget > 35f)
                {
                  //  Debug.Log("Target far = " + distTotarget);
                    agent.isStopped = false;
                    agent.SetDestination(target.transform.position);
                    distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
                }
                else if (distTotarget < 35f)
                {
                   // Debug.Log("Target close = " + distTotarget);
                    agent.isStopped = true;
                }

                if (unitState == State.Chase)
                {
                    var lookPos = target.transform.position - transform.position;

                    Quaternion rotation = Quaternion.LookRotation(lookPos);

                    rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

                    float angle = Quaternion.Angle(towerRotateObject.rotation, rotation);

                    if (angle > 0.5f) towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);
                    
                    if(angle < 0.5f && distTotarget < 35f && (tankState != TankState.Shooting && tankState != TankState.Reloading))
                    {
                        Debug.Log("Start shooting seq!");
                        TurnShootingSeq(true); 
                    }
                    


                }
            }

            //if (unitState == State.Idle) return;

            //if (curFireTimer > 0f) curFireTimer -= Time.deltaTime;

            //if (target != null)
            //{
            //    if (unitState == State.AttackAfterPause) return;

            //    if (distTotarget < 40f && unitState != State.Attack)
            //    {
            //        var lookPos = target.transform.position - transform.position;
            //        Quaternion rotation = Quaternion.LookRotation(lookPos);

            //        rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

            //        towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);



            //        //float angle = Quaternion.Angle(towerRotateObject.rotation, target.rotation);
            //        // if ()
            //        //  Debug.Log("Trying to attack! Rot = " + towerRotateObject.rotation.eulerAngles);
            //        //  this.transform.LookAt(target.transform.position);
            //        //  this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

            //        agent.isStopped = true;
            //        unitState = State.Attack;



            //        //Collider[] colliders = Physics.OverlapSphere(AttackSpherePoint.position, 10f);

            //        //foreach (var hitCollider in colliders)
            //        //{
            //        //    if (hitCollider.gameObject == target)
            //        //    {
            //        //        Debug.Log("Damage to " + target.name);

            //        //        System.Random rand = new System.Random();

            //        //        int dmg = rand.Next(minDamage, maxDamage);

            //        //        target.GetComponent<S_Unit>().CalcDamage(dmg);
            //        //        break;
            //        //    }
            //        //}
            //        // unitState = State.Idle;
            //        // this.CallWithDelay(ResetState, 2f);

            //        return;
            //    }
            //    else if (unitState == State.Attack)
            //    {
            //        var lookPos = target.transform.position - transform.position;
            //        Quaternion rotation = Quaternion.LookRotation(lookPos);

            //        rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

            //        towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);

            //        //float angle = Quaternion.Angle(towerRotateObject.rotation, target.rotation);
            //        // if ()
            //        //   Debug.Log("Trying to attack state! Rot = " + towerRotateObject.rotation.eulerAngles);
            //        //  this.transform.LookAt(target.transform.position);
            //        //  this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

            //        agent.isStopped = true;
            //        //unitState = State.Attack;
            //    }
            //    else if (unitState == State.Chase)
            //    {

            //        agent.SetDestination(target.transform.position);
            //        distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
            //        return;
            //    }
            //}
        }

        [Client]
        public void ClientUpdate()
        {
            if (unitState == State.Idle) return;

            //currentPauseBetweenAttack -= Time.deltaTime;

            //if(currentPauseBetweenAttack <= 0)
            //{
            //    currentPauseBetweenAttack = 0;
            //    TurnShootingSeq(false);
            //}

            if (target != null)
            {
                if (unitState == State.TargetLooking)
                {
                    var lookPos = target.transform.position - transform.position;

                    Quaternion rotation = Quaternion.LookRotation(lookPos);

                    rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

                    float angle = Quaternion.Angle(towerRotateObject.rotation, rotation);

                    if(angle > 0.5f) towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);
                    
                    if (angle < 0.5f && distTotarget < 35f)
                    {
                       // Debug.Log("Look straight!");
                    }


                }

                if (tankState == TankState.Shooting)
                {
                    // GameObject.Find("CameraRotator").transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));
                    barrelRotateObject.Rotate(barrelRotateRate * Time.deltaTime, 0, 0);
                }


            }

        }

        private void Update()
        {
            if (isServer) ServerUpdate();
            else if (isClient) ClientUpdate();
        }

        [Server]
        private void TurnShootingSeq(bool on)
        {
            if(on)
            {
                //currentPauseBetweenAttack = pauseBetweenAttack;
                currentFireCooldown = 0;
                currentShot = 0;
                currentPauseBetweenAttack = 0;

                tankState = TankState.Shooting;
                ClientShooting(true);
            }
            else
            {
               // tankState = TankState.Idle;
                ClientShooting(false);
            }
        }

        [Server]
        private void MakeShot()
        {
            currentShot++;
            currentFireCooldown = fireRate;

            if(target == null)
            {
                CalcDistances();
                return;
            }

            ClientMakeShot();
            Transform bulletTransform = Instantiate(projectilePrefab, AttackSpherePoint.position, Quaternion.identity);
            Vector3 shootDir = (target.transform.position - AttackSpherePoint.position).normalized;
            
            System.Random rand = new System.Random();

            bulletTransform.GetComponent<S_TankProjectile>().SetData(rand.Next(minDamage, maxDamage), Teamid, shootDir, 100f);

            if(currentShot == shotsAmount)
            {
                currentPauseBetweenAttack = pauseBetweenAttack;
                tankState = TankState.Reloading;
                Debug.Log("Reloading!");
                TurnShootingSeq(false);
                CalcDistances();
            }
        }

        [ClientRpc]
        private void ClientMakeShot()
        {
            Transform bulletTransform = Instantiate(projectilePrefab, AttackSpherePoint.position, Quaternion.identity);
            Vector3 shootDir = (target.transform.position - AttackSpherePoint.position).normalized;

            System.Random rand = new System.Random();

            bulletTransform.GetComponent<S_TankProjectile>().SetData(0, Teamid, shootDir, 100f);

            anim.Play("GunShot");
        }

        [Server]
        public override void StartBehaviour()
        {
            CalcDistances();

            ShowHealth(Teamid);

            if (target != null) unitState = State.Chase;
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
                Debug.Log("checking unit for tank");
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

            if(target == null) ClientGetTarget(null);
            else ClientGetTarget(target);

            if (target != null) unitState = State.Chase;
        }

        [ClientRpc]
        public void ClientGetTarget(GameObject newTarget)
        {
            if (newTarget == null)
            {
                target = null;
                unitState = State.Idle;
                tankState = TankState.Idle;
                return;
            }
            target = newTarget;
            unitState = State.TargetLooking;
            //Change state for client
        }

        [ClientRpc]
        public void ClientShooting(bool shoot)
        {
            if (shoot)
                tankState = TankState.Shooting;
            else
                tankState = TankState.Idle;
        }

    }
}
