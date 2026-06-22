using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GerenciadorDeCreditos : MonoBehaviour
{
    [Header("Configurações da Logo")]
    [SerializeField] private RectTransform rectLogo;
    [SerializeField] private float escalaFinalLogo = 1.2f;
    [SerializeField] private float duracaoAumentoLogo = 1.2f;
    [Tooltip("Posição Y na tela para onde a logo deve subir (ex: 350 ou 400)")]
    [SerializeField] private float posicaoYFinalLogo = 350f;
    [SerializeField] private float duracaoSubidaLogo = 2.0f;
    [SerializeField] private float tempoEsperaAposLogo = 0.3f;

    [Header("Configurações do Texto de Créditos")]
    [Tooltip("Arraste o próprio TextMeshProUGUI dos créditos aqui")]
    [SerializeField] private TextMeshProUGUI textoCreditosScroll;
    [Tooltip("Velocidade do scroll. Valores maiores fazem o texto subir mais rápido.")]
    [SerializeField] private float velocidadescroll = 120f;

    [Header("Mensagem Final")]
    [SerializeField] private TextMeshProUGUI textoAgradecimento;
    [SerializeField] private float duracaoFadeAgradecimento = 1.5f;

    private bool scrollAtivo = false;
    private RectTransform rectTextoScroll;
    private float alturaDoTexto;
    private Vector2 posicaoOriginalLogo;

    private void Awake()
    {
        if (rectLogo != null)
        {
            posicaoOriginalLogo = rectLogo.anchoredPosition;
        }
    }

    private void OnEnable()
    {
        IniciarSequenciaDeCreditos();
    }

    private void IniciarSequenciaDeCreditos()
    {
        // 1. Reseta os estados iniciais dos elementos
        if (rectLogo != null)
        {
            rectLogo.localScale = Vector3.zero;
            rectLogo.anchoredPosition = posicaoOriginalLogo;
        }

        if (textoAgradecimento != null) textoAgradecimento.gameObject.SetActive(false);
        if (textoCreditosScroll != null) textoCreditosScroll.gameObject.SetActive(true);
        scrollAtivo = false;

        // 2. Prepara o texto dos créditos exatamente no limite inferior da tela
        if (textoCreditosScroll != null)
        {
            rectTextoScroll = textoCreditosScroll.rectTransform;
            textoCreditosScroll.ForceMeshUpdate();
            alturaDoTexto = textoCreditosScroll.preferredHeight;

            // O topo do texto (Pivot Y: 1) começa exatamente no Y: 0.
            rectTextoScroll.anchoredPosition = new Vector2(rectTextoScroll.anchoredPosition.x, 0f);
        }

        // 3. Sequência sincronizada usando DOTween
        Sequence sequenciaCinema = DOTween.Sequence();

        // PASSO 1: Aumenta a logo no centro
        sequenciaCinema.Append(rectLogo.DOScale(new Vector3(escalaFinalLogo, escalaFinalLogo, 1f), duracaoAumentoLogo).SetEase(Ease.OutBack));

        // PASSO 2: Espera um breve momento
        sequenciaCinema.AppendInterval(tempoEsperaAposLogo);

        // PASSO 3: Sobe a logo para o topo da tela
        sequenciaCinema.Append(rectLogo.DOAnchorPosY(posicaoYFinalLogo, duracaoSubidaLogo).SetEase(Ease.InOutQuad));

        // Ativa o scroll exatamente quando a logo começa a subir
        sequenciaCinema.InsertCallback(duracaoAumentoLogo + tempoEsperaAposLogo, () => {
            scrollAtivo = true;
            Debug.Log("<color=cyan>[CREDITOS] Subida imediata iniciada!</color>");
        });
    }

    private void Update()
    {
        if (!scrollAtivo || rectTextoScroll == null) return;

        // Move o texto dos créditos continuamente para cima
        rectTextoScroll.anchoredPosition += Vector2.up * velocidadescroll * Time.deltaTime;

        // Quando o fundo do texto passar do topo da tela, finaliza
        if (rectTextoScroll.anchoredPosition.y >= (Screen.height + alturaDoTexto))
        {
            scrollAtivo = false;
            textoCreditosScroll.gameObject.SetActive(false);

            if (rectLogo != null)
            {
                rectLogo.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InQuad).OnComplete(MostrarAgradecimentoFinal);
            }
            else
            {
                MostrarAgradecimentoFinal();
            }
        }
    }

    private void MostrarAgradecimentoFinal()
    {
        if (textoAgradecimento == null) return;

        textoAgradecimento.gameObject.SetActive(true);
        textoAgradecimento.alpha = 0f;
        textoAgradecimento.text = "Obrigado por jogar.\nVote GLT.";

        textoAgradecimento.DOFade(1f, duracaoFadeAgradecimento).SetEase(Ease.Linear);
    }
} // <-- Esta última chave fecha a classe principal. Tudo precisa estar antes dela!