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
    [SerializeField] private CanvasGroup painelPretoFade; // Um painel 100% preto cobrindo tudo
    [SerializeField] private float velocidadeFade = 0.5f;

    [Header("Mob Sakai UI Particle")]
    [SerializeField] private RectTransform uiParticleRect; // O RectTransform do seu objeto "UIParticle"

    private RectTransform fillRectTransform;

    void Start()
    {
        // Garante que o painel preto comece transparente para vermos o loading
        if (painelPretoFade != null) painelPretoFade.alpha = 0f;

        // Guarda o componente RectTransform da barra para evitar chamadas repetidas de GetComponent
        if (barraDeProgresso != null)
        {
            fillRectTransform = barraDeProgresso.GetComponent<RectTransform>();
        }

        StartCoroutine(RotinaDeLoading());
    }

    private IEnumerator RotinaDeLoading()
    {
        telaLoading.SetActive(true);
        telaHome.SetActive(false);
        barraDeProgresso.fillAmount = 0f;

        // Ativa a emissão de partículas no início do loading
        ToggleParticleEmission(true);

        // 1. Enche a barra e atualiza a posição das partículas frame a frame
        float tempo = 0f;
        while (tempo < tempoDeLoading)
        {
            tempo += Time.deltaTime;
            barraDeProgresso.fillAmount = tempo / tempoDeLoading;

            // Move as partículas junto com o fillAmount atual de forma matemática precisa
            AtualizarPosicaoDasParticulas();

            yield return null;
        }
        barraDeProgresso.fillAmount = 1f;
        AtualizarPosicaoDasParticulas();

        // Desliga as partículas já que o carregamento terminou (evita spawnar no meio do Fade)
        ToggleParticleEmission(false);

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

    private void AtualizarPosicaoDasParticulas()
    {
        if (fillRectTransform == null || uiParticleRect == null) return;

        // 1. Pega a largura total real da barra de loading
        float totalWidth = fillRectTransform.rect.width;

        // 2. Calcula o deslocamento X exato baseado no preenchimento atual (fillAmount)
        float currentX = totalWidth * barraDeProgresso.fillAmount;

        // 3. Descobre a ponta exata da barra no espaço local dela, respeitando seu Pivot
        Vector3 localPointOnBar = new Vector3(currentX - (totalWidth * fillRectTransform.pivot.x), 0, 0);

        // Converte esse ponto local para uma coordenada global "World Space" do Unity
        Vector3 worldPoint = fillRectTransform.TransformPoint(localPointOnBar);

        // 4. Traduz a posição global para o espaço exato do objeto UIParticle
        if (uiParticleRect.parent != null)
        {
            // Se tiver um objeto pai, calcula em relação a ele (evita problemas de escala/posição de nós superiores)
            uiParticleRect.anchoredPosition = uiParticleRect.parent.InverseTransformPoint(worldPoint);
        }
        else
        {
            // Caso esteja na raiz do Canvas
            uiParticleRect.position = worldPoint;
        }
    }

    private void ToggleParticleEmission(bool state)
    {
        if (uiParticleRect == null) return;

        // Procura pelo Particle System dentro do objeto UIParticle (ou nele mesmo)
        ParticleSystem ps = uiParticleRect.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var emission = ps.emission;
            emission.enabled = state;
        }
    }
}