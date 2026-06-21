using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class GerenciadorDeLigacao : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public static GerenciadorDeLigacao Instancia { get; private set; }

    [Header("Telas Principais")]
    [SerializeField] private GameObject painelChamadaRecebida;
    [SerializeField] private GameObject painelChamadaAtiva;

    [Header("Elementos de UI - Recebida")]
    [SerializeField] private RectTransform botaoDeslizarAtender;
    [SerializeField] private RectTransform trilhoDoDeslize;

    [Header("Elementos de UI - Ativa")]
    [SerializeField] private TextMeshProUGUI textoCronometro;

    [Header("Configurações do Deslize")]
    [Range(0.5f, 1f)]
    [Tooltip("Porcentagem do trilho que o jogador precisa arrastar para aceitar a chamada")]
    [SerializeField] private float porcentagemParaAtender = 0.75f;

    [Header("Vibração Física do Aparelho")]
    [SerializeField] private float intervaloVibracaoCelular = 1.2f;

    private bool chamadaAtiva = false;
    private float tempoDecorrido = 0f;
    private Coroutine rotinaCronometro;
    private Coroutine rotinaVibracaoFisica;
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
    }

    public void InicializarComponentes()
    {
        if (inicializado) return;
        if (botaoDeslizarAtender != null) posicaoInicialBotao = botaoDeslizarAtender.anchoredPosition;
        if (trilhoDoDeslize != null) larguraMaximaTrilho = trilhoDoDeslize.rect.width;
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

        // --- INTEGRAÇÃO COM SCREEN MANAGER ---
        // Aqui garantimos que o Gerenciador de Telas assuma o controle visual
        if (GerenciadorDeTelas.Instancia != null)
        {
            GerenciadorDeTelas.Instancia.AbrirTela("LigaçãoAtendida");
        }
        else
        {
            // Fallback manual caso o sistema de telas falhe
            if (painelChamadaRecebida != null) painelChamadaRecebida.SetActive(false);
            if (painelChamadaAtiva != null) painelChamadaAtiva.SetActive(true);
        }

        Debug.Log("<color=green>[LIGAÇÃO] Chamada Aceita! Iniciando áudio e contador.</color>");

        tempoDecorrido = 0f;
        if (rotinaCronometro != null) StopCoroutine(rotinaCronometro);
        if (gameObject.activeInHierarchy)
        {
            rotinaCronometro = StartCoroutine(RotinaCronometro());
        }
    }

    private IEnumerator RotinaCronometro()
    {
        while (chamadaAtiva)
        {
            tempoDecorrido += Time.deltaTime;
            int minutos = Mathf.FloorToInt(tempoDecorrido / 60f);
            int segundos = Mathf.FloorToInt(tempoDecorrido % 60f);

            if (textoCronometro != null)
            {
                textoCronometro.text = string.Format("{0:00}:{1:00}", minutos, segundos);
            }
            yield return null;
        }
    }

    public void DesligarChamada()
    {
        chamadaAtiva = false;
        if (rotinaCronometro != null) StopCoroutine(rotinaCronometro);
        if (rotinaVibracaoFisica != null) StopCoroutine(rotinaVibracaoFisica);

        gameObject.SetActive(false);
        Debug.Log("<color=red>[LIGAÇÃO] Chamada encerrada.</color>");
    }
}