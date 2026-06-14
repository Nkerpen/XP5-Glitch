using UnityEngine;
using UnityEngine.UI;

public class SistemaEmailPuzzle : MonoBehaviour
{
    [Header("Telas do Sistema")]
    [SerializeField] private GameObject painelGlitchGameOver;
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject corpoDoEmail;
    [SerializeField] private GameObject painelCaixaVazia;

    [Header("Referências de Navegação")]
    [Tooltip("Arraste aqui o objeto que tem o script NavegacaoCelular")]
    [SerializeField] private NavegacaoCelular gerenciadorNavegacao;

    [Tooltip("Arraste aqui o objeto irmão 'Tela_Mailbox' da hierarquia")]
    [SerializeField] private GameObject telaMailbox;

    [Tooltip("Arraste aqui o objeto principal pai 'Tela_Email'")]
    [SerializeField] private GameObject objetoTelaEmailPrincipal;

    [Header("Elementos Interativos")]
    [SerializeField] private Button botaoLinkArmadilha;
    [SerializeField] private Button[] botoesDeErro;

    private int errosEncontrados = 0;
    private int totalErros;

    // TRAVA DE SEGURANÇA: Salva se o puzzle já foi finalizado nesta sessão
    private bool puzzleConcluido = false;

    private void OnEnable()
    {
        // CORREÇÃO CRÍTICA: Se o jogador já completou o puzzle e navegou de volta para cá,
        // mantém APENAS a caixa vazia ativa. O painel de vitória DEVE continuar falso
        // para não bloquear os cliques da barra de navegação inferior!
        if (puzzleConcluido)
        {
            if (corpoDoEmail != null) corpoDoEmail.SetActive(false);
            if (painelCaixaVazia != null) painelCaixaVazia.SetActive(true);
            if (painelGlitchGameOver != null) painelGlitchGameOver.SetActive(false);
            if (painelVitoria != null) painelVitoria.SetActive(false); // Mantém desligado!
            return; // Corta a execução aqui com o estado de caixa de entrada limpa
        }

        // Reset normal do jogo caso ele ainda esteja jogando o puzzle
        totalErros = botoesDeErro.Length;
        errosEncontrados = 0;

        if (painelGlitchGameOver != null) painelGlitchGameOver.SetActive(false);
        if (painelVitoria != null) painelVitoria.SetActive(false);

        if (corpoDoEmail != null) corpoDoEmail.SetActive(true);
        if (painelCaixaVazia != null) painelCaixaVazia.SetActive(false);

        // Configuração do botão de Phishing (Armadilha)
        if (botaoLinkArmadilha != null)
        {
            botaoLinkArmadilha.gameObject.SetActive(true);
            botaoLinkArmadilha.interactable = true;
            botaoLinkArmadilha.onClick.RemoveAllListeners();
            botaoLinkArmadilha.onClick.AddListener(ClicarNaArmadilha);
        }

        // Configuração dos botões invisíveis de falha
        foreach (Button btn in botoesDeErro)
        {
            if (btn == null) continue;

            btn.gameObject.SetActive(true);
            btn.interactable = true;

            // Deixa a imagem transparente/invisível no início
            Color corInvisivel = Color.red;
            corInvisivel.a = 0f;
            btn.GetComponent<Image>().color = corInvisivel;

            btn.onClick.RemoveAllListeners();

            // Evita problemas de escopo na Unity
            Button botaoLocal = btn;
            botaoLocal.onClick.AddListener(() => EncontrarErro(botaoLocal));
        }
    }

    private void ClicarNaArmadilha()
    {
        Debug.Log("O jogador caiu no Phishing!");

        if (painelGlitchGameOver != null)
            painelGlitchGameOver.SetActive(true);

        // Esconde os botões de erro para limpar a tela de Game Over
        foreach (Button btn in botoesDeErro)
        {
            if (btn != null)
            {
                btn.gameObject.SetActive(false);
            }
        }

        if (botaoLinkArmadilha != null)
        {
            botaoLinkArmadilha.interactable = false;
        }
    }

    private void EncontrarErro(Button botaoClicado)
    {
        errosEncontrados++;

        // Feedback visual de acerto (revela a marcação vermelha)
        Color corMarcada = Color.red;
        corMarcada.a = 0.4f;
        botaoClicado.GetComponent<Image>().color = corMarcada;

        botaoClicado.interactable = false;

        Debug.Log($"Falhas encontradas: {errosEncontrados}/{totalErros}");

        // Condição de Vitória
        if (errosEncontrados >= totalErros)
        {
            puzzleConcluido = true; // Ativa a trava definitiva

            if (corpoDoEmail != null) corpoDoEmail.SetActive(false);
            if (painelCaixaVazia != null) painelCaixaVazia.SetActive(true);
            if (painelVitoria != null) painelVitoria.SetActive(true); // Ativa para mostrar o "Continuar"

            if (botaoLinkArmadilha != null)
            {
                botaoLinkArmadilha.interactable = false;
            }
        }
    }

    /// <summary>
    /// Vincule esta função ao OnClick() do botão "Continuar" da tela de vitória!
    /// </summary>
    public void BotaoContinuarVitoria()
    {
        // 1. Apaga o painel de vitória da tela imediatamente
        if (painelVitoria != null)
            painelVitoria.SetActive(false);

        // 2. Desativa ESPECIFICAMENTE a Tela_Email usando a referência direta do Inspector
        if (objetoTelaEmailPrincipal != null)
        {
            objetoTelaEmailPrincipal.SetActive(false);
        }
        else
        {
            // Fallback de segurança usando parent antigo caso a referência direta suma
            if (transform.parent != null) transform.parent.gameObject.SetActive(false);
        }

        // 3. Ativa a tela irmã da Mailbox de verdade através da navegação do celular
        if (telaMailbox != null)
        {
            if (gerenciadorNavegacao != null)
            {
                gerenciadorNavegacao.AbrirApp(telaMailbox);
            }
            else
            {
                telaMailbox.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Faltando referenciar a 'Tela Mailbox' no Inspector!");
            if (gerenciadorNavegacao != null) gerenciadorNavegacao.BotaoHome();
        }
    }

    /// <summary>
    /// Vincule esta função ao OnClick() do seu botão "Tentar Novamente".
    /// </summary>
    public void TentarNovamente()
    {
        Debug.Log("Reiniciando o puzzle de e-mail de forma segura...");

        Time.timeScale = 1f;
        puzzleConcluido = false; // Reseta a trava

        if (painelGlitchGameOver != null)
            painelGlitchGameOver.SetActive(false);

        // Atualiza o estado chamando o OnEnable manualmente
        OnEnable();
    }
}