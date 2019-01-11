using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rtHook : MonoBehaviour
{
    public float HookSpeed = 5f;
    public GameObject Hooked = null;

    private GameObject _hook;
    private Vector3 _hookinitlocalPos;
    private Vector3 _hookinitlocalScale;
    private GameObject _hookmax;
    private Vector3 _hookmaxPos = Vector3.zero;
    private HookControl _hc;

    private enum State
    {
        Empty,
        FlyingOut,
        OnTarget,
        FlyingIn,
    }
    private State _hookState;

    private void Start()
    {
        _hook = transform.GetChild(0).gameObject;
        _hc = _hook.GetComponent<HookControl>();
        _hookinitlocalPos = new Vector3(_hook.transform.localPosition.x, _hook.transform.localPosition.y, _hook.transform.localPosition.z);
        _hookinitlocalScale = new Vector3(_hook.transform.localScale.x, _hook.transform.localScale.y, _hook.transform.localScale.z);
        _hookmax = transform.GetChild(1).gameObject;
        _hookState = State.Empty;
    }

    private void Update()
    {
        ConsoleProDebug.Watch("Hook State", _hookState.ToString());
        if (_hookState == State.FlyingOut)
        {
            Vector3 nextpos = (_hookmaxPos - _hook.transform.position).normalized;
            _hook.transform.Translate(nextpos * Time.deltaTime * HookSpeed, Space.World);
            if (Vector3.Distance(_hook.transform.position, _hookmaxPos) <= 0.1f)
            {
                _hookState = State.FlyingIn;
            }
        }
        if (_hookState == State.FlyingIn)
        {
            Vector3 nextpos = (transform.position - _hook.transform.position).normalized;
            _hook.transform.Translate(nextpos * Time.deltaTime * HookSpeed, Space.World);
            if (Hooked != null)
            {
                Hooked.transform.Translate(nextpos * Time.deltaTime * HookSpeed, Space.World);
            }
            if (Vector3.Distance(_hook.transform.position, transform.position) <= 0.1f)
            {
                _hookState = State.Empty;
                _hc.CanHook = false;
                _hook.transform.parent = transform;
                _hook.transform.localScale = _hookinitlocalScale;
                _hook.transform.localEulerAngles = Vector3.zero;
                _hook.transform.localPosition = _hookinitlocalPos;
                // Need to set hooked's rigidbody back
                Hooked = null;
            }
        }
    }

    public void Hook(bool buttondown)
    {
        // If button down
        if (buttondown)
        {
            if (_hookState == State.Empty)
            {
                // Then we could fire the hook
                _hookState = State.FlyingOut;
                // Tell the hook that it can now hook players
                _hc.CanHook = true;
                // Record where the hook should go to in world position
                _hookmaxPos = new Vector3(_hookmax.transform.position.x, _hookmax.transform.position.y, _hookmax.transform.position.z);
                // Also need to make hook out of parent
                _hook.transform.parent = null;
            }
        }
    }

    public void HookOnHit(GameObject hit)
    {
        _hookState = State.OnTarget;
        Hooked = hit;
        StartCoroutine(hookhelper(0.25f));
    }

    IEnumerator hookhelper(float time)
    {
        yield return new WaitForSeconds(time);
        _hookState = State.FlyingIn;
    }
}
