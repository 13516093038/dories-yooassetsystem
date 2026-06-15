using Dories.YooAssetSystem.Runtime.Patch;
using UnityEngine;

public class PatchTest : MonoBehaviour
{
   [SerializeField] private PatchEntity patchEntity;

    void Start()
    {
        patchEntity.BuildNeedUpdateListener((downlaoder) =>
            {
                Debug.Log("Need update");
            })
            .BuildPatchCompleteListener(() =>
            {
                Debug.Log("Patch complete");
            })
        .BuildPatchFailedListener((error) =>
            {
                Debug.Log("Patch failed: " + error);
            })
        .BuildPatchErrorListener((error) =>
            {
                Debug.Log("Patch error: " + error);
            })
        .StartPatch();
    }
}
