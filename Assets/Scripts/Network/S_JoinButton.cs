using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror
{
    public class S_JoinButton : MonoBehaviour
    {
        [SerializeField] private S_NetworkManagerSteel netManager = null;
        [SerializeField] private Button joinBtn = null;
        // Start is called before the first frame update

        private void OnEnable()
        {
            S_NetworkManagerSteel.OnClientConnected += HandleClientConnected;
            S_NetworkManagerSteel.OnClientDisconnected += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            S_NetworkManagerSteel.OnClientConnected -= HandleClientConnected;
            S_NetworkManagerSteel.OnClientDisconnected -= HandleClientDisconnected;
        }
        public void JoinGame()
        {
            //Debug.Log("Join");
            netManager.StartClient();
            joinBtn.interactable = false;
        }

        public void HandleClientConnected()
        {
            var colors = joinBtn.colors;
            colors.normalColor = Color.green;
            joinBtn.colors = colors;
            joinBtn.interactable = true;
        }

        public void HandleClientDisconnected()
        {
            var colors = joinBtn.colors;
            colors.normalColor = Color.red;
            joinBtn.colors = colors;
            joinBtn.interactable = true;
        }
    }
}
