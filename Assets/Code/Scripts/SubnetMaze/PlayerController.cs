using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public NetworkNode currentNode;
    public float moveSpeed = 5f;
    private ConnectionManager connManager;
    private Vector3 targetMovePosition;
    private bool isMoving = false;

    void Start()
    {
        connManager = FindObjectOfType<ConnectionManager>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleClick();

        if (isMoving)
        {
            transform.position = Vector3.Lerp(transform.position, targetMovePosition, Time.deltaTime * moveSpeed);
            if (Vector3.Distance(transform.position, targetMovePosition) < 0.05f) isMoving = false;
        }
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            NetworkNode targetNode = hit.collider.GetComponent<NetworkNode>();
            if (targetNode != null && targetNode != currentNode)
            {
                // 调用管理器的 CanConnect
                if (connManager.CanConnect(currentNode.ipUint, targetNode.ipUint))
                {
                    MoveToNode(targetNode);
                }
            }
        }
    }

    void MoveToNode(NetworkNode node)
    {
        currentNode = node;
        targetMovePosition = node.transform.position + Vector3.up * 1.5f;
        isMoving = true;
    }
}