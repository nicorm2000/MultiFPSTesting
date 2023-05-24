using UnityEngine.SceneManagement;
using UnityEngine;
using MultiFPS.Gameplay;
using MultiFPS.UI.HUD;
using System.Collections.Generic;
using Mirror;

using MultiFPS.Gameplay.Gamemodes;
using MultiFPS.UI.Gamemodes;

namespace MultiFPS.UI {
    public class ClientInterfaceManager : MonoBehaviour
    {
        public static ClientInterfaceManager Instance;

        //UI prefabs
        public GameObject PauseMenuUI;
        public GameObject ChatUI;
        public GameObject ScoreboardUI;
        public GameObject KillfeedUI;
        public GameObject PlayerHudUI;
        public GameObject PlayerNametag;

        public GameObject GameplayCamera;
        //these colors are here because we may want to adjust them easily in the inspector
        public UIColorSet UIColorSet;

        public SkinContainer[] characterSkins;
        public ItemSkinContainer[] ItemSkinContainers;

        List<UICharacterNametag> _spawnedNametags = new List<UICharacterNametag>();

        [Header("Gamemodes UI Prefabs")]
        [SerializeField] GameObject[] gamemodesUI;

        public void Awake()
        {

            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                //if this happens it means that player returns to hub scene with Client Manager from previous hub scene load, so we dont
                //need another one, so destroy this one

                //Destroy(gameObject);
                return;
            }


            SceneManager.sceneLoaded += OnSceneLoaded;

            GameManager.GameEvent_CharacterTeamAssigned += OnCharacterTeamAssigned;
            ClientFrontend.ClientFrontendEvent_OnObservedCharacterSet += OnObservedCharacterSet;

            UserSettings.SelectedItemSkins = new int[ItemSkinContainers.Length];

            for (int i = 0; i < ItemSkinContainers.Length; i++)
            {
                UserSettings.SelectedItemSkins[i] = -1;
            }

            ClientFrontend.ClientEvent_OnJoinedToGame += InstantiateUIforGivenGamemode;

            //by default cursor is hidden, show it for main menu
            ClientFrontend.ShowCursor(true);
        }

        /// <summary>
        /// This method will instantiate UI proper for gamemode, for example if we play Defuse gamemode, spawn UI wchich have
        /// score numbers for both teams, and bomb icon which we will display and color in red when bomb is planted
        /// </summary>
        void InstantiateUIforGivenGamemode(Gamemode gamemode, NetworkIdentity player)
        {
            int gamemodeID = (int)gamemode.Indicator;

            if (gamemodeID >= gamemodesUI.Length || gamemodesUI[gamemodeID] == null) return; //no ui for this gamemode avaible

            Instantiate(gamemodesUI[gamemodeID]).GetComponent<UIGamemode>().SetupUI(gamemode, player);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            var index = SceneManager.GetActiveScene().buildIndex;

            ClientFrontend.Hub = (index == 0);

            ClientFrontend.ShowCursor(index == 0);
            ClientFrontend.SetClientTeam(-1);

            //if we loaded non-hub scene, then spawn all the UI prefabs for player, then on disconnecting they will
            //be destroyed by scene unloading
            if (index != 0)
            {
                if (PauseMenuUI)
                    Instantiate(PauseMenuUI);
                if (ChatUI)
                    Instantiate(ChatUI);
                if (ScoreboardUI)
                    Instantiate(ScoreboardUI);
                if (KillfeedUI)
                    Instantiate(KillfeedUI);
                if (PlayerHudUI)
                    Instantiate(PlayerHudUI).GetComponent<Crosshair>().Setup();
            }
        }

        //reassign nametags when spectated player is changed
        public void OnObservedCharacterSet(CharacterInstance characterInstance)
        {
            DespawnAllNametags();

            PlayerInstance[] players = new List<PlayerInstance>(GameManager.Players.Values).ToArray();

            for (int i = 0; i < players.Length; i++)
            {
                OnCharacterTeamAssigned(players[i].MyCharacter);
            }
        }

        public void OnCharacterTeamAssigned(CharacterInstance characterInstance)
        {
            if (!characterInstance) return;
            //dont spawn nametag for player if we dont know yet which team our player belongs to
            if (!ClientFrontend.ClientTeamAssigned) return;

            //dont spawn matkers for enemies
            if (ClientFrontend.ThisClientTeam != characterInstance.Team || GameManager.Gamemode.FFA) return;

            if (characterInstance.CurrentHealth <= 0) return;
            //dont spawn nametag for player who views world from first person perspective

            if (characterInstance.netId == ClientFrontend.ObservedCharacterNetID())
                return;

            UICharacterNametag playerNameTag = Instantiate(PlayerNametag).GetComponent<UICharacterNametag>();
            playerNameTag.Set(characterInstance);

            _spawnedNametags.Add(playerNameTag);
        }

        void DespawnAllNametags()
        {
            for (int i = 0; i < _spawnedNametags.Count; i++)
            {
                _spawnedNametags[i].DespawnMe();
            }
            _spawnedNametags.Clear();
        }

        private void OnDestroy()
        {
            ClientFrontend.ClientEvent_OnJoinedToGame -= InstantiateUIforGivenGamemode;
        }
    }

    [System.Serializable]
    public class ItemSkinContainer
    {
        public string ItemName;
        public SingleItemSkinContainer[] Skins;
    }
    [System.Serializable]
    public class SingleItemSkinContainer
    {
        public string SkinName;
        public Material Skin;
    }
}
