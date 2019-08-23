using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum AnimationController_Script_AnimationId
{
    None,
    Born,
    Idle,
    Moving,
    Attack_1,
    Attack_2,
    Attack_3,
    Attack_4,
    Attack_5,
    Destroy,
    Platform
}

public class AnimationController_Script : MonoBehaviour
{
    public AnimationController_Script_AnimationId Default_AnimationId = AnimationController_Script_AnimationId.None;

    public NotifyEvent_2P<AnimationController_Script, AnimationController_Script_AnimationId> OnStartAnimation = new NotifyEvent_2P<AnimationController_Script, AnimationController_Script_AnimationId>();
    public NotifyEvent_2P<AnimationController_Script, AnimationController_Script_AnimationId> OnEndAnimation = new NotifyEvent_2P<AnimationController_Script, AnimationController_Script_AnimationId>();
    public NotifyEvent_2P<AnimationController_Script, AnimationController_Script_AnimationId> OnCancelAnimation = new NotifyEvent_2P<AnimationController_Script, AnimationController_Script_AnimationId>();
    public NotifyEvent<AnimationController_Script> OnStopPlaying = new NotifyEvent<AnimationController_Script>();

    private bool fLooped = false;
    private AnimationController_Script_AnimationId fAnimation_Id = AnimationController_Script_AnimationId.None;
    private Animator fAnimator = null;

    private static Dictionary<AnimationController_Script_AnimationId, string> fAnimationNames = null;


    public void StopPlaying()
    {
        if (fAnimation_Id != AnimationController_Script_AnimationId.None)
        {
            OnCancelAnimation.Invoke(this, fAnimation_Id);
        }

        fAnimation_Id = AnimationController_Script_AnimationId.None;
        OnStopPlaying.Invoke(this);
    }


    public Animator Animator
    {
        get
        {
            if (fAnimator == null)
                fAnimator = this.GetComponent<Animator>();

            return fAnimator;
        }

        set
        {
            fAnimator = value;
        }
    }
    
    public AnimationController_Script_AnimationId AnimationId
    {
        get
        {
            return fAnimation_Id;
        }
    }

    public void SetAnimationId(AnimationController_Script_AnimationId anim_id, bool loop)
    {
        if ((fAnimation_Id == anim_id) ||
            (Animator == null))
            return;

        if (fAnimation_Id != AnimationController_Script_AnimationId.None)
        {
            OnCancelAnimation.Invoke(this, fAnimation_Id);
        }

        fAnimation_Id = anim_id;

        string anim_str = AnimationIdToString(anim_id);
        Animator.Play(anim_str);
        fLooped = loop;
    }

    public static string AnimationIdToString(AnimationController_Script_AnimationId value)
    {
        return fAnimationNames[value];
    }

    public void Do_AnimationStart(AnimationController_Script_AnimationId anim_id)
    {
        OnStartAnimation.Invoke(this, anim_id);
    }

    public void Do_AnimationEnd(AnimationController_Script_AnimationId anim_id)
    {
        OnEndAnimation.Invoke(this, anim_id);

        if (!fLooped)
            StopPlaying();
    }



    private void Awake()
    {
        if (fAnimationNames == null)
        {
            fAnimationNames = new Dictionary<AnimationController_Script_AnimationId, string>();
            fAnimationNames.Add(AnimationController_Script_AnimationId.None, "none");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Born, "born");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Idle, "idle");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Moving, "moving");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Attack_1, "attack_1");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Attack_2, "attack_2");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Attack_3, "attack_3");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Attack_4, "attack_4");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Attack_5, "attack_5");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Destroy, "destroy");
            fAnimationNames.Add(AnimationController_Script_AnimationId.Platform, "platform");
        }
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
