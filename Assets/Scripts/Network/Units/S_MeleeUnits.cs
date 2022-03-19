using UnityEngine;

namespace Mirror
{
    public class S_MeleeUnits : S_Unit
    {

        private bool ShouldAttack = false;

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

            if (target != null)
            {
                if (unitState == State.Attack) return;

                if ((distTotarget < 1.5f && unitState != State.Attack) || ShouldAttack)
                {
                    // Debug.Log("Trying to attack!");
                    this.transform.LookAt(target.transform.position);
                    this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

                    agent.isStopped = true;
                    unitState = State.Attack;

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
                    unitState = State.Idle;
                    this.CallWithDelay(ResetState, 2f);

                    return;
                }
                else if (unitState == State.Chase)
                {

                    agent.SetDestination(target.transform.position);
                    distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);

                    ShouldAttack = false;

                    if (distTotarget < 5.5f)
                    {
                        Collider[] colliders = Physics.OverlapSphere(AttackSpherePoint.position, 1.5f);

                        foreach (var hitCollider in colliders)
                        {
                            if (hitCollider.gameObject.name == target.name)
                            {

                                //Debug.Log("In range for attack = " + unitState);
                                ShouldAttack = true;
                                //break;
                            }
                        }
                    }
                    return;
                }
                else if(distTotarget > 1.5f)
                {
                    agent.SetDestination(target.transform.position);
                    distTotarget = Vector3.Distance(this.gameObject.transform.position, target.transform.position);
                }
            }
        }
    }
}
