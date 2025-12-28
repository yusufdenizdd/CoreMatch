using UnityEngine;

public class TopBarController : MonoBehaviour
{
    [SerializeField] private GameObject topBarPanel;

    private bool isOpen = false;

    public void ToggleTopBar()
    {
        isOpen = !isOpen;
        topBarPanel.SetActive(isOpen);
    }
}
