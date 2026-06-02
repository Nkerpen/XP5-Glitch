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

    public void IniciarChat(NoDeDialogo inicio)
    {
        // Limpa as mensagens da conversa anterior
        foreach (Transform filho in contentArea) Destroy(filho.gameObject);

        // Inicia a nova conversa
        dialogoAtual = inicio;
        painelEscolhas.SetActive(false);
        StartCoroutine(TocarMensagensDoNPC());
    }

    // Usaremos essa função pública depois, para carregar o Chat 1 ou o Chat do Puzzle!
    public void IniciarChat(NoDeDialogo inicio)
    {
        dialogoAtual = inicio;
        painelEscolhas.SetActive(false); // Esconde os botões
        StartCoroutine(TocarMensagensDoNPC());
    }

    private IEnumerator TocarMensagensDoNPC()
    {
        // Lê cada mensagem da lista do grupo
        foreach (MensagemNPC msg in dialogoAtual.mensagens)
        {
            // Espera o tempo de digitação (ex: 1.5s)
            yield return new WaitForSeconds(msg.tempoDeDigitacao);

            // Cria o balão
            GameObject balao = Instantiate(prefabBalaoNPC, contentArea);

            // Pega as referências de texto dentro do balão (0 é o Nome, 1 é a Mensagem)
            TextMeshProUGUI[] textos = balao.GetComponentsInChildren<TextMeshProUGUI>();

            // Aplica os dados do Personagem
            textos[0].text = msg.autor.nome;
            textos[0].color = msg.autor.corDoNome;
            textos[1].text = msg.textoDaMensagem;

            // Se quiser mudar a cor de fundo do balão:
            // balao.GetComponent<Image>().color = msg.autor.corDoBalao;
        }

        // Quando todas as mensagens do grupo forem enviadas, mostra as opções pro jogador
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

        if (escolha.encerraPuzzle) return;

        dialogoAtual = escolha.proximoNo;
        StartCoroutine(TocarMensagensDoNPC());
    }
}