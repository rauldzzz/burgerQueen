using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject heldItem = null;
    public string heldItemName = null;
    public Counter sourceCounter = null;
    public Counter activeCounter = null;

    private Animator handAnimator;

    void Start()
    {
        handAnimator = GetComponentInChildren<Animator>();
    }

    public bool IsHoldingItem() => heldItem != null;

    public bool CanInteractWithCounter(Counter counter)
    {
        return activeCounter == null || activeCounter == counter;
    }

    public bool TryClaimCounter(Counter counter)
    {
        if (counter == null) return false;
        if (activeCounter != null && activeCounter != counter) return false;

        activeCounter = counter;
        return true;
    }

    public void ReleaseCounter(Counter counter)
    {
        if (activeCounter == counter)
        {
            activeCounter = null;
        }
    }

    public void PickUp(GameObject item, string itemName, Counter source = null)
    {
        if (IsHoldingItem()) return;
        StartCoroutine(PickUpRoutine(item, itemName, source));
    }

    private IEnumerator PickUpRoutine(GameObject item, string itemName, Counter source)
    {
        // Play grab animation first
        if (handAnimator != null)
            handAnimator.SetTrigger("Grab");

        // Wait for the animation to finish
        yield return new WaitForSeconds(GetAnimationLength("Hand_Grab"));

        // Then give the item
        heldItem = item;
        heldItemName = Counter.NormalizeItemName(itemName);
        sourceCounter = source;
        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0, 13f, 3f);
    }

    private float GetAnimationLength(string clipName)
    {
        if (handAnimator == null) return 0f;

        foreach (AnimationClip clip in handAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 0.3f; // fallback
    }

    public GameObject Drop()
    {
        if (heldItem == null) return null;

        GameObject dropped = heldItem;
        dropped.transform.SetParent(null);
        heldItem = null;
        heldItemName = null;
        sourceCounter = null;

        if (handAnimator != null)
            handAnimator.SetTrigger("Release");

        return dropped;
    }
}