using System.Collections.Generic;
using UnityEngine;

public class NavegacaoCelular : MonoBehaviour
{
    [Header("Configura��es de Telas")]
    [SerializeField] private GameObject telaHome;

    [Tooltip("Arraste aqui o objeto pai 'Tela_Aplicativos'")]
    [SerializeField] private GameObject telaAplicativos;

    [Header("HUD")]
    [SerializeField] private ClockController relogio;

    private GameObject telaAtual;
    private Stack<GameObject> historicoTelas = new Stack<GameObject>();

    private void Start()
    {
        telaAtual = telaHome;
        AtualizarRelogio();
    }

    // Chame essa fun��o nos bot�es dos aplicativos (ex: Clicou no app de E-mail)
    public void AbrirApp(GameObject novoApp)
    {
        if (telaAtual != null)
        {
            // PROTEÇÃO 1: Só salva no histórico se a tela atual NÃO for a tela mãe/container
            if (telaAtual != telaAplicativos)
            {
                historicoTelas.Push(telaAtual); // Salva a tela atual na pilha
            }

            // Só desativa a tela atual se ela NÃO for a tela mãe
            if (telaAtual != telaAplicativos)
            {
                telaAtual.SetActive(false); // Esconde a tela atual
            }
        }

        // --- A SOLUÇÃO ENTRA AQUI ---
        // Se o novo app que estamos abrindo fica dentro da Tela_Aplicativos, 
        // precisamos garantir que o PAI (Tela_Aplicativos) seja ligado primeiro!
        if (telaAplicativos != null && novoApp.transform.IsChildOf(telaAplicativos.transform))
        {
            telaAplicativos.SetActive(true);
            telaHome.SetActive(false); // Esconde a home caso estivéssemos lá
        }
        // ------------------------------

        telaAtual = novoApp;
        telaAtual.SetActive(true); // Mostra o novo app
        AtualizarRelogio();
    }

    // Anexe ao bot�o "Seta / Tri�ngulo" da barra inferior
    public void BotaoVoltar()
    {
        if (historicoTelas.Count > 0)
        {
            if (telaAtual != null) telaAtual.SetActive(false);
            
            telaAtual = historicoTelas.Pop();
            telaAtual.SetActive(true);

            // --- A MÁGICA PARA CONSERTAR O BUG ---
            // Se a tela que resgatamos do histórico for a Home, garantimos que a gaveta de aplicativos seja fechada!
            if (telaAtual == telaHome && telaAplicativos != null)
            {
                telaAplicativos.SetActive(false);
            }

            AtualizarRelogio();
        }
    }

    public void BotaoHome()
    {
        if (telaAtual != null) telaAtual.SetActive(false);
        
        // Se o jogador clicar no botão Home físico, força o fechamento da gaveta também
        if (telaAplicativos != null) telaAplicativos.SetActive(false); 

        telaAtual = telaHome;
        telaAtual.SetActive(true);
        historicoTelas.Clear();

        AtualizarRelogio();
    }

    //Mostra o relogio so na Home
    private void AtualizarRelogio()
    {
        if (relogio == null) return;

        bool naHomeOuApps = (telaAtual == telaHome || telaAtual == telaAplicativos);
        relogio.SetVisible(naHomeOuApps);
    }
}