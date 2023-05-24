﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiFPS.Gameplay
{


    public class ToolMotion : MonoBehaviour
    {
        CharacterInstance _characterInstance;
        [Header("Look motion")]
        [SerializeField] float speed = 6;
        [SerializeField] float multiplier = 0.002f;
        [Header("Position motion")]
        [SerializeField] float heightDevation = 0.06f;
        [SerializeField] float movingForwardBackwarDevation = 0.04f;
        [SerializeField] float strafeDevation = 0.05f;
        [SerializeField] float strafeAngleDevation = 5f;
        [SerializeField] float strafeAngleDevationSpeed = 10f;

        [SerializeField] Vector3 _toolMotionBorders = new Vector3(0.5f,0.5f,0.5f);

        float lastmX;
        float lastmY;

        float _itemRecoil;
        [SerializeField] float _itemStabilizationSpeed = 1f;

        float _fallingTime = 0;

        Vector3 finalMotion;
        Vector3 finalAirPosition;
        bool isFalling = false;

        Coroutine _hittedGroundProcedure;

        Vector3 _currentFPPModelOffset;

        //take damage camera shake properties
        [SerializeField] float hitAnimSpeed = 1f;

        public void Start()
        {
            _characterInstance = transform.root.GetComponent<CharacterInstance>();
            _characterInstance.ToolMotion = this;

            _characterInstance.CharacterItemManager.Client_EquipmentChanged += OnEquipmentChanged;
        }

        void OnEquipmentChanged(int slotID) 
        {
            
        }

        void Update()
        {
            _currentFPPModelOffset = _characterInstance.CharacterItemManager.CurrentlyUsedItem ?
                 _characterInstance.CharacterItemManager.CurrentlyUsedItem.FPPModelOffset :
                 Vector3.zero;

            if (_itemRecoil < 0)
            {
                _itemRecoil += Time.deltaTime * _itemStabilizationSpeed;
            }
            else
                _itemRecoil = 0;

            if (_characterInstance.IsScoping)
            {
                transform.localPosition = _currentFPPModelOffset;
                transform.localRotation = Quaternion.identity;
                return;
            }

            float mX = Mathf.Clamp(_characterInstance.lookInput.x - lastmX, -1f, 1f);
            float mY = Mathf.Clamp(_characterInstance.lookInput.y - lastmY, -1f, 1f);

            lastmX = _characterInstance.lookInput.x;
            lastmY = _characterInstance.lookInput.y;

            float crouching = _characterInstance.ReadActionKeyCode(ActionCodes.Crouch) ? 0.11f : 0f;

            //position
            Vector3 restLocalPos = new Vector3(strafeDevation * _characterInstance.movementInput.x, (_characterInstance.lookInput.x / 90f) * heightDevation, -movingForwardBackwarDevation * _characterInstance.movementInput.y - crouching + _itemRecoil);
            Vector3 motion = new Vector3(finalMotion.x - mY * multiplier, finalMotion.y + mX * multiplier, finalMotion.z);

            finalMotion = Vector3.Lerp(motion, restLocalPos, Time.deltaTime * speed);

            if (!_characterInstance.isGrounded)
            {
                _fallingTime += Time.deltaTime * 0.05f;
                finalAirPosition.y = Mathf.Clamp(-_fallingTime, -0.12f, 0);
                finalAirPosition.z = 0.5f * finalAirPosition.y;
                isFalling = true;
            }
            else if (isFalling)
            {
                isFalling = false;

                if (_hittedGroundProcedure != null)
                    StopCoroutine(_hittedGroundProcedure);
                StartCoroutine(HittedGroundProcedure(Mathf.Clamp(_fallingTime, 0, 0.07f)));

                _fallingTime = 0;
            }

            //constrain toolmotion to specified borders
            if (finalMotion.x > _toolMotionBorders.x) finalMotion.x = _toolMotionBorders.x;
            else if (finalMotion.x < -_toolMotionBorders.x) finalMotion.x = -_toolMotionBorders.x;

            if (finalMotion.y > _toolMotionBorders.y) finalMotion.y = _toolMotionBorders.y;
            else if (finalMotion.y < -_toolMotionBorders.y) finalMotion.y = -_toolMotionBorders.y;

            if (finalMotion.z > _toolMotionBorders.z) finalMotion.z = _toolMotionBorders.z;
            else if (finalMotion.z < -_toolMotionBorders.z) finalMotion.z = -_toolMotionBorders.z;



            transform.localPosition = _currentFPPModelOffset + finalMotion + finalAirPosition;

            //rotation
            Quaternion angleDevation = Quaternion.Euler(0, 0, transform.localEulerAngles.z - (strafeAngleDevation * strafeAngleDevationSpeed * Time.deltaTime * _characterInstance.movementInput.x));
            transform.localRotation = Quaternion.Lerp(angleDevation, Quaternion.identity, strafeAngleDevationSpeed * Time.deltaTime);
        }

        IEnumerator HittedGroundProcedure(float force)
        {
            Vector3 startPos = finalAirPosition;
            Vector3 posUp = new Vector3(0, force * 0.4f, -force * 0.125f);

            float timeNeeded = (posUp.y - finalAirPosition.y) / hitAnimSpeed;
            float timeNeeded2 = (force * 0.5f) / hitAnimSpeed;

            float timer = 0;

            while ((timeNeeded + timeNeeded2) >= timer)
            {
                timer += Time.deltaTime;
                if (timer <= timeNeeded)
                {
                    finalAirPosition = Vector3.Slerp(startPos, posUp, timer / timeNeeded);
                }
                else
                {
                    finalAirPosition = Vector3.Slerp(posUp, Vector3.zero, (timer - timeNeeded) / timeNeeded2);
                }
                yield return null;
            }

            finalAirPosition = Vector3.zero;
        }

        public void Shoot(float recoil)
        {
            if (_characterInstance.FPP)
                _itemRecoil -= recoil;
        }
    }
}