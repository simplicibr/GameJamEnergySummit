using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ItemData
{
    public string nomeItem;
    public bool ehMaterial;
}

/// <summary>
/// Gerenciador de Inventário compacto com limite de espaço, descarte e reparo da nave.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Limites do Inventário")]
    [Tooltip("Limite máximo de unidades de itens que o inventário pode carregar.")]
    public int limiteInventario = 20;

    [Tooltip("Quantidade total atual de itens no inventário.")]
    [SerializeField] private int totalItensAtual = 0;

    [Header("Conserto da Nave")]
    [Tooltip("Progresso atual de peças entregues para consertar a nave.")]
    public int progressoConserto = 0;
    
    [Tooltip("Objetivo de peças entregues para consertar a nave.")]
    public int objetivoConserto = 5;

    private Dictionary<string, int> inventario = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Tenta adicionar recursos ao inventário se houver limite disponível.
    /// </summary>
    /// <returns>True se adicionado com sucesso; False se estiver cheio.</returns>
    public bool AdicionarRecurso(string nome, int qtd)
    {
        if (string.IsNullOrEmpty(nome) || qtd <= 0) return false;
        string chave = nome.Trim();

        // Valida se a adição ultrapassa o limite de peso/itens total
        if (totalItensAtual + qtd > limiteInventario)
        {
            Debug.Log($"[Inventário] Inventário CHEIO! Espaço restante: {limiteInventario - totalItensAtual}. Não é possível coletar {qtd}x '{chave}'.");
            return false;
        }

        if (inventario.ContainsKey(chave)) inventario[chave] += qtd;
        else inventario.Add(chave, qtd);

        totalItensAtual += qtd;
        Debug.Log($"[Inventário] Adicionado {qtd}x '{chave}'. Total: {totalItensAtual}/{limiteInventario}");
        return true;
    }

    /// <summary>
    /// Remove e descarta uma quantidade de itens do inventário.
    /// </summary>
    public void DescartarItem(string nomeItem, int quantidade)
    {
        if (string.IsNullOrEmpty(nomeItem) || quantidade <= 0) return;
        string chave = nomeItem.Trim();

        if (inventario.ContainsKey(chave))
        {
            int qtdNoInventario = inventario[chave];
            int qtdDescarte = Mathf.Min(quantidade, qtdNoInventario);

            inventario[chave] -= qtdDescarte;
            totalItensAtual -= qtdDescarte;

            if (inventario[chave] == 0) inventario.Remove(chave);

            Debug.Log($"[Inventário] Descartado {qtdDescarte}x '{chave}'. Estoque atual do item: {ObterQuantidade(chave)}. Total geral: {totalItensAtual}/{limiteInventario}");
        }
    }

    public int ObterQuantidade(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return 0;
        string chave = nome.Trim();
        return inventario.ContainsKey(chave) ? inventario[chave] : 0;
    }

    /// <summary>
    /// Combina Metal e Zimbrium para criar uma BateriaNave.
    /// </summary>
    public bool CraftarItem(string itemCriado)
    {
        if (itemCriado != "BateriaNave") return false;

        int qtdMetal = ObterQuantidade("Metal");
        int qtdZimbrium = ObterQuantidade("Zimbrium");

        if (qtdMetal >= 3 && qtdZimbrium >= 2)
        {
            // Consome ingredientes
            inventario["Metal"] -= 3;
            inventario["Zimbrium"] -= 2;
            totalItensAtual -= 5;

            // Remove chaves vazias para limpeza do dicionário
            if (inventario["Metal"] == 0) inventario.Remove("Metal");
            if (inventario["Zimbrium"] == 0) inventario.Remove("Zimbrium");

            // Adiciona o item craftado diretamente (consome 5 itens e gera 1, então sempre há espaço)
            if (inventario.ContainsKey("BateriaNave")) inventario["BateriaNave"] += 1;
            else inventario.Add("BateriaNave", 1);
            totalItensAtual += 1;

            Debug.Log($"[Craft] BateriaNave criada! Total geral no inventário: {totalItensAtual}/{limiteInventario}");
            return true;
        }

        Debug.Log($"[Craft] Recursos insuficientes para BateriaNave! Metal: {qtdMetal}/3, Zimbrium: {qtdZimbrium}/2");
        return false;
    }

    /// <summary>
    /// Remove itens do inventário do jogador e entrega para reparar a nave.
    /// </summary>
    public void EntregarPecaParaNave(string nomeItem, int quantidade)
    {
        if (string.IsNullOrEmpty(nomeItem) || quantidade <= 0) return;
        string chave = nomeItem.Trim();

        int qtdNoInventario = ObterQuantidade(chave);
        if (qtdNoInventario >= quantidade)
        {
            // Deduz do inventário
            inventario[chave] -= quantidade;
            if (inventario[chave] == 0) inventario.Remove(chave);
            totalItensAtual -= quantidade;

            // Atualiza progresso da nave
            progressoConserto += quantidade;
            Debug.Log($"[Nave] Peça(s) entregue(s): {quantidade}x '{chave}'. Progresso do conserto: {progressoConserto}/{objetivoConserto}");

            if (progressoConserto >= objetivoConserto)
            {
                Debug.Log("Nave Consertada! Pronto para decolar!");
            }
        }
        else
        {
            Debug.Log($"[Nave] Itens insuficientes para entrega! Requer: {quantidade}x '{chave}' (Possui: {qtdNoInventario})");
        }
    }
}
