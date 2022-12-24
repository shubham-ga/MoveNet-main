using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    // ladder end
    public void AfterLadderAnimation()
    {
        GameManager.gameManager.characterMove.isGravity = true;
        GameManager.gameManager.characterMove.isStart = true;
        GameManager.gameManager.characterMove.isUp = false;
        GameManager.gameManager.characterMove.playerAnimator.SetBool("isIdle", true);
    }
}
