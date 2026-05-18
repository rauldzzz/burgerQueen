using UnityEngine;
using System.Collections;

public class PlayerInteraction : MonoBehaviour
{
    public GameObject heldItem = null;
    public string heldItemName = null;

    private Animator handAnimator;

    void Start()
    {
        handAnimator = GetComponentInChildren<Animator>();
    }

    public bool IsHoldingItem() => heldItem != null;

    public void PickUp(GameObject item, string itemName)
    {
        StartCoroutine(PickUpRoutine(item, itemName));
    }

    private IEnumerator PickUpRoutine(GameObject item, string itemName)
    {
        // Play grab animation first
        if (handAnimator != null)
            handAnimator.SetTrigger("Grab");

        // Wait for the animation to finish
        yield return new WaitForSeconds(GetAnimationLength("Hand_Grab"));

        // Then give the item
        heldItem = item;
        heldItemName = Counter.NormalizeItemName(itemName);
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

        if (handAnimator != null)
            handAnimator.SetTrigger("Release");

        return dropped;
    }
}