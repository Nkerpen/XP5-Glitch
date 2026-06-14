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
    [Tooltip("Força as letras a subirem fisicamente, burlando o Layout Group. Aumente este valor para subir o texto.")]
    [SerializeField] private float deslocamentoYDigitando = 20f;

    [Header("Animação dos Balões")]
    [SerializeField] private float duracaoSurgimentoBalao = 0.3f;
    [SerializeField] private Ease transicaoSurgimentoBalao = Ease.OutBack;

    [Header("Dados do Puzzle / Chat")]
    [SerializeField] private NoDeDialogo dialogoInicial;
    private NoDeDialogo dialogoAtual;
    private Coroutine rotinaDeMensagens;
    private Coroutine rotinaAnimacaoDigitando;

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

        scrollRectTransform = scrollDoChat.GetComponent<RectTransform>();

        painelEscolhas.anchoredPosition = posicaoEscondido;
        painelEscolhas.gameObject.SetActive(false);
        SetChatBottomMargin(margemFundoSemEscolhas);
    }

    public void IniciarChat(NoDeDialogo inicio)
    {
        foreach (Transform filho in contentArea) Destroy(filho.gameObject);

        dialogoAtual = inicio;
        EsconderPainelEscolhas();

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }

    private IEnumerator TocarMensagensDoNPC()
    {
        foreach (MensagemNPC msg in dialogoAtual.mensagens)
        {
            // 1. Instancia o balão básico do NPC baseado no prefab anexado
            GameObject balao = Instantiate(prefabBalaoNPC, contentArea);
            TextMeshProUGUI[] textos = balao.GetComponentsInChildren<TextMeshProUGUI>();

            VerticalAlignmentOptions alinhamentoOriginal = VerticalAlignmentOptions.Top;

            if (textos.Length >= 2)
            {
                textos[0].text = msg.autor.nome;
                textos[0].color = msg.autor.corDoNome;
                alinhamentoOriginal = textos[1].verticalAlignment;
            }

            // 2. Associa a foto do autor atual
            Transform fotoTransform = balao.transform.Find("FotoPersonagem");
            if (fotoTransform != null)
            {
                Image fotoImage = fotoTransform.GetComponent<Image>();
                if (fotoImage != null && msg.autor.foto != null)
                {
                    fotoImage.sprite = msg.autor.foto;
                }
            }

            // 3. Modifica a cor de fundo do balão conforme o NPC
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

            // 4. Animação de surgimento elástico (escala do balão de 0 a 1)
            balao.transform.localScale = Vector3.zero;
            balao.transform.DOScale(Vector3.one, duracaoSurgimentoBalao).SetEase(transicaoSurgimentoBalao);

            // 5. Lógica de digitação por vértices com proteção de sobreposição de nome
            if (msg.tempoDeDigitacao > 0f)
            {
                // Esconde o nome do NPC para evitar que o Anônimo fique por cima do texto
                if (textos.Length >= 1) textos[0].enabled = false;

                if (textos.Length >= 2)
                {
                    textos[1].text = "<i><color=#888888>digitando...</color></i>";
                    rotinaAnimacaoDigitando = StartCoroutine(AnimarTextoDigitando(textos[1]));
                }

                if (fundoBalao != null) fundoBalao.enabled = false;

                StartCoroutine(ForcarScrollParaBaixo());

                // Aguarda o tempo estipulado fingindo que o personagem escreve
                yield return new WaitForSeconds(msg.tempoDeDigitacao);

                // Finaliza a animação de onda
                if (rotinaAnimacaoDigitando != null) StopCoroutine(rotinaAnimacaoDigitando);

                // Devolve as configurações de visibilidade normais do balão
                if (fundoBalao != null) fundoBalao.enabled = true;
                if (textos.Length >= 2) textos[1].verticalAlignment = alinhamentoOriginal;

                // Traz o nome do personagem de volta
                if (textos.Length >= 1) textos[0].enabled = true;
            }

            // 6. Insere a fala definitiva do Scriptable Object
            if (textos.Length >= 2)
            {
                textos[1].text = msg.textoDaMensagem;
            }

            // Força a atualização imediata dos Layout Groups para não quebrar o layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(balao.GetComponent<RectTransform>());

            // Pequeno soco ("punch") na escala dando impacto tátil de mensagem nova entregue
            balao.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0f), 0.2f, 5, 0.5f);

            StartCoroutine(ForcarScrollParaBaixo());
        }

        // Fim das mensagens do NPC, chama as opções do jogador
        AtualizarBotoesDeEscolha();
    }

    // Corotina responsável pelo balanço físico de onda e ajuste de altura das letrinhas
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
        GameObject balao = Instantiate(prefabBalaoJogador, contentArea);
        var textoBalao = balao.GetComponentInChildren<TextMeshProUGUI>();
        if (textoBalao != null) textoBalao.text = dialogoAtual.escolhas[index].textoDaEscolha;

        // Animação de surgimento suave do balão do JOGADOR
        balao.transform.localScale = Vector3.zero;
        balao.transform.DOScale(Vector3.one, duracaoSurgimentoBalao).SetEase(transicaoSurgimentoBalao);

        StartCoroutine(ForcarScrollParaBaixo());
        EsconderPainelEscolhas();

        if (dialogoAtual.escolhas[index].encerraPuzzle)
        {
            if (dialogoAtual.escolhas[index].jogadorGanhou)
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

        dialogoAtual = dialogoAtual.escolhas[index].proximoNo;

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }
}