using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class SistemaDeChatPuzzle : MonoBehaviour
{
    private bool avancarHistoriaAposChat = false;

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

    [Header("Telas de Fim de Jogo (Opcionais)")]
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private GameObject painelVitoria;

    private void Start()
    {
        if (painelEscolhas == null || scrollDoChat == null)
        {
            Debug.LogError($"[SistemaDeChatPuzzle] Referências faltando no Inspector em {gameObject.name}!");
            return;
        }

        scrollRectTransform = scrollDoChat.GetComponent<ScrollRect>().GetComponent<RectTransform>();

        painelEscolhas.anchoredPosition = posicaoEscondido;
        painelEscolhas.gameObject.SetActive(false);
        SetChatBottomMargin(margemFundoSemEscolhas);
    }

    public void IniciarChat(NoDeDialogo noInicial, bool limparHistorico)
    {
        gameObject.SetActive(true);
        if (noInicial == null) return;

        idDoChatAtual = noInicial.idDaConversa;

        string chaveFinalizado = idDoChatAtual + "_" + noInicial.idDoNo + "_Finalizada";
        string chaveUltimoNo = idDoChatAtual + "_" + noInicial.idDoNo + "_UltimoNo";

        if (PlayerPrefs.GetInt(chaveFinalizado, 0) == 1)
        {
            Debug.Log($"[SistemaDeChat] O bloco '{noInicial.idDoNo}' da conversa '{idDoChatAtual}' já foi concluído.");
            gameObject.SetActive(false);
            return;
        }

        if (limparHistorico)
        {
            foreach (Transform filho in contentArea) Destroy(filho.gameObject);
        }

        string ultimoNoSalvo = PlayerPrefs.GetString(chaveUltimoNo, "");

        if (!string.IsNullOrEmpty(ultimoNoSalvo) && ultimoNoSalvo != noInicial.idDoNo)
        {
            NoDeDialogo noCarregado = Resources.Load<NoDeDialogo>("Dialogos/" + ultimoNoSalvo);
            if (noCarregado != null)
            {
                dialogoAtual = noCarregado;
                Debug.Log($"[SistemaDeChat] Continuando '{idDoChatAtual}' a partir do nó salvo: '{ultimoNoSalvo}'.");
            }
            else
            {
                dialogoAtual = noInicial;
            }
        }
        else
        {
            dialogoAtual = noInicial;
            PlayerPrefs.SetString(chaveUltimoNo, dialogoAtual.idDoNo);
            PlayerPrefs.Save();
        }

        EsconderPainelEscolhas();

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }

    private void SalvarProgressoAtual()
    {
        if (dialogoAtual == null) return;
        PlayerPrefs.SetString(idDoChatAtual + "_" + dialogoAtual.idDoNo + "_UltimoNo", dialogoAtual.idDoNo);
        PlayerPrefs.Save();
    }

    private IEnumerator TocarMensagensDoNPC()
    {
        if (dialogoAtual.mensagens == null || dialogoAtual.mensagens.Count == 0)
        {
            ChecarFimDeConversaOuEscolhas();
            yield break;
        }

        foreach (MensagemNPC msg in dialogoAtual.mensagens)
        {
            GameObject balao = Instantiate(prefabBalaoNPC, contentArea);
            if (GerenciadorDeAudio.Instancia != null) GerenciadorDeAudio.Instancia.TocarMensagemNPC();
            MeshPro_Fetch_Textos(balao, out TextMeshProUGUI[] textos);

            VerticalAlignmentOptions alinhamentoOriginal = VerticalAlignmentOptions.Top;

            if (textos.Length >= 2)
            {
                textos[0].gameObject.SetActive(true);
                textos[0].text = msg.autor.nome;
                textos[0].color = msg.autor.corDoNome;
                alinhamentoOriginal = textos[1].verticalAlignment;
            }

            Transform fotoTransform = balao.transform.Find("FotoPersonagem");
            if (fotoTransform != null)
            {
                fotoTransform.gameObject.SetActive(true);

                if (msg.autor.foto != null)
                {
                    Image fotoImage = fotoTransform.GetComponent<Image>();
                    if (fotoImage != null) fotoImage.sprite = msg.autor.foto;
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

        ChecarFimDeConversaOuEscolhas();
    }

    private void MeshPro_Fetch_Textos(GameObject target, out TextMeshProUGUI[] arr)
    {
        arr = target.GetComponentsInChildren<TextMeshProUGUI>();
    }

    private void ChecarFimDeConversaOuEscolhas()
    {
        if (dialogoAtual == null || dialogoAtual.escolhas == null || dialogoAtual.escolhas.Length == 0)
        {
            Debug.Log($"[SistemaDeChat] Nó '{dialogoAtual?.idDoNo}' terminou sem escolhas de resposta.");

            MarcarChatComoConcluido();

            string idTratado = dialogoAtual != null && dialogoAtual.idDoNo != null ? dialogoAtual.idDoNo.Trim() : "";
            if (idTratado == "PC5Rota1Parte2" || idTratado == "PC5Rota2Parte2" || idTratado == "PC5Rota3" || idTratado.Contains("Rota"))
            {
                Debug.Log("<color=green>[ANTI-BUG] Gatilho acionado preventivamente pela checagem de contingência!</color>");
                avancarHistoriaAposChat = true;
            }

            Debug.Log($"[DEBUG] APÓS MarcarChatComoConcluido: avancarHistoriaAposChat={avancarHistoriaAposChat}");

            if (avancarHistoriaAposChat)
            {
                Debug.Log("[DEBUG] Condição Verdadeira! Iniciando transição de cena/etapa...");
                avancarHistoriaAposChat = false;
                StartCoroutine(TempoParaLeituraEAvanco(1.5f));
            }
            else
            {
                Debug.LogWarning("[DEBUG] A transição de história não disparou porque avancarHistoriaAposChat ficou em FALSE.");
            }
        }
        else
        {
            AtualizarBotoesDeEscolha();
        }
    }

    private IEnumerator TempoParaLeituraEAvanco(float tempo)
    {
        Debug.Log("[DEBUG] TempoParaLeituraEAvanco INICIOU, aguardando " + tempo + "s");
        yield return new WaitForSeconds(tempo);

        Debug.Log("[DEBUG] TempoParaLeituraEAvanco TERMINOU o wait. GerenciadorDeNarrativa.Instancia é null? " + (GerenciadorDeNarrativa.Instancia == null));

        if (GerenciadorDeNarrativa.Instancia != null)
        {
            Debug.Log("<color=green>[SistemaDeChat] Sucesso absoluto! Chamando GerenciadorDeNarrativa.Instancia.AvancarHistoria().</color>");
            GerenciadorDeNarrativa.Instancia.AvancarHistoria();
        }
        else
        {
            Debug.LogError("<color=red>[ERRO] O GerenciadorDeNarrativa não foi encontrado na cena corrente!</color>");
        }
    }

    private void MarcarChatComoConcluido()
    {
        if (string.IsNullOrEmpty(idDoChatAtual) || dialogoAtual == null) return;

        Debug.Log($"[SistemaDeChat] Salvando conclusão do bloco: ({idDoChatAtual}) - Nó: {dialogoAtual.idDoNo}");

        PlayerPrefs.SetInt(idDoChatAtual + "_" + dialogoAtual.idDoNo + "_Finalizada", 1);
        PlayerPrefs.DeleteKey(idDoChatAtual + "_" + dialogoAtual.idDoNo + "_UltimoNo");
        PlayerPrefs.Save();

        string idTratado = dialogoAtual.idDoNo.Trim();
        bool ehNoFinalValidoDetetive = (idTratado == "PC5Rota1Parte2" ||
                                       idTratado == "PC5Rota2Parte2" ||
                                       idTratado == "PC5Rota3");

        Debug.Log($"[DEBUG] idDoNo Tratado='{idTratado}' | ehNoFinalValidoDetetive={ehNoFinalValidoDetetive}");

        if (ehNoFinalValidoDetetive)
        {
            Debug.Log($"[SISTEMA DE CHAT] Fim de árvore detectado no nó [{dialogoAtual.idDoNo}]. Ativando transição.");
            avancarHistoriaAposChat = true;
        }
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

    private void Update()
    {
        var teclado = UnityEngine.InputSystem.Keyboard.current;
        if (teclado == null) return;

        if (teclado.rKey.wasPressedThisFrame)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("<color=magenta>[SistemaDeChat] TODOS OS SAVES FORAM RESETADOS MANUALMENTE (KEY R)!</color>");
        }

        if (teclado.sKey.wasPressedThisFrame)
        {
            Debug.Log("[SistemaDeChat] Executando comando manual de SKIP (KEY S)");
            ForçarPularDialogoDoNPC();
        }
    }

    private void ForçarPularDialogoDoNPC()
    {
        if (dialogoAtual == null || (painelEscolhas != null && painelEscolhas.gameObject.activeSelf)) return;

        Debug.Log($"[DEBUG - SKIP] Pulando diálogos do nó atual: {dialogoAtual.idDoNo}");

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        if (rotinaAnimacaoDigitando != null) StopCoroutine(rotinaAnimacaoDigitando);

        foreach (Transform filho in contentArea) Destroy(filho.gameObject);

        if (dialogoAtual.mensagens != null && dialogoAtual.mensagens.Count > 0)
        {
            MensagemNPC ultimaMsg = dialogoAtual.mensagens[dialogoAtual.mensagens.Count - 1];
            GameObject balao = Instantiate(prefabBalaoNPC, contentArea);
            MeshPro_Fetch_Textos(balao, out TextMeshProUGUI[] textos);

            if (textos.Length >= 2)
            {
                textos[0].gameObject.SetActive(true);
                textos[0].text = ultimaMsg.autor.nome;
                textos[0].color = ultimaMsg.autor.corDoNome;
                textos[1].text = ultimaMsg.textoDaMensagem;
            }

            Transform fotoTransform = balao.transform.Find("FotoPersonagem");
            if (fotoTransform != null && ultimaMsg.autor.foto != null)
            {
                fotoTransform.gameObject.SetActive(true);
                fotoTransform.GetComponent<Image>().sprite = ultimaMsg.autor.foto;
            }

            Image fundoBalao = balao.transform.Find("balaoNPC")?.GetComponent<Image>();
            if (fundoBalao != null) fundoBalao.color = ultimaMsg.autor.corDoBalao;

            LayoutRebuilder.ForceRebuildLayoutImmediate(balao.GetComponent<RectTransform>());
        }

        StartCoroutine(ForcarScrollParaBaixo());
        ChecarFimDeConversaOuEscolhas();
    }

    private void AtivarBotoes(bool estado)
    {
        foreach (var btn in botoesDeEscolha) btn.gameObject.SetActive(estado);
    }

    // --- MÉTODOS PÚBLICOS DE REDIRECIONAMENTO (COMPATIBILIDADE) ---
    public void UpdateBotoesDeEscolha() { AtualizarBotoesDeEscolha(); }
    public void UpdateBotoesDeEscolha(NoDeDialogo no) { AtualizarBotoesDeEscolha(); }
    public void BlackboxAtualizarBotoesDeEscolha() { AtualizarBotoesDeEscolha(); }

    // --- MÉTODO PRINCIPAL DE SELEÇÃO VISUAL DAS ESCOLHAS ---
    private void AtualizarBotoesDeEscolha()
    {
        AtivarBotoes(false);

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
        if (dialogoAtual == null || dialogoAtual.escolhas == null || index >= dialogoAtual.escolhas.Length) return;

        RespostaJogador escolha = dialogoAtual.escolhas[index];

        if (escolha.avancaAHistoria)
        {
            avancarHistoriaAposChat = true;
        }

        GameObject balao = Instantiate(prefabBalaoJogador, contentArea);
        if (GerenciadorDeAudio.Instancia != null) GerenciadorDeAudio.Instancia.TocarEnvioMensagem();
        var textoBalao = balao.GetComponentInChildren<TextMeshProUGUI>();
        if (textoBalao != null) textoBalao.text = escolha.textoDaEscolha;

        balao.transform.localScale = Vector3.zero;
        balao.transform.DOScale(Vector3.one, duracaoSurgimentoBalao).SetEase(transicaoSurgimentoBalao);

        StartCoroutine(ForcarScrollParaBaixo());
        EsconderPainelEscolhas();

        // --- GATILHO DA PULSAÇÃO DO BOTÃO + COM BASE NO SUCESSO ---
        if (escolha.encerraPuzzle && escolha.jogadorGanhou)
        {
            if (GerenciadorDeNarrativa.Instancia != null)
            {
                GerenciadorDeNarrativa.Instancia.AtivarPulsoBotaoAdicionar();
            }
        }

        if (escolha.encerraPuzzle)
        {
            MarcarChatComoConcluido();
            if (avancarHistoriaAposChat)
            {
                avancarHistoriaAposChat = false;
                StartCoroutine(TempoParaLeituraEAvanco(1.5f));
            }
            return;
        }

        if (escolha.proximoNo == null)
        {
            MarcarChatComoConcluido();
            if (avancarHistoriaAposChat)
            {
                avancarHistoriaAposChat = false;
                StartCoroutine(TempoParaLeituraEAvanco(1.5f));
            }
            return;
        }

        dialogoAtual = escolha.proximoNo;
        SalvarProgressoAtual();

        if (rotinaDeMensagens != null) StopCoroutine(rotinaDeMensagens);
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }
}