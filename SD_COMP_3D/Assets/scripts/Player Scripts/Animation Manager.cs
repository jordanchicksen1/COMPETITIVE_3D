using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    // Set this up in the Inspector to match your Animator's bool parameter names.
    // Expected order: 0 = Run, 1 = HoldRun, 2 = Idle, 3 = Throw, 4 = Jump
    [SerializeField]
    private List<string> animationBools;

    [SerializeField]
    private float jumpWaitTime = 0.5f;
    [SerializeField]
    private float throwWaitTime = 0.5f;

    // True while a one-shot action animation (jump/throw) is mid-play,
    // so movement animations (Run/HoldRun/Idle) don't override it early.
    public bool IsBusy { get; private set; }

    private void ResetAllBools()
    {
        for (int i = 0; i < animationBools.Count; i++)
        {
            animator.SetBool(animationBools[i], false);
        }
    }

    public void PlayRun()
    {
        if (IsBusy) return;
        ResetAllBools();
        animator.SetBool(animationBools[0], true);
    }

    public void PlayHoldRun()
    {
        if (IsBusy) return;
        ResetAllBools();
        animator.SetBool(animationBools[1], true);
    }

    public void PlayIdle()
    {
        if (IsBusy) return;
        ResetAllBools();
        animator.SetBool(animationBools[2], true);
    }

    public void PlayThrow()
    {
        StopAllCoroutines();
        StartCoroutine(PlayThrowAnimation());
    }

    public void PlayJump()
    {
        StopAllCoroutines();
        StartCoroutine(PlayJumpAnimation());
    }

    private IEnumerator PlayThrowAnimation()
    {
        IsBusy = true;
        ResetAllBools();
        animator.SetBool(animationBools[3], true);

        yield return new WaitForSeconds(throwWaitTime);

        animator.SetBool(animationBools[3], false);
        IsBusy = false;
        // Movement state (Idle/Run/HoldRun) picks back up automatically next
        // frame via PlayerController3D's Update loop once IsBusy is false.
    }

    private IEnumerator PlayJumpAnimation()
    {
        IsBusy = true;
        ResetAllBools();
        animator.SetBool(animationBools[4], true);

        yield return new WaitForSeconds(jumpWaitTime);

        animator.SetBool(animationBools[4], false);
        IsBusy = false;
    }
}