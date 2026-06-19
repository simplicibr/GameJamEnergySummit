using UnityEngine;

/// <summary>
/// Representa um item coletável no cenário (ex: Metal, Zimbrium).
/// Compatível tanto com colisões/triggers 2D quanto 3D.
/// </summary>
public class ItemColetavel : MonoBehaviour
{
    [Header("Configurações do Recurso")]
    [Tooltip("Nome do recurso que será enviado ao inventário do GameManager (ex: Metal, Zimbrium).")]
    public string tipoRecurso = "Metal";

    [Tooltip("Quantidade deste recurso concedida ao jogador ao coletar.")]
    public int quantidade = 1;

    // Flag para evitar dupla coleta se múltiplos triggers dispararem no mesmo frame
    private bool jaColetado = false;

    #region Detecção de Colisão / Trigger (3D)

    private void OnTriggerEnter(Collider other)
    {
        VerificarEColetar(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        VerificarEColetar(collision.gameObject);
    }

    #endregion

    #region Detecção de Colisão / Trigger (2D)

    private void OnTriggerEnter2D(Collider2D other)
    {
        VerificarEColetar(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        VerificarEColetar(collision.gameObject);
    }

    #endregion

    /// <summary>
    /// Identifica se o objeto colidido é o jogador e executa a lógica de coleta.
    /// </summary>
    /// <param name="objetoColidido">O GameObject que colidiu com o item.</param>
    private void VerificarEColetar(GameObject objetoColidido)
    {
        if (jaColetado) return;

        // Verifica se quem colidiu foi o Player (por Tag ou pelo componente PlayerController)
        if (objetoColidido.CompareTag("Player") || objetoColidido.GetComponent<PlayerController>() != null)
        {
            jaColetado = true;
            Coletar();
        }
    }

    /// <summary>
    /// Adiciona o recurso ao GameManager e destrói o objeto coletável do cenário.
    /// </summary>
    private void Coletar()
    {
        if (GameManager.Instance != null)
        {
            // Adiciona o recurso ao inventário
            GameManager.Instance.AdicionarRecurso(tipoRecurso, quantidade);
        }
        else
        {
            Debug.LogWarning("[ItemColetavel] GameManager.Instance não encontrado na cena!");
        }

        // Destrói o próprio objeto para removê-lo do cenário
        Destroy(gameObject);
    }
}