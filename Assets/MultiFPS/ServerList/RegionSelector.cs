using MultiFPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiFPS.UI
{
    public class RegionSelector : MonoBehaviour
    {

        [SerializeField] ServerList _serverList;
        static bool connected;

        [SerializeField] Button _showChangeRegionPanelButton;

        [SerializeField] Region[] _regions;
        [SerializeField] GameObject _regionButtonPrefab;
        [SerializeField] Transform _gridParent;

        [SerializeField] Text _errorMessage;
        [SerializeField] Text _regionIndicator;

        string _pp_region = "_pp_region";

        int _optionID = -1;
        void Start()
        {
            _regionIndicator.text = string.Empty;
            _errorMessage.text = string.Empty;

            for (int i = 0; i < _regions.Length; i++)
            {
                Button region = Instantiate(_regionButtonPrefab, _gridParent).GetComponent<Button>();

                region.GetComponentInChildren<Text>().text = _regions[i].Name;

                region.gameObject.name = $"{i} Region";

                int option = i;

                region.onClick.AddListener(() => TryConnectToRegion(option));
            }

            _errorMessage.transform.SetAsLastSibling();

            _regionButtonPrefab.SetActive(false);

            _showChangeRegionPanelButton.onClick.AddListener(ShowPanel);

            if (connected)
                gameObject.SetActive(false);

            _optionID = PlayerPrefs.GetInt(_pp_region);

            if (_optionID != -1)
                TryConnectToRegion(_optionID);
        }

        void ShowPanel()
        {
            gameObject.SetActive(true);
            _errorMessage.text = string.Empty;
        }

        public void TryConnectToRegion(int optionID)
        {
            _optionID = optionID;
            connected = false;
            WebRequestManager.Instance.SetDomain(_regions[optionID].Domain);
            WebRequestManager.Instance.Get("/regionCheck", OnRegionSelectedSuccessfully, OnRegionSelectedError);

            _errorMessage.text = "Connecting...";
        }

        void OnRegionSelectedSuccessfully(string downloadHandler = "", int responseCode = 0)
        {
            if (responseCode == 202)
            {
                gameObject.SetActive(false);
                connected = true;
            }
            else
                OnRegionSelectedError("", 0);

            PlayerPrefs.SetInt(_pp_region, _optionID);
            _regionIndicator.text = $"Region: {_regions[_optionID].Name}";

            _serverList.OnRegionSet();
        }
        void OnRegionSelectedError(string downloadHandler = "", int responseCode = 0)
        {
            _errorMessage.text = "Could not receive response from this region, please select different one";
        }

        [System.Serializable]
        public class Region
        {
            public string Name;
            public string Domain;
        }
    }
}