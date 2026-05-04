using UnityEngine;

public class CompassNeedleLocal : MonoBehaviour
{
    [SerializeField]
    private Transform playerTransform;

    void Update()
    {
        if (playerTransform == null)
        {
            playerTransform = Camera.main.transform;
        }
        float playerYRotation = playerTransform.eulerAngles.y;

        transform.localRotation = Quaternion.Euler(0, -playerYRotation, 0);
    }
}
