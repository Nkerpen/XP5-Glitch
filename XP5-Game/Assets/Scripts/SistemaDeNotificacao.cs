using System.Collections;
using TMPro;
using UnityEngine;

public class SistemaDeNotificacao : MonoBehaviour
{
    [Header("Elementos de UI")]
    [SerializeField] private RectTransform painelNotificacao;
    [SerializeField] private TextMeshProUGUI textoMensagem;

    [Header("Configurações")]
    [SerializeField] private Vector2 posicaoEscondida = new Vector2(0, 200);
    [SerializeField] private Vector2 posicaoVisivel = new Vector2(0, -50);
    [SerializeField] private float tempoAnimacao = 0.5f;
    [SerializeField] private float tempoExibicao = 3f;

    public void MostrarNotificacao(string mensagem)
    {
        textoMensagem.text = mensagem;
        StartCoroutine(AnimarNotificacao());
    }

    private IEnumerator AnimarNotificacao()
    {
        // Desce a notificação (usando Lerp para suavizar a animação sem precisar de Animator)
        float tempo = 0;
        while (tempo < tempoAnimacao)
        {
            painelNotificacao.anchoredPosition = Vector2.Lerp(posicaoEscondida, posicaoVisivel, tempo / tempoAnimacao);
            tempo += Time.deltaTime;
            yield return null;
        }
        painelNotificacao.anchoredPosition = posicaoVisivel;

        // Espera o jogador ler
        yield return new WaitForSeconds(tempoExibicao);

        // Sobe a notificação
        tempo = 0;
        while (tempo < tempoAnimacao)
        {
            painelNotificacao.anchoredPosition = Vector2.Lerp(posicaoVisivel, posicaoEscondida, tempo / tempoAnimacao);
            tempo += Time.deltaTime;
            yield return null;
        }
        painelNotificacao.anchoredPosition = posicaoEscondida;
    }
}