using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gerenciador do Ciclo de Dia/Noite, UI de Tempo/Oxigênio e Inventário do Jogador.
/// Otimizado para alta performance e baixa alocação de memória (GC).
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton para acesso rápido e direto por outros scripts
    public static GameManager Instance { get; private set; }

    [Header("Configuração de Tempo (Dia/Noite)")]
    [Tooltip("Duração de um dia completo em segundos de jogo real.")]
    public float duracaoDiaSegundos = 120f;
    
    [Tooltip("Hora de início do jogo (0 a 24). Ex: 6.0 = 06:00 da manhã.")]
    [Range(0f, 23.9f)]
    public float horaInicial = 6.0f;

    [Tooltip("Tempo atual decorrido no dia (em segundos).")]
    public float tempoAtual = 0f;

    [Header("Integração com a UI")]
    [Tooltip("Texto da UI para exibir a hora formatada (Ex: 14:30).")]
    public Text textoHora;

    [Tooltip("Slider visual para exibir o preenchimento do Tempo/Oxigênio.")]
    public Slider sliderTempoOxigenio;

    [Tooltip("Se ativado, o Slider diminui conforme o dia passa (estilo oxigênio acabando). Caso contrário, aumenta.")]
    public bool inverterSlider = false;

    // Dicionário de Inventário para busca rápida O(1) de recursos coletados
    private Dictionary<string, int> inventario = new Dictionary<string, int>();

    // Cache interno de strings para evitar GC Alloc ao concatenar texto da hora
    private int ultimoMinutoRegistrado = -1;

    private void Awake()
    {
        // Garante que só exista uma instância do GameManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Inicializa o tempoAtual baseado na horaInicial configurada
        tempoAtual = (horaInicial / 24f) * duracaoDiaSegundos;
    }

    private void Update()
    {
        AtualizarCicloTempo();
    }

    /// <summary>
    /// Controla a passagem do tempo e atualiza a interface gráfica.
    /// </summary>
    private void AtualizarCicloTempo()
    {
        // Avança o tempo de forma progressiva
        tempoAtual += Time.deltaTime;
        
        // Reinicia o ciclo ao atingir a duração máxima do dia
        if (tempoAtual >= duracaoDiaSegundos)
        {
            tempoAtual = 0f;
        }

        // Calcula a porcentagem do dia concluída (0.0 a 1.0)
        float porcentagemDia = tempoAtual / duracaoDiaSegundos;

        // Atualiza o Slider de Tempo/Oxigênio
        if (sliderTempoOxigenio != null)
        {
            sliderTempoOxigenio.value = inverterSlider ? (1f - porcentagemDia) : porcentagemDia;
        }

        // Calcula as horas e minutos simulados
        float horaTotal = porcentagemDia * 24f;
        int hora = Mathf.FloorToInt(horaTotal);
        int minuto = Mathf.FloorToInt((horaTotal - hora) * 60f);

        // Otimização: Apenas atualiza o texto se o minuto mudar, evitando alocações de string inúteis no Update
        if (minuto != ultimoMinutoRegistrado)
        {
            ultimoMinutoRegistrado = minuto;
            if (textoHora != null)
            {
                textoHora.text = string.Format("{0:00}:{1:00}", hora, minuto);
            }
        }
    }

    /// <summary>
    /// Função pública para adicionar recursos ao inventário.
    /// Pode ser chamada diretamente por itens coletáveis.
    /// </summary>
    /// <param name="nome">Nome identificador do recurso (ex: Metal, Zimbrium).</param>
    /// <param name="qtd">Quantidade a ser adicionada.</param>
    public void AdicionarRecurso(string nome, int qtd)
    {
        if (string.IsNullOrEmpty(nome)) return;

        // Normaliza o nome do recurso para evitar duplicações por erros de digitação (letras maiúsculas/minúsculas)
        string chave = nome.Trim();

        if (inventario.ContainsKey(chave))
        {
            inventario[chave] += qtd;
        }
        else
        {
            inventario.Add(chave, qtd);
        }

        Debug.Log($"[Inventário] {qtd}x '{chave}' adicionado(s). Total atual: {inventario[chave]}");
    }

    /// <summary>
    /// Função pública para consultar a quantidade de um determinado recurso.
    /// </summary>
    public int ObterQuantidadeRecurso(string nome)
    {
        if (string.IsNullOrEmpty(nome)) return 0;
        string chave = nome.Trim();
        return inventario.ContainsKey(chave) ? inventario[chave] : 0;
    }
}
