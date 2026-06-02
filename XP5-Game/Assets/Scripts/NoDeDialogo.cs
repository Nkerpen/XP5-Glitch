using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NovoDialogo", menuName = "Glitch/Dialogo de Chat")]
public class NoDeDialogo : ScriptableObject
{
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
    public float tempoDeDigitacao = 1.5f; // Simula o tempo do "Fulano está digitando..."
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