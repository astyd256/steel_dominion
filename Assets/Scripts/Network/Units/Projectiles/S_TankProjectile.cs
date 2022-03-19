using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_TankProjectile : MonoBehaviour
{
    private int damage = 0;
    private int teamId = 0;
    private Vector3 shootDir;
    private float moveSpeed = 0f;

    private Mirror.S_NetworkManagerSteel gameroom;

    protected Mirror.S_NetworkManagerSteel GameRoom
    {
        get
        {
            if (gameroom != null) { return gameroom; }
            return gameroom = Mirror.NetworkManager.singleton as Mirror.S_NetworkManagerSteel;
        }
    }

    public void SetData(int Damage, int TeamId, Vector3 ShootDir, float MoveSpeed)
    {
        damage = Damage;
        teamId = TeamId;
        shootDir = ShootDir;
        moveSpeed = MoveSpeed;
        transform.rotation = Quaternion.LookRotation(shootDir);
        Destroy(this.gameObject, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            Destroy(this.gameObject);
            return;
        }

        if (other.GetComponent<Mirror.S_Unit>().GetTeam() != teamId)
        {
           other.GetComponent<Mirror.S_Unit>().CalcDamage(damage);

            Destroy(this.gameObject);
            return;
        }
    }

    private void Update()
    {
        transform.position += shootDir * moveSpeed * Time.deltaTime;
    }
}
