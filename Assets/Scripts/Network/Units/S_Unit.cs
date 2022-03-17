using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using TMPro;

namespace Mirror
{
    public class S_Unit : NetworkBehaviour
    {
        [SerializeField]
        private Canvas canvasUI = null;
        [SerializeField]
        private Slider healthBar = null;
        [SerializeField]
        private NavMeshAgent agent = null;

        private enum State
        {
            Chase,
            Attack,
            Idle
        }

        State unitState = State.Idle;

        private int Teamid = 0;
        private GameObject target = null;

        private bool chase = false;

        private S_NetworkManagerSteel gameroom;

        private S_NetworkManagerSteel GameRoom
        {
            get
            {
                if (gameroom != null) { return gameroom; }
                return gameroom = NetworkManager.singleton as S_NetworkManagerSteel;
            }
        }

        [Server]
        public void SetTeam(int teamid)
        {
            Teamid = teamid;
        }

        [Server]
        public void StartBehaviour()
        {
            CalcDistances();

            ShowHealth();
            //this.transform.LookAt(target.transform.position);
            //this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);

            chase = (target != null);
        }

        [ServerCallback]

        private void Update()
        {
            if (chase) agent.SetDestination(target.transform.position);
        }

        [ClientRpc]
        public void ShowHealth()
        {
            canvasUI.gameObject.SetActive(true);
        }

        public void CalcDistances()
        {
            float minDistance = 1000000;
            List<GameObject> unitlists = new List<GameObject>();

            if (Teamid == 0) unitlists = GameRoom.GetBattlePlayerUnits(1).ToList();
            else unitlists = GameRoom.GetBattlePlayerUnits(0).ToList();

            foreach (GameObject unit in unitlists)
            {
                float dist = Vector3.Distance(this.gameObject.transform.position, unit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    target = unit;
                }
            }
        }
    }
}
