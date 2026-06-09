using UnityEngine;

[CreateAssetMenu(fileName = "TrashData", menuName = "Inventory/TrashData")]
public class TrashData : ItemData
{
    [SerializeField] private GameObject dropPrefab;

    public GameObject DropPrefab => dropPrefab;
}
