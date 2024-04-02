using UnityEngine;
using TMPro;

public class VersionManager : MonoBehaviour
{
    [SerializeField]
    private string prefix = "v";
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = prefix + Application.version;
    }
}
