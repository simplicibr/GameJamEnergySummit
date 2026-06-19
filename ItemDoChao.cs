using UnityEngine;

/// <summary>
/// Script identificador de itens no chão que podem ser coletados pelo Player.
/// </summary>
public class ItemDoChao : MonoBehaviour
{
    [Tooltip("Nome/ID do item (ex: Metal, Zimbrium).")]
    public string nomeDoItem = "Metal";

    [Tooltip("Quantidade deste item no objeto.")]
    public int quantidade = 1;
}
