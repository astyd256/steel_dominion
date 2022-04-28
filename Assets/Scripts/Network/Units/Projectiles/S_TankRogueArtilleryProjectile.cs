using UnityEngine;

public class S_TankRogueArtilleryProjectile : MonoBehaviour
{
//#if UNITY_SERVER
    [SerializeField] private float _maxDamageDistance = 5f;
    [SerializeField] private float _minDamageDistance = 15f;

#if !UNITY_SERVER
    private Transform _meshTransform = null;
    private Rigidbody _rigidbody = null;
    private bool _isAlive = true;
    private float _lifeTime = 0.3f;
#endif
    //#endif

    private int _teamid = -1;
    private int _damage = 0;

#if !UNITY_SERVER
    private void Start()
    {
        _meshTransform = transform.GetChild(0);
        _rigidbody = GetComponent<Rigidbody>();
    }
#endif
    public void SetData(int damage, int teamid)
    {
        _damage = damage;
        _teamid = teamid;
    }

    private void OnTriggerEnter(Collider other)
    {
#if !UNITY_SERVER
        if (!_isAlive) return;

        Instantiate(S_GameAssets.i.pfRogueBigShotHitPS, transform.position + new Vector3(0,1f,0), Quaternion.identity);
#endif
        //#if UNITY_SERVER
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f, 1 << 7);
        foreach (var col in colliders)
        {
            if (col.transform.GetComponent<Mirror.S_Unit>() == null) continue;

            if (col.transform.GetComponent<Mirror.S_Unit>().GetTeam() != _teamid)
            {
                float _damageDistance = Vector3.Distance(transform.position, col.transform.position);

                if (_damageDistance <= _maxDamageDistance) col.transform.GetComponent<Mirror.S_Unit>().CalcDamage(_damage,1);
                else if (_damageDistance >= _minDamageDistance) col.transform.GetComponent<Mirror.S_Unit>().CalcDamage((float)_damage / 2f,1);
                else
                {
                    float _finalDamage = (float)_damage * (1f - (_damageDistance - _maxDamageDistance) / (_minDamageDistance - _maxDamageDistance));
                    col.transform.GetComponent<Mirror.S_Unit>().CalcDamage(_finalDamage,1);
                }
            }
        }
        //#endif
        Debug.Log("Destroy projectile");
#if UNITY_SERVER
        Destroy(gameObject);
#endif
#if !UNITY_SERVER
        _isAlive = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.isKinematic = true;
#endif
    }

#if !UNITY_SERVER
    private void Update()
    {
        if(_isAlive) _meshTransform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
        else
        {
            if(_lifeTime>0) _lifeTime -= Time.deltaTime;
            else
            {
                Destroy(gameObject);
            }
        }
    }
#endif
}
