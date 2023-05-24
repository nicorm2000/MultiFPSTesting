using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror.SimpleWeb;
using MultiFPS.Gameplay.Gamemodes;

namespace MultiFPS
{
    public class ServerCommunicatorServerList : ServerCommunicator
    {

        protected override void OnPlayerConnected(NetworkConnectionToClient conn)
        {
            WWWForm form = new WWWForm();
            form.AddField("Port", (_transport.port).ToString());
            form.AddField("CurrentPlayers", NetworkServer.connections.Count);
            WebRequestManager.Instance.Post("/gameUpdate", form, null, null);
        }

        protected override void OnPlayerDisconnected(NetworkConnectionToClient conn)
        {
            base.OnPlayerDisconnected(conn);

            if (NetworkServer.connections.Count <= 0) return;

            WWWForm form = new WWWForm();
            form.AddField("Port", (_transport.port).ToString());
            form.AddField("CurrentPlayers", NetworkServer.connections.Count);
            WebRequestManager.Instance.Post("/gameUpdate", form, null, null);
        }

        protected override void CloseGame()
        {
            WWWForm form = new WWWForm();
            form.AddField("Port", (_transport.port).ToString());
            WebRequestManager.Instance.Post("/deleteRoom", form, OnGameDeregistered, null);

            void OnGameDeregistered(string data, int code) 
            {
                Application.Quit();
            }
        }

        public override void OnGameBooted()
        {
            WWWForm form = new WWWForm();
            form.AddField("Port", _transport.port);
            WebRequestManager.Instance.Post("/gameBooted", form, null, null);
        }
    }
}