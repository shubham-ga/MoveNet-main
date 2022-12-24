using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    internal CharacterMove characterMove;
    public static GameManager gameManager;
    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = this;
        }
    }
}
