using UnityEngine;
using UnityEngine.UI;

public class GerenciadorDeNarrativa : MonoBehaviour
{
    public static GerenciadorDeNarrativa Instancia;

    [Header("Sistemas")]
    [SerializeField] private SistemaDeNotificacao sistemaNotificacao;
    [SerializeField] private NavegacaoCelular navegacaoCelular;
    
    [Header("Aplicativos (Abertura Direta)")]
    [SerializeField] private GameObject telaAppEmail; 

    [Header("Botões para Simular Clique (E Esconder)")]
    [SerializeField] private Button botaoContatoGrupo;     
    [SerializeField] private Button botaoContatoGolpista;  

[Header("Travas Visuais (O que começa escondido)")]
    [SerializeField] private GameObject iconeAppEmailNaHome; 
    [SerializeField] private GameObject botaoEmail1_Puzzle;  
    [SerializeField] private GameObject botaoEmail2_Anthony; 
    [SerializeField] private GameObject painelEscondeEmail; // <--- NOVA VARIÁVEL AQUI
    private int etapaAtual = 0;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

private void Start()
    {
        // Garante que tudo comece desligado (invisível) até a hora certa
        if (iconeAppEmailNaHome != null) iconeAppEmailNaHome.SetActive(false);
        if (botaoContatoGolpista != null) botaoContatoGolpista.gameObject.SetActive(false);
        if (botaoEmail1_Puzzle != null) botaoEmail1_Puzzle.SetActive(false);
        if (botaoEmail2_Anthony != null) botaoEmail2_Anthony.SetActive(false); 
        if (painelEscondeEmail != null) painelEscondeEmail.SetActive(true); 
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
            case 0: // COMEÇO -> Notificação do Diálogo 1
                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação", 
                    "Charles: o John tá online", 
                    () => { if(botaoContatoGrupo != null) botaoContatoGrupo.onClick.Invoke(); }
                );
                break;
                
            case 1: // FIM DIÁLOGO 1 -> Libera Email 1 (Puzzle)
                if (iconeAppEmailNaHome != null) iconeAppEmailNaHome.SetActive(true); // Liga o App na Home
                if (botaoEmail1_Puzzle != null) botaoEmail1_Puzzle.SetActive(true); // Liga o E-mail 1 na Caixa

                sistemaNotificacao.MostrarNotificacao(
                    "Suporte PayPal", 
                    "Sua conta será bloqueada hoje!", 
                    () => navegacaoCelular.AbrirApp(telaAppEmail)
                );
                break;
                
            case 2: // VENCEU PUZZLE EMAIL -> Libera Email 2 (Anthony)
                
                // DESLIGA O PAINEL FALSO PRIMEIRO:
                if (painelEscondeEmail != null) painelEscondeEmail.SetActive(false); 
                
                // LIGA O BOTÃO VERDADEIRO DO ANTHONY LOGO EM SEGUIDA:
                if (botaoEmail2_Anthony != null) botaoEmail2_Anthony.SetActive(true); 

                sistemaNotificacao.MostrarNotificacao(
                    "Novo E-mail", 
                    "Anthony: Informações Importantes", 
                    () => {
                        navegacaoCelular.AbrirApp(telaAppEmail);
                        if (botaoEmail2_Anthony != null) 
                            botaoEmail2_Anthony.GetComponent<Button>().onClick.Invoke();
                    }
                );
                break;
                
            case 3: // LEU EMAIL 2 -> Notificação do Diálogo 2
                sistemaNotificacao.MostrarNotificacao(
                    "Grupo de Investigação", 
                    "Eva: Vocês viram aquele email?", 
                    () => { if(botaoContatoGrupo != null) botaoContatoGrupo.onClick.Invoke(); }
                );
                break;

            case 4: // FIM DIÁLOGO 2 -> Notificação do Golpista
                if (botaoContatoGolpista != null) botaoContatoGolpista.gameObject.SetActive(true); // Destranca Golpista

                sistemaNotificacao.MostrarNotificacao(
                    "Desconhecido", 
                    "Me passe o código agora.", 
                    () => { if(botaoContatoGolpista != null) botaoContatoGolpista.onClick.Invoke(); }
                );
                break;
        }
    }
}