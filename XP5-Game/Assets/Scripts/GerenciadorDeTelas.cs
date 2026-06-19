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
    /// <param name="idTelaParaAbrir">IDs válidos: Mailbox, Notas, Caesar, EmailPuzzle, EmailAnthony, Contatos, Chat</param>
    public void AbrirTela(string idTelaParaAbrir)
    {
        if (string.IsNullOrEmpty(idTelaParaAbrir)) return;

        string idTratado = idTelaParaAbrir.Trim();

        if (idTratado.ToLower() == "home")
        {
            VoltarParaHome();
            return;
        }

        Debug.Log($"[ScreenManager] Solicitando abertura do App/Subtela: <color=yellow>{idTratado}</color>");

        // --- SISTEMA DE HISTÓRICO ---
        // Salva onde estávamos antes de mudar
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
            // Sem histórico? Força a Home por segurança
            VoltarParaHome();
        }
    }

    private bool ExecutarTrocaVisivel(string idAlvo)
    {
        bool encontrouTela = false;
        bool abrindoCaesar = idAlvo.ToLower() == "caesar" || idAlvo.ToLower() == "notacaesar";
        bool abrindoChat = idAlvo.ToLower() == "chat";

        foreach (var tela in todasAsTelas)
        {
            if (tela.painelDaTela == null) continue;

            string idConfigurado = tela.idDaTela.Trim().ToLower();

            // Regra de ativação do ID exato
            if (idConfigurado == idAlvo.ToLower())
            {
                tela.painelDaTela.SetActive(true);
                encontrouTela = true;
                AplicarFadeSeConfigurado(tela);
                Debug.Log($"<color=green>[ScreenManager] Tela [{tela.idDaTela}] ATIVADA.</color>");
            }
            // Regras especiais de contingência (Telas filhas / aninhadas)
            else if (abrindoCaesar && idConfigurado == "notas")
            {
                tela.painelDaTela.SetActive(true);
            }
            else if (abrindoChat && idConfigurado == "contatos")
            {
                tela.painelDaTela.SetActive(true);
            }
            // Desativa o restante
            else
            {
                tela.painelDaTela.SetActive(false);
            }
        }

        return encontrouTela;
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

    private void Update()
    {
        // --- CORREÇÃO DO NOVO INPUT SYSTEM ---
        // Obtém o teclado usando o pacote Input System ativo
        var teclado = UnityEngine.InputSystem.Keyboard.current;
        if (teclado == null) return;

        // Atalho de teclado para testar o botão voltar no editor do Unity (Tecla Esc)
        if (teclado.escapeKey.wasPressedThisFrame)
        {
            BotaoVoltarFisico();
        }
    }

    private void AktualizarRelogio() // Corrigido erro de digitação para manter consistência interna
    {
        AtualizarRelogio();
    }

    private void CorrigirChamada() { } // Apenas metódo auxiliar de segurança

    private void OnValidate() { } // Proteção do Editor

    private void AtualizarRelogio()
    {
        if (relogio == null) return;

        // Mantém o relógio visível apenas na Home
        bool mostrarRelogio = (idTelaAtual == "Home");
        relogio.SetVisible(mostrarRelogio);
    }
}