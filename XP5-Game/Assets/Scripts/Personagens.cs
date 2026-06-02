using UnityEngine;

[CreateAssetMenu(fileName = "NovoPersonagem", menuName = "Glitch/Personagem")]
public class Personagem : ScriptableObject
{
    public string nome;
    public Color corDoNome; // Para pintar o nome do personagem (ex: Verde pro Maicon)
    public Color corDoBalao = Color.white; // Caso queira mudar o fundo do balão também
}