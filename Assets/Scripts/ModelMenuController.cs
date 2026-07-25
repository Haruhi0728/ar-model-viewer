using UnityEngine;

public class ModelMenuController : MonoBehaviour
{
    [SerializeField] AutoPlaceOnPlane placer;
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject[] modelPrefabs;

    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    public void SelectModel(int index)
    {
        if (index < 0 || index >= modelPrefabs.Length || modelPrefabs[index] == null)
        {
            return;
        }

        placer.SetModelPrefab(modelPrefabs[index]);
        menuPanel.SetActive(false);
    }
}
