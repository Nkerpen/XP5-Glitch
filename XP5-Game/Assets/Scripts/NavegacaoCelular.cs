using System.Collections.Generic;
using UnityEngine;

public class NavegacaoCelular : MonoBehaviour
{
    [SerializeField] private GameObject telaHome;

    private GameObject telaAtual;
    private Stack<GameObject> historicoTelas = new Stack<GameObject>();

    private void Start()
    {
        telaAtual = telaHome;
    }

    // Chame essa função nos botões dos aplicativos (ex: Clicou no app de E-mail)
    public void AbrirApp(GameObject novoApp)
    {
        if (telaAtual != null)
        {
            historicoTelas.Push(telaAtual); // Salva a tela atual na pilha
            telaAtual.SetActive(false);     // Esconde a tela atual
        }

        telaAtual = novoApp;
        telaAtual.SetActive(true); // Mostra o novo app
    }

    // Anexe ao botão "Seta / Triângulo" da barra inferior
    public void BotaoVoltar()
    {
        if (historicoTelas.Count > 0)
        {
            telaAtual.SetActive(false);
            telaAtual = historicoTelas.Pop(); // Tira a última tela da pilha e volta pra ela
            telaAtual.SetActive(true);
        }
    }

    // Anexe ao botão "Círculo" da barra inferior
    public void BotaoHome()
    {
        if (telaAtual != telaHome)
        {
            telaAtual.SetActive(false);
            historicoTelas.Clear(); // Limpa a memória de navegação
            telaAtual = telaHome;
            telaAtual.SetActive(true);
        }
    }
}