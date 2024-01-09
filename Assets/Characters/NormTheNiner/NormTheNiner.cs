using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormTheNiner : Player
{

    public override void Neutral()
    {
        //we do forward tilt, he doesn't have a different neutral.
        ForwardTilt();
    }

    #region Tilt Attacks
    public override void UpTilt()
    {
        base.UpTilt();
        animator.SetTrigger("upTilt");
    }

    public override void DownTilt()
    {
        base.DownTilt();
        animator.SetTrigger("downTilt");
    }

    public override void ForwardTilt()
    {
        base.ForwardTilt();
        animator.SetTrigger("forwardTilt");
    }

    #endregion

    #region Smash Attacks
    public override void UpSmash()
    {
        base.UpSmash();
    }

    public override void DownSmash()
    {
        base.DownSmash();
    }

    public override void ForwardSmash()
    {
        base.ForwardSmash();
    }
    #endregion

    #region Special Attacks

    public override void NeutralSpecial()
    {
        base.NeutralSpecial();
    }

    public override void UpSpecial()
    {
        base.UpSpecial();
    }

    public override void DownSpecial()
    {
        base.DownSpecial();
    }

    public override void ForwardSpecial()
    {
        base.ForwardSpecial();
    }
    #endregion


    #region Aerials
    public override void NeutralAerial()
    {
        base.NeutralAerial();
    }

    public override void UpAerial()
    {
        base.UpAerial();
    }

    public override void DownAerial()
    {
        base.DownAerial();
    }

    public override void ForwardAerial()
    {
        base.ForwardAerial();
    }

    public override void BackAerial()
    {
        base.BackAerial();
    }
    #endregion
}
