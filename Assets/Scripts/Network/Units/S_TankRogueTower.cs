using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror
{
    public class S_TankRogueTower : NetworkBehaviour
    {
        private S_TankRogueMovement tankMainScript = null;
        private Transform curTarget = null;

        protected float turnAmount = 0;
        protected float turnSpeed;

        [Header("General settings")]
        [SerializeField] private Transform towerObject;

        [Header("Movement settings")]
        [SerializeField] protected float turnSpeedMax = 50f;
        [SerializeField] protected float turnSpeedAcceleration = 50f;
        [SerializeField] protected float turnIdleSlowdown = 500f;

        [Header("Weapon settings")]
        [SerializeField] protected float shootingDistance = 40f;
        [SerializeField] private Transform barrelRotateObject = null;
        [SerializeField] private Transform projectilePrefab = null;
        [SerializeField] protected Transform AttackSpherePoint = null;
        [SerializeField] private Animation weaponAnim = null;
        [SerializeField] private float fireRate = 1.25f;
        [SerializeField] private float currentFireCooldown = 0;
        [SerializeField] private float pauseBetweenAttack = 6.5f;
        [SerializeField] private float currentPauseBetweenAttack = 0;
        [SerializeField] private int shotsAmount = 6;
        [SerializeField] private int currentShot = 0;
        [SerializeField] private float barrelRotateRate = 150f;
        

        private enum TowerState
        {
            Idle,
            Shooting,
            Reloading,
            TargetLooking
        }

        [SerializeField] private TowerState towerState = TowerState.Idle;

        public void Start()
        {
            tankMainScript = transform.GetComponent<S_TankRogueMovement>();
        }

        public void Update()
        {
            if (isServer) ServerUpdate();
            else if (isClient) ClientUpdate();
        }

        [ServerCallback]
        public void ServerUpdate()
        {
            Transform oldTarget = curTarget;
            curTarget = tankMainScript.GetTarget();

            if (curTarget == null) return;
            if (oldTarget != curTarget) ClientSetTarget(curTarget);
            else if (curTarget == null) ClientSetTarget(null);

            if (towerState != TowerState.Shooting && towerState != TowerState.Reloading) towerState = TowerState.TargetLooking;

            Vector3 dirToLookPosition = (curTarget.position - towerObject.position).normalized;

            float angleToDir = Vector3.SignedAngle(-towerObject.right, dirToLookPosition, Vector3.up);
            //Debug.Log("Angle to look = "+angleToDir);
            if (angleToDir > 4f) turnAmount = 1f;
            else if (angleToDir < -4f) turnAmount = -1f;
           // else if (angleToDir > 2f) turnAmount = 0.15f;
           // else if (angleToDir < -2f) turnAmount = -0.15f; 
            else turnAmount = 0f;

            HandleMovement();

            float distanceToTarget = Vector3.Distance(towerObject.position, curTarget.position);
            if (distanceToTarget <= shootingDistance && towerState == TowerState.TargetLooking)
            {
                Vector3 lookPos = curTarget.transform.position - transform.position;

                Quaternion rotation = Quaternion.LookRotation(lookPos);

                rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

                float angle = Quaternion.Angle(towerObject.rotation, rotation);

                if (angle <= 5f) TurnShootingSeq(true);
            }

            if (towerState == TowerState.Shooting)
            {
                currentFireCooldown -= Time.deltaTime;
                if (currentFireCooldown <= 0) MakeShot();
            }
            else if(towerState == TowerState.Reloading)
            {
                currentPauseBetweenAttack -= Time.deltaTime;
                if (currentPauseBetweenAttack <= 0)
                {
                    if(curTarget != null) towerState = TowerState.TargetLooking;
                    else towerState = TowerState.Idle;
                }
            }
        }

        [ClientCallback]
        public void ClientUpdate()
        {
            if (curTarget == null) return;

            Vector3 dirToLookPosition = (curTarget.position - towerObject.position).normalized;

            float angleToDir = Vector3.SignedAngle(-towerObject.right, dirToLookPosition, Vector3.up);
            //Debug.Log("Angle to look = "+angleToDir);
            if (angleToDir > 4f) turnAmount = 1f;
            else if (angleToDir < -4f) turnAmount = -1f;
            // else if (angleToDir > 2f) turnAmount = 0.15f;
            // else if (angleToDir < -2f) turnAmount = -0.15f; 
            else turnAmount = 0f;

            HandleMovement();

            if (towerState == TowerState.Idle) return;
            else if (towerState == TowerState.Shooting) barrelRotateObject.Rotate(barrelRotateRate * Time.deltaTime, 0, 0);
  
        }
        //TODO: add lient barrel rotation and stopping and shooting
        //#if !UNITY_SERVER //// server update
        //        void Update()
        //        {
        //            Debug.Log("TowerServer - get target");
        //            Transform oldTarget = curTarget;
        //            curTarget = tankMainScript.GetTarget();
        //            Debug.Log("TowerServer - new target - " + curTarget);
        //            Debug.Log("TowerServer - object - " + towerObject);
        //            if (curTarget == null) return;

        //            if(oldTarget != curTarget) ClientSetTarget(curTarget);
        //            else if (curTarget = null) ClientSetTarget(null);

        //            Vector3 dirToLookPosition = (curTarget.position - towerObject.position).normalized;
        //            float angleToDir = Vector3.SignedAngle(towerObject.forward + new Vector3(90, 0, -90).normalized, dirToLookPosition, Vector3.up);

        //            if (angleToDir > 10f) turnAmount = 1f;
        //            else if (angleToDir < -10f) turnAmount = -1f;
        //            else turnAmount = 0f;

        //            HandleMovement();
        //        }
        //#endif

        //#if UNITY_SERVER //// client update
        //        void Update()
        //         {
        //            Debug.Log("TowerClient - get target");
        //            if (curTarget == null) return;

        //            Vector3 dirToLookPosition = (curTarget.position - towerObject.position).normalized;
        //           // float angleToDir = Vector3.SignedAngle(towerObject.forward + new Vector3(90, 0, -90).normalized, dirToLookPosition, Vector3.up);
        //           float angleToDir = Vector3.SignedAngle(towerObject.forward, dirToLookPosition, Vector3.up);

        //            if (angleToDir > 10f) turnAmount = 1f;
        //            else if (angleToDir < -10f) turnAmount = -1f;
        //            else turnAmount = 0f;

        //            HandleMovement();
        //        }
        //#endif
        [Server]
        private void MakeShot()
        {
            currentShot++;
            currentFireCooldown = fireRate;

            if (curTarget == null)
            {
                tankMainScript.CalcDistances();
                return;
            }

            Transform bulletTransform = Instantiate(projectilePrefab, AttackSpherePoint.position, Quaternion.identity);

            System.Random rand = new System.Random();

            float distanceToTarget = Vector3.Distance(towerObject.position, curTarget.position);
            Vector3 spread = new Vector3((float)rand.Next(-5, 5) * (distanceToTarget / 100f), (float)rand.Next(-5, 5) * (distanceToTarget / 100f), (float)rand.Next(-5, 5) * (distanceToTarget / 100f));
            Vector3 shootDir = ((curTarget.transform.Find("TargetPoint").position + spread) - AttackSpherePoint.position).normalized;
            ClientMakeShot(shootDir);

            bulletTransform.GetComponent<S_TankProjectile>().SetData(rand.Next(tankMainScript.GetDamage(false), tankMainScript.GetDamage(true)), tankMainScript.GetTeam(), shootDir, 100f);

            if (currentShot == shotsAmount)
            {
                currentPauseBetweenAttack = pauseBetweenAttack;
                towerState = TowerState.Reloading;
               // Debug.Log("Reloading!");
                TurnShootingSeq(false);
                tankMainScript.CalcDistances();
            }
        }

        [ClientRpc]
        private void ClientMakeShot(Vector3 shootDir)
        {
            Transform bulletTransform = Instantiate(projectilePrefab, AttackSpherePoint.position, Quaternion.identity);

            System.Random rand = new System.Random();

            bulletTransform.GetComponent<S_TankProjectile>().SetData(0, tankMainScript.GetTeam(), shootDir, 100f);

            weaponAnim.Play("GunShot");
        }

        [Server]
        private void TurnShootingSeq(bool on)
        {
            if (on)
            {
                //currentPauseBetweenAttack = pauseBetweenAttack;
                currentFireCooldown = 0;
                currentShot = 0;
                currentPauseBetweenAttack = 0;

                towerState = TowerState.Shooting;
                ClientShooting(true);
            }
            else
            {
                // tankState = TankState.Idle;
                ClientShooting(false);
            }
        }

        [ClientRpc]
        public void ClientShooting(bool shoot)
        {
            if (shoot) towerState = TowerState.Shooting;
            else towerState = TowerState.Idle;
        }

        [ClientRpc]
        public void ClientSetTarget(Transform newTarget)
        {
            if(newTarget == null)
            {
                curTarget = null;
                towerState = TowerState.Idle;
            }
            else
            {
                curTarget = newTarget;
                towerState = TowerState.TargetLooking;
            }
        }

        public void HandleMovement()
        {
            if (turnAmount > 0 || turnAmount < 0)
            {
                if ((turnSpeed > 0 && turnAmount < 0) || (turnSpeed < 0 && turnAmount > 0))
                {
                    float minTurnAmount = 10f;
                    turnSpeed = turnAmount * minTurnAmount;
                }
                turnSpeed += turnAmount * turnSpeedAcceleration * Time.deltaTime;
            }
            else
            {
                if (turnSpeed > 2f) turnSpeed -= turnIdleSlowdown * Time.deltaTime;
                else if (turnSpeed < -2f) turnSpeed += turnIdleSlowdown * Time.deltaTime;
                else turnSpeed = 0f;
            }

            turnSpeed = Mathf.Clamp(turnSpeed, -turnSpeedMax, turnSpeedMax);

            // towerObject.rotation = Quaternion.Euler(-90,0,towerObject.eulerAngles.z + turnSpeed*Time.deltaTime);
            //Debug.Log("TurnAmount = " + turnAmount);
            //Debug.Log("TurnSpeed = " + turnSpeed);
            towerObject.Rotate(new Vector3(0f,0f, turnSpeed*Time.deltaTime));
        }
    } 
}
