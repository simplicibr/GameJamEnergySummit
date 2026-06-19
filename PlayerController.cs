using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de Movimentação Mobile (Touch/Joystick) com Coleta por Toque/Raycast.
/// Stamina é usada apenas para a coleta física de itens do chão.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Configuração de Movimento")]
    [Tooltip("Velocidade normal de caminhada.")]
    public float velocidadeAndar = 4.0f;

    [Tooltip("Velocidade máxima de corrida (Livre - não consome stamina).")]
    public float velocidadeCorrida = 7.0f;

    [Tooltip("Velocidade de rotação do personagem.")]
    public float velocidadeRotacao = 12.0f;

    [Tooltip("Ativar para movimentação 2D (Eixos X e Y) e Raycast 2D. Desativar para 3D (Eixos X e Z).")]
    public bool usarMovimentacao2D = false;

    [Header("Controles do Joystick Virtual (Mobile)")]
    public float joystickHorizontal;
    public float joystickVertical;

    [Range(0.1f, 1f)]
    public float limiteCorrida = 0.8f;

    [Header("Sistema de Stamina (Apenas Coleta)")]
    [Tooltip("Capacidade máxima de stamina.")]
    public float staminaMaxima = 100f;

    [Tooltip("Custo de stamina descontado por item coletado.")]
    public float custoStaminaColeta = 10f;

    [Tooltip("Velocidade de regeneração da stamina.")]
    public float taxaRegeneracaoStamina = 15f;

    [Tooltip("Slider da UI associado à barra de Stamina.")]
    public Slider sliderStamina;

    private float staminaAtual;
    private float tempoUltimoGasto;
    private const float DELAY_REGENERACAO = 1.0f; // 1 segundo após gastar para regenerar

    private void Start()
    {
        staminaAtual = staminaMaxima;
        AtualizarUIStamina();
    }

    private void Update()
    {
        ProcessarMovimentoEEstamina();
        DetectarCliqueColeta();
    }

    /// <summary>
    /// Processa movimentação (corrida livre) e a regeneração natural da stamina.
    /// </summary>
    private void ProcessarMovimentoEEstamina()
    {
        Vector2 inputJoystick = new Vector2(joystickHorizontal, joystickVertical);
        float magnitudeInput = inputJoystick.magnitude;

        // Corrida agora é livre, sem necessidade de stamina atual
        bool estaCorrendo = magnitudeInput > limiteCorrida;
        float velocidadeFinal = estaCorrendo ? velocidadeCorrida : velocidadeAndar;

        if (magnitudeInput < 0.05f) velocidadeFinal = 0f;

        // --- Regeneração da Stamina ---
        if (staminaAtual < staminaMaxima && Time.time - tempoUltimoGasto >= DELAY_REGENERACAO)
        {
            staminaAtual += taxaRegeneracaoStamina * Time.deltaTime;
        }

        staminaAtual = Mathf.Clamp(staminaAtual, 0f, staminaMaxima);
        AtualizarUIStamina();

        // --- Movimentação com Translate ---
        Vector2 inputDirecionado = magnitudeInput > 1f ? inputJoystick.normalized : inputJoystick;
        Vector3 direcaoMovimento = usarMovimentacao2D ? 
            new Vector3(inputDirecionado.x, inputDirecionado.y, 0f) : 
            new Vector3(inputDirecionado.x, 0f, inputDirecionado.y);

        if (direcaoMovimento.sqrMagnitude > 0.001f)
        {
            transform.Translate(direcaoMovimento * velocidadeFinal * Time.deltaTime, Space.World);
            RotacionarPersonagem(direcaoMovimento);
        }
    }

    /// <summary>
    /// Rotaciona visualmente o personagem.
    /// </summary>
    private void RotacionarPersonagem(Vector3 direcao)
    {
        if (usarMovimentacao2D)
        {
            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angulo - 90f);
        }
        else
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoAlvo, Time.deltaTime * velocidadeRotacao);
        }
    }

    /// <summary>
    /// Detecta toques/cliques na tela e realiza Raycast para encontrar e coletar itens do chão.
    /// </summary>
    private void DetectarCliqueColeta()
    {
        if (Input.GetMouseButtonDown(0) && Camera.main != null)
        {
            ItemDoChao item = null;
            GameObject objetoAtingido = null;

            if (usarMovimentacao2D)
            {
                // Raycast 2D baseado na posição do clique no plano mundial
                Vector3 posicaoMundial = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit2D = Physics2D.Raycast(posicaoMundial, Vector2.zero);
                if (hit2D.collider != null)
                {
                    item = hit2D.collider.GetComponent<ItemDoChao>();
                    if (item != null) objetoAtingido = hit2D.collider.gameObject;
                }
            }
            else
            {
                // Raycast 3D baseado no raio projetado da câmera
                Ray raio = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(raio, out RaycastHit hit))
                {
                    item = hit.collider.GetComponent<ItemDoChao>();
                    if (item != null) objetoAtingido = hit.collider.gameObject;
                }
            }

            // Executa a lógica de coleta se colidiu com um ItemDoChao válido
            if (item != null && objetoAtingido != null)
            {
                if (staminaAtual >= custoStaminaColeta)
                {
                    if (InventoryManager.Instance != null)
                    {
                        // Só gasta stamina e destrói o objeto se o inventário tiver espaço disponível
                        if (InventoryManager.Instance.AdicionarRecurso(item.nomeDoItem, item.quantidade))
                        {
                            GastarStaminaAoColetar();
                            Destroy(objetoAtingido);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[PlayerController] InventoryManager.Instance não encontrado!");
                    }
                }
                else
                {
                    Debug.Log($"[Coleta] Stamina insuficiente ({staminaAtual:0.0}/{custoStaminaColeta}) para coletar '{item.nomeDoItem}'!");
                }
            }
        }
    }

    /// <summary>
    /// Função pública disparada para deduzir a stamina durante a coleta.
    /// </summary>
    public void GastarStaminaAoColetar()
    {
        staminaAtual = Mathf.Max(0f, staminaAtual - custoStaminaColeta);
        tempoUltimoGasto = Time.time;
        AtualizarUIStamina();
        Debug.Log($"[Stamina] Consumo por coleta executado. Stamina atual: {staminaAtual:0.0}");
    }

    private void AtualizarUIStamina()
    {
        if (sliderStamina != null)
        {
            sliderStamina.value = staminaAtual / staminaMaxima;
        }
    }

    public float ObterStaminaAtual() => staminaAtual;
}
