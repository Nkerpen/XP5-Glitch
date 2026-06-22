using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class GlitchManager : MonoBehaviour
{
    [SerializeField] private GameObject canvasGlitch;
    [SerializeField] private RawImage rawImageGlitch;
    [SerializeField] private RenderTexture renderTextureGlitch;

    private Material glitchMaterial;
    private readonly string propriedadeIntensidade = "_GlitchIntensity";

    void Awake()
    {
        if (rawImageGlitch != null && rawImageGlitch.material != null)
        {
            glitchMaterial = Instantiate(rawImageGlitch.material);
            rawImageGlitch.material = glitchMaterial;
        }
        DesativarCanvasGlitch();
    }

    public void IniciarGlitch(float intensidade)
    {
        StartCoroutine(CapturarEIniciar(intensidade));
    }

    private IEnumerator CapturarEIniciar(float intensidade)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        // Flip vertical para corrigir inversão do OpenG
        Graphics.Blit(screenshot, renderTextureGlitch, new Vector2(1, 1), new Vector2(0, 0));
        Destroy(screenshot);

        rawImageGlitch.texture = renderTextureGlitch;
        if (canvasGlitch != null) canvasGlitch.SetActive(true);
        if (glitchMaterial != null)
            glitchMaterial.SetFloat(propriedadeIntensidade, intensidade);
    }

    public void PararGlitch()
    {
        if (glitchMaterial != null)
            glitchMaterial.SetFloat(propriedadeIntensidade, 0f);
        DesativarCanvasGlitch();
    }

    private void DesativarCanvasGlitch()
    {
        if (canvasGlitch != null) canvasGlitch.SetActive(false);
    }

    public void GlitchRapido(float intensidade, float duracao)
    {
        StartCoroutine(RotinaGlitchRapido(intensidade, duracao));
    }

    private IEnumerator RotinaGlitchRapido(float intensidade, float duracao)
    {
        yield return StartCoroutine(CapturarEIniciar(intensidade));
        yield return new WaitForSeconds(duracao);
        PararGlitch();
    }
}