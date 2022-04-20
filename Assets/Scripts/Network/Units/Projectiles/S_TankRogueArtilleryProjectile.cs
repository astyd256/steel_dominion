using UnityEngine;

public class S_TankRogueArtilleryProjectile : MonoBehaviour
{
//#if UNITY_SERVER
    [SerializeField] private float _maxDamageDistance = 5f;
    [SerializeField] private float _minDamageDistance = 15f;
//#endif

    private int _teamid = -1;
    private int _damage = 0;

    public void SetData(int damage, int teamid)
    {
        _damage = damage;
        _teamid = teamid;
    }

    private void OnTriggerEnter(Collider other)
    {
        //#if UNITY_SERVER
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, 1 << 7);
        foreach (var col in colliders)
        {
            if (col.transform.GetComponent<Mirror.S_Unit>() == null) continue;

            if (col.transform.GetComponent<Mirror.S_Unit>().GetTeam() != _teamid)
            {
                float _damageDistance = Vector3.Distance(transform.position, col.transform.position);

                if (_damageDistance <= _maxDamageDistance) col.transform.GetComponent<Mirror.S_Unit>().CalcDamage(_damage);
                else if (_damageDistance >= _minDamageDistance) col.transform.GetComponent<Mirror.S_Unit>().CalcDamage((float)_damage / 2f);
                else
                {
                    float _finalDamage = (float)_damage * (1f - (_damageDistance - _maxDamageDistance) / (_minDamageDistance - _maxDamageDistance));
                    col.transform.GetComponent<Mirror.S_Unit>().CalcDamage(_finalDamage);
                }
            }
        }
        //#endif
        Debug.Log("Destroy projectile");
        Destroy(gameObject);
    }
}
