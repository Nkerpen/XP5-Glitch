using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class SistemaDeNotificacao : MonoBehaviour
{
    [Header("UI da Notificação")]
    [SerializeField] private GameObject painelNotificacao;
    [SerializeField] private RectTransform rectTransformNotificacao; // A âncora que vamos mover
    [SerializeField] private TextMeshProUGUI textoTitulo;
    [SerializeField] private TextMeshProUGUI textoMensagem;
    [SerializeField] private Button botaoNotificacao;

    [Header("Configurações de Animação")]
    [SerializeField] private float tempoNaTela = 4f;
    [SerializeField] private float velocidadeAnimacao = 0.4f; // Tempo que leva para descer/subir
    [SerializeField] private float posicaoYEscondida = 300f;  // Altura fora da tela (ajuste no Inspector)
    [SerializeField] private float posicaoYVisivel = -50f;    // Altura dentro da tela (ajuste no Inspector)

    private Coroutine rotinaAtual;

    private void Start()
    {
        if (painelNotificacao != null)
        {
            painelNotificacao.SetActive(false);
            
            // Garante que a notificação comece fisicamente fora da tela
            if (rectTransformNotificacao != null)
            {
                rectTransformNotificacao.anchoredPosition = new Vector2(
                    rectTransformNotificacao.anchoredPosition.x, 
                    posicaoYEscondida
                );
            }
        }
    }

    public void MostrarNotificacao(string titulo, string mensagem, UnityAction acaoAoClicar = null)
    {
        // Se já tiver uma notificação rolando, interrompe para mostrar a nova
        if (rotinaAtual != null) StopCoroutine(rotinaAtual);

        if (botaoNotificacao != null)
        {
            botaoNotificacao.onClick.RemoveAllListeners();
            if (acaoAoClicar != null)
            {
                botaoNotificacao.onClick.AddListener(() => {
                    acaoAoClicar(); // Executa o comando de abrir o app
                    
                    // Se o jogador clicar, a notificação recolhe na mesma hora
                    if (rotinaAtual != null) StopCoroutine(rotinaAtual);
                    StartCoroutine(AnimarDeslizamento(posicaoYEscondida, true)); 
                });
            }
        }

        // Inicia a sequência: Descer -> Esperar -> Subir
        rotinaAtual = StartCoroutine(RotinaExibirNotificacao(titulo, mensagem));
    }

    private IEnumerator RotinaExibirNotificacao(string titulo, string mensagem)
    {
        textoTitulo.text = titulo;
        textoMensagem.text = mensagem;
        
        painelNotificacao.SetActive(true);
        if (GerenciadorDeAudio.Instancia != null) GerenciadorDeAudio.Instancia.TocarNotificacao();
        
        // 1. Anima a descida
        yield return StartCoroutine(AnimarDeslizamento(posicaoYVisivel, false));

        // 2. Aguarda o tempo de leitura
        yield return new WaitForSeconds(tempoNaTela);

        // 3. Anima a subida e desliga
        yield return StartCoroutine(AnimarDeslizamento(posicaoYEscondida, true));
    }

    private IEnumerator AnimarDeslizamento(float destinoY, bool desativarNoFinal)
    {
        if (rectTransformNotificacao == null) yield break;

        Vector2 posInicial = rectTransformNotificacao.anchoredPosition;
        Vector2 posFinal = new Vector2(posInicial.x, destinoY);
        float tempoDecorrido = 0f;

        // O laço que cria o movimento fluido frame por frame
        while (tempoDecorrido < velocidadeAnimacao)
        {
            tempoDecorrido += Time.deltaTime;
            // SmoothStep cria o efeito de "frear" suavemente no final do movimento
            float t = Mathf.SmoothStep(0f, 1f, tempoDecorrido / velocidadeAnimacao);
            rectTransformNotificacao.anchoredPosition = Vector2.Lerp(posInicial, posFinal, t);
            yield return null;
        }

        // Garante que cravou na posição exata
        rectTransformNotificacao.anchoredPosition = posFinal;

        if (desativarNoFinal)
        {
            painelNotificacao.SetActive(false);
        }
    }
}