using UnityEngine;
using UnityEngine.UI; // Precisamos disso para usar a classe Button

public class GerenciadorDeNarrativa : MonoBehaviour
{
    public static GerenciadorDeNarrativa Instancia;

    [Header("Sistemas")]
    [SerializeField] private SistemaDeNotificacao sistemaNotificacao;
    [SerializeField] private NavegacaoCelular navegacaoCelular;
    
    [Header("Aplicativos (Abertura Direta)")]
    [SerializeField] private GameObject telaAppEmail; 

    [Header("Botões de Contato (Para simular o clique)")]
    [SerializeField] private Button botaoContatoGrupo;     // O botão do "Grupo de Investigação"
    [SerializeField] private Button botaoContatoGolpista;  // O botão do "Número Desconhecido"

    private int etapaAtual = 0;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public void IniciarJogo()
    {
        Invoke(nameof(DispararNotificacaoAtual), 1.5f);
    }

    public void AvancarHistoria()
    {
        etapaAtual++;
        Invoke(nameof(DispararNotificacaoAtual), 3f); 
    }

    private void DispararNotificacaoAtual()
    {
        switch (etapaAtual)
        {
            case 0:
                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação", 
                    "Charles: o John tá online", 
                    () => {
                        // Clica virtualmente no botão do grupo!
                        if(botaoContatoGrupo != null) botaoContatoGrupo.onClick.Invoke();
                    }
                );
                break;
            case 1:
                sistemaNotificacao.MostrarNotificacao(
                    "Suporte PayPal", 
                    "Sua conta será bloqueada hoje!", 
                    () => navegacaoCelular.AbrirApp(telaAppEmail) // O e-mail continua abrindo normal
                );
                break;
            case 2:
                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação", 
                    "Eva: Vocês viram aquele email?", 
                    () => {
                        // Clica no botão do grupo de novo para a parte 2
                        if(botaoContatoGrupo != null) botaoContatoGrupo.onClick.Invoke();
                    }
                );
                break;
            case 3:
                sistemaNotificacao.MostrarNotificacao(
                    "Desconhecido", 
                    "Me passe o código agora.", 
                    () => {
                        // Clica no botão do golpista!
                        if(botaoContatoGolpista != null) botaoContatoGolpista.onClick.Invoke();
                    }
                );
                break;
        }
    }
}