
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MultiFPS.Gameplay
{
    public class RocketLauncher : Gun
    {

        [SerializeField] GameObject _projectilePrefab;
        [SerializeField] float _projectileRigidbodyForce = 2000f;

        protected override void Use()
        {

            if (CurrentAmmo <= 0 || _isReloading || _doingMelee) return;

            if (isServer)
                SpawnThrowable(_myOwner.lookInput);
            else
            {
                CmdSpawnThrowable(_myOwner.lookInput);
            }

            //*if (isOwned)

            //bots
            if (isServer)
            {
                Server_CurrentAmmo--;
            }
            else //clients
            {
            }

            ChangeCurrentAmmoCount(CurrentAmmo - 1);
            //base item
            if (!_myOwner) return;

            if (isOwned)
            {
                SingleUse(); //for client to for example immediately see muzzleflash when he fires his gun
                CmdSingleUse();
            }
            else if (isServer)
            {
                SingleUse();
                RpcSingleUse();
            }
        }


        protected override void SingleUse()
        {
            base.SingleUse();

            if (!_myOwner) return;

            if (_myOwner.FPP)
            {
                _myOwner.Animator.SetTrigger(AnimationNames.ITEM_FIRE);
            }

            if (_myAnimator.runtimeAnimatorController)
                _myAnimator.SetTrigger(AnimationNames.ITEM_FIRE);

            _audioSource.PlayOneShot(fireClip);
            _particleSystem.Play();
        }

        [Command]
        void CmdSpawnThrowable(Vector2 look)
        {
            if (Server_CurrentAmmo > 0)
            {
                SpawnThrowable(look);
                Server_CurrentAmmo--;
            }
        }
        void SpawnThrowable(Vector2 look)
        {
            GameObject throwable = Instantiate(_projectilePrefab, _myOwner.FPPLook.position, Quaternion.Euler(look.x, look.y, 0));

            Vector3 force = Quaternion.Euler(look.x, look.y, 0) * Vector3.forward * _projectileRigidbodyForce;

            throwable.GetComponent<Throwable>().Activate(_myOwner, force);
            NetworkServer.Spawn(throwable);
        }
    }
}