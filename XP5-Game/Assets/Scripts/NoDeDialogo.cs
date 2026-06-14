using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NovoDialogo", menuName = "Glitch/Dialogo de Chat")]
public class NoDeDialogo : ScriptableObject
{
    [Header("Identificadores de Progresso (Trava)")]
    [Tooltip("O nome do chat completo. Use o mesmo para todos os nós desta mesma conversa (Ex: Grupo_Investigacao)")]
    public string idDaConversa;

    [Tooltip("O ID único deste bloco específico de texto. Use para sabermos onde o player parou (Ex: No_Inicial, Rota_A_Ganhou)")]
    public string idDoNo;

    [Header("Mensagens do Grupo antes da sua resposta")]
    public List<MensagemNPC> mensagens;

    [Header("Escolhas do Jogador")]
    public RespostaJogador[] escolhas;
}

[System.Serializable]
public class MensagemNPC
{
    public Personagem autor;
    [TextArea(2, 4)]
    public string textoDaMensagem;
    public float tempoDeDigitacao = 1.5f;
}

[System.Serializable]
public class RespostaJogador
{
    [TextArea(2, 3)]
    public string textoDaEscolha;
    public NoDeDialogo proximoNo;
    public bool encerraPuzzle;
    public bool jogadorGanhou;
}