using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SistemaDeBoot : MonoBehaviour
{
    [Header("Telas")]
    [SerializeField] private GameObject telaLoading;
    [SerializeField] private GameObject telaHome;

    [Header("HUD")]
    [SerializeField] private NavegacaoCelular navegacao;

    [Header("Elementos do Loading")]
    [SerializeField] private Image barraDeProgresso;
    [SerializeField] private float tempoDeLoading = 3f;
    
    [Header("Efeito de Transição")]
    [SerializeField] private CanvasGroup painelPretoFade; // Um painel 100% preto cobrindo tudo
    [SerializeField] private float velocidadeFade = 0.5f;

    void Start()
    {
        // Garante que o painel preto comece transparente para vermos o loading
        if (painelPretoFade != null) painelPretoFade.alpha = 0f;
        StartCoroutine(RotinaDeLoading());
    }

    private IEnumerator RotinaDeLoading()
    {
        telaLoading.SetActive(true);
        telaHome.SetActive(false);
        barraDeProgresso.fillAmount = 0f;

        // 1. Enche a barra
        float tempo = 0f;
        while (tempo < tempoDeLoading)
        {
            tempo += Time.deltaTime;
            barraDeProgresso.fillAmount = tempo / tempoDeLoading;
            yield return null;
        }
        barraDeProgresso.fillAmount = 1f;

        // --- 2. TOCA O SOM DE PLAY ---
        if (GerenciadorDeAudio.Instancia != null) GerenciadorDeAudio.Instancia.TocarPlay();

        // --- 3. FADE OUT (Escurece a tela toda) ---
        if (painelPretoFade != null)
        {
            tempo = 0f;
            while (tempo < velocidadeFade)
            {
                tempo += Time.deltaTime;
                painelPretoFade.alpha = tempo / velocidadeFade;
                yield return null;
            }
            painelPretoFade.alpha = 1f;
        }

        // Troca as telas no escuro e liga a música!
        telaLoading.SetActive(false);
        telaHome.SetActive(true);
        if (GerenciadorDeAudio.Instancia != null) GerenciadorDeAudio.Instancia.IniciarMusicaFundo();

        // Libera o relógio junto com a Home, ainda no escuro
        if (navegacao != null) navegacao.OnBootCompleto();

        // --- 4. FADE IN (Clareia a tela revelando a Home) ---
        if (painelPretoFade != null)
        {
            tempo = 0f;
            while (tempo < velocidadeFade)
            {
                tempo += Time.deltaTime;
                painelPretoFade.alpha = 1f - (tempo / velocidadeFade);
                yield return null;
            }
            painelPretoFade.alpha = 0f;
        }

        // Inicia as notificações!
        if (GerenciadorDeNarrativa.Instancia != null) GerenciadorDeNarrativa.Instancia.IniciarJogo();
    }
}