using UnityEngine;

public class GameEntry : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this);
    }
}