﻿using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager singleton;

    [SerializeField] private string matchScene;
    [SerializeField] private GameObject startMatchButton;
    [Space]
    [SerializeField] private GameObject clientsPanel;
    [SerializeField] private GameObject clientVisualizationPrefab;
    
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
        else
            Debug.LogWarning("Multiple LobbyManager exist", this);
    }

    public void SetupLobby()
    {
        startMatchButton.SetActive(isServer);
        
        clientsPanel.transform.DestroyAllChildren();

        List<Client> clients = GameManager.singleton.clients;
        
        if (clients != null)
            foreach (Client client in clients)
            {
                GameObject go = Instantiate(clientVisualizationPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
                go.transform.SetParent(clientsPanel.transform, false);
                go.GetComponent<Image>().color = Random.ColorHSV(0f, 1f, 0.9f, 0.9f, 1f, 1f);
            }
    }
    
    //[Server] // Allows only the server to start a match 
    public void StartMatch()
    {
        RpcStartMatchAllClients();
    }

    [ClientRpc] // Called on all clients
    public void RpcStartMatchAllClients()
    {
        //SceneManager.LoadScene(matchScene, LoadSceneMode.Single);
        MatchManager.Instance.SetNewMatchPhase(new WaitingForPlayers());
        gameObject.SetActive(false);
    }

}
