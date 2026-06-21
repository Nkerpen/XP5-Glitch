using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GerenciadorDeTelas : MonoBehaviour
{
    public static GerenciadorDeTelas Instancia { get; private set; }

    [System.Serializable]
    public struct TelaConfig
    {
        public string idDaTela;
        public GameObject painelDaTela;
        [Tooltip("Opcional: Se marcado, esta tela usa um CanvasGroup para fazer efeito de Fade ao abrir.")]
        public CanvasGroup canvasGroupDaTela;
    }

    [Header("Estrutura da Hierarquia (Canvas)")]
    [Tooltip("Arraste o objeto 'Tela_Aplicativos' que é o painel pai de Mailbox, Notas, Chat, etc.")]
    [SerializeField] private GameObject painelMãeTelaAplicativos;
    [SerializeField] private GameObject telaHome;

    [Header("Configuração de Telas Principais")]
    [SerializeField] private List<TelaConfig> todasAsTelas;

    [Header("Configurações de Transição (DOTween)")]
    [SerializeField] private bool usarFade = true;
    [SerializeField] private float duracaoFade = 0.2f;

    [Header("HUD / Elementos de Interface")]
    [SerializeField] private ClockController relogio;

    // Sistema de histórico usando a lógica de Pilha (Stack)
    private string idTelaAtual = "Home";
    private Stack<string> historicoTelas = new Stack<string>();

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Garante as referências cruciais
        if (painelMãeTelaAplicativos == null || telaHome == null)
        {
            Debug.LogError($"[ScreenManager] ERRO CRÍTICO: Referências cruciais (Tela_Aplicativos ou Tela_Home) faltando no Inspector de {gameObject.name}!");
        }

        VoltarParaHome();
    }

    /// <summary>
    /// Vinculado ao botão 'Home' físico da barra inferior.
    /// Limpa o histórico e fecha absolutamente tudo, voltando para a tela inicial.
    /// </summary>
    public void VoltarParaHome()
    {
        Debug.Log("[ScreenManager] Voltando para a Tela Home (Histórico limpo).");

        if (telaHome != null)
            telaHome.SetActive(true);

        if (painelMãeTelaAplicativos != null)
            painelMãeTelaAplicativos.SetActive(false);

        foreach (var tela in todasAsTelas)
        {
            if (tela.painelDaTela != null)
            {
                if (tela.canvasGroupDaTela != null)
                    tela.canvasGroupDaTela.alpha = 0f;

                tela.painelDaTela.SetActive(false);
            }
        }

        idTelaAtual = "Home";
        historicoTelas.Clear(); // Estaca zero, limpa a navegação anterior

        AtualizarRelogio();
    }

    /// <summary>
    /// Abre o aplicativo ou sub-tela correspondente ao ID enviado, salvando a tela anterior na memória.
    /// </summary>
    public void AbrirTela(string idTelaParaAbrir)
    {
        if (string.IsNullOrEmpty(idTelaParaAbrir)) return;

        string idTratado = idTelaParaAbrir.Trim();

        if (idTratado.ToLower() == "home")
        {
            StringVoltarHomeSegura();
            return;
        }

        Debug.Log($"[ScreenManager] Solicitando abertura do App/Subtela: <color=yellow>{idTratado}</color>");

        // --- SISTEMA DE HISTÓRICO ---
        if (idTelaAtual != idTratado)
        {
            historicoTelas.Push(idTelaAtual);
        }

        // Liga a mãe de todos os apps para garantir renderização dos filhos
        if (painelMãeTelaAplicativos != null)
            painelMãeTelaAplicativos.SetActive(true);

        bool encontrouTela = ExecutarTrocaVisivel(idTratado);

        if (encontrouTela)
        {
            if (telaHome != null)
                telaHome.SetActive(false);

            idTelaAtual = idTratado;
        }
        else
        {
            // Se falhou, remove o histórico falso empilhado e desliga a mãe
            if (historicoTelas.Count > 0) historicoTelas.Pop();

            if (painelMãeTelaAplicativos != null && idTelaAtual == "Home")
                painelMãeTelaAplicativos.SetActive(false);

            Debug.LogError($"[ScreenManager] Erro: ID de tela '{idTelaParaAbrir}' não foi encontrado no Inspector!");
        }

        AtualizarRelogio();
    }

    /// <summary>
    /// Vinculado ao botão físico de 'Seta/Voltar' da barra inferior do celular.
    /// Regressa exatamente para o aplicativo ou menu que o jogador estava visualizando.
    /// </summary>
    public void BotaoVoltarFisico()
    {
        if (historicoTelas.Count > 0)
        {
            string idAnterior = historicoTelas.Pop();
            Debug.Log($"[ScreenManager] Retornando histórico para: <color=cyan>{idAnterior}</color>");

            if (idAnterior == "Home")
            {
                VoltarParaHome();
                return;
            }

            bool encontrou = ExecutarTrocaVisivel(idAnterior);
            if (encontrou) idTelaAtual = idAnterior;

            AtualizarRelogio();
        }
        else
        {
            VoltarParaHome();
        }
    }

    private bool ExecutarTrocaVisivel(string idAlvo)
    {
        bool encontrouTela = false;
        string alvoLower = idAlvo.ToLower();

        bool abrindoCaesar = alvoLower == "caesar" || alvoLower == "notacaesar";
        bool abrindoChat = alvoLower == "chat";

        foreach (var tela in todasAsTelas)
        {
            if (tela.painelDaTela == null) continue;

            string idConfigurado = tela.idDaTela.Trim().ToLower();

            // 1. Regra de ativação do ID exato (Liga o painel atual solicitado)
            if (idConfigurado == alvoLower)
            {
                tela.painelDaTela.SetActive(true);
                encontrouTela = true;
                AplicarFadeSeConfigurado(tela);
                Debug.Log($"<color=green>[ScreenManager] Tela [{tela.idDaTela}] ATIVADA.</color>");
            }
            // 2. Regras especiais de contingência (Telas filhas / aninhadas)
            else if (abrindoCaesar && idConfigurado == "notas")
            {
                tela.painelDaTela.SetActive(true);
            }
            else if (abrindoChat && idConfigurado == "contatos")
            {
                tela.painelDaTela.SetActive(true);
            }
            // 3. REGRA GERAL BASEADA EM PANELS: Se não for a tela atual ou uma sub-tela válida, desliga o painel completamente.
            else
            {
                tela.painelDaTela.SetActive(false);
            }
        }

        return encontrouTela;
    }

    private void AplicarFadeSeConfigurado(TelaConfig tela)
    {
        if (tela.canvasGroupDaTela != null)
        {
            tela.canvasGroupDaTela.blocksRaycasts = true;
            tela.canvasGroupDaTela.interactable = true;

            if (usarFade)
            {
                tela.canvasGroupDaTela.DOKill();
                tela.canvasGroupDaTela.alpha = 0f;
                tela.canvasGroupDaTela.DOFade(1f, duracaoFade).SetUpdate(true);
            }
            else
            {
                tela.canvasGroupDaTela.alpha = 1f;
            }
        }
    }

    private void Update()
    {
        // --- CORREÇÃO DO NOVO INPUT SYSTEM ---
        var teclado = UnityEngine.InputSystem.Keyboard.current;
        if (teclado == null) return;

        // Atalho de teclado para testar o botão voltar no editor do Unity (Tecla Esc)
        if (teclado.escapeKey.wasPressedThisFrame)
        {
            BotaoVoltarFisico();
        }
    }

    /// <summary>
    /// Força o fechamento imediato de qualquer interface de aplicativo ou puzzle aberta,
    /// limpando a navegação e abrindo o layout registrado de ligação recebida.
    /// </summary>
    public void ForçarFechamentoParaChamada()
    {
        Debug.LogWarning("[ScreenManager] Limpando telas para a entrada do Cliffhanger de Voz.");

        // 1. Desativamos manualmente os painéis antigos para evitar problemas de concorrência com IDs falsos
        if (telaHome != null)
            telaHome.SetActive(false);

        if (painelMãeTelaAplicativos != null)
            painelMãeTelaAplicativos.SetActive(false);

        // 2. Como o objeto pai "Ligações" contém o GerenciadorDeLigacao, garantimos que 
        // o script ganhe contexto de execução ativo caso ele estivesse desativado na hierarquia.
        var scriptLigacao = GerenciadorDeLigacao.Instancia;
        if (scriptLigacao == null)
        {
            scriptLigacao = FindAnyObjectByType<GerenciadorDeLigacao>(FindObjectsInactive.Include);
        }

        if (scriptLigacao != null)
        {
            scriptLigacao.gameObject.SetActive(true);

            // Garante visibilidade e interatividade caso o objeto pai "Ligações" possua um CanvasGroup
            CanvasGroup cgPai = scriptLigacao.GetComponent<CanvasGroup>();
            if (cgPai != null)
            {
                cgPai.alpha = 1f;
                cgPai.blocksRaycasts = true;
                cgPai.interactable = true;
            }
        }

        // 3. Limpa a pilha de navegação para evitar que o jogador aperte "Voltar" durante uma chamada prioritária
        historicoTelas.Clear();

        // 4. Abre diretamente o layout da chamada recebida registrado na sua lista struct
        AbrirTela("LigaçãoRecebida");
    }

    private void StringVoltarHomeSegura()
    {
        VoltarParaHome();
    }

    private void UpdateRelogio() // Alias para compatibilidade caso necessário
    {
        AtualizarRelogio();
    }

    private void Desktop_UpdateRelogio() // Caso tenha dependências externas
    {
        AtualizarRelogio();
    }

    private void @AtualizarRelogio()
    {
        if (relogio == null) return;

        // Mantém o relógio visível apenas na Home
        bool mostrarRelogio = (idTelaAtual == "Home");
        relogio.SetVisible(mostrarRelogio);
    }
}