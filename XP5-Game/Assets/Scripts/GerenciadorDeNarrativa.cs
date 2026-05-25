using System.Collections;
using UnityEngine;

public class GerenciadorDeNarrativa : MonoBehaviour
{
    [Header("Dependências")]
    // Referência ao script de notificação que criamos antes
    [SerializeField] private SistemaDeNotificacao sistemaDeNotificacao;

    [Header("Configurações de Tempo")]
    [SerializeField] private float atrasoPrimeiraMensagem = 3f;

    // O OnEnable é chamado automaticamente quando o GameObject é ativado na cena
    private void OnEnable()
    {
        StartCoroutine(EnviarPrimeiraMensagem());
    }

    private IEnumerator EnviarPrimeiraMensagem()
    {
        // Aguarda os segundos estipulados após a Home aparecer
        yield return new WaitForSeconds(atrasoPrimeiraMensagem);

        // Puxando o diálogo direto do seu GDD para a Rota 1
        sistemaDeNotificacao.MostrarNotificacao("Você tem um novo Email");
    }
}