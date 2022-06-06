using UnityEngine;
using UnityEngine.AI;

namespace Mirror
{
    public class S_MeleeUnits : S_Unit
    {
        private bool ShouldAttack = false;

        [Header("Attack settings")]
        [SerializeField] private float attackCooldown = 2f;
        private float curAttackCooldown = 0f;
        [SerializeField] private float attackPause = 0.37f;
        private float curAttackPause = 0;

#if !UNITY_SERVER
        [SerializeField] private Animation weaponAnim = null;
#endif

        [SerializeField]
        private float attackDistance = 5f;

        public override void OnStartServer()
        {
            base.OnStartServer();
            path = new NavMeshPath();
        }

        [ServerCallback]
        private void Update()
        {
            if (!isAlive) return;

            if (curAttackCooldown > 0) curAttackCooldown -= Time.deltaTime;

            if (curAttackPause > 0)
            {
                curAttackPause -= Time.deltaTime;
                if(curAttackPause <= 0)
                {
                    Debug.Log("Make hit!");
                    MakeHit();
                }
            }

            CalcDistances();
            if(target != null)
            {
                agent.CalculatePath(target.transform.position, path);

                if(path.status != NavMeshPathStatus.PathInvalid)
                {
                    float TargetCheckDistance = 10f;
                    float distanceCheckToTarget = Vector3.Distance(transform.position, target.transform.position);

                    if(distanceCheckToTarget < TargetCheckDistance)
                    {
                        Collider[] colliders = Physics.OverlapSphere(transform.position, attackDistance, 1 << 7);
                   
                        foreach(Collider collider in colliders) if(collider.gameObject == target) ShouldAttack = true;                          
                    }

                    if(!ShouldAttack)
                    {
                        //Enemy is far
                        Vector3 dirToMovePosition = (path.corners[1] - transform.position).normalized;

                        float obstacleDistanceCheck = 7.5f;
                        RaycastHit hit;

                        if(!Physics.BoxCast(transform.position, new Vector3(1.5f,1.5f,1.5f), transform.forward,out hit, transform.rotation, obstacleDistanceCheck, 1))
                        {
                            forwardAmount = 1f;
                        }
                        else forwardAmount = 0f;

                        float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);
                        if (angleToDir > 0) turnAmount = 1f;
                        else turnAmount = -1f;

                        unitState = State.Moving;
                    }
                    else
                    {
                        //Enemy in attack range
                        Vector3 dirToLookPosition = (target.transform.position - transform.position).normalized;
                        float angleToDir = Vector3.SignedAngle(transform.forward, dirToLookPosition, Vector3.up);

                        if (angleToDir > 15f) turnAmount = 1f;
                        else if (angleToDir < -15f) turnAmount = -1f;
                        else turnAmount = 0f;

                        forwardAmount = 0f;

                        if (curAttackCooldown <= 0) HandleAttack();

                        unitState= State.Idle;
                    }
                }
            }
            else
            {
                //No target
                forwardAmount = 0f;
                turnAmount = 0f;

                unitState = State.Idle;
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
                if (turnSpeed > 0) turnSpeed -= turnIdleSlowdown * Time.deltaTime;
                else if (turnSpeed < 0) turnSpeed += turnIdleSlowdown * Time.deltaTime;
                else if (turnSpeed > -1f && turnSpeed < +1f) turnSpeed = 0f;
            }

            float speedNormalized = speed / speedMax;
            float invertSpeedNormalized = Mathf.Clamp(1 - speedNormalized, .75f, 1f);

            turnSpeed = Mathf.Clamp(turnSpeed, -turnSpeedMax, turnSpeedMax);

            unitRB.angularVelocity = new Vector3(0, turnSpeed * (invertSpeedNormalized * 1f) * Mathf.Deg2Rad, 0);
        }

        [ServerCallback]
        public void HandleAttack()
        {
            curAttackPause = attackPause;
            curAttackCooldown = attackCooldown;
            ClientMakeAttack();
        }

        [ServerCallback]
        public void MakeHit()
        {
            if (target == null) return;

            Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, 1 << 7);
            foreach (var col in colliders)
            {
                if (col.gameObject == target)
                {
                    System.Random rand = new System.Random();

                    int dmg = rand.Next(minDamage, maxDamage);

                    if (target != null) target.GetComponent<S_Unit>().CalcDamage(dmg,0);

                    break;
                }
            }

            ShouldAttack = false;
        }

        [ClientRpc]
        private void ClientMakeAttack()
        {
#if !UNITY_SERVER
            if (weaponAnim != null) weaponAnim.Play("ArmsAttack");
#endif
        }

        [ClientRpc]
        public override void SetHealthBarValue(float newVal, int damage, int particlesId)
        {
            healthBar.value = newVal;
            S_DamageText.Create(this.transform.position, damage);

            if(newVal == 0f) Instantiate(S_GameAssets.i.pfGreasleyDestroyPS, this.transform.position, Quaternion.identity);
            else if (particlesId == 0) Instantiate(S_GameAssets.i.pfGreasleyHitPS, this.transform.position, Quaternion.identity);

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject p in players)
            {
                uint _playerNetID = p.GetComponent<S_GamePlayer>().netId;
                if (_playerNetID == 1 && p.GetComponent<S_GamePlayer>().hasAuthority)
                {
                    p.GetComponent<S_GamePlayer>().UpdateHealthText(damage, (Teamid == 0) ? false : true);
                    break;
                }
                else if (_playerNetID == 2 && p.GetComponent<S_GamePlayer>().hasAuthority)
                {
                    p.GetComponent<S_GamePlayer>().UpdateHealthText(damage, (Teamid == 1) ? false : true);
                    break;
                }
            }
        }
    }
}
