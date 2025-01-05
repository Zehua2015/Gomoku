using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animation_Controller : MonoBehaviour
{
    public GameObject ChangeSidePanel2;
    public GameObject GameOverPanel;
    public Animation ChangeSideAnimation;
    public Animation GameOverAnimation;



    //public CanvasGroup playerPanel;
    //public Image transitionImage;
    public float fadeDuration = 0.5f;
    public float rotateDuration = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        //playerPanel.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void playChangeSideAnimation()
    //{
    //    StartCoroutine(playChangeSideAni());
    //}

    public float playChangeSideAnimation()
    {
        StartCoroutine(playAndDeactiveChangSideAnimation());
        return ChangeSideAnimation.GetClip("ChangeSidePanel").length;
    }

    private IEnumerator playAndDeactiveChangSideAnimation()
    {
        ChangeSidePanel2.SetActive(true);
        ChangeSideAnimation.Play("ChangeSidePanel");

        float aniLength = ChangeSideAnimation.GetClip("ChangeSidePanel").length;
        yield return new WaitForSeconds(aniLength);
        ChangeSidePanel2.SetActive(false);
    }

    public void playGameOverSideAnimation()
    {
        GameOverPanel.SetActive(true);
        //StartCoroutine(playAndDeactiveGameOverAnimation());
        //return GameOverAnimation.GetClip("GameOverPanel").length;
        GameOverAnimation.Play("GameOverPanel");
        return;
    }

    public void DeactiveGameOverPanel()
    {
        GameOverPanel.SetActive(false);
    }
    //private IEnumerator playAndDeactiveGameOverAnimation()
    //{
    //    GameOverPanel.SetActive(true);
    //    GameOverAnimation.Play("GameOverPanel");

    //    float aniLength = GameOverAnimation.GetClip("GameOverPanel").length;
    //    yield return new WaitForSeconds(aniLength);
    //    GameOverPanel.SetActive(false);
    //}

    //public void PlayChangeSidePanel()
    //{
    //    StartCoroutine(PlayChangeSideAnimation());
    //}

    //private IEnumerator PlayChangeSideAnimation()
    //{
    //    yield return StartCoroutine(FadeTo(playerPanel, 0.95f));
    //    //transitionImage.gameObject.SetActive(true);
    //    StartCoroutine(RotateImage(transitionImage.rectTransform));

    //    yield return new WaitForSeconds(2f);
    //    yield return StartCoroutine(FadeTo(transitionImage.gameObject.GetComponent<CanvasGroup>(), 1f));
    //    transitionImage.gameObject.SetActive(false);
    //    yield return StartCoroutine(FadeTo(playerPanel, 1f));
    //}

    //private IEnumerator FadeTo(CanvasGroup transitionCanvas, float targetAlpha)
    //{
    //    float startAlpha = transitionCanvas.alpha;
    //    float elapsed = 0f;
    //    while (elapsed < fadeDuration)
    //    {
    //        elapsed += Time.deltaTime;
    //        transitionCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
    //        yield return null;
    //    }
    //    transitionCanvas.alpha = targetAlpha;
    //}

    //private IEnumerator RotateImage(RectTransform rect)
    //{
    //    float elapsed = 0f;
    //    while (elapsed < rotateDuration)
    //    {
    //        elapsed += Time.deltaTime;
    //        rect.Rotate(new Vector3(0, 0, 360f * Time.deltaTime));
    //        yield return null;
    //    }

    //}
}
