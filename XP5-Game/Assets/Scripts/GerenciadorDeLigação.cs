using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GerenciadorDeLigacao : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public static GerenciadorDeLigacao Instancia { get; private set; }

    [System.Serializable]
    public struct LinhaLegenda
    {
        public float tempoDeInicio;
        public string textoDaLegenda;

        public LinhaLegenda(float tempo, string texto)
        {
            tempoDeInicio = tempo;
            textoDaLegenda = texto;
        }
    }

    [Header("Telas Principais")]
    [SerializeField] private GameObject painelChamadaRecebida;
    [SerializeField] private GameObject painelChamadaAtiva;

    [Header("Elementos de UI - Recebida")]
    [SerializeField] private RectTransform botaoDeslizarAtender;
    [SerializeField] private RectTransform trilhoDoDeslize;

    [Header("Elementos de UI - Ativa")]
    [SerializeField] private TextMeshProUGUI textoCronometro;

    [Tooltip("Arraste o seu quadrado/texto de subtítulo aqui")]
    [SerializeField] private TextMeshProUGUI textoLegenda;

    [Header("Efeito de Fade Out (Fim do Jogo)")]
    [Tooltip("Arraste uma imagem preta que cubra a tela toda (ela precisa ter o componente CanvasGroup)")]
    [SerializeField] private CanvasGroup imagemFadePreto;
    [Tooltip("Duração em segundos do Fade Out visual e do áudio")]
    [SerializeField] private float duracaoFadeOut = 3.0f;

    [Header("Configurações de Áudio")]
    [SerializeField] private AudioSource audioSourceAmeaca;

    private List<LinhaLegenda> legendasDaAmeaca = new List<LinhaLegenda>();

    [Header("Configurações do Deslize")]
    [Range(0.5f, 1f)]
    [Tooltip("Porcentagem do trilho que o jogador precisa arrastar para aceitar a chamada")]
    [SerializeField] private float porcentagemParaAtender = 0.75f;

    [Header("Vibração Física do Aparelho")]
    [SerializeField] private float intervaloVibracaoCelular = 1.2f;

    private bool chamadaAtiva = false;
    private float tempoDecorrido = 0f;
    private float volumeOriginalAudio = 1f;
    private Coroutine rotinaCronometro;
    private Coroutine rotinaVibracaoFisica;
    private Coroutine rotinaLegendas;
    private Vector2 posicaoInicialBotao;
    private float larguraMaximaTrilho;
    private bool inicializado = false;

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            InicializarComponentes();
        }
        else if (Instancia != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InicializarComponentes();
        MapearLegendasAutomaticamente();
    }

    private void MapearLegendasAutomaticamente()
    {
        legendasDaAmeaca.Clear();
        legendasDaAmeaca.Add(new LinhaLegenda(0.000f, "*chiado*"));
        legendasDaAmeaca.Add(new LinhaLegenda(4.490f, ""));
        legendasDaAmeaca.Add(new LinhaLegenda(4.580f, "Escute com atenção."));
        legendasDaAmeaca.Add(new LinhaLegenda(6.531f, ""));
        legendasDaAmeaca.Add(new LinhaLegenda(6.608f, "Vou dizer isso uma única vez."));
        legendasDaAmeaca.Add(new LinhaLegenda(9.069f, ""));
        legendasDaAmeaca.Add(new LinhaLegenda(10.078f, "Pare de procurar o John."));
        legendasDaAmeaca.Add(new LinhaLegenda(11.684f, "Uma vez que lida com algo..."));
        legendasDaAmeaca.Add(new LinhaLegenda(13.624f, "... muito além da sua capacidade."));
        legendasDaAmeaca.Add(new LinhaLegenda(15.889f, ""));
        legendasDaAmeaca.Add(new LinhaLegenda(16.431f, "Se insistir..."));
        legendasDaAmeaca.Add(new LinhaLegenda(17.400f, ""));
        legendasDaAmeaca.Add(new LinhaLegenda(17.656f, "... acabará como ele."));
        legendasDaAmeaca.Add(new LinhaLegenda(19.340f, "Desligue o celular..."));
        legendasDaAmeaca.Add(new LinhaLegenda(20.382f, "... e finja que essa conversa..."));
        legendasDaAmeaca.Add(new LinhaLegenda(21.601f, "... nunca aconteceu."));
        legendasDaAmeaca.Add(new LinhaLegenda(22.985f, ""));
    }

    public void InicializarComponentes()
    {
        if (inicializado) return;
        if (botaoDeslizarAtender != null) posicaoInicialBotao = botaoDeslizarAtender.anchoredPosition;
        if (trilhoDoDeslize != null) larguraMaximaTrilho = trilhoDoDeslize.rect.width;
        if (textoLegenda != null) textoLegenda.text = "";

        if (imagemFadePreto != null)
        {
            imagemFadePreto.alpha = 0f;
            imagemFadePreto.blocksRaycasts = false;
        }

        if (audioSourceAmeaca != null) volumeOriginalAudio = audioSourceAmeaca.volume;

        inicializado = true;
    }

    public void DispararChamadaRecebida(float delay)
    {
        gameObject.SetActive(true);
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(RotinaAguardarParaLigar(delay));
        }
    }

    public IEnumerator RotinaAguardarParaLigar(float delay)
    {
        yield return new WaitForSeconds(delay);
        InicializarComponentes();

        if (painelChamadaRecebida != null) painelChamadaRecebida.SetActive(true);
        if (painelChamadaAtiva != null) painelChamadaAtiva.SetActive(false);
        chamadaAtiva = false;

        if (botaoDeslizarAtender != null) botaoDeslizarAtender.anchoredPosition = posicaoInicialBotao;

        if (rotinaVibracaoFisica != null) StopCoroutine(rotinaVibracaoFisica);
        if (gameObject.activeInHierarchy)
        {
            rotinaVibracaoFisica = StartCoroutine(RotinaVibrarAparelho());
        }

        Debug.Log("<color=cyan>[LIGAÇÃO] Chamada recebida tocando e CELULAR VIBRANDO...</color>");
    }

    private IEnumerator RotinaVibrarAparelho()
    {
        while (!chamadaAtiva)
        {
#if !UNITY_EDITOR
            Handheld.Vibrate();
#endif
            yield return new WaitForSeconds(intervaloVibracaoCelular);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (chamadaAtiva || botaoDeslizarAtender == null || trilhoDoDeslize == null) return;

        larguraMaximaTrilho = trilhoDoDeslize.rect.width;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(trilhoDoDeslize, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        float novoX = Mathf.Clamp(localPoint.x, posicaoInicialBotao.x, larguraMaximaTrilho);
        botaoDeslizarAtender.anchoredPosition = new Vector2(novoX, botaoDeslizarAtender.anchoredPosition.y);

        float limiteAtender = larguraMaximaTrilho * porcentagemParaAtender;
        if (novoX >= limiteAtender)
        {
            AtenderChamada();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (chamadaAtiva) return;
        botaoDeslizarAtender.DOAnchorPos(posicaoInicialBotao, 0.3f).SetEase(Ease.OutCubic);
    }

    private void AtenderChamada()
    {
        chamadaAtiva = true;

        if (rotinaVibracaoFisica != null) StopCoroutine(rotinaVibracaoFisica);

        if (GerenciadorDeTelas.Instancia != null)
        {
            GerenciadorDeTelas.Instancia.AbrirTela("LigaçãoAtendida");
        }
        else
        {
            if (painelChamadaRecebida != null) painelChamadaRecebida.SetActive(false);
            if (painelChamadaAtiva != null) painelChamadaAtiva.SetActive(true);
        }

        Debug.Log("<color=green>[LIGAÇÃO] Chamada Aceita! Iniciando áudio, legendas e contador.</color>");

        tempoDecorrido = 0f;
        if (rotinaCronometro != null) StopCoroutine(rotinaCronometro);
        if (rotinaLegendas != null) StopCoroutine(rotinaLegendas);

        if (gameObject.activeInHierarchy)
        {
            rotinaCronometro = StartCoroutine(RotinaCronometro());
            rotinaLegendas = StartCoroutine(RotinaSincronizarLegendas());
        }
    }

    private IEnumerator RotinaCronometro()
    {
        while (chamadaAtiva)
        {
            tempoDecorrido += Time.deltaTime;
            int minutes = Mathf.FloorToInt(tempoDecorrido / 60f);
            int seconds = Mathf.FloorToInt(tempoDecorrido % 60f);

            if (textoCronometro != null)
            {
                textoCronometro.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            yield return null;
        }
    }

    private IEnumerator RotinaSincronizarLegendas()
    {
        if (audioSourceAmeaca == null)
        {
            Debug.LogError("[LIGAÇÃO] AudioSource da ameaça não foi atribuído!");
            yield break;
        }

        audioSourceAmeaca.volume = volumeOriginalAudio;
        audioSourceAmeaca.Play();
        int indiceLegendaAtual = 0;

        while (audioSourceAmeaca.isPlaying)
        {
            float tempoAtualAudio = audioSourceAmeaca.time;

            if (indiceLegendaAtual < legendasDaAmeaca.Count && tempoAtualAudio >= legendasDaAmeaca[indiceLegendaAtual].tempoDeInicio)
            {
                if (textoLegenda != null)
                {
                    textoLegenda.text = legendasDaAmeaca[indiceLegendaAtual].textoDaLegenda;
                }
                indiceLegendaAtual++;
            }

            yield return null;
        }

        if (textoLegenda != null) textoLegenda.text = "";

        Debug.Log("<color=orange>[LIGAÇÃO] Diálogo encerrado. Iniciando transição de Fade Out...</color>");
        StartCoroutine(RotinaExecutarFadeOutEFinishing());
    }

    private IEnumerator RotinaExecutarFadeOutEFinishing()
    {
        chamadaAtiva = false;
        if (rotinaCronometro != null) StopCoroutine(rotinaCronometro);

        // --- INTEGRAÇÃO COM GERENCIADOR DE ÁUDIO ---
        // Comanda a ambientação para apagar aos poucos usando o mesmo tempo do fade visual
        if (GerenciadorDeAudio.Instancia != null)
        {
            GerenciadorDeAudio.Instancia.PararMusicaFundoComFade(duracaoFadeOut);
        }

        if (imagemFadePreto != null)
        {
            imagemFadePreto.blocksRaycasts = true;
            imagemFadePreto.DOFade(1f, duracaoFadeOut).SetEase(Ease.InOutQuad);
        }

        if (audioSourceAmeaca != null)
        {
            audioSourceAmeaca.DOFade(0f, duracaoFadeOut).SetEase(Ease.InOutQuad);
        }

        yield return new WaitForSeconds(duracaoFadeOut);

        Debug.Log("<color=gold>[FIM] Fade Out completo de tela e música! Abrindo créditos...</color>");
        ChamarCreditosDoJogo();
    }

    private void ChamarCreditosDoJogo()
    {
        if (GerenciadorDeTelas.Instancia != null)
        {
            GerenciadorDeTelas.Instancia.AbrirTela("Creditos");
        }

        gameObject.SetActive(false);
    }

    public void DesligarChamada()
    {
        chamadaAtiva = false;
        if (rotinaCronometro != null) StopCoroutine(rotinaCronometro);
        if (rotinaVibracaoFisica != null) StopCoroutine(rotinaVibracaoFisica);
        if (rotinaLegendas != null) StopCoroutine(rotinaLegendas);

        if (audioSourceAmeaca != null) audioSourceAmeaca.Stop();

        gameObject.SetActive(false);
        Debug.Log("<color=red>[LIGAÇÃO] Chamada encerrada.</color>");
    }
}