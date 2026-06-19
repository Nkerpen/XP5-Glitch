using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NavegacaoCelular : MonoBehaviour
{
    public static NavegacaoCelular Instancia { get; private set; }

    [System.Serializable]
    public struct TelaConfig
    {
        public string idDaTela;
        public GameObject painelDaTela;
        [Tooltip("Opcional: CanvasGroup para efeito de Fade ao abrir.")]
        public CanvasGroup canvasGroupDaTela;
    }

    [Header("Configurações de Estrutura")]
    [SerializeField] private GameObject telaHome;
    [Tooltip("Arraste aqui o objeto pai 'Tela_Aplicativos'")]
    [SerializeField] private GameObject telaAplicativos;

    [Header("Mapeamento das Telas do Celular")]
    [Tooltip("IDs sugeridos: Contatos, Chat, Mailbox, EmailPuzzle, EmailDetective, Notas, NotasCaesar")]
    [SerializeField] private List<TelaConfig> todasAsTelas;

    [Header("Configurações de Transição (DOTween)")]
    [SerializeField] private bool usarFade = true;
    [SerializeField] private float duracaoFade = 0.2f;

    [Header("HUD")]
    [SerializeField] private ClockController relogio;

    private string idTelaAtual = "Home";
    private Stack<string> historicoTelas = new Stack<string>();

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (telaAplicativos == null || telaHome == null)
        {
            Debug.LogError($"[NavegacaoCelular] ERRO CRÍTICO: Referências de estrutura faltando no Inspector!");
        }

        BotaoHome(); // Inicializa o celular limpo na Home
    }

    private void Update()
    {
        // Suporte ao Novo Input System para o botão Voltar (Tecla Esc no PC)
        var teclado = UnityEngine.InputSystem.Keyboard.current;
        if (teclado == null) return;

        if (teclado.escapeKey.wasPressedThisFrame)
        {
            BotaoVoltar();
        }
    }

    /// <summary>
    /// Use esta função nos botões do seu fluxo para avançar de tela.
    /// Ex: botão do ícone de Chat chama AbrirApp("Contatos"). Botão do contato chama AbrirApp("Chat").
    /// </summary>
    public void AbrirApp(string idTelaParaAbrir)
    {
        if (string.IsNullOrEmpty(idTelaParaAbrir)) return;
        string idTratado = idTelaParaAbrir.Trim();

        if (idTratado.ToLower() == "home")
        {
            BotaoHome();
            return;
        }

        // Evita duplicar a mesma tela no histórico se o jogador clicar duas vezes seguidas
        if (idTelaAtual == idTratado) return;

        Debug.Log($"[NavegacaoCelular] Avançando fluxo. Tela Atual antes da mudança: {idTelaAtual}");

        // --- CORREÇÃO DE FLUXO DA PILHA ---
        // Se estávamos na Home, limpa qualquer resquício antigo e empilha a Home como base estável
        if (idTelaAtual == "Home")
        {
            historicoTelas.Clear();
            historicoTelas.Push("Home");
        }
        else
        {
            // Se já estávamos dentro de um app e fomos para uma sub-tela (ex: Contatos -> Chat)
            historicoTelas.Push(idTelaAtual);
        }

        // Ativa a estrutura de aplicativos e esconde a Home do celular
        if (telaAplicativos != null) telaAplicativos.SetActive(true);
        if (telaHome != null) telaHome.SetActive(false);

        bool encontrou = ExecutarTrocaDePainelVisivel(idTratado);

        if (encontrou)
        {
            idTelaAtual = idTratado;
            Debug.Log($"[NavegacaoCelular] Nova Tela Atual definida como: {idTelaAtual}. Total na Pilha agora: {historicoTelas.Count}");
        }
        else
        {
            // Se der erro de digitação de ID, remove da pilha para não quebrar o fluxo
            if (historicoTelas.Count > 0) historicoTelas.Pop();
            Debug.LogError($"[NavegacaoCelular] Erro: ID '{idTelaParaAbrir}' não foi encontrado na lista mapeada!");
        }

        AtualizarRelogio();
    }

    /// <summary>
    /// Vinculado ao botão físico de "Seta/Voltar" da barra inferior do celular.
    /// Desempilha o histórico e gerencia a ativação correta das telas de menu.
    /// </summary>
    public void BotaoVoltar()
    {
        Debug.Log($"[NavegacaoCelular] Botão Voltar pressionado. Tamanho atual da pilha: {historicoTelas.Count}");

        if (historicoTelas.Count > 0)
        {
            string idAnterior = historicoTelas.Pop();
            Debug.Log($"[NavegacaoCelular] Removendo da pilha. Voltando para: <color=cyan>{idAnterior}</color>");

            if (idAnterior == "Home")
            {
                // Se o topo da pilha diz que viemos da Home, limpa as sub-telas e reativa a Home
                RetornarParaEstruturaHome();
            }
            else
            {
                // Se voltamos para uma sub-tela de menu (ex: de 'Chat' para 'Contatos')
                if (telaAplicativos != null) telaAplicativos.SetActive(true);
                if (telaHome != null) telaHome.SetActive(false);

                bool encontrou = ExecutarTrocaDePainelVisivel(idAnterior);
                if (encontrou) idTelaAtual = idAnterior;
            }

            AtualizarRelogio();
        }
        else
        {
            Debug.LogWarning("[NavegacaoCelular] ATENÇÃO: O botão voltar foi clicado, mas a pilha de histórico está TOTALMENTE VAZIA (0)!");
            BotaoHome();
        }
    }

    /// <summary>
    /// Vinculado ao botão físico "Home" (Círculo ou Quadrado) da barra inferior.
    /// Limpa completamente o histórico por ser um reset de fluxo.
    /// </summary>
    public void BotaoHome()
    {
        Debug.Log("[NavegacaoCelular] Botão Home Pressionado. Histórico de navegação limpo.");
        RetornarParaEstruturaHome();
        historicoTelas.Clear(); // Limpa a pilha permanentemente
    }

    private void RetornarParaEstruturaHome()
    {
        if (telaHome != null) telaHome.SetActive(true);
        if (telaAplicativos != null) telaAplicativos.SetActive(false);

        foreach (var tela in todasAsTelas)
        {
            if (tela.painelDaTela != null) tela.painelDaTela.SetActive(false);
        }

        idTelaAtual = "Home";
        AtualizarRelogio();
    }

    /// <summary>
    /// O cérebro do fluxo: gerencia quais telas ligam e desligam de acordo com as regras do seu design.
    /// </summary>
    private bool ExecutarTrocaDePainelVisivel(string idAlvo)
    {
        bool encontrou = false;
        string alvoLower = idAlvo.ToLower();

        // Mapeamento lógico de sub-telas para prever o seu flow
        bool abrindoNotasCaesar = (alvoLower == "notascaesar");
        bool abrindoChatAtivo = (alvoLower == "chat");
        bool abrindoPuzzleEmail = (alvoLower == "emailpuzzle" || alvoLower == "emaildetective");

        foreach (var tela in todasAsTelas)
        {
            if (tela.painelDaTela == null) continue;

            string idConfigurado = tela.idDaTela.Trim().ToLower();

            // 1. Condição: É a tela exata que queremos abrir?
            if (idConfigurado == alvoLower)
            {
                tela.painelDaTela.SetActive(true);
                encontrou = true;
                AplicarFadeSeConfigurado(tela);
            }
            // 2. Condição de fluxo especial: Se abrir o puzzle de notas, a tela base de Notas pode opcionalmente sumir ou ficar por trás
            else if (abrindoNotasCaesar && idConfigurado == "notas")
            {
                tela.painelDaTela.SetActive(false);
            }
            // 3. Condição de fluxo especial: Se estiver no Chat ativo, esconde a lista de Contatos
            else if (abrindoChatAtivo && idConfigurado == "contatos")
            {
                tela.painelDaTela.SetActive(false);
            }
            // 4. Condição de fluxo especial: Se estiver lendo um e-mail/puzzle, esconde a Mailbox de fundo
            else if (abrindoPuzzleEmail && idConfigurado == "mailbox")
            {
                tela.painelDaTela.SetActive(false);
            }
            // Desativa as telas que não pertencem a este fluxo de forma alguma
            else
            {
                tela.painelDaTela.SetActive(false);
            }
        }
        return encontrou;
    }

    private void AplicarFadeSeConfigurado(TelaConfig tela)
    {
        if (usarFade && tela.canvasGroupDaTela != null)
        {
            tela.canvasGroupDaTela.DOKill();
            tela.canvasGroupDaTela.alpha = 0f;
            tela.canvasGroupDaTela.DOFade(1f, duracaoFade).SetUpdate(true);
        }
        else if (tela.canvasGroupDaTela != null)
        {
            tela.canvasGroupDaTela.alpha = 1f;
        }
    }

    private void AtualizarRelogio()
    {
        if (relogio == null) return;
        // O relógio só aparece visualmente se o jogador estiver na tela Home do celular
        bool naHome = (idTelaAtual == "Home");
        relogio.SetVisible(naHome);
    }
}