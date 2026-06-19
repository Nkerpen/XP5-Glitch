using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GerenciadorDeNarrativa : MonoBehaviour
{
    public static GerenciadorDeNarrativa Instancia;

    [Header("Sistemas Globais")]
    [SerializeField] private SistemaDeNotificacao sistemaNotificacao;

    [Header("Panels de Chat (Scripts Atribuídos nos Panels)")]
    [SerializeField] private SistemaDeChatPuzzle sistemaDeChatPrincipal;

    [Header("Diálogos (Scriptable Objects)")]
    [SerializeField] private NoDeDialogo chat1_Inicial;
    [SerializeField] private NoDeDialogo chat2_SobreDetetive;
    [SerializeField] private NoDeDialogo chatGolpista;
    [SerializeField] private NoDeDialogo chat3_Detetive;

    [Header("Aplicativos / Panels (Abertura Direta)")]
    [Tooltip("Tela de chamada ainda pode ser controlada separadamente se for um pop-up por cima.")]
    [SerializeField] private GameObject telaChamadaRecebida;

    [Header("Botões de Contato (Para Invocar Cliques)")]
    [SerializeField] private Button botaoContatoGrupo;
    [SerializeField] private Button botaoContatoGolpista;
    [SerializeField] private Button botaoContatoDetetive;
    [Tooltip("Arraste o botão de '+' usado para adicionar novos contatos")]
    [SerializeField] private Button botaoAdicionarContato;

    [Header("Contatos na Hierarquia (Para Efeito de Desbloqueio)")]
    [Tooltip("Arraste o objeto 'ContatoBloqueado(Golpista)' que tem Canvas Group")]
    [SerializeField] private GameObject contatoBloqueadoGolpista;
    [Tooltip("Arraste o objeto 'Desbloqueado(Golpista)' que tem Canvas Group")]
    [SerializeField] private GameObject contatoDesbloqueadoGolpista;

    [Header("Travas Visuais - E-mail")]
    [SerializeField] private GameObject iconeAppEmailNaHome;
    [SerializeField] private GameObject botaoEmail1_Puzzle;
    [SerializeField] private GameObject botaoEmail2_Detective;
    [SerializeField] private GameObject painelEscondeEmail;

    [Header("Travas Visuais - Notas")]
    [Tooltip("Arraste o botão do App de Notas que fica na Home do celular")]
    [SerializeField] private GameObject botaoAppNotasNaHome;

    private int etapaAtual = 0;
    private Tweener tweenPiscarVermelho; // Guarda a animação para poder pará-la depois
    private Tweener tweenPulsarBotaoMais; // Guarda a animação de pulsação do botão +

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Garante o estado inicial correto da interface ao abrir a demo
        if (iconeAppEmailNaHome != null) iconeAppEmailNaHome.SetActive(false);
        if (botaoContatoGolpista != null) botaoContatoGolpista.gameObject.SetActive(false);
        if (botaoContatoDetetive != null) botaoContatoDetetive.gameObject.SetActive(false);
        if (botaoEmail1_Puzzle != null) botaoEmail1_Puzzle.SetActive(false);
        if (botaoEmail2_Detective != null) botaoEmail2_Detective.SetActive(false);
        if (painelEscondeEmail != null) painelEscondeEmail.SetActive(true);

        // Estado inicial do Notas (Começa desativado na Home, igual ao E-mail)
        if (botaoAppNotasNaHome != null) botaoAppNotasNaHome.SetActive(false);

        // Estado inicial do Golpista
        if (contatoBloqueadoGolpista != null) contatoBloqueadoGolpista.SetActive(true);
        if (contatoDesbloqueadoGolpista != null) contatoDesbloqueadoGolpista.SetActive(false);

        // Configura o clique do botão + para parar a pulsação quando for usado pelo jogador
        if (botaoAdicionarContato != null)
        {
            botaoAdicionarContato.onClick.AddListener(PararEfeitoPulsarBotaoMais);
        }
    }

    public void IniciarJogo()
    {
        // Reinicia completamente a partida
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        etapaAtual = 0;
        DispararNotificacaoAtual();
    }

    public void AvancarHistoria()
    {
        etapaAtual++;
        Debug.Log($"[GerenciadorDeNarrativa] Avançando para a Etapa: {etapaAtual}");

        if (etapaAtual == 1)
        {
            AtivarElementosEtapa1();
        }

        CancelInvoke(nameof(DispararNotificacaoAtual));
        Invoke(nameof(DispararNotificacaoAtual), 1.5f);
    }

    private void AtivarElementosEtapa1()
    {
        if (iconeAppEmailNaHome != null) iconeAppEmailNaHome.SetActive(true);
        if (botaoEmail1_Puzzle != null) botaoEmail1_Puzzle.SetActive(true);
    }

    private void DispararNotificacaoAtual()
    {
        switch (etapaAtual)
        {
            case 0: // ----------------- CHAT #1 INICIAL (GRUPO) -----------------
                if (botaoContatoGrupo != null)
                {
                    botaoContatoGrupo.onClick.RemoveAllListeners();
                    botaoContatoGrupo.onClick.AddListener(() => {
                        if (etapaAtual == 0)
                        {
                            if (NavegacaoCelular.Instancia != null)
                            {
                                NavegacaoCelular.Instancia.AbrirApp("Contatos");
                                NavegacaoCelular.Instancia.AbrirApp("Chat");
                            }
                            sistemaDeChatPrincipal.IniciarChat(chat1_Inicial, limparHistorico: true);
                        }
                        else
                        {
                            Debug.Log("[GerenciadorDeNarrativa] Chat #1 do Grupo já foi finalizado e está inacessível.");
                        }
                    });
                }

                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação",
                    "Charles: O que você tá fazendo com o celular do John?",
                    () => { if (botaoContatoGrupo != null) botaoContatoGrupo.onClick.Invoke(); }
                );
                break;

            case 1: // ----------------- PUZZLE EMAIL (PAYPAL FALSO) -----------------
                AtivarElementosEtapa1();
                sistemaNotificacao.MostrarNotificacao(
                    "Suporte PayPal",
                    "Alerta de segurança na sua conta!",
                    () => {
                        if (NavegacaoCelular.Instancia != null)
                        {
                            NavegacaoCelular.Instancia.AbrirApp("Mailbox");
                            NavegacaoCelular.Instancia.AbrirApp("EmailPuzzle");
                        }
                    }
                );
                break;

            case 2: // ----------------- LIBERA EMAIL DO DETETIVE -----------------
                if (painelEscondeEmail != null) painelEscondeEmail.SetActive(false);
                if (botaoEmail2_Detective != null) botaoEmail2_Detective.SetActive(true);

                sistemaNotificacao.MostrarNotificacao(
                    "Novo E-mail",
                    "detective.dragonfly: Me adicione.",
                    () => {
                        if (NavegacaoCelular.Instancia != null)
                        {
                            NavegacaoCelular.Instancia.AbrirApp("Mailbox");
                            NavegacaoCelular.Instancia.AbrirApp("EmailAnthony");
                        }
                    }
                );
                break;

            case 3: // ----------------- CHAT #2 (MESMO GRUPO) -----------------
                if (botaoContatoGrupo != null)
                {
                    botaoContatoGrupo.onClick.RemoveAllListeners();
                    botaoContatoGrupo.onClick.AddListener(() => {
                        if (etapaAtual == 3)
                        {
                            if (NavegacaoCelular.Instancia != null)
                            {
                                NavegacaoCelular.Instancia.AbrirApp("Contatos");
                                NavegacaoCelular.Instancia.AbrirApp("Chat");
                            }
                            sistemaDeChatPrincipal.IniciarChat(chat2_SobreDetetive, limparHistorico: false);
                        }
                        else
                        {
                            Debug.Log("[GerenciadorDeNarrativa] Chat #2 do Grupo já foi finalizado e está inacessível.");
                        }
                    });
                }

                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação",
                    "Eva: Quem é esse tal de detetive?",
                    () => { if (botaoContatoGrupo != null) botaoContatoGrupo.onClick.Invoke(); }
                );
                break;

            case 4: // ----------------- INTERRUPÇÃO VISUAL DO GOLPISTA -----------------
                ExecutarDesbloqueioVisualGolpista();

                if (botaoContatoGolpista != null)
                {
                    botaoContatoGolpista.gameObject.SetActive(true);

                    botaoContatoGolpista.onClick.RemoveAllListeners();
                    botaoContatoGolpista.onClick.AddListener(() => {
                        if (etapaAtual == 4)
                        {
                            PararEfeitoPiscarGolpista();
                            if (NavegacaoCelular.Instancia != null)
                            {
                                NavegacaoCelular.Instancia.AbrirApp("Contatos");
                                NavegacaoCelular.Instancia.AbrirApp("Chat");
                            }
                            sistemaDeChatPrincipal.IniciarChat(chatGolpista, limparHistorico: true);
                        }
                        else
                        {
                            Debug.Log("[GerenciadorDeNarrativa] Conversa com o Golpista encerrada. Acesso bloqueado.");
                        }
                    });

                    IniciarEfeitoPiscarGolpista();
                }

                sistemaNotificacao.MostrarNotificacao(
                    "Suporte do App",
                    "URGENTE: Confirme seu código de acesso imediatamente.",
                    () => { if (botaoContatoGolpista != null) botaoContatoGolpista.onClick.Invoke(); }
                );
                break;

            case 5: // ----------------- CHAT #3 (DETETIVE DRAGONFLY) -----------------
                if (botaoContatoDetetive != null)
                {
                    botaoContatoDetetive.gameObject.SetActive(true);
                    botaoContatoDetetive.onClick.RemoveAllListeners();
                    botaoContatoDetetive.onClick.AddListener(() => {
                        if (NavegacaoCelular.Instancia != null)
                        {
                            NavegacaoCelular.Instancia.AbrirApp("Contatos");
                            NavegacaoCelular.Instancia.AbrirApp("Chat");
                        }
                        sistemaDeChatPrincipal.IniciarChat(chat3_Detetive, limparHistorico: true);
                    });
                }

                sistemaNotificacao.MostrarNotificacao(
                    "Detetive Dragonfly",
                    "Fiquei sabendo que você está com o celular do John. Precisamos conversar.",
                    () => { if (botaoContatoDetetive != null) botaoContatoDetetive.onClick.Invoke(); }
                );
                break;

            case 6: // ----------------- LIBERA BOTÃO DO APP NOTAS -----------------
                if (botaoAppNotasNaHome != null) botaoAppNotasNaHome.SetActive(true);

                sistemaNotificacao.MostrarNotificacao(
                    "Notas",
                    "Nota criptografada encontrada.",
                    () => {
                        if (NavegacaoCelular.Instancia != null)
                        {
                            NavegacaoCelular.Instancia.AbrirApp("Notas");
                        }
                    }
                );
                break;

            case 7: // ----------------- LIGAÇÃO RECEBIDA -----------------
                if (telaChamadaRecebida != null) telaChamadaRecebida.SetActive(true);
                break;
        }
    }

    private void ExecutarDesbloqueioVisualGolpista()
    {
        if (contatoBloqueadoGolpista == null || Refugee_Get_CanvasGroup(contatoBloqueadoGolpista) == null)
        {
            if (contatoBloqueadoGolpista != null) contatoBloqueadoGolpista.SetActive(false);
            if (contatoDesbloqueadoGolpista != null) contatoDesbloqueadoGolpista.SetActive(true);
            return;
        }

        CanvasGroup cgBloqueado = Refugee_Get_CanvasGroup(contatoBloqueadoGolpista);
        CanvasGroup cgDesbloqueado = Refugee_Get_CanvasGroup(contatoDesbloqueadoGolpista);

        contatoDesbloqueadoGolpista.SetActive(true);
        cgDesbloqueado.alpha = 0f;
        cgBloqueado.alpha = 1f;

        contatoBloqueadoGolpista.transform.DOScale(1.05f, 0.15f).SetLoops(2, LoopType.Yoyo);

        cgBloqueado.DOFade(0f, 0.6f).SetDelay(0.2f).OnComplete(() => {
            contatoBloqueadoGolpista.SetActive(false);
            contatoBloqueadoGolpista.transform.localScale = Vector3.one;
        });

        cgDesbloqueado.DOFade(1f, 0.6f).SetDelay(0.2f);
    }

    private CanvasGroup Refugee_Get_CanvasGroup(GameObject obj)
    {
        return obj != null ? obj.GetComponent<CanvasGroup>() : null;
    }

    private void IniciarEfeitoPiscarGolpista()
    {
        if (botaoContatoGolpista == null) return;

        Image imagemBotao = botaoContatoGolpista.GetComponent<Image>();
        if (imagemBotao == null) return;

        if (tweenPiscarVermelho != null) tweenPiscarVermelho.Kill();

        tweenPiscarVermelho = imagemBotao.DOColor(new Color(1f, 0.2f, 0.2f, 1f), 0.4f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }

    private void PararEfeitoPiscarGolpista()
    {
        if (tweenPiscarVermelho != null)
        {
            tweenPiscarVermelho.Kill();
            tweenPiscarVermelho = null;
        }

        if (botaoContatoGolpista != null)
        {
            Image imagemBotao = botaoContatoGolpista.GetComponent<Image>();
            if (imagemBotao != null) imagemBotao.color = Color.white;
        }
    }

    // --- FUNÇÕES PÚBLICAS E MÉTODOS DE PULSAÇÃO DO BOTÃO + ---

    /// <summary>
    /// Chamado externamente pelo SistemaDeChatPuzzle quando as flags booleanas do nó indicarem vitória.
    /// </summary>
    public void AtivarPulsoBotaoAdicionar()
    {
        Debug.Log("[GerenciadorDeNarrativa] Puzzle do golpista concluído com sucesso. Iniciando pulsação visual.");
        IniciarEfeitoPulsarBotaoMais();
    }

    private void IniciarEfeitoPulsarBotaoMais()
    {
        if (botaoAdicionarContato == null) return;

        if (tweenPulsarBotaoMais != null) tweenPulsarBotaoMais.Kill();

        botaoAdicionarContato.transform.localScale = Vector3.one;

        // Faz o botão pulsar (escala de 1.0x para 1.15x continuamente)
        tweenPulsarBotaoMais = botaoAdicionarContato.transform.DOScale(1.15f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }

    private void PararEfeitoPulsarBotaoMais()
    {
        if (tweenPulsarBotaoMais != null)
        {
            tweenPulsarBotaoMais.Kill();
            tweenPulsarBotaoMais = null;
        }

        if (botaoAdicionarContato != null)
        {
            botaoAdicionarContato.transform.localScale = Vector3.one;
        }
    }

    public void PuzzleResolvido()
    {
        AvancarHistoria();
    }
}