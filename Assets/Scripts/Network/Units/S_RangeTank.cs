using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace Mirror
{
    public class S_RangeTank : S_Unit
    {
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
                        if (hitCollider.gameObject == target)
                        {
                            Debug.Log("Damage to " + target.name);

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
                    return;
                }
            }
        }

    }
}
