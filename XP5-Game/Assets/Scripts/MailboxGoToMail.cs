using UnityEngine;
using UnityEngine.UI;

public class NavegacaoCanvas : MonoBehaviour
{
    [Header("Configuração de Telas para ESTE Botão")]
    [SerializeField] private GameObject canvasParaAbrir;
    [SerializeField] private GameObject canvasParaFechar;

    void Start()
    {
        // O código busca o componente Button que está no mesmo objeto que este script
        Button botao = GetComponent<Button>();

        if (botao != null)
        {
            // Limpa lógicas antigas e cria o clique automaticamente
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(TrocarDeTela);
        }
        else
        {
            Debug.LogError($"[Erro] O script NavegacaoCanvas está no objeto '{gameObject.name}', mas esse objeto não tem um componente Button!");
        }
    }

    private void TrocarDeTela()
    {
        // Abre a tela de destino
        if (canvasParaAbrir != null)
        {
            canvasParaAbrir.SetActive(true);
        }

        // Fecha a tela atual
        if (canvasParaFechar != null)
        {
            canvasParaFechar.SetActive(false);
        }
    }
}