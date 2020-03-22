using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Rewired;
using TMPro;

public class NetworkMenuPlayerController : NetworkBehaviour
{
    public string Name;
    public GameObject PointerPrefab;
    private GameObject Pointer;
    public NetworkTestMenuData Data;
    private FSM<NetworkMenuPlayerController> _abstractPlayerFSM;
    private GameObject _selectedPlayerSpot;

    public override void OnStartLocalPlayer()
    {
        Name = (NetworkManagerBirfia.singleton as NetworkManagerBirfia).Name;
        Pointer = Instantiate(PointerPrefab);
        Pointer.transform.parent = transform;
        Pointer.transform.localPosition = Vector3.zero;
        CmdFetchInfo();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        if (_abstractPlayerFSM != null)
            _abstractPlayerFSM.Update();
    }

    [Command]
    public void CmdConfirm()
    {

    }

    [Command]
    public void CmdFetchInfo()
    {
        GameObject playerslots = GameObject.Find("PlayerSlots");
        print(playerslots.transform.childCount);
        bool[] colliderEnabled = new bool[6];
        bool[] headImageEnabled = new bool[6];
        string[] names = new string[6];
        for (int i = 0; i < playerslots.transform.childCount; i++)
        {
            Transform holder = playerslots.transform.GetChild(i);
            colliderEnabled[i] = holder.GetComponent<BoxCollider>().enabled;
            headImageEnabled[i] = holder.GetChild(1).GetChild(0).gameObject.activeSelf;
            names[i] = holder.GetChild(0).GetComponentInChildren<TextMeshPro>().text;
        }
        TargetFetchInfo(GetComponent<NetworkIdentity>().connectionToClient, colliderEnabled, headImageEnabled, names);
    }

    [TargetRpc]
    public void TargetFetchInfo(NetworkConnection conn, bool[] colliderEnabled, bool[] headImageEnabled, string[] names)
    {
        GameObject playerslots = GameObject.Find("PlayerSlots");
        for (int i = 0; i < playerslots.transform.childCount; i++)
        {
            Transform holder = playerslots.transform.GetChild(i);
            holder.GetComponent<BoxCollider>().enabled = colliderEnabled[i];
            holder.GetChild(1).GetChild(0).gameObject.SetActive(headImageEnabled[i]);
            holder.GetChild(0).GetComponentInChildren<TextMeshPro>().text = names[i];
        }
        _abstractPlayerFSM = new FSM<NetworkMenuPlayerController>(this);
        _abstractPlayerFSM.TransitionTo<PointerState>();
    }

    [Command]
    public void CmdSelectGameObject(GameObject selectedSpot, string Name)
    {
        if (!selectedSpot.GetComponent<BoxCollider>().enabled)
        {
            TargetUnselect(GetComponent<NetworkIdentity>().connectionToClient);
            return;
        }
        selectedSpot.GetComponent<BoxCollider>().enabled = false;
        selectedSpot.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().text = Name;
        selectedSpot.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        selectedSpot.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().color = Color.black;
        selectedSpot.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
        // Tell Game State About Selection
        NetworkMenuGameState.instance.ConfirmSelection(GetComponent<NetworkIdentity>().connectionToClient, selectedSpot.transform.GetSiblingIndex(), true, Name);
        RpcSelectGameObject(selectedSpot, Name);
    }

    [TargetRpc]
    public void TargetUnselect(NetworkConnection connection)
    {
        _abstractPlayerFSM.TransitionTo<PointerState>();
    }

    [ClientRpc]
    public void RpcSelectGameObject(GameObject selectedSpot, string Name)
    {
        selectedSpot.GetComponent<BoxCollider>().enabled = false;
        selectedSpot.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().text = Name;
        selectedSpot.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
        selectedSpot.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().color = Color.black;
        selectedSpot.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
    }

    [Command]
    public void CmdUnselect(GameObject selectedSpot)
    {
        selectedSpot.GetComponent<BoxCollider>().enabled = true;
        selectedSpot.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().text = "";
        selectedSpot.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        NetworkMenuGameState.instance.ConfirmSelection(GetComponent<NetworkIdentity>().connectionToClient, selectedSpot.transform.GetSiblingIndex(), false, "");
        RpcUnselect(selectedSpot);
    }

    [ClientRpc]
    public void RpcUnselect(GameObject selectedSpot)
    {
        selectedSpot.GetComponent<BoxCollider>().enabled = true;
        selectedSpot.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().text = "";
        selectedSpot.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
    }

    private abstract class PlayerState : FSM<NetworkMenuPlayerController>.State
    {
        protected float _VLAxisRaw
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetAxisRaw("Move Vertical");
            }
        }
        protected float _HLAxisRaw
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetAxisRaw("Move Horizontal");
            }
        }
        protected bool _ADown
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetButtonDown("Jump");
            }
        }
        protected bool _BDown
        {
            get
            {
                return ReInput.players.GetPlayer(0).GetButtonDown("Block");
            }
        }
        protected NetworkTestMenuData Data;
        protected bool _vAxisInUse = true;
        protected bool _hAxisInUse = true;
        public override void OnEnter()
        {
            base.OnEnter();
            Data = Context.Data;
            _vAxisInUse = true;
            _hAxisInUse = true;
        }

        public override void Update()
        {
            base.Update();
            if (_VLAxisRaw == 0f) _vAxisInUse = false;
            if (_HLAxisRaw == 0f) _hAxisInUse = false;
        }
    }

    private class PointerState : PlayerState
    {
        public GameObject _lastDetectedSpot;
        public override void OnEnter()
        {
            base.OnEnter();
            Context.Pointer.SetActive(true);
            _lastDetectedSpot = null;
        }

        public override void Update()
        {
            base.Update();
            Vector3 nextPosition = Context.Pointer.transform.position + new Vector3(_HLAxisRaw, -_VLAxisRaw) * Time.deltaTime * Data.CursorMoveSpeed;
            nextPosition.x = Mathf.Clamp(nextPosition.x, -7f, 7f);
            nextPosition.y = Mathf.Clamp(nextPosition.y, -4f, 4f);
            Context.Pointer.transform.position = nextPosition;
            _detectSpot();
        }

        private void _detectSpot()
        {
            RaycastHit hit;
            if (Physics.Raycast(Context.Pointer.transform.position, Vector3.forward, out hit, 50f))
            {
                if (_ADown)
                {
                    Context._selectedPlayerSpot = hit.collider.gameObject;
                    TransitionTo<SelectedState>();
                }
                if (_lastDetectedSpot == hit.collider.gameObject) return;
                // Reset if need to
                if (_lastDetectedSpot != null && _lastDetectedSpot.GetComponent<BoxCollider>().enabled)
                {
                    _lastDetectedSpot.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().text = "";
                    _lastDetectedSpot.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                }
                _lastDetectedSpot = hit.collider.gameObject;
                hit.collider.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().text = Context.Name;
                hit.collider.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                hit.collider.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().color = Data.PlayerHoverSpriteAndNameColor;
                hit.collider.transform.GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = Data.PlayerHoverSpriteAndNameColor;
            }
            else
            {
                // Reset if need to
                if (_lastDetectedSpot != null && _lastDetectedSpot.GetComponent<BoxCollider>().enabled)
                {
                    _lastDetectedSpot.transform.GetChild(0).GetComponentInChildren<TextMeshPro>().text = "";
                    _lastDetectedSpot.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                    _lastDetectedSpot = null;
                }
            }
        }
    }

    private class SelectedState : PlayerState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.Pointer.SetActive(false);
            Context.CmdSelectGameObject(Context._selectedPlayerSpot, Context.Name);
        }

        public override void Update()
        {
            base.Update();
            if (_BDown)
            {
                Context.CmdUnselect(Context._selectedPlayerSpot);
                TransitionTo<PointerState>();
                return;
            }
        }
    }
}
