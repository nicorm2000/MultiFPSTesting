using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiFPS.UI;
namespace MultiFPS.Gameplay
{
    [DisallowMultipleComponent]
    /// <summary>
    /// This class is always instantiated on scene once, and will manage player input, so we dont
    /// have to check on every player instance separately if it belongs to us, we can just apply input
    /// read from this script to one spawned player prefab that is ours
    /// </summary>
    public class PlayerGameplayInput : MonoBehaviour
    {
        public static PlayerGameplayInput Instance { private set; get; }
        private CharacterInstance _myCharIntance;
        private CharacterMotor _motor;

        private void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (!_myCharIntance) return;

            //game managament related input
            if (Input.GetKeyDown(KeyCode.L))
            {
                if(ClientFrontend.GamemodeUI)
                    ClientFrontend.GamemodeUI.Btn_ShowTeamSelector();
            }

            if (ClientFrontend.GamePlayInput())
            {
                //character related input
                if (Input.GetKeyDown(KeyCode.Space)) _motor.Jump();

                if (Input.GetKeyDown(KeyCode.E)) _myCharIntance.CharacterItemManager.TryGrabItem();
                if (Input.GetKeyDown(KeyCode.G)) _myCharIntance.CharacterItemManager.TryDropItem();

                if (Input.GetKeyDown(KeyCode.Alpha1)) _myCharIntance.CharacterItemManager.ClientTakeItem(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) _myCharIntance.CharacterItemManager.ClientTakeItem(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) _myCharIntance.CharacterItemManager.ClientTakeItem(2);
                if (Input.GetKeyDown(KeyCode.Alpha4)) _myCharIntance.CharacterItemManager.ClientTakeItem(3);
                if (Input.GetKeyDown(KeyCode.X)) _myCharIntance.CharacterItemManager.ClientTakeItem(4);

                if (Input.GetKeyDown(KeyCode.R)) _myCharIntance.CharacterItemManager.Reload();
                if (Input.GetKeyDown(KeyCode.Q)) _myCharIntance.CharacterItemManager.TakePreviousItem();
                if (Input.GetKey(KeyCode.V) && _myCharIntance.CharacterItemManager.CurrentlyUsedItem) 
                    _myCharIntance.CharacterItemManager.CurrentlyUsedItem.PushMeele();

                _myCharIntance.SetActionKeyCode(ActionCodes.Trigger2, Input.GetMouseButton(1));
                _myCharIntance.SetActionKeyCode(ActionCodes.Trigger1, Input.GetMouseButton(0));

                if (!_myCharIntance.Block)
                {
                    _myCharIntance.movementInput.x = Input.GetAxis("Horizontal");
                    _myCharIntance.movementInput.y = Input.GetAxis("Vertical");
                }
                else
                    _myCharIntance.movementInput = Vector2.zero;


                /*  Vector2 desiredVelocity = GetLookInput() * _sensitivityScaler * UserSettings.MouseSensitivity * _myCharIntance.SensitivityItemFactorMultiplier;

                  _velocity = new Vector2
                  (
                      Mathf.MoveTowards(_velocity.x, desiredVelocity.x, _acceleration.x*Time.deltaTime),
                      Mathf.MoveTowards(_velocity.y, desiredVelocity.y, _acceleration.y*Time.deltaTime)
                  );

                  _myCharIntance.lookInput += _velocity * Time.deltaTime;*/
                _myCharIntance.lookInput += GetLookInput() * UserSettings.MouseSensitivity * _myCharIntance.SensitivityItemFactorMultiplier;
                /*
                  _myCharIntance.lookInput -= new Vector2(
                      Input.GetAxis("Mouse Y") * UserSettings.MouseSensitivity * _myCharIntance.SensitivityItemFactorMultiplier,
                      -Input.GetAxis("Mouse X") * UserSettings.MouseSensitivity * _myCharIntance.SensitivityItemFactorMultiplier)* _sensitivityScaler * Time.deltaTime;
                */
                //_myCharIntance.lookInput.y += Input.GetAxis("Mouse X") * UserSettings.MouseSensitivity * _myCharIntance.SensitivityItemFactorMultiplier;
                //_myCharIntance.lookInput.x -= Input.GetAxis("Mouse Y") * UserSettings.MouseSensitivity * _myCharIntance.SensitivityItemFactorMultiplier;

                _myCharIntance.SetActionKeyCode(ActionCodes.Sprint, Input.GetKey(KeyCode.LeftShift));// && !Input.GetMouseButton(0) && !Input.GetMouseButton(1);
                _myCharIntance.SetActionKeyCode(ActionCodes.Crouch, Input.GetKey(KeyCode.LeftControl));
            }
            else
            {
                _myCharIntance.movementInput = Vector2.zero;
                _myCharIntance.SetActionKeyCode(ActionCodes.Sprint, false);
            }
        }

        [Header("Mouse look properties")]
        Vector2 _velocity;
        [SerializeField] Vector2 _acceleration;
        [SerializeField] float inputLagPeriod;
       // [SerializeField] float _sensitivityScaler = 10000;
        float inputLagTimer;
        private Vector2 lastInput;

        Vector2 GetLookInput() 
        {
            inputLagTimer += Time.deltaTime;

            Vector2 input = new Vector2 (-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));

            if ((Mathf.Approximately(0, input.x) && Mathf.Approximately(0, input.y)) == false || inputLagTimer >= inputLagPeriod) 
            {
                lastInput = input;
                inputLagTimer = 0;
            }

            return lastInput;
        }

        public void AssignCharacterToBeControlledByPlayer(CharacterInstance character)
        {
            _myCharIntance = character;
            _motor = character.GetComponent<CharacterMotor>();
        }
    }
}