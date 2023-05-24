using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiFPS {
    public class ServerList : MonoBehaviour
    {
        [SerializeField] ServerListServerRepresenter _representerPrefab;

        public string receivedJson;

        [SerializeField] List<ServerRepresenterData> _servers = new List<ServerRepresenterData>();
        private ServerListServerRepresenter[] _spawnedTiles = new ServerListServerRepresenter[0];
        [SerializeField] GameObject _noRoomsMessage;
        [SerializeField] Transform _gridParent;

        [SerializeField] Button _refreshButton;

        private void Awake()
        {
            _refreshButton.onClick.AddListener(Refresh);

            _representerPrefab.gameObject.SetActive(false);
            _noRoomsMessage.gameObject.SetActive(false);
        }

        private void Start()
        {
        }

        public void OnRegionSet() 
        {
            Refresh();
        }

        void Refresh() 
        {
            WebRequestManager.Instance.Get("/serverList", ReadJson, null);
        }

        void ReadJson(string json, int code)
        {
            ClearSpawnedTiles();

            receivedJson = json;

            _servers.Clear();

            List<string> serversList = new List<string>();

            if (receivedJson.Length <= 2)
            {
                _noRoomsMessage.SetActive(true);
                return;
            }
            _noRoomsMessage.SetActive(false);

            receivedJson = json.Replace("[", "");
            receivedJson = receivedJson.Replace("]", "");

            receivedJson = json.Replace("},{", "}[{");

            string[] rooms = receivedJson.Split('[');

            for (int i = 0; i < rooms.Length; i++)
            {
                if (!string.IsNullOrEmpty(rooms[i]))
                    serversList.Add(rooms[i]);
            }

            for (int i = 0; i < serversList.Count; i++)
            {
                _servers.Add(WebRequestManager.Deserialize<ServerRepresenterData>(serversList[i]));
            }
            DrawTiles();
        }
        void DrawTiles()
        {
            _representerPrefab.gameObject.SetActive(true);


            _spawnedTiles = new ServerListServerRepresenter[_servers.Count];

            if (_servers.Count <= 0)
            {
                _noRoomsMessage.gameObject.SetActive(true);
                return;
            }

            for (int i = 0; i < _servers.Count; i++)
            {
                _spawnedTiles.SetValue(Instantiate(_representerPrefab.gameObject, _gridParent.transform).GetComponent<ServerListServerRepresenter>(),i);
                _spawnedTiles[i].Setup(_servers[i]);
            }

            _representerPrefab.gameObject.SetActive(false);
        }

        void ClearSpawnedTiles() {
            for (int i = 0; i < _spawnedTiles.Length; i++)
            {
                Destroy(_spawnedTiles[i].gameObject);
            }
            _spawnedTiles = new ServerListServerRepresenter[0];
        }
    }
    public class ServersContainer 
    {
        public ServerRepresenterData[] Servers;
    }
    public class ServerRepresenterData
    {
        //dont display for user
        public string ServerAddress;
        public string Port;

        //display for user
        public string ServerName;
        public string GamemodeID;
        public string MapID;
        public string CurrentPlayers;
        public string MaxPlayers;
    }
}