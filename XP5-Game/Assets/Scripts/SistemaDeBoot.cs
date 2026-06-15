using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SistemaDeBoot : MonoBehaviour
{
    [Header("Telas")]
    [SerializeField] private GameObject telaLoading;
    [SerializeField] private GameObject telaHome;

    [Header("Elementos do Loading")]
    [SerializeField] private Image barraDeProgresso;
    [SerializeField] private float tempoDeLoading = 3f;
    
    [Header("Efeito de Transição")]
    [SerializeField] private CanvasGroup canvasLoading; // ADICIONE um componente CanvasGroup na sua Tela_Loading!

    void Start()
    {
        StartCoroutine(RotinaDeLoading());
    }

    private IEnumerator RotinaDeLoading()
    {
        telaLoading.SetActive(true);
        telaHome.SetActive(false);
        if (canvasLoading != null) canvasLoading.alpha = 1f;

        float tempoDecorrido = 0f;
        while (tempoDecorrido < tempoDeLoading)
        {
            tempoDecorrido += Time.deltaTime;
            barraDeProgresso.fillAmount = tempoDecorrido / tempoDeLoading;
            yield return null;
        }

        barraDeProgresso.fillAmount = 1f;

        // --- FADE OUT E TROCA ---
        telaHome.SetActive(true); // Liga a home escondida atrás do loading

        if (canvasLoading != null)
        {
            float fadeTime = 0.5f;
            tempoDecorrido = 0f;
            while (tempoDecorrido < fadeTime)
            {
                tempoDecorrido += Time.deltaTime;
                canvasLoading.alpha = 1f - (tempoDecorrido / fadeTime);
                yield return null;
            }
        }

        telaLoading.SetActive(false); // Desliga o loading

        // Manda o Gerenciador disparar a primeira notificação!
        if (GerenciadorDeNarrativa.Instancia != null)
        {
            GerenciadorDeNarrativa.Instancia.IniciarJogo();
        }
    }
}