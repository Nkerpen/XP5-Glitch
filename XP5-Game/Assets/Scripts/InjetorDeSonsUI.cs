using UnityEngine;
using UnityEngine.UI; // Necessário para acessar a classe Button

public class InjetorDeSonsUI : MonoBehaviour
{
    private void Start()
    {
        // O "true" aqui é o segredo: ele manda buscar até nos objetos que estão desativados/escondidos!
        Button[] todosOsBotoes = GetComponentsInChildren<Button>(true);

        foreach (Button botao in todosOsBotoes)
        {
            // Adiciona o comando de tocar som na lista do OnClick de cada botão via código
            botao.onClick.AddListener(() => 
            {
                if (GerenciadorDeAudio.Instancia != null)
                {
                    GerenciadorDeAudio.Instancia.TocarClique();
                }
            });
        }

        Debug.Log($"[Áudio] Som de clique injetado automaticamente em {todosOsBotoes.Length} botões!");
    }
}