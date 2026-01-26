using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI ipText;
    public TextMeshProUGUI maskBitsText;
    public TextMeshProUGUI maskSubnetText;

    private ConnectionManager connManager;
    private PlayerController player;

    void Start()
    {
        connManager = FindObjectOfType<ConnectionManager>();
        player = FindObjectOfType<PlayerController>();
    }

    void Update()
    {
        if (connManager == null || player == null || player.currentNode == null) return;

        ipText.text = $"LOCAL_IP: {player.currentNode.ipAddress}";
        maskBitsText.text = $"MASK_PREFIX: /{connManager.maskBits}";
        
        uint maskUint = connManager.GetMaskUint();
        // 引用位置：ConnectionManager
        maskSubnetText.text = $"SUBNET_MASK: {ConnectionManager.UintToIP(maskUint)}";
    }
}