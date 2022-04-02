using UnityEngine;

namespace Mirror
{
    public class S_MeleeUnits : S_Unit
    {
        [SerializeField]
        private bool ShouldAttack = false;
        [SerializeField]
        private float attackCooldown = 0f;

        [SerializeField]
        private Animation weaponAnim = null;

        [Server]
        public override void ResetState()
        {
           // Debug.Log("Reseting");
            ShouldAttack = false;
            CalcDistances();

            if (target != null) unitState = State.Chase;
        }

        [ServerCallback]

        private void Update()
        {
            if (unitState == State.Idle) return;

            if(unitState == State.AttackAfterPause)
            {
                attackCooldown -= Time.deltaTime;

                if(attackCooldown <= 0f)
                {
                    ResetState();
                    attackCooldown = 2f;
                }
            }

            if (target != null)
            {
               // Debug.Log("Update = " + unitState + " Dis = " + distTotarget);
                if (unitState == State.Attack)
                {
                    if(attackCooldown > 0f) attackCooldown -= Time.deltaTime;
                    else if(attackCooldown <= 0f)
                    {
                        Collider[] colliders = Physics.OverlapSphere(AttackSpherePoint.position, 2f);
                        // Debug.Log("Trying to attack!");
                        foreach (var hitCollider in colliders)
                        {
                            if (hitCollider.gameObject.name == target.name)
                            {
                                // Debug.Log("Damage to " + target.name);
                                // Debug.Log("enemy hit");

                                System.Random rand = new System.Random();

                                int dmg = rand.Next(minDamage, maxDamage);

                                target.GetComponent<S_Unit>().CalcDamage(dmg);
                                break;
                            }
                        }
                        unitState = State.AttackAfterPause;
                        attackCooldown = 2f;
                    }

                    return;
                };

                if (((distTotarget < 1.5f  || ShouldAttack) && unitState != State.Attack) && unitState != State.AttackAfterPause && unitState !=State.PreAttack)
                {
                    // Debug.Log("Trying to attack!");
                    this.transform.LookAt(target.transform.position);
                    this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

                    agent.isStopped = true;
                    unitState = State.Attack;
                    attackCooldown = 0.37f;

                    ClientMakeAttack();
                   
                   // this.CallWithDelay(ResetState, 2f);

                    return;
                }
                else if (unitState == State.Chase)
                {
                    CalcDistances();
                    agent.SetDestination(target.transform.position);
                    distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);

                    ShouldAttack = false;

                    if (distTotarget < 6.5f)
                    {
                        // Debug.Log("Check Should attack!");
                        Collider[] colliders = Physics.OverlapSphere(AttackSpherePoint.position, 1f);

                        foreach (var hitCollider in colliders)
                        {
                            //  Debug.Log("Check  =  "+ hitCollider.name);
                            if (hitCollider.gameObject == target)
                            {
                                //Debug.Log("Should attack!");
                                //Debug.Log("In range for attack = " + unitState);
                                ShouldAttack = true;
                                //break;
                            }
                        }
                    }
                    return;
                }
                //else if(distTotarget > 2f && unitState != State.Attack)
                //{
                //    agent.SetDestination(target.transform.position);
                //    distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
                //}
            }
        }

        [ClientRpc]
        private void ClientMakeAttack()
        {
            //Transform bulletTransform = Instantiate(projectilePrefab, AttackSpherePoint.position, Quaternion.identity);
            //Vector3 shootDir = (target.transform.position - AttackSpherePoint.position).normalized;

            //System.Random rand = new System.Random();

            //bulletTransform.GetComponent<S_TankProjectile>().SetData(0, Teamid, shootDir, 100f);

            if(weaponAnim != null) weaponAnim.Play("ArmsAttack");

        }
    }
}
