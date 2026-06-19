using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estrutura de dados simples para representar metadados de um item.
/// </summary>
[System.Serializable]
public struct ItemData
{
    public string nomeItem;
    public bool ehMaterial;

    public ItemData(string nome, bool material)
    {
        nomeItem = nome;
        ehMaterial = material;
    }
}

/// <summary>
/// Gerenciador de Inventário leve por Dicionário e Sistema de Crafting.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // Singleton para fácil acesso global
    public static InventoryManager Instance { get; private set; }

    // Dicionário de Inventário por ID de string (ID -> Quantidade)
    private Dictionary<string, int> inventario = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adiciona um recurso ao inventário.
    /// </summary>
    public void AdicionarRecurso(string nome, int qtd)
    {
        if (string.IsNullOrEmpty(nome)) return;
        string chave = nome.Trim();

        if (inventario.ContainsKey(chave))
        {
            inventario[chave] += qtd;
        }
        else
        {
            inventario.Add(chave, qtd);
        }
        Debug.Log($"[Inventário] Adicionado {qtd}x '{chave}'. Total em estoque: {inventario[chave]}");
    }

    /// <summary>
    /// Retorna a quantidade disponível de um recurso no inventário.
    /// </summary>
    public int ObterQuantidade(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return 0;
        string chave = nome.Trim();
        return inventario.ContainsKey(chave) ? inventario[chave] : 0;
    }

    /// <summary>
    /// Sistema de Crafting. Consome ingredientes e adiciona o item final se houver recursos suficientes.
    /// </summary>
    /// <param name="itemCriado">Nome do item resultante a ser criado.</param>
    /// <returns>True se o item foi craftado com sucesso; False caso contrário.</returns>
    public bool CraftarItem(string itemCriado)
    {
        if (string.IsNullOrEmpty(itemCriado)) return false;
        string chave = itemCriado.Trim();

        // Receita para BateriaNave: 3 Metal + 2 Zimbrium
        if (chave == "BateriaNave")
        {
            int qtdMetal = ObterQuantidade("Metal");
            int qtdZimbrium = ObterQuantidade("Zimbrium");

            if (qtdMetal >= 3 && qtdZimbrium >= 2)
            {
                // Subtrai os ingredientes
                inventario["Metal"] -= 3;
                inventario["Zimbrium"] -= 2;

                // Adiciona o item craftado
                AdicionarRecurso("BateriaNave", 1);
                Debug.Log("[Craft] BateriaNave craftada com sucesso!");
                return true;
            }
            else
            {
                Debug.Log($"[Craft] Recursos insuficientes para BateriaNave! Requerido: 3 Metal (Possui: {qtdMetal}), 2 Zimbrium (Possui: {qtdZimbrium})");
                return false;
            }
        }

        Debug.LogWarning($"[Craft] Receita para o item '{itemCriado}' não registrada.");
        return false;
    }
}
