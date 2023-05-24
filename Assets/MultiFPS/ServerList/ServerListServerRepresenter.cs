using Mirror.SimpleWeb;
using MultiFPS.Gameplay.Gamemodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS {
    public class ServerListServerRepresenter : MonoBehaviour
    {
        [SerializeField] Text _roomName;
        [SerializeField] Text _gamemodeName;
        [SerializeField] Text _mapName;
        [SerializeField] Text _playerCount;

        string _serverAddress;
        ushort _port;

        public void Setup(ServerRepresenterData data)
        {
            int gamemodeID = System.Convert.ToInt32(data.GamemodeID);
            int mapID = System.Convert.ToInt32(data.MapID);

            _roomName.text = data.ServerName;
            _gamemodeName.text = ((Gamemodes)gamemodeID).ToString();
            _mapName.text = RoomCreator.Instance.Maps[mapID].Name;
            _playerCount.text = $"{data.CurrentPlayers}/{RoomCreator.Instance.PlayerNumberOptions[System.Convert.ToInt32(data.MaxPlayers)]}";

            _port = (ushort)System.Convert.ToInt32(data.Port);
            _serverAddress = data.ServerAddress;

            GetComponent<Button>().onClick.AddListener(ButtonConnect);
        }

        void ButtonConnect() 
        {
            CustomNetworkManager._instance.networkAddress = _serverAddress;
            CustomNetworkManager._instance.GetComponent<SimpleWebTransport>().port = _port;
            CustomNetworkManager._instance.ConnectToTheGame();
        }
    }
}