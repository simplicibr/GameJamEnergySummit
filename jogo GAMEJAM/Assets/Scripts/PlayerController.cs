using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de Movimentação Mobile (Touch/Joystick) e Gerenciamento de Stamina.
/// Projetado para visão Topdown 2D ou 3D usando física simples com transform.Translate.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Configuração de Movimento")]
    [Tooltip("Velocidade normal de caminhada.")]
    public float velocidadeAndar = 4.0f;

    [Tooltip("Velocidade máxima de corrida (consome stamina).")]
    public float velocidadeCorrida = 7.0f;

    [Tooltip("Velocidade com que o personagem gira em direção ao movimento (apenas 3D).")]
    public float velocidadeRotacao = 12.0f;

    [Tooltip("Ativar para movimentação 2D (Eixos X e Y). Desativar para movimentação 3D (Eixos X e Z).")]
    public bool usarMovimentacao2D = false;

    [Header("Controles do Joystick Virtual (Mobile)")]
    [Tooltip("Valor do eixo horizontal (-1 a 1). Atualizado pelo script do joystick.")]
    public float joystickHorizontal;
    
    [Tooltip("Valor do eixo vertical (-1 a 1). Atualizado pelo script do joystick.")]
    public float joystickVertical;

    [Tooltip("Limite de inclinação do joystick para ativar a corrida. Ex: > 0.8f significa empurrar quase tudo.")]
    [Range(0.1f, 1f)]
    public float limiteCorrida = 0.8f;

    [Header("Sistema de Stamina")]
    [Tooltip("Capacidade máxima de stamina do jogador.")]
    public float staminaMaxima = 100f;
    
    [Tooltip("Consumo de stamina por segundo ao correr.")]
    public float taxaConsumoStamina = 25f;

    [Tooltip("Regeneração de stamina por segundo.")]
    public float taxaRegeneracaoStamina = 15f;

    [Tooltip("Slider da UI associado à barra de Stamina.")]
    public Slider sliderStamina;

    // Variáveis de controle de estado privado
    private float staminaAtual;
    private float tempoUltimoSprint;
    private const float DELAY_REGENERACAO = 1.0f; // 1 segundo parado ou andando devagar antes de regenerar

    private void Start()
    {
        staminaAtual = staminaMaxima;
        AtualizarUIStamina();
    }

    private void Update()
    {
        ProcessarMovimentoEEstamina();
    }

    /// <summary>
    /// Processa os inputs do joystick, calcula a velocidade correta com base na stamina e move o jogador.
    /// </summary>
    private void ProcessarMovimentoEEstamina()
    {
        // Cria o vetor de input com base nos eixos do joystick virtual
        Vector2 inputJoystick = new Vector2(joystickHorizontal, joystickVertical);
        float magnitudeInput = inputJoystick.magnitude;

        // Verifica se o jogador quer correr (joystick empurrado além do limite e possui stamina)
        bool querCorrer = magnitudeInput > limiteCorrida;
        bool estaCorrendo = querCorrer && staminaAtual > 0.01f;

        // Define a velocidade atual
        float velocidadeFinal = estaCorrendo ? velocidadeCorrida : velocidadeAndar;

        // Se não houver movimento significativo, a velocidade é zero
        if (magnitudeInput < 0.05f)
        {
            velocidadeFinal = 0f;
        }

        // --- Gerenciamento de Stamina ---
        if (estaCorrendo && magnitudeInput >= 0.05f)
        {
            // Consome stamina ao correr
            staminaAtual -= taxaConsumoStamina * Time.deltaTime;
            tempoUltimoSprint = Time.time; // Reseta o cronômetro para o delay de regeneração
        }
        else
        {
            // Se passou 1 segundo parado ou andando devagar, regenera a stamina
            if (Time.time - tempoUltimoSprint >= DELAY_REGENERACAO)
            {
                staminaAtual += taxaRegeneracaoStamina * Time.deltaTime;
            }
        }

        // Restringe a stamina aos limites válidos
        staminaAtual = Mathf.Clamp(staminaAtual, 0f, staminaMaxima);
        AtualizarUIStamina();

        // --- Movimentação ---
        // Normaliza o input se ele exceder 1 (evita correr mais rápido na diagonal)
        Vector2 inputDirecionado = magnitudeInput > 1f ? inputJoystick.normalized : inputJoystick;

        Vector3 direcaoMovimento = Vector3.zero;

        if (usarMovimentacao2D)
        {
            // Movimentação Topdown 2D (X e Y)
            direcaoMovimento = new Vector3(inputDirecionado.x, inputDirecionado.y, 0f);
        }
        else
        {
            // Movimentação Topdown 3D (X e Z)
            direcaoMovimento = new Vector3(inputDirecionado.x, 0f, inputDirecionado.y);
        }

        // Move o personagem de forma direta usando transform.Translate
        if (direcaoMovimento.sqrMagnitude > 0.001f)
        {
            transform.Translate(direcaoMovimento * velocidadeFinal * Time.deltaTime, Space.World);
            RotacionarPersonagem(direcaoMovimento);
        }
    }

    /// <summary>
    /// Rotaciona visualmente o personagem na direção em que está se movendo.
    /// </summary>
    private void RotacionarPersonagem(Vector3 direcao)
    {
        if (usarMovimentacao2D)
        {
            // Rotação 2D no eixo Z (frente é Y positivo)
            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo - 90f);
        }
        else
        {
            // Rotação 3D suave no eixo Y
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * velocidadeRotacao);
        }
    }

    /// <summary>
    /// Atualiza diretamente o valor do slider da UI passado por referência.
    /// </summary>
    private void AtualizarUIStamina()
    {
        if (sliderStamina != null)
        {
            sliderStamina.value = staminaAtual / staminaMaxima;
        }
    }

    /// <summary>
    /// Permite que outros scripts consultem a stamina atual (ex: para fadiga visual).
    /// </summary>
    public float ObterStaminaAtual() => staminaAtual;
}
