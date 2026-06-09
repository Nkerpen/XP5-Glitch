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
    [SerializeField] private ScrollRect scrollDoChat;

    [Header("Painel de Escolhas")]
    [SerializeField] private GameObject painelEscolhas;
    [SerializeField] private Button[] botoesDeEscolha;
    [SerializeField] private TextMeshProUGUI[] textosDosBotoes;

    [Header("Dados do Puzzle / Chat")]
    [SerializeField] private NoDeDialogo dialogoInicial;
    private NoDeDialogo dialogoAtual;
    private Coroutine rotinaDeMensagens;

 [Header("Telas de Fim de Jogo")]
    [SerializeField] private GameObject painelGameOver;
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject botaoContatoGolpista; // Arraste o botão "Numero_Anonimo" aqui

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

           // verifica se existe a foto, se existir declara a foto do personagem no chat
            Transform fotoTransform = balao.transform.Find("FotoPersonagem");
            if (fotoTransform != null)
            {
                Image fotoImage = fotoTransform.GetComponent<Image>();
                if (fotoImage != null && msg.autor.foto != null)
                {
                    fotoImage.sprite = msg.autor.foto;
                }
            }
            
            // --- A MÁGICA DA COR ENTRA AQUI ---
            // Procura o componente Image (que está no BalaoNpc) e pinta com a cor do Personagem
            Transform balaoTransform = balao.transform.Find("balaoNPC");
            if (balaoTransform != null)
            {
                Image fundoBalao = balaoTransform.GetComponent<Image>();
                if (fundoBalao != null)
                {
                    fundoBalao.color = msg.autor.corDoBalao;
                }
            }
            // --- A MÁGICA DA COR ENTRA AQUI ---
            // Procura o componente Image (que está no BalaoNpc) e pinta com a cor do Personagem
            //Image fundoBalao = balao.GetComponentInChildren<Image>();
            //if (fundoBalao != null)
            //{
            //    fundoBalao.color = msg.autor.corDoBalao;
            //}
            // Empurra a tela para baixo após a mensagem do NPC aparecer
            StartCoroutine(ForcarScrollParaBaixo());
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
    }

    private IEnumerator ForcarScrollParaBaixo()
    {
        // Força a Unity a atualizar as caixas do Layout Group
        Canvas.ForceUpdateCanvases();
        
        // Espera a tela terminar de desenhar o frame atual
        yield return new WaitForEndOfFrame();
        
        // Empurra a barra de rolagem para o fundo (0 é o fundo, 1 é o topo)
        if (scrollDoChat != null)
        {
            scrollDoChat.verticalNormalizedPosition = 0f;
        }
    }

    private void FazerEscolha(int index)
    {
        RespostaJogador escolha = dialogoAtual.escolhas[index];

        // Balão do Jogador
        GameObject balao = Instantiate(prefabBalaoJogador, contentArea);
        balao.GetComponentInChildren<TextMeshProUGUI>().text = escolha.textoDaEscolha;

        // Empurra a tela para baixo após a sua mensagem aparecer
        StartCoroutine(ForcarScrollParaBaixo());

        painelEscolhas.SetActive(false);

       if (escolha.encerraPuzzle)
        {
            if (escolha.jogadorGanhou)
            {
                Debug.Log("Vitória! O jogador não caiu no golpe.");
                
                // --- APAGA O CONTATO DA LISTA ---
                if (botaoContatoGolpista != null)
                {
                    botaoContatoGolpista.SetActive(false); 
                }

                painelVitoria.SetActive(true); 
            }
            else
            {
                Debug.Log("Game Over!");
                painelGameOver.SetActive(true);
            }
            return; 
        }

        // Se não encerrou o puzzle, continua o papo normalmente...
        dialogoAtual = escolha.proximoNo;
        // Se já existir alguém digitando, manda parar imediatamente
        if (rotinaDeMensagens != null) 
        {
            StopCoroutine(rotinaDeMensagens);
        }

        // Começa a nova rotina e salva ela na variável
        rotinaDeMensagens = StartCoroutine(TocarMensagensDoNPC());
    }
}