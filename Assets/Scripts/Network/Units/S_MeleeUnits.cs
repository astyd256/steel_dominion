using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Mirror
{
    public class S_MeleeUnits : NetworkBehaviour
    {
        public Canvas canvasUI = null;
        public TMP_Text testText = null;

        private int Teamid = 0;
        private GameObject target = null;

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
            ChangeText(target.name);
            this.transform.LookAt(target.transform.position);
            this.transform.rotation = Quaternion.Euler(-90, this.transform.rotation.eulerAngles.y, this.transform.rotation.eulerAngles.z);
        }

        [Client]
        public void ChangeText(string newText)
        {
            canvasUI.gameObject.SetActive(true);
            testText.text = newText;
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

