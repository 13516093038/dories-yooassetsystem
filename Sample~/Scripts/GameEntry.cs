using UnityEngine;

namespace Dories.YooAssetSystem.Sample
{
    public class GameEntry : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}