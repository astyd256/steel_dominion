using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mirror
{
    public class S_TankRogueArtillery : NetworkBehaviour
    {
        private S_TankRogueMovement _tankMainScript = null;

        [Header("Weapon settings")]
        [SerializeField] protected float _shootingDistance = 40f;
        [SerializeField] private Transform _projectilePrefab = null;
        [SerializeField] protected Transform _AttackSpherePoint = null;
        [SerializeField] private float _fireRate = 10f;
        [SerializeField] private float _minDistanceToShoot = 10f;
        [SerializeField] private int _mediumDamage = 35;

        private Transform _curTarget = null;

        private float _currentFireCooldown = 0;
        private int _teamid = -1;

        [SerializeField] private bool _isAlive =false;
        public override void OnStartServer()
        {
            _tankMainScript = transform.GetComponent<S_TankRogueMovement>();
            _tankMainScript._behaviourStarting += StartBehaviour;
        }
        public override void OnStopServer()
        {
            _tankMainScript._behaviourStarting -= StartBehaviour;
        }

        public override void OnStartClient()
        {
            _tankMainScript = transform.GetComponent<S_TankRogueMovement>();
        }

        [Server]
        private void StartBehaviour()
        {
            _teamid = _tankMainScript.GetTeam();
            _isAlive = true;
        }
        [ServerCallback]
        public void Update()
        {
            if (isServer) ServerUpdate();
        }

        [ServerCallback]
        public void ServerUpdate()
        {
            if (!_isAlive) return;

            if (_currentFireCooldown > 0) _currentFireCooldown -= Time.deltaTime;
            else if (FindTarget()) MakeShot();
        }

        private bool FindTarget()
        {
            _curTarget = null;
            float minDistance = 1000000;
            List<GameObject> unitlists = new List<GameObject>();

            if (_teamid == 0) unitlists = _tankMainScript.GetGameRoom().GetBattlePlayerUnits(1).ToList();
            else unitlists = _tankMainScript.GetGameRoom().GetBattlePlayerUnits(0).ToList();

            foreach (GameObject unit in unitlists)
            {
                float dist = Vector3.Distance(this.gameObject.transform.position, unit.transform.position);
                if (dist < minDistance && dist >= _minDistanceToShoot)
                {
                    minDistance = dist;
                    _curTarget = unit.transform;
                }
            }

            if (_curTarget != null) return true;
            else return false;
        }

        //#if !UNITY_SERVER //// server update
        //        void Update()
        //        {
        //            Debug.Log("TowerServer - get target");
        //            Transform oldTarget = curTarget;
        //            curTarget = tankMainScript.GetTarget();
        //            Debug.Log("TowerServer - new target - " + curTarget);
        //            Debug.Log("TowerServer - object - " + towerObject);
        //            if (curTarget == null) return;

        //            if(oldTarget != curTarget) ClientSetTarget(curTarget);
        //            else if (curTarget = null) ClientSetTarget(null);

        //            Vector3 dirToLookPosition = (curTarget.position - towerObject.position).normalized;
        //            float angleToDir = Vector3.SignedAngle(towerObject.forward + new Vector3(90, 0, -90).normalized, dirToLookPosition, Vector3.up);

        //            if (angleToDir > 10f) turnAmount = 1f;
        //            else if (angleToDir < -10f) turnAmount = -1f;
        //            else turnAmount = 0f;

        //            HandleMovement();
        //        }
        //#endif

        [Server]
        private void MakeShot()
        {
            _currentFireCooldown = _fireRate;

            if (_curTarget == null)
            {
                _currentFireCooldown = 0f;
                return;
            }

            Transform bulletTransform = Instantiate(_projectilePrefab, _AttackSpherePoint.position, Quaternion.identity);

            System.Random rand = new System.Random();

            float distanceToTarget = Vector3.Distance(transform.position, _curTarget.position);
            float _timeTofly = Mathf.Clamp(distanceToTarget / 60f * 5f,3.5f,8f);
            
            Debug.Log("Time to fly = " + _timeTofly);

            Vector3 spread = new Vector3((float)rand.Next(-5, 5) * (distanceToTarget / 75f), (float)rand.Next(-5, 5) * (distanceToTarget / 75f), (float)rand.Next(-5, 5) * (distanceToTarget / 75f));
            Vector3 _finalEnemyPosition = _curTarget.transform.position + spread;
            ClientMakeShot(_finalEnemyPosition, _timeTofly);

            bulletTransform.GetComponent<S_TankRogueArtilleryProjectile>().SetData(rand.Next(_mediumDamage-5, _mediumDamage+5), _tankMainScript.GetTeam());

            float vx = (_finalEnemyPosition.x - _AttackSpherePoint.position.x) / _timeTofly;
            float vz = (_finalEnemyPosition.z - _AttackSpherePoint.position.z) / _timeTofly;
            float vy = ((_finalEnemyPosition.y - _AttackSpherePoint.position.y) - 0.5f * Physics.gravity.y * _timeTofly * _timeTofly) / _timeTofly;

            bulletTransform.GetComponent<Rigidbody>().velocity =new Vector3(vx,vy,vz);
        }

        [ClientRpc]
        private void ClientMakeShot(Vector3 enemyPosition, float _flyTime)
        {
            Debug.Log("Spawn projectile art");
            Transform bulletTransform = Instantiate(_projectilePrefab, _AttackSpherePoint.position, Quaternion.identity);

            bulletTransform.GetComponent<S_TankRogueArtilleryProjectile>().SetData(0, _tankMainScript.GetTeam());

            float vx = (enemyPosition.x - _AttackSpherePoint.position.x) / _flyTime;
            float vz = (enemyPosition.z - _AttackSpherePoint.position.z) / _flyTime;
            float vy = ((enemyPosition.y - _AttackSpherePoint.position.y) - 0.5f * Physics.gravity.y * _flyTime * _flyTime) / _flyTime;

            bulletTransform.GetComponent<Rigidbody>().velocity = new Vector3(vx, vy, vz);
        }
    }
}
