using UnityEngine;
using UnityEngine.UI;

public class UIPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private RawImage targetImage;
    [SerializeField] private float pulseSpeed = 2f;      // Velocidade do movimento (subida e descida)
    [SerializeField] private float pulseRange = 0.05f;   // O quanto ela estica (0.05 = 5% do tamanho original)

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
        if (targetImage == null) return;

        // Calcula a oscilação suave usando a função Seno (Mathf.Sin)
        float wave = Mathf.Sin(Time.time * pulseSpeed);

        // Calcula o offset (variação) de tamanho baseado na onda
        float scaleOffset = wave * pulseRange;

        // Aplica o novo tamanho somando o offset ao tamanho inicial (eixos X e Y)
        targetImage.transform.localScale = initialScale + new Vector3(scaleOffset, scaleOffset, 0);
    }
}