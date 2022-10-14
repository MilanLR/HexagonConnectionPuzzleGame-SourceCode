using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fades : MonoBehaviour
{
    private Image CopiedText;
    private bool invoked = false;

    void Start() {
        CopiedText = GetComponent<Image>();
    }

    public void FadeInAndOut() {
        if (!invoked) {
            StartCoroutine(ShowCopySign(0.2f, 0.8f, 0.3f));
        }
    }

    IEnumerator ShowCopySign(float FadeInTime, float StayTime, float FadeOutTime)
    {
        invoked = true;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / FadeInTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(0.0f, 1.0f, t));
            CopiedText.color = newColor;

            yield return null;
        }

        yield return new WaitForSecondsRealtime(StayTime);
        
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / FadeInTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(1.0f, 0.0f, t));
            CopiedText.color = newColor;
            yield return null;
        }
        CopiedText.color = new Color(1, 1, 1, 0);
        invoked = false;
    }

}
