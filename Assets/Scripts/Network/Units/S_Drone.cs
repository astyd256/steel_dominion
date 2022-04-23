using UnityEngine;
using UnityEngine.AI;
//TODO: add next hover point check for enemy vicinity add forward movement for far hover points
namespace Mirror
{
    public class S_Drone : S_Unit
    {
        private Vector3 _pointToHover;
        private bool _isHovering = false;
        [Header("Drone settings")]
        [SerializeField] private float _HoverCooldown = 5f;
        private float _curHoverCooldown = 0f;
        [SerializeField] private float _RangeOfHoverStartingForSmall = 5f;
        [SerializeField] private float _RangeOfHoverStartingForBig = 10f;
        private float _curRangeOfHoverStarting = 5f;

        [Header("Attack settings")]
        [SerializeField] protected Transform _attackSpherePoint = null;
        [SerializeField] private float attackCooldown = 2f;
        private float curAttackCooldown = 0f;

        private LineRenderer _lineRenderer = null;

        [SerializeField]
        private Animation weaponAnim = null;

        public override void OnStartServer()
        {
            base.OnStartServer();
            path = new NavMeshPath();
        }

        public override void OnStartClient()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            base.OnStartClient();
        }

        [ServerCallback]
        private void Update()
        {
            if (!isAlive) return;

            //if (curAttackCooldown > 0) curAttackCooldown -= Time.deltaTime;
            //else
            //{
            //    //Debug.Log("Make hit!");
            //    //MakeHit();
            //}

            CalcDistances();
            if (target != null)
            {
                agent.CalculatePath(target.transform.position, path);

                if (path.status != NavMeshPathStatus.PathInvalid)
                {
                    float distanceCheckToTarget = Vector3.Distance(transform.position, target.transform.position);

                    if (distanceCheckToTarget > _curRangeOfHoverStarting)
                    {
                        _isHovering = false;

                        if(target.GetComponent<S_Unit>().GetUnitType() == SO_UnitItemData.UnitType.small) _curRangeOfHoverStarting = _RangeOfHoverStartingForSmall;
                        else _curRangeOfHoverStarting = _RangeOfHoverStartingForBig;


                        _curHoverCooldown = 0f;

                        Vector3 dirToMovePosition = (path.corners[1] - transform.position).normalized;

                        float obstacleDistanceCheck = 6f;
                        RaycastHit hit;

                        if (!Physics.BoxCast(transform.position, new Vector3(1.5f, 1.5f, 1.5f), transform.forward, out hit, transform.rotation, obstacleDistanceCheck, 1))
                        {
                            forwardAmount = 1f;
                        }
                        else forwardAmount = 0f;

                        float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);
                        if (angleToDir > 0) turnAmount = 1f;
                        else turnAmount = -1f;

                        unitState = State.Moving;

                        //Collider[] colliders = Physics.OverlapSphere(transform.position, attackDistance, 1 << 7);

                        //foreach (Collider collider in colliders) if (collider.gameObject == target) ShouldAttack = true;

                    }
                    else
                    {
                        unitState = State.Hovering;
                        _isHovering = true;

                        if (target.GetComponent<S_Unit>().GetUnitType() == SO_UnitItemData.UnitType.small) _curRangeOfHoverStarting = _RangeOfHoverStartingForSmall+2f;
                        else _curRangeOfHoverStarting = _RangeOfHoverStartingForBig+2f;

                        forwardAmount = 0f;
                        turnAmount = 0f;

                        if (_curHoverCooldown > 0) _curHoverCooldown -= Time.deltaTime;
                        else
                        {
                            _curHoverCooldown = _HoverCooldown;
                            FindNextHoverPoint();
                        }

                        if (_isHovering)
                        {
                            Quaternion rotation = Quaternion.LookRotation(target.transform.position - transform.position);
                            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);

                            float angle = Quaternion.Angle(transform.rotation, rotation);
                            if (angle > 0.5f) transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 30f);

                            Vector3 dirToMovePosition = (_pointToHover - transform.position).normalized;
                            float dot = Vector3.Dot(transform.right, dirToMovePosition);

                            float _distanceToPoint = Vector3.Distance(transform.position, _pointToHover);

                            if (_distanceToPoint > 2f)
                            {
                                RaycastHit hit;

                                if (dot > 0)
                                {
                                    bool isHit = Physics.BoxCast(transform.position, new Vector3(1.75f, 1.75f, 1.75f), transform.right, out hit, transform.rotation, 2f, 1 << 7);
                                    if (!isHit) forwardAmount = 1f;
                                    else forwardAmount = 0f;
                                }
                                else
                                {
                                    bool isHit = Physics.BoxCast(transform.position, new Vector3(1.75f, 1.75f, 1.75f), -transform.right, out hit, transform.rotation, 2f, 1 << 7);
                                    if (!isHit) forwardAmount = -1f;
                                    else forwardAmount = 0f;
                                }
                            }
                            else forwardAmount = 0f;

                        }
                    }

                    //if (!ShouldAttack)
                    //{
                    //    //Enemy is far
                    //    Vector3 dirToMovePosition = (path.corners[1] - transform.position).normalized;

                    //    float obstacleDistanceCheck = 7.5f;
                    //    RaycastHit hit;

                    //    if (!Physics.BoxCast(transform.position, new Vector3(1.5f, 1.5f, 1.5f), transform.forward, out hit, transform.rotation, obstacleDistanceCheck, 1))
                    //    {
                    //        forwardAmount = 1f;
                    //    }
                    //    else forwardAmount = 0f;

                    //    float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);
                    //    if (angleToDir > 0) turnAmount = 1f;
                    //    else turnAmount = -1f;

                    //    unitState = State.Moving;
                    //}
                    //else
                    //{
                    //    //Enemy in attack range
                    //    Vector3 dirToLookPosition = (target.transform.position - transform.position).normalized;
                    //    float angleToDir = Vector3.SignedAngle(transform.forward, dirToLookPosition, Vector3.up);

                    //    if (angleToDir > 15f) turnAmount = 1f;
                    //    else if (angleToDir < -15f) turnAmount = -1f;
                    //    else turnAmount = 0f;

                    //    forwardAmount = 0f;

                    //    if (curAttackCooldown <= 0) HandleAttack();

                    //    unitState = State.Idle;
                    //}
                }

                if(_isHovering)
                {
                    if(curAttackCooldown > 0) curAttackCooldown -= Time.deltaTime;
                    else HandleAttack();
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

            if (!_isHovering) agent.Move(transform.forward * speed * Time.deltaTime);
            else agent.Move(transform.right * speed * Time.deltaTime);

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

        private void FindNextHoverPoint()
        {
            _pointToHover = Random.insideUnitSphere * _curRangeOfHoverStarting + target.transform.position;
            _pointToHover.y = 2f;

            _pointToHover = (_pointToHover - target.transform.position).normalized * _curRangeOfHoverStarting + target.transform.position;
            _pointToHover.y = 2f;

            NavMeshHit hit;
            NavMesh.SamplePosition(_pointToHover, out hit, 10f, 1);
            _pointToHover = hit.position;
            _pointToHover.y = 2f;
        }

        [ServerCallback]
        public void HandleAttack()
        {
            curAttackCooldown = attackCooldown;
            MakeHit();
            //ClientMakeAttack();
        }

        [ServerCallback]
        public void MakeHit()
        {
            if (target == null) return;

            System.Random rand = new System.Random();
            float distanceToTarget = Vector3.Distance(_attackSpherePoint.position, target.transform.position);
            Vector3 spread = new Vector3((float)rand.Next(-5, 5) * (distanceToTarget / 100f), (float)rand.Next(-5, 5) * (distanceToTarget / 100f), (float)rand.Next(-5, 5) * (distanceToTarget / 100f));
            Vector3 shootDir = ((target.transform.Find("TargetPoint").position + spread) - _attackSpherePoint.position).normalized;

            RaycastHit[] hits = Physics.RaycastAll(_attackSpherePoint.position, shootDir, 10f, 1 << 7);

            Vector3 _laserHitPoint = _attackSpherePoint.position + shootDir*10f;
            foreach (RaycastHit hit in hits)
            {
                if(hit.collider.gameObject == target)
                {
                    int dmg = rand.Next(minDamage, maxDamage);

                    if (target != null)
                    {
                        target.GetComponent<S_Unit>().CalcDamage(dmg,1);
                        _laserHitPoint = hit.point;
                    }

                    break;
                }
            }

            ClientMakeAttack(_laserHitPoint);   
        }

        [ClientRpc]
        private void ClientMakeAttack(Vector3 laserEnd)
        {
            _lineRenderer.SetPosition(0, _attackSpherePoint.position);
            _lineRenderer.SetPosition(1,laserEnd);
            if (weaponAnim != null) weaponAnim.Play("LaserAttack");
        }
    }
}
