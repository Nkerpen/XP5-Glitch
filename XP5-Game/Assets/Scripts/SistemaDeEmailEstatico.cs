using UnityEngine;
using UnityEngine.Events; // Necessário para criar a listinha no Inspector

public class LeitorDeEmailEstatico : MonoBehaviour
{
    private bool jaFoiLido = false;

    [Header("Ações Visuais (O que liga/desliga ao fechar)")]
    public UnityEvent acoesAoFecharEmail; 

    public void FecharEmailEAvancar()
    {
        // Só avança a história se for a PRIMEIRA vez que fecha o e-mail
        if (!jaFoiLido)
        {
            jaFoiLido = true;
            if (GerenciadorDeNarrativa.Instancia != null)
            {
                GerenciadorDeNarrativa.Instancia.AvancarHistoria();
            }
        }

        // Roda a lista de coisas para ligar/desligar que você configurar na Unity
        acoesAoFecharEmail?.Invoke();
    }
}