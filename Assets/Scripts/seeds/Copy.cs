using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;

public class Copy : MonoBehaviour
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void passCopyToBrowser(string str);
#endif

    public void CopySeedToClipBoard() {
#if UNITY_WEBGL
        passCopyToBrowser(GetComponent<TextMeshProUGUI>().text);
#endif
        GUIUtility.systemCopyBuffer = GetComponent<TextMeshProUGUI>().text;
    } 
    
}
