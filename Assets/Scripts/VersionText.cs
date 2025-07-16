using TMPro;
using UnityEngine;
public class VersionText : MonoBehaviour
{
    void Start()
    {
        GetComponent<TMP_Text>().text = $"Ver. {Application.version}";
    }
}
