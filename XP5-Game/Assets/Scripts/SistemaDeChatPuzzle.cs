using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening; // Importante para as animações funcionarem

public class SistemaDeChatPuzzle : MonoBehaviour
{
    [Header("Estrutura da UI")]
    [SerializeField] private Transform contentArea;
    [SerializeField] private GameObject prefabBalaoNPC;
    [SerializeField] private GameObject prefabBalaoJogador;
    [SerializeField] private ScrollRect scrollDoChat;
    private RectTransform scrollRectTransform; // Para controlar o tamanho do chat dinamicamente

    [Header("Painel de Escolhas")]
    [SerializeField] private RectTransform painelEscolhas; // Tipo alterado para RectTransform para o DOTween
    [SerializeField] private Button[] botoesDeEscolha;
    [SerializeField] private TextMeshProUGUI[] textosDosBotoes;

    [Header("Configurações de Animação (DOTween)")]
    [SerializeField] private Vector2 posicaoEscondido = new Vector2(0, -500);
    [SerializeField] private Vector2 posicaoVisivel = new Vector2(0, 120); // Ajustado para sua tela       
    [SerializeField] private float duracaoAnimacao = 0.4f;
    [SerializeField] private Ease tipoDeTransicao = Ease.OutBack;

    [Header("Configuração do Chat Dinâmico")]
    [SerializeField] private float margemFundoSemEscolhas = 40f;  // Chat usa a tela toda (área útil livre)
    [SerializeField] private float margemFundoComEscolhas = 450f; // Chat encolhe para dar espaço às escolhas

    [Header("Dados do Puzzle / Chat")]
    [SerializeField] private NoDeDialogo dialogoInicial;
    private NoDeDialogo dialogoAtual;
    private Coroutine rotinaDeMensagens;

    [Header("Telas de Fim de Jogo")]
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject botaoContatoGolpista; // Botão "Numero_Anonimo"

    private void Start()
    {
        if (painelEscolhas == null || scrollDoChat == null)
        {
            Debug.LogError($"[SistemaDeChatPuzzle] Referências faltando no Inspector!");
            return;
        }

        // Pega o RectTransform do próprio ScrollRect do chat
        scrollRectTransform = scrollDoChat.GetComponent<RectTransform>();

        // Força o estado inicial limpo (chat ocupando a tela toda)
        painelEscolhas.anchoredPosition = posicaoEscondido;
        painelEscolhas.gameObject.SetActive(false);
        SetChatBottomMargin(margemFundoSemEscolhas);
    }

    public void IniciarChat(NoDeDialogo inicio)
    {
        // Limpa as mensagens da conversa anterior
        foreach (Transform filho in contentArea) Destroy(filho.gameObject);

        // Inicia a nova conversa
        dialogoAtual = inicio;

        // Esconde o painel de escolhas e redefine o tamanho do chat
        EsconderPainelEscolhas();

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }

    private IEnumerator TocarMensagensDoNPC()
    {
        // Lê cada mensagem da lista do grupo
        foreach (MensagemNPC msg in dialogoAtual.mensagens)
        {
            // Espera o tempo de digitação
            yield return new WaitForSeconds(msg.tempoDeDigitacao);

            // Instancia o contêiner (Content_NPC)
            GameObject balao = Instantiate(prefabBalaoNPC, contentArea);

            // Pega as referências de texto dentro do balão (0 é o Nome, 1 é a Mensagem)
            TextMeshProUGUI[] textos = balao.GetComponentsInChildren<TextMeshProUGUI>();
            if (textos.Length >= 2)
            {
                textos[0].text = msg.autor.nome;
                textos[0].color = msg.autor.corDoNome;
                textos[1].text = msg.textoDaMensagem;
            }

            // Verifica se existe a foto, se existir declara a foto do personagem no chat
            Transform fotoTransform = balao.transform.Find("FotoPersonagem");
            if (fotoTransform != null)
            {
                Image fotoImage = fotoTransform.GetComponent<Image>();
                if (fotoImage != null && msg.autor.foto != null)
                {
                    fotoImage.sprite = msg.autor.foto;
                }
            }

            // Procura o componente Image (que está no BalaoNpc) e pinta com a cor do Personagem
            Transform balaoTransform = balao.transform.Find("balaoNPC");
            if (balaoTransform != null)
            {
                Image fundoBalao = balaoTransform.GetComponent<Image>();
                if (fundoBalao != null)
                {
                    fundoBalao.color = msg.autor.corDoBalao;
                }
            }

            // Empurra a tela para baixo após a mensagem do NPC aparecer
            StartCoroutine(ForcarScrollParaBaixo());
        }

        // Quando todas as mensagens forem enviadas, mostra as opções pro jogador
        AtualizarBotoesDeEscolha();
    }

    private void AtualizarBotoesDeEscolha()
    {
        foreach (var btn in botoesDeEscolha) btn.gameObject.SetActive(false);

        for (int i = 0; i < dialogoAtual.escolhas.Length; i++)
        {
            if (i >= botoesDeEscolha.Length)
            {
                Debug.LogWarning("Aviso: O diálogo tem mais escolhas do que botões na UI! A escolha " + i + " foi ignorada.");
                break;
            }

            botoesDeEscolha[i].gameObject.SetActive(true);
            textosDosBotoes[i].text = dialogoAtual.escolhas[i].textoDaEscolha;

            int indexCopia = i;
            botoesDeEscolha[i].onClick.RemoveAllListeners();
            botoesDeEscolha[i].onClick.AddListener(() => FazerEscolha(indexCopia));
        }

        // Ativa e sobe o painel suavemente
        MostrarPainelEscolhas();
    }

    private void MostrarPainelEscolhas()
    {
        painelEscolhas.DOKill();
        painelEscolhas.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(painelEscolhas);

        // ANIMAÇÃO 1: Sobe o painel de escolhas
        painelEscolhas.DOAnchorPos(posicaoVisivel, duracaoAnimacao).SetEase(tipoDeTransicao);

        // ANIMAÇÃO 2: "Encolhe" a base do chat para dar espaço às escolhas
        DOTween.To(() => scrollRectTransform.offsetMin.y, x => SetChatBottomMargin(x), margemFundoComEscolhas, duracaoAnimacao)
            .SetEase(tipoDeTransicao)
            .OnUpdate(() => {
                if (scrollDoChat != null) scrollDoChat.verticalNormalizedPosition = 0f;
            })
            .OnComplete(() => StartCoroutine(ForcarScrollParaBaixo()));
    }

    private void EsconderPainelEscolhas()
    {
        painelEscolhas.DOKill();

        // ANIMAÇÃO 1: Desce o painel de escolhas para sumir da tela
        painelEscolhas.DOAnchorPos(posicaoEscondido, duracaoAnimacao * 0.75f).SetEase(Ease.InQuad)
            .OnComplete(() => painelEscolhas.gameObject.SetActive(false));

        // ANIMAÇÃO 2: Faz o chat "descer" e expandir para ocupar a tela inteira novamente
        DOTween.To(() => scrollRectTransform.offsetMin.y, x => SetChatBottomMargin(x), margemFundoSemEscolhas, duracaoAnimacao * 0.75f)
            .SetEase(Ease.InQuad)
            .OnUpdate(() => {
                if (scrollDoChat != null) scrollDoChat.verticalNormalizedPosition = 0f;
            });
    }

    // Função auxiliar que altera o "Bottom" (a base) do RectTransform da UI do Chat
    private void SetChatBottomMargin(float bottomMargin)
    {
        if (scrollRectTransform == null) return;
        scrollRectTransform.offsetMin = new Vector2(scrollRectTransform.offsetMin.x, bottomMargin);
    }

    private IEnumerator ForcarScrollParaBaixo()
    {
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        if (scrollDoChat != null)
        {
            scrollDoChat.verticalNormalizedPosition = 0f;
        }
    }

    private void FazerEscolha(int index)
    {
        RespostaJogador escolha = dialogoAtual.escolhas[index];

        // Balão do Jogador
        GameObject balao = Instantiate(prefabBalaoJogador, contentArea);
        var textoBalao = balao.GetComponentInChildren<TextMeshProUGUI>();
        if (textoBalao != null) textoBalao.text = escolha.textoDaEscolha;

        // Empurra a tela para baixo após a sua mensagem aparecer
        StartCoroutine(ForcarScrollParaBaixo());

        // Esconde o painel com animação de descida
        EsconderPainelEscolhas();

        if (escolha.encerraPuzzle)
        {
            if (escolha.jogadorGanhou)
            {
                Debug.Log("Vitória! O jogador não caiu no golpe.");

                // --- APAGA O CONTATO DA LISTA (Mantido do puzzle antigo) ---
                if (botaoContatoGolpista != null)
                {
                    botaoContatoGolpista.SetActive(false);
                }

                painelVitoria.SetActive(true);
            }
            else
            {
                Debug.Log("Game Over!");
                painelGameOver.SetActive(true);
            }
            return;
        }

        // Se não encerrou o puzzle, continua o papo normalmente...
        dialogoAtual = escolha.proximoNo;

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }
}