using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ObscuringItemFader : MonoBehaviour
{
    private SpriteRenderer SpriteRenderer;

    private void Awake()
    {
        SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void Fadeout()
    {
        StartCoroutine(FadeOutRoutine());
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        float currentAlpha = SpriteRenderer.color.a;
        float distance = 1f - currentAlpha;
        while(1f- currentAlpha > 0.01)
        {
            currentAlpha= currentAlpha+ distance / Settings.fadeInSeconds * Time.deltaTime;
            SpriteRenderer.color = new Color(1f, 1f, 1f, currentAlpha);
            yield return null;
        }
        SpriteRenderer.color=new Color(1f,1f,1f,1f);
    }
    public IEnumerator FadeOutRoutine()
    {
        float currentAlpha = SpriteRenderer.color.a;
        float distance=currentAlpha-Settings.tagetAlpha;

        while(currentAlpha-Settings.tagetAlpha > 0.01f)
        {
            currentAlpha=currentAlpha-distance/Settings.fadeOutSeconds*Time.deltaTime;
            SpriteRenderer.color=new Color(1f,1f,1f,currentAlpha);
            yield return null;  
        }

        SpriteRenderer.color=new Color(1f,1f,1f,Settings.tagetAlpha);
    }
}
