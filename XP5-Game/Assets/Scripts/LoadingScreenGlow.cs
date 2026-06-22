using UnityEngine;
using UnityEngine.UI;

public class UIPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private RawImage targetImage;
    [SerializeField] private float pulseSpeed = 2f;      // Velocidade do movimento (subida e descida)
    [SerializeField] private float pulseRange = 0.05f;   // O quanto ela estica (0.05 = 5% do tamanho original)

    [Header("Mob Sakai UI Particle Settings")]
    [SerializeField] private Image loadingBarFill;       // A imagem (Filled) da barra que enche de fato
    [SerializeField] private RectTransform uiParticleRect; // O RectTransform do objeto "UIParticle" (pai do sistema de partículas)

    private Vector3 initialScale;

    void Start()
    {
        // Se você não arrastar a imagem no Inspector, o script tenta pegar do próprio GameObject
        if (targetImage == null)
        {
            targetImage = GetComponent<RawImage>();
        }

        if (targetImage != null)
        {
            initialScale = targetImage.transform.localScale;
        }
        else
        {
            Debug.LogError("Nenhuma RawImage foi encontrada ou atribuída ao script de Pulso!");
        }
    }

    void Update()
    {
        // 1. Executa o efeito de pulso na RawImage
        HandlePulse();

        // 2. Executa o acompanhamento das partículas na ponta do preenchimento da barra
        HandleParticleFollow();
    }

    private void HandlePulse()
    {
        if (targetImage == null) return;

        // Calcula a oscilação suave usando a função Seno (Mathf.Sin)
        float wave = Mathf.Sin(Time.time * pulseSpeed);

        // Calcula o offset (variação) de tamanho baseado na onda
        float scaleOffset = wave * pulseRange;

        // Aplica o novo tamanho somando o offset ao tamanho inicial (eixos X e Y)
        targetImage.transform.localScale = initialScale + new Vector3(scaleOffset, scaleOffset, 0);
    }

    private void HandleParticleFollow()
    {
        // Só executa se ambas as referências estiverem configuradas no Inspector
        if (loadingBarFill == null || uiParticleRect == null) return;

        RectTransform fillRect = loadingBarFill.GetComponent<RectTransform>();
        if (fillRect == null) return;

        // Pega a largura total real da barra de loading
        float totalWidth = fillRect.rect.width;

        // Calcula a posição X exata baseada no preenchimento atual (fillAmount vai de 0 a 1)
        float currentX = totalWidth * loadingBarFill.fillAmount;

        // Alinha a posição baseando-se na posição ancorada e no Pivot da barra (geralmente X = 0 ou 0.5)
        Vector2 newPos = fillRect.anchoredPosition;
        newPos.x += currentX - (totalWidth * fillRect.pivot.x);

        // Atualiza a posição do objeto do Mob Sakai
        uiParticleRect.anchoredPosition = newPos;
    }
}