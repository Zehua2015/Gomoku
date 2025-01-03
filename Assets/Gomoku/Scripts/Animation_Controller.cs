using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animation_Controller : MonoBehaviour
{
    public CanvasGroup transitionCanvas;
    public Image transitionImage;
    public float fadeDuration = 0.5f;
    public float rotateDuration = 2f;

    // Start is called before the first frame update
    void Start()
    {
        transitionCanvas.alpha = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayChangeSidePanel()
    {
        StartCoroutine(PlayChangeSideAnimation());
    }

    private IEnumerator PlayChangeSideAnimation()
    {
        yield return StartCoroutine(FadeTo(0.5f));
        transitionImage.gameObject.SetActive(true);
        StartCoroutine(RotateImage(transitionImage.rectTransform));

        yield return new WaitForSeconds(1f);
        transitionImage.gameObject.SetActive(false);
        yield return StartCoroutine(FadeTo(1f));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = transitionCanvas.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            transitionCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        transitionCanvas.alpha = targetAlpha;
    }

    private IEnumerator RotateImage(RectTransform rect)
    {
        float elapsed = 0f;
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            rect.Rotate(new Vector3(0, 0, 180f * Time.deltaTime));
            yield return null;
        }

    }
}
