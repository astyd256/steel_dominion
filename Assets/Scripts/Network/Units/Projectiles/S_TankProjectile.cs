using UnityEngine;

public class S_TankProjectile : MonoBehaviour
{
    private int damage = 0;
    private int teamId = 0;
    private Vector3 shootDir;
    private float moveSpeed = 0f;

#if !UNITY_SERVER
    private bool dead = false;
#endif

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
#if !UNITY_SERVER
        if (dead) return;
#endif
        if (other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
#if !UNITY_SERVER
            dead = true;
            moveSpeed = 0.2f;
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;

            Instantiate(S_GameAssets.i.pfRogueSmallShotHitPS, transform.position, Quaternion.identity);
#endif
#if UNITY_SERVER
            Destroy(this.gameObject);
#endif
            return;
        }

        if (other.GetComponent<Mirror.S_Unit>().GetTeam() != teamId)
        {
           other.GetComponent<Mirror.S_Unit>().CalcDamage(damage,1);

#if !UNITY_SERVER
            dead = true;
            moveSpeed = 0.2f;
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;

            Instantiate(S_GameAssets.i.pfRogueSmallShotHitPS, transform.position, Quaternion.identity);
#endif
#if UNITY_SERVER
            Destroy(this.gameObject);
#endif
            return;
        }
    }

    private void Update()
    {
#if !UNITY_SERVER
        if (!dead) transform.position += shootDir * moveSpeed * Time.deltaTime;
        else
        {
            moveSpeed -= Time.deltaTime;
            if(moveSpeed < 0) Destroy(this.gameObject);
        }
#endif
#if UNITY_SERVER
        transform.position += shootDir * moveSpeed * Time.deltaTime;
#endif
    }
}
