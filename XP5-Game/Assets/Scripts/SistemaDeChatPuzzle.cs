using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class SistemaDeChatPuzzle : MonoBehaviour
{
    [Header("Estrutura da UI")]
    [SerializeField] private Transform contentArea;
    [SerializeField] private GameObject prefabBalaoNPC;
    [SerializeField] private GameObject prefabBalaoJogador;
    [SerializeField] private ScrollRect scrollDoChat;
    private RectTransform scrollRectTransform;

    [Header("Painel de Escolhas")]
    [SerializeField] private RectTransform painelEscolhas;
    [SerializeField] private Button[] botoesDeEscolha;
    [SerializeField] private TextMeshProUGUI[] textosDosBotoes;

    [Header("Configurações de Animação (DOTween)")]
    [SerializeField] private Vector2 posicaoEscondido = new Vector2(0, -500);
    [SerializeField] private Vector2 posicaoVisivel = new Vector2(0, 120);
    [SerializeField] private float duracaoAnimacao = 0.4f;
    [SerializeField] private Ease tipoDeTransicao = Ease.OutBack;

    [Header("Configuração do Chat Dinâmico")]
    [SerializeField] private float margemFundoSemEscolhas = 40f;
    [SerializeField] private float margemFundoComEscolhas = 450f;

    [Header("Ajuste de Posição do Textinho")]
    [Tooltip("Força as letras a subirem fisicamente, burlando o Layout Group.")]
    [SerializeField] private float deslocamentoYDigitando = 20f;

    [Header("Animação dos Balões")]
    [SerializeField] private float duracaoSurgimentoBalao = 0.3f;
    [SerializeField] private Ease transicaoSurgimentoBalao = Ease.OutBack;

    [Header("Dados do Puzzle / Chat Atual")]
    private NoDeDialogo dialogoAtual;
    private Coroutine rotinaDeMensagens;
    private Coroutine rotinaAnimacaoDigitando;
    private string idDoChatAtual;

    [Header("Telas de Fim de Jogo")]
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject botaoContatoGolpista;

    private void Start()
    {
        if (painelEscolhas == null || scrollDoChat == null)
        {
            Debug.LogError($"[SistemaDeChatPuzzle] Referências faltando no Inspector!");
            return;
        }

        scrollRectTransform = scrollDoChat.GetComponent<ScrollRect>().GetComponent<RectTransform>();

        painelEscolhas.anchoredPosition = posicaoEscondido;
        painelEscolhas.gameObject.SetActive(false);
        SetChatBottomMargin(margemFundoSemEscolhas);
    }

    public void IniciarChat(NoDeDialogo noInicial)
    {
        if (noInicial == null) return;

        idDoChatAtual = noInicial.idDaConversa;

        // Verifica se essa conversa inteira já foi finalizada pelo jogador
        if (PlayerPrefs.GetInt(idDoChatAtual + "_Finalizada", 0) == 1)
        {
            Debug.Log($"[SistemaDeChat] A conversa '{idDoChatAtual}' já foi concluída em definitivo. Acesso bloqueado.");
            return;
        }

        foreach (Transform filho in contentArea) Destroy(filho.gameObject);

        // Sistema de checkpoints: Recupera onde o jogador parou
        string ultimoNoSalvo = PlayerPrefs.GetString(idDoChatAtual + "_UltimoNo", "");

        if (!string.IsNullOrEmpty(ultimoNoSalvo) && ultimoNoSalvo != noInicial.idDoNo)
        {
            NoDeDialogo noCarregado = Resources.Load<NoDeDialogo>("Dialogos/" + ultimoNoSalvo);
            if (noCarregado != null)
            {
                dialogoAtual = noCarregado;
                Debug.Log($"[SistemaDeChat] Continuando '{idDoChatAtual}' a partir do nó '{ultimoNoSalvo}'.");
            }
            else
            {
                dialogoAtual = noInicial;
            }
        }
        else
        {
            dialogoAtual = noInicial;
            SalvarProgressoAtual();
        }

        EsconderPainelEscolhas();

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }

    private void SalvarProgressoAtual()
    {
        if (dialogoAtual == null) return;
        PlayerPrefs.SetString(idDoChatAtual + "_UltimoNo", dialogoAtual.idDoNo);
        PlayerPrefs.Save();
    }

    private IEnumerator TocarMensagensDoNPC()
    {
        foreach (MensagemNPC msg in dialogoAtual.mensagens)
        {
            GameObject balao = Instantiate(prefabBalaoNPC, contentArea);
            TextMeshProUGUI[] textos = balao.GetComponentsInChildren<TextMeshProUGUI>();

            VerticalAlignmentOptions alinhamentoOriginal = VerticalAlignmentOptions.Top;

            if (textos.Length >= 2)
            {
                textos[0].text = msg.autor.nome;
                textos[0].color = msg.autor.corDoNome;
                alinhamentoOriginal = textos[1].verticalAlignment;
            }

            Transform fotoTransform = balao.transform.Find("FotoPersonagem");
            if (fotoTransform != null)
            {
                Image fotoImage = fotoTransform.GetComponent<Image>();
                if (fotoImage != null && msg.autor.foto != null)
                {
                    fotoImage.sprite = msg.autor.foto;
                }
            }

            Image fundoBalao = null;
            Transform balaoTransform = balao.transform.Find("balaoNPC");
            if (balaoTransform != null)
            {
                fundoBalao = balaoTransform.GetComponent<Image>();
                if (fundoBalao != null)
                {
                    fundoBalao.color = msg.autor.corDoBalao;
                }
            }

            balao.transform.localScale = Vector3.zero;
            balao.transform.DOScale(Vector3.one, duracaoSurgimentoBalao).SetEase(transicaoSurgimentoBalao);

            if (msg.tempoDeDigitacao > 0f)
            {
                if (textos.Length >= 1) textos[0].enabled = false;

                if (textos.Length >= 2)
                {
                    textos[1].text = "<i><color=#888888>digitando...</color></i>";
                    rotinaAnimacaoDigitando = StartCoroutine(AnimarTextoDigitando(textos[1]));
                }

                if (fundoBalao != null) fundoBalao.enabled = false;

                StartCoroutine(ForcarScrollParaBaixo());

                yield return new WaitForSeconds(msg.tempoDeDigitacao);

                if (rotinaAnimacaoDigitando != null) StopCoroutine(rotinaAnimacaoDigitando);

                if (fundoBalao != null) fundoBalao.enabled = true;
                if (textos.Length >= 2) textos[1].verticalAlignment = alinhamentoOriginal;

                if (textos.Length >= 1) textos[0].enabled = true;
            }

            if (textos.Length >= 2)
            {
                textos[1].text = msg.textoDaMensagem;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(balao.GetComponent<RectTransform>());
            balao.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0f), 0.2f, 5, 0.5f);

            StartCoroutine(ForcarScrollParaBaixo());
        }

        if (dialogoAtual.escolhas == null || dialogoAtual.escolhas.Length == 0)
        {
            MarcarChatComoConcluido();
        }
        else
        {
            AtualizarBotoesDeEscolha();
        }
    }

    private void MarcarChatComoConcluido()
    {
        Debug.Log($"player completou chat ({idDoChatAtual})");

        PlayerPrefs.SetInt(idDoChatAtual + "_Finalizada", 1);
        PlayerPrefs.DeleteKey(idDoChatAtual + "_UltimoNo");
        PlayerPrefs.Save();
    }

    private IEnumerator AnimarTextoDigitando(TextMeshProUGUI campoTexto)
    {
        float velocidadeMetros = 4f;
        float alturaBalanço = 5f;

        campoTexto.ForceMeshUpdate();
        TMP_TextInfo textInfo = campoTexto.textInfo;

        while (true)
        {
            campoTexto.ForceMeshUpdate();
            textInfo = campoTexto.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                float offsetOnda = i * 0.25f;
                float deslocamentoY = (Mathf.Sin(Time.time * velocidadeMetros + offsetOnda) * alturaBalanço) + deslocamentoYDigitando;

                vertices[vertexIndex + 0].y += deslocamentoY;
                vertices[vertexIndex + 1].y += deslocamentoY;
                vertices[vertexIndex + 2].y += deslocamentoY;
                vertices[vertexIndex + 3].y += deslocamentoY;
            }

            campoTexto.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            yield return null;
        }
    }

    // ATUALIZADO: Método Update totalmente reescrito usando o Novo Input System da Unity
    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("[SistemaDeChat] TODOS OS SAVES FORAM RESETADOS PARA TESTE!");
        }
    }

    private void AtualizarBotoesDeEscolha()
    {
        foreach (var btn in botoesDeEscolha) btn.gameObject.SetActive(false);

        for (int i = 0; i < dialogoAtual.escolhas.Length; i++)
        {
            if (i >= botoesDeEscolha.Length) break;

            botoesDeEscolha[i].gameObject.SetActive(true);
            textosDosBotoes[i].text = dialogoAtual.escolhas[i].textoDaEscolha;

            int indexCopia = i;
            botoesDeEscolha[i].onClick.RemoveAllListeners();
            botoesDeEscolha[i].onClick.AddListener(() => FazerEscolha(indexCopia));
        }

        MostrarPainelEscolhas();
    }

    private void MostrarPainelEscolhas()
    {
        painelEscolhas.DOKill();
        painelEscolhas.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(painelEscolhas);

        painelEscolhas.DOAnchorPos(posicaoVisivel, duracaoAnimacao).SetEase(tipoDeTransicao);

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

        painelEscolhas.DOAnchorPos(posicaoEscondido, duracaoAnimacao * 0.75f).SetEase(Ease.InQuad)
            .OnComplete(() => painelEscolhas.gameObject.SetActive(false));

        DOTween.To(() => scrollRectTransform.offsetMin.y, x => SetChatBottomMargin(x), margemFundoSemEscolhas, duracaoAnimacao * 0.75f)
            .SetEase(Ease.InQuad)
            .OnUpdate(() => {
                if (scrollDoChat != null) scrollDoChat.verticalNormalizedPosition = 0f;
            });
    }

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

        GameObject balao = Instantiate(prefabBalaoJogador, contentArea);
        var textoBalao = balao.GetComponentInChildren<TextMeshProUGUI>();
        if (textoBalao != null) textoBalao.text = escolha.textoDaEscolha;

        balao.transform.localScale = Vector3.zero;
        balao.transform.DOScale(Vector3.one, duracaoSurgimentoBalao).SetEase(transicaoSurgimentoBalao);

        StartCoroutine(ForcarScrollParaBaixo());
        EsconderPainelEscolhas();

        if (escolha.encerraPuzzle)
        {
            MarcarChatComoConcluido();

            if (escolha.jogadorGanhou)
            {
                if (botaoContatoGolpista != null) botaoContatoGolpista.SetActive(false);
                painelVitoria.SetActive(true);
            }
            else
            {
                painelGameOver.SetActive(true);
            }
            return;
        }

        dialogoAtual = escolha.proximoNo;
        SalvarProgressoAtual();

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }
}