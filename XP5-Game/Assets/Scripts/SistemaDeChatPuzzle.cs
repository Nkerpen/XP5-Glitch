using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SistemaDeChatPuzzle : MonoBehaviour
{
    [Header("Estrutura da UI")]
    [SerializeField] private Transform contentArea;
    [SerializeField] private GameObject prefabBalaoNPC;
    [SerializeField] private GameObject prefabBalaoJogador;

    [Header("Painel de Escolhas")]
    [SerializeField] private GameObject painelEscolhas;
    [SerializeField] private Button[] botoesDeEscolha;
    [SerializeField] private TextMeshProUGUI[] textosDosBotoes;

    [Header("Dados do Puzzle / Chat")]
    [SerializeField] private NoDeDialogo dialogoInicial;
    private NoDeDialogo dialogoAtual;

    [Header("Telas de Fim de Jogo")]
    [SerializeField] private GameObject painelGameOver;
    // Se quiser, pode criar um painelVitoria também no futuro!

    public void IniciarChat(NoDeDialogo inicio)
    {
        // Limpa as mensagens da conversa anterior
        foreach (Transform filho in contentArea) Destroy(filho.gameObject);

        // Inicia a nova conversa
        dialogoAtual = inicio;
        painelEscolhas.SetActive(false);
        StartCoroutine(TocarMensagensDoNPC());
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
            textos[0].text = msg.autor.nome;
            textos[0].color = msg.autor.corDoNome;
            textos[1].text = msg.textoDaMensagem;

            // --- A MÁGICA DA COR ENTRA AQUI ---
            // Procura o componente Image (que está no BalaoNpc) e pinta com a cor do Personagem
            Image fundoBalao = balao.GetComponentInChildren<Image>();
            if (fundoBalao != null)
            {
                fundoBalao.color = msg.autor.corDoBalao;
            }
        }

        // Quando todas as mensagens forem enviadas, mostra as opções pro jogador
        AtualizarBotoesDeEscolha();
    }
    private void AtualizarBotoesDeEscolha()
    {
        painelEscolhas.SetActive(true);
        foreach (var btn in botoesDeEscolha) btn.gameObject.SetActive(false);

        for (int i = 0; i < dialogoAtual.escolhas.Length; i++)
        {
            botoesDeEscolha[i].gameObject.SetActive(true);
            textosDosBotoes[i].text = dialogoAtual.escolhas[i].textoDaEscolha;

            int indexCopia = i;
            botoesDeEscolha[i].onClick.RemoveAllListeners();
            botoesDeEscolha[i].onClick.AddListener(() => FazerEscolha(indexCopia));
        }
    }

    private void FazerEscolha(int index)
    {
        RespostaJogador escolha = dialogoAtual.escolhas[index];

        // Balão do Jogador
        GameObject balao = Instantiate(prefabBalaoJogador, contentArea);
        balao.GetComponentInChildren<TextMeshProUGUI>().text = escolha.textoDaEscolha;

        painelEscolhas.SetActive(false);

        if (escolha.encerraPuzzle)
        {
            if (escolha.jogadorGanhou)
            {
                Debug.Log("Vitória! O jogador não caiu no golpe.");
                // Futuramente você liga uma tela de vitória ou libera uma pista nova aqui.
            }
            else
            {
                Debug.Log("Game Over!");
                // Mostra a tela de erro
                painelGameOver.SetActive(true);
            }
            return; // Termina a função para não tentar carregar a próxima mensagem
        }

        // Se não encerrou o puzzle, continua o papo normalmente...
        dialogoAtual = escolha.proximoNo;
        StartCoroutine(TocarMensagensDoNPC());

        dialogoAtual = escolha.proximoNo;
        StartCoroutine(TocarMensagensDoNPC());
    }
}