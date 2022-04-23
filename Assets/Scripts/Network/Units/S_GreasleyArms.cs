using UnityEngine;

public class S_GreasleyArms : MonoBehaviour
{
#if !UNITY_SERVER
    [SerializeField] private Transform _attackPoint = null;
    [SerializeField] private GameObject _attackParticleSystem = null;

    public void PlayParticles()
    {
        Instantiate(_attackParticleSystem, _attackPoint.position, Quaternion.identity);
    }
#endif
}
