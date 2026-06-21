using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AdaptadorDeTelaProporcional : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect ultimaSafeArea = new Rect(0, 0, 0, 0);
    private Vector2 ultimaResolucaoTela = new Vector2(0, 0);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Garante que o contêiner comece esticado ocupando a tela cheia nas âncoras
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;

        AtualizarEscalaEArea();
    }

    void Update()
    {
        if (ultimaSafeArea != Screen.safeArea || ultimaResolucaoTela.x != Screen.width || ultimaResolucaoTela.y != Screen.height)
        {
            AtualizarEscalaEArea();
        }
    }

    void AtualizarEscalaEArea()
    {
        ultimaSafeArea = Screen.safeArea;
        ultimaResolucaoTela = new Vector2(Screen.width, Screen.height);

        // Se o dispositivo não tiver safe area (ex: editor padrão), zera os offsets
        if (Screen.width == 0 || Screen.height == 0) return;

        // Calcula o recuo em pixels baseado na Safe Area real do aparelho
        float esquerdo = ultimaSafeArea.x;
        float direito = Screen.width - (ultimaSafeArea.x + ultimaSafeArea.width);
        float baixo = ultimaSafeArea.y;
        float topo = Screen.height - (ultimaSafeArea.y + ultimaSafeArea.height);

        // Aplica os recuos mantendo as âncoras em Stretch-Stretch (0,0 a 1,1)
        rectTransform.offsetMin = new Vector2(esquerdo, baixo);
        rectTransform.offsetMax = new Vector2(-direito, -topo);

        Debug.Log($"[Adaptador] Margens aplicadas via Offsets -> Topo: {topo}px | Baixo: {baixo}px");
    }
}