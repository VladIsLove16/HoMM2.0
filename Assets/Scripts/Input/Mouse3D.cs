using UnityEngine;

public class Mouse3D : MonoBehaviour {

    public static Mouse3D Instance { get; private set; }

    [SerializeField] private LayerMask mouseColliderLayerMask = new LayerMask();
    [SerializeField] private Transform mouseTransform;
    private Ray currenRay;
    private void Awake() {
        Instance = this;
    }
    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        currenRay = ray;
    }
    public static bool GetMouseWorldPosition(out Vector3 position) {
        if (Instance == null) {
            Debug.LogError("Mouse3D Object does not exist!");
        }
        return Instance.GetMouseWorldPosition_Instance(out position);
    }

    private bool GetMouseWorldPosition_Instance(out Vector3 position) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask)) {
            position = raycastHit.point;
            return true;
        } 
        else
        {
            position = Vector3.zero;
            return false;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color= Color.yellow;
        Gizmos.DrawRay(currenRay.origin,currenRay.direction*100f);
    }
}
