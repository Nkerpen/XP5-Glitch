using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events; // Necessário para a mágica do botão

public class SistemaDeNotificacao : MonoBehaviour
{
    [Header("UI da Notificação")]
    [SerializeField] private GameObject painelNotificacao;
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoMensagem;
    [SerializeField] private Button botaoNotificacao; // Adicione um componente Button no Painel_Notificacao!

    [Header("Configurações")]
    [SerializeField] private float tempoNaTela = 4f;

    private Coroutine rotinaAtual;

    private void Start()
    {
        if (painelNotificacao != null) painelNotificacao.SetActive(false);
    }

    // Agora ela recebe a ação de clicar!
    public void MostrarNotificacao(string titulo, string mensagem, UnityAction acaoAoClicar = null)
    {
        if (rotinaAtual != null) StopCoroutine(rotinaAtual);

        // Prepara o botão
        if (botaoNotificacao != null)
        {
            botaoNotificacao.onClick.RemoveAllListeners();
            if (acaoAoClicar != null)
            {
                botaoNotificacao.onClick.AddListener(() => {
                    acaoAoClicar(); // Executa a abertura do App
                    painelNotificacao.SetActive(false); // Fecha a notificação
                });
            }
        }

        rotinaAtual = StartCoroutine(RotinaExibirNotificacao(titulo, mensagem));
    }

    private IEnumerator RotinaExibirNotificacao(string titulo, string mensagem)
    {
        textoTitulo.text = titulo;
        textoMensagem.text = mensagem;
        painelNotificacao.SetActive(true);
        yield return new WaitForSeconds(tempoNaTela);
        painelNotificacao.SetActive(false);
    }
}