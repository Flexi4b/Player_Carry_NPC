using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Carry_NPC : MonoBehaviour
{
    public bool ReadyForTeleport;

    [SerializeField] private GameObject _PickedUp_NPC;
    private NPC_Behavior _NPC;
    private AiLivesTracker _aiLives;
    private LockedUpNPC _lockedUpNPC;
    private Cages _cages;
    private CageBehavior _cageBehavior;
    private PlayerInput _playerInput;
    private InputAction _carry;
    private MeshRenderer _meshRendererNPC;
    private CapsuleCollider _capsuleColliderNPC;
    private bool _uppies;
    private float _isCarrying;

    private GameObject _skin;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    void Update()
    {
        //get the value from the player input
        _isCarrying = _carry.ReadValue<float>();

        if (_uppies)
        {
            _PickedUp_NPC.SetActive(true);
        }
        else
        {
            _PickedUp_NPC.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //when player makes contact with a NPC and has pressed the carry button
        if (collision.gameObject.CompareTag("NPC") && _isCarrying == 1)
        {
            //get the NPC_Behavior script from collision
            _NPC = collision.gameObject.GetComponent<NPC_Behavior>();
            _NPC.State = NPC_Behavior.StateOfNPC.BeingCarried;

            //get the AiLivesTracker script from collision
            _aiLives = collision.gameObject.GetComponent<AiLivesTracker>();
            _aiLives.IsCaged(true);

            //Look for the tag "Skin" in the collisiond gameobject
            _skin = FindChildWithTag(collision.gameObject, "Skin");
            _skin.SetActive(false);

            _aiLives.IsCaged(false);

            _uppies = true;

            ReadyForTeleport = true;

            //get the CapsuleCollider script from collision
            _capsuleColliderNPC = collision.gameObject.GetComponent<CapsuleCollider>();
            _capsuleColliderNPC.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EmptyCage"))
        {
            if (_uppies)
            {
                //get the CageBehavior script from collision
                _cageBehavior = other.gameObject.GetComponent<CageBehavior>();
                _cageBehavior.SetFreeProgress = 0;
                _cageBehavior.CageIsOpen = true;

                if (_cageBehavior.CageIsOpen)
                {
                    _cageBehavior.LockedSkin = _skin;

                    //get the LockedUpNPC script from collision
                    _lockedUpNPC = other.gameObject.GetComponent<LockedUpNPC>();
                    _lockedUpNPC.LockedNPC.SetActive(true);

                    //get the Cages script from collision
                    _cages = other.gameObject.GetComponentInParent<Cages>();
                    _cages._LockedCages.Add(other.gameObject);

                    _cageBehavior.CageIsOpen = false;
                }
            }

            if (ReadyForTeleport)
            {
                //Change the state of the captured NPC so it moves to the correct cage
                _NPC.State = NPC_Behavior.StateOfNPC.MoveToCage;
                _NPC.SetCageTarget(other.gameObject);
            }
            
            _uppies = false;
            ReadyForTeleport = false;
        }
    }

    private void OnEnable()
    {
        _carry = _playerInput.Player.Carry;
        _carry.Enable();
    }

    /// <summary>
    /// Finds the gameobject with a tag in a parent gameobject
    /// </summary>
    public GameObject FindChildWithTag(GameObject parent, string tag)
    {
        GameObject child = null;

        foreach (Transform transform in parent.transform) 
        {
            if (transform.CompareTag(tag))
            {
                child = transform.gameObject;
                break;
            }
        }

        return child;
    }
}
