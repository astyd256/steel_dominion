using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Mirror
{
    public class S_TankRogueMovement : S_Unit
    {
        private bool targetNear = false;
        private bool pathBlocked = false;

        private bool retreating = false;
        private float retreatCooldown = 3f;
        private float curRetreatCooldown = 0f;
        private Vector3 targetposition = new Vector3(0, 0, 0);

        private S_TankRogueTower tankTower = null;

        public override void OnStartServer()
        {
            base.OnStartServer();
            path = new NavMeshPath();
            tankTower = transform.Find("Tower").GetComponent<S_TankRogueTower>();
        }

        [Server]
        public Transform GetTarget()
        {
            if (target != null) return target.transform;
            else return null;
        }

        public int GetDamage(bool max)
        {
            if (max) return maxDamage;
            else return minDamage;
        }

        //[ServerCallback]
        //public void ServerUpdate()
        //{

        //    //if (curFireTimer > 0f) curFireTimer -= Time.deltaTime;

        //    //if (target != null)
        //    //{
        //    //    if (unitState == State.AttackAfterPause) return;

        //    //    if (distTotarget < 40f && unitState != State.Attack)
        //    //    {
        //    //        var lookPos = target.transform.position - transform.position;
        //    //        Quaternion rotation = Quaternion.LookRotation(lookPos);

        //    //        rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

        //    //        towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);



        //    //        //float angle = Quaternion.Angle(towerRotateObject.rotation, target.rotation);
        //    //        // if ()
        //    //        //  Debug.Log("Trying to attack! Rot = " + towerRotateObject.rotation.eulerAngles);
        //    //        //  this.transform.LookAt(target.transform.position);
        //    //        //  this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

        //    //        agent.isStopped = true;
        //    //        unitState = State.Attack;



        //    //        //Collider[] colliders = Physics.OverlapSphere(AttackSpherePoint.position, 10f);

        //    //        //foreach (var hitCollider in colliders)
        //    //        //{
        //    //        //    if (hitCollider.gameObject == target)
        //    //        //    {
        //    //        //        Debug.Log("Damage to " + target.name);

        //    //        //        System.Random rand = new System.Random();

        //    //        //        int dmg = rand.Next(minDamage, maxDamage);

        //    //        //        target.GetComponent<S_Unit>().CalcDamage(dmg);
        //    //        //        break;
        //    //        //    }
        //    //        //}
        //    //        // unitState = State.Idle;
        //    //        // this.CallWithDelay(ResetState, 2f);

        //    //        return;
        //    //    }
        //    //    else if (unitState == State.Attack)
        //    //    {
        //    //        var lookPos = target.transform.position - transform.position;
        //    //        Quaternion rotation = Quaternion.LookRotation(lookPos);

        //    //        rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

        //    //        towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);

        //    //        //float angle = Quaternion.Angle(towerRotateObject.rotation, target.rotation);
        //    //        // if ()
        //    //        //   Debug.Log("Trying to attack state! Rot = " + towerRotateObject.rotation.eulerAngles);
        //    //        //  this.transform.LookAt(target.transform.position);
        //    //        //  this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

        //    //        agent.isStopped = true;
        //    //        //unitState = State.Attack;
        //    //    }
        //    //    else if (unitState == State.Chase)
        //    //    {

        //    //        agent.SetDestination(target.transform.position);
        //    //        distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
        //    //        return;
        //    //    }
        //    //}
        //}

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

            //if (target != null)
            //{
            //    if (unitState == State.TargetLooking)
            //    {
            //        var lookPos = target.transform.position - transform.position;

            //        Quaternion rotation = Quaternion.LookRotation(lookPos);

            //        rotation.eulerAngles = new Vector3(-90, 0, rotation.eulerAngles.y + 90f);

            //        float angle = Quaternion.Angle(towerRotateObject.rotation, rotation);

            //        if(angle > 0.5f) towerRotateObject.rotation = Quaternion.Slerp(towerRotateObject.rotation, rotation, Time.deltaTime * turningRate);
                    
            //        if (angle < 0.5f && distTotarget < 35f)
            //        {
            //           // Debug.Log("Look straight!");
            //        }


            //    }

            //    if (tankState == TankState.Shooting)
            //    {
            //        // GameObject.Find("CameraRotator").transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));
            //        barrelRotateObject.Rotate(barrelRotateRate * Time.deltaTime, 0, 0);
            //    }


            //}

        }

        [ServerCallback]
        private void Update()
        {
            if (!isAlive) return;

            CalcDistances();
            if (target != null && !retreating) targetposition = target.transform.position;
            else if (!retreating) targetposition = new Vector3(999f, 999f, 999f);
            
            if(curRetreatCooldown > 0f && unitState == State.Idle) curRetreatCooldown -= Time.deltaTime;

            if(unitState == State.Idle && curRetreatCooldown <= 0f)
            {
                //Check for retreat
                Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, 1 << 7);
                foreach (var col in colliders)
                {
                    if (col.transform.GetComponent<S_Unit>() == null) continue;

                    if (col.transform.GetComponent<S_Unit>().GetTeam() != Teamid)
                    {
                        bool suitablePosition = false;
                        System.Random rand = new System.Random();

                        while (!suitablePosition)
                        {
                            float walkRadius = rand.Next(20, 35);
                            Vector3 randomDirection = Random.insideUnitSphere * walkRadius + transform.position;
                            NavMeshHit hit;
                            NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);

                            float distance = Vector3.Distance(transform.position, hit.position);
                            if (distance > 10f)
                            {
                                suitablePosition = true;
                                int enemyTeamid = (Teamid == 0) ? 1 : 0;


                                foreach(GameObject enemyUnit in GameRoom.GetBattlePlayerUnitsByTeam(enemyTeamid))
                                {
                                    if (Vector3.Distance(hit.position, enemyUnit.transform.position) <= 10f)
                                        suitablePosition = false;
                                }

                                if (!suitablePosition) continue;

                                retreating = true;
                                targetposition = hit.position;
                            }
                        }
                    }
                }
            }

            if(targetposition != new Vector3(999f,999f,999f))
            {
                agent.CalculatePath(targetposition, path);

                if(path.status != NavMeshPathStatus.PathInvalid)
                {
                    float reachedTargetDistance = (retreating) ? 4f : 40f;//4f;
                    float distanceToTarget = Vector3.Distance(transform.position, targetposition);

                    if (distanceToTarget > reachedTargetDistance)
                    {
                        //Target is far
                        targetNear = false;
                        unitState = State.Moving;

                        Vector3 dirToMovePosition = (path.corners[1] - transform.position).normalized;
                        float dot = Vector3.Dot(transform.forward, dirToMovePosition);

                        if (dot > 0)
                        {
                            float maxDistance = 7.5f;
                            RaycastHit hit;

                            if (!Physics.BoxCast(transform.position, new Vector3(1.5f, 1.5f, 1.5f), transform.forward, out hit, transform.rotation, maxDistance, 1))
                            {
                                pathBlocked = false;
                                forwardAmount = 1f;
                            }
                            else
                            {
                                pathBlocked = true;
                                forwardAmount = 0f;
                            }
                        }
                        else
                        {
                            float reverseDistance = 45f;
                            if (distanceToTarget > reverseDistance) forwardAmount = 1f;
                            else forwardAmount = -1f;
                        }

                        float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);

                        if (angleToDir > 0) turnAmount = 1f;
                        else turnAmount = -1f;
                    }
                    else
                    {
                        //Target is close
                        if (retreating)
                        {
                            retreating = false;
                            curRetreatCooldown = retreatCooldown;
                        }

                        targetNear = true;
                        unitState = State.Idle;

                        if (speed > 3f) forwardAmount = -1f;
                        else forwardAmount = 0f;

                        turnAmount = 0f;
                    }

                }
            }

            HandleMovement();
        }

        [ServerCallback]
        public void HandleMovement()
        {
            if (forwardAmount > 0)
            {
                if (speed < 0) speed += forwardAmount * brakeSpeed * Time.deltaTime;
                else speed += forwardAmount * acceleration * Time.deltaTime;

            }
            else if (forwardAmount < 0)
            {
                if (speed > 0) speed += forwardAmount * brakeSpeed * Time.deltaTime;
                else speed += forwardAmount * reverseSpeed * Time.deltaTime;
            }
            else if (forwardAmount == 0)
            {
                if (speed > 0) speed -= idleSlowdown * Time.deltaTime;
                else if (speed < 0) speed += idleSlowdown * Time.deltaTime;
            }

            speed = Mathf.Clamp(speed, speedMin, speedMax);

            agent.Move(transform.forward * speed * Time.deltaTime);

            if (speed < 0 && !targetNear && !pathBlocked)
            {
                turnAmount = turnAmount * -1f;
            }

            if (turnAmount > 0 || turnAmount < 0)
            {
                if ((turnSpeed > 0 && turnAmount < 0) || (turnSpeed < 0 && turnAmount > 0))
                {
                    float minTurnAmount = 20f;
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

            float speedNormalized = speed / speedMax;
            float invertSpeedNormalized = Mathf.Clamp(1 - speedNormalized, .75f, 1f);

            turnSpeed = Mathf.Clamp(turnSpeed, -turnSpeedMax, turnSpeedMax);

            unitRB.angularVelocity = new Vector3(0, turnSpeed * (invertSpeedNormalized * 1f) * Mathf.Deg2Rad, 0);
        }

        [ServerCallback]
        private void OnDrawGizmos()
        {
            if(retreating)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(targetposition, new Vector3(5f,5f,5f));
            }
        }
    }
}
