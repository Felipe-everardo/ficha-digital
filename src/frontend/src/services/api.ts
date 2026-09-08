export type ApiStatus = {
  application: string
  message: string
  version: string
  checkedAtUtc: string
}

export type CriarClienteInput = {
  nomeReferencia: string
}

export type ClienteCriado = {
  id: string
  nomeParaExibicao: string
  criadoEmUtc: string
}

export type ClienteResumo = {
  id: string
  nomeReferencia: string
  nomeCompleto: string | null
  nomeParaExibicao: string
  pronomes: string | null
  celular: string | null
  email: string | null
  instagram: string | null
  criadoEmUtc: string
  ultimaFicha: FichaClienteResumo | null
}

export type TipoProcedimento = 'NaoInformado' | 'Tatuagem' | 'Piercing'

export type FormaPagamento =
  | 'Dinheiro'
  | 'Pix'
  | 'CartaoDebito'
  | 'CartaoCredito'
  | 'Transferencia'
  | 'Outro'

export type SituacaoPagamento = 'Pago'

export type CategoriaDespesa =
  | 'Materiais'
  | 'Aluguel'
  | 'Contas'
  | 'Manutencao'
  | 'Marketing'
  | 'ImpostosETaxas'
  | 'PagamentoProfissional'
  | 'Outro'

export type Atendimento = {
  id: string
  fichaId: string
  dataRealizacao: string
  valorCobrado: number
  desconto: number
  valorFinal: number
  formaPagamento: FormaPagamento
  situacaoPagamento: SituacaoPagamento
  registradoEmUtc: string
  atualizadoEmUtc: string
}

export type RegistrarAtendimentoInput = {
  dataRealizacao: string
  valorCobrado: number
  desconto: number
  formaPagamento: FormaPagamento
}

export type Despesa = {
  id: string
  data: string
  categoria: CategoriaDespesa
  descricao: string
  valor: number
  profissionalId: string
  profissionalNome: string
  registradaEmUtc: string
  atualizadaEmUtc: string
}

export type SalvarDespesaInput = {
  data: string
  categoria: CategoriaDespesa
  descricao: string
  valor: number
}

export type FiltrosFinanceiro = {
  dataDe?: string
  dataAte?: string
}

export type FinanceiroResultado = {
  resumo: {
    totalRecebido: number
    totalSaidas: number
    saldo: number
    atendimentosPagos: number
  }
  despesas: Despesa[]
  pagina: number
  tamanhoPagina: number
  totalDespesas: number
  totalPaginas: number
}

export type FiltrosClientes = {
  busca?: string
  profissionalId?: string
  tipoProcedimento?: TipoProcedimento
  atendimentoDe?: string
  atendimentoAte?: string
}

export type ClientesPaginados = {
  itens: ClienteResumo[]
  pagina: number
  tamanhoPagina: number
  totalItens: number
  totalPaginas: number
}

export type ConviteFichaCriado = {
  fichaId: string
  conviteId: string
  linkPreenchimento: string
  expiraEmUtc: string
}

export type FichaResumo = {
  id: string
  clienteId: string
  clienteNome: string
  profissionalResponsavelId: string | null
  profissionalResponsavelNome: string
  tipoProcedimento: TipoProcedimento
  status: string
  criadaEmUtc: string
  concluidaEmUtc: string | null
  atendimento: Atendimento | null
  conviteExpiraEmUtc: string | null
  conviteExpirado: boolean
}

export type FiltrosFichas = {
  busca?: string
  profissionalId?: string
  tipoProcedimento?: TipoProcedimento
  status?: string
  criadaDe?: string
  criadaAte?: string
  atendimentoDe?: string
  atendimentoAte?: string
}

export type ProfissionalResumo = {
  id: string
  nomeCompleto: string
  especialidades: string[]
}

export type FichaClienteResumo = {
  id: string
  status: string
  tipoProcedimento: TipoProcedimento
  profissionalResponsavelId: string | null
  profissionalResponsavelNome: string
  criadaEmUtc: string
}

export type ClienteDetalhe = {
  id: string
  nomeReferencia: string
  nomeCompleto: string | null
  nomeSocial: string | null
  nomeParaExibicao: string
  pronomes: string | null
  dataNascimento: string | null
  celular: string | null
  email: string | null
  instagram: string | null
  contatoEmergenciaNome: string | null
  contatoEmergenciaCelular: string | null
  dadosPessoaisPreenchidosEmUtc: string | null
  criadoEmUtc: string
  fichas: FichaClienteResumo[]
}

export type FichasPaginadas = {
  itens: FichaResumo[]
  pagina: number
  tamanhoPagina: number
  totalItens: number
  totalPaginas: number
  resumoFinanceiro: {
    totalRecebido: number
    atendimentosRegistrados: number
    fichasSemRegistro: number
  }
}

export type ClienteFichaDetalhe = {
  id: string
  nomeReferencia: string
  nomeCompleto: string | null
  nomeSocial: string | null
  nomeParaExibicao: string
  pronomes: string | null
  dataNascimento: string | null
  celular: string | null
  email: string | null
  instagram: string | null
  contatoEmergenciaNome: string | null
  contatoEmergenciaCelular: string | null
  dadosPessoaisPreenchidosEmUtc: string | null
}

export type QuestionarioSaudeDetalhe = {
  versao: number
  temDiabetes: boolean
  tipoDiabetes: string | null
  possuiPressaoAlta: boolean
  temAlergia: boolean
  descricaoAlergia: string | null
  possuiCondicaoCardiaca: boolean
  temEpilepsia: boolean
  temHemofilia: boolean
  usaMarcaPasso: boolean
  estaGravidaOuAmamentando: boolean
  respondidoEmUtc: string
}

export type AceiteTermoResumo = {
  versaoTermo: number
  nomeAssinante: string
  aceitoEmUtc: string
}

export type FichaDetalhe = {
  id: string
  status: string
  criadaEmUtc: string
  conviteExpiraEmUtc: string | null
  conviteExpirado: boolean
  profissionalResponsavelId: string | null
  profissionalResponsavelNome: string
  tipoProcedimento: TipoProcedimento
  cliente: ClienteFichaDetalhe
  questionarioSaude: QuestionarioSaudeDetalhe | null
  aceiteTermo: AceiteTermoResumo | null
  atendimento: Atendimento | null
}

export type TermoConsentimento = {
  versao: number
  conteudo: string
  conteudoHash: string
}

export type ConviteFichaAberto = {
  fichaId: string
  status: string
  questionarioRespondido: boolean
  dadosPessoaisPreenchidos: boolean
  nomeReferencia: string
  profissionalResponsavelNome: string
  tipoProcedimento: TipoProcedimento
  dadosPessoais: {
    nomeCompleto: string | null
    nomeSocial: string | null
    pronomes: string | null
    dataNascimento: string | null
    celular: string | null
    email: string | null
    instagram: string | null
    contatoEmergenciaNome: string | null
    contatoEmergenciaCelular: string | null
  }
  termoConsentimento: TermoConsentimento
}

export type PreencherDadosPessoaisInput = {
  nomeCompleto: string
  nomeSocial: string
  pronomes: string
  dataNascimento: string
  celular: string
  email: string
  instagram: string
  contatoEmergenciaNome: string
  contatoEmergenciaCelular: string
}

export type DadosPessoaisPreenchidos = {
  fichaId: string
  clienteId: string
  nomeParaExibicao: string
  preenchidosEmUtc: string
}

export type ResponderQuestionarioSaudeInput = {
  temDiabetes: boolean
  tipoDiabetes: string | null
  possuiPressaoAlta: boolean
  temAlergia: boolean
  descricaoAlergia: string | null
  possuiCondicaoCardiaca: boolean
  temEpilepsia: boolean
  temHemofilia: boolean
  usaMarcaPasso: boolean
  estaGravidaOuAmamentando: boolean
}

export type QuestionarioSaudeRespondido = {
  questionarioId: string
  fichaId: string
  versao: number
  respondidoEmUtc: string
}

export type AceitarTermoConsentimentoInput = {
  versaoTermo: number
  conteudoHash: string
  nomeAssinante: string
  aceitouTermo: boolean
}

export type TermoConsentimentoAceito = {
  aceiteId: string
  fichaId: string
  versaoTermo: number
  aceitoEmUtc: string
  statusFicha: string
}

export type ProblemDetails = {
  title?: string
  detail?: string
  status?: number
}

export type ValidationProblemDetails = {
  title: string
  status: number
  errors: Record<string, string[]>
}

export class ApiValidationError extends Error {
  readonly errors: Record<string, string[]>

  constructor(errors: Record<string, string[]>) {
    super('A API encontrou erros de validação.')
    this.name = 'ApiValidationError'
    this.errors = errors
  }
}

export class ApiRequestError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiRequestError'
    this.status = status
  }
}

export async function getApiStatus(): Promise<ApiStatus> {
  const response = await fetch('/api/status')

  if (!response.ok) {
    throw new Error('A API respondeu com um erro.')
  }

  return response.json() as Promise<ApiStatus>
}

export async function criarCliente(
  cliente: CriarClienteInput,
  antiforgeryToken: string,
): Promise<ClienteCriado> {
  const response = await fetch('/api/clientes', {
    method: 'POST',
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'X-CSRF-TOKEN': antiforgeryToken,
    },
    body: JSON.stringify(cliente),
  })

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblemDetails

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    throw new Error('Não foi possível concluir o cadastro.')
  }

  return response.json() as Promise<ClienteCriado>
}

export async function preencherDadosPessoais(
  token: string,
  dados: PreencherDadosPessoaisInput,
): Promise<DadosPessoaisPreenchidos> {
  const opcionalOuNull = (valor: string) => valor.trim() || null
  const response = await fetch('/api/fichas/dados-pessoais', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      token,
      nomeCompleto: dados.nomeCompleto,
      nomeSocial: opcionalOuNull(dados.nomeSocial),
      pronomes: opcionalOuNull(dados.pronomes),
      dataNascimento: dados.dataNascimento,
      celular: dados.celular,
      email: opcionalOuNull(dados.email),
      instagram: opcionalOuNull(dados.instagram),
      contatoEmergenciaNome: opcionalOuNull(dados.contatoEmergenciaNome),
      contatoEmergenciaCelular: opcionalOuNull(
        dados.contatoEmergenciaCelular,
      ),
    }),
  })

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblemDetails

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ??
        problem?.title ??
        'Não foi possível salvar seus dados pessoais.',
    )
  }

  return response.json() as Promise<DadosPessoaisPreenchidos>
}

export async function listarClientes(
  pagina: number,
  tamanhoPagina: number,
  filtros: FiltrosClientes = {},
  signal?: AbortSignal,
): Promise<ClientesPaginados> {
  const parametros = new URLSearchParams({
    pagina: pagina.toString(),
    tamanhoPagina: tamanhoPagina.toString(),
  })
  adicionarFiltros(parametros, filtros)
  const response = await fetch(`/api/clientes?${parametros}`, {
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : 'Não foi possível consultar os clientes.',
    )
  }

  return response.json() as Promise<ClientesPaginados>
}

export async function obterDetalheCliente(
  clienteId: string,
  signal?: AbortSignal,
): Promise<ClienteDetalhe> {
  const response = await fetch(
    `/api/clientes/${encodeURIComponent(clienteId)}`,
    {
      credentials: 'same-origin',
      signal,
    },
  )

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : response.status === 404
          ? 'O cliente informado não foi encontrado.'
          : 'Não foi possível consultar o cliente.',
    )
  }

  return response.json() as Promise<ClienteDetalhe>
}

export async function listarProfissionais(
  signal?: AbortSignal,
): Promise<ProfissionalResumo[]> {
  const response = await fetch('/api/profissionais', {
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : 'Não foi possível consultar os profissionais.',
    )
  }

  return response.json() as Promise<ProfissionalResumo[]>
}

export async function emitirConviteFicha(
  clienteId: string,
  tipoProcedimento: Exclude<TipoProcedimento, 'NaoInformado'>,
  antiforgeryToken: string,
): Promise<ConviteFichaCriado> {
  const response = await fetch(
    `/api/clientes/${encodeURIComponent(clienteId)}/fichas/convites`,
    {
      method: 'POST',
      credentials: 'same-origin',
      headers: {
        'Content-Type': 'application/json',
        'X-CSRF-TOKEN': antiforgeryToken,
      },
      body: JSON.stringify({ tipoProcedimento }),
    },
  )

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ??
        problem?.title ??
        'Não foi possível gerar o convite.',
    )
  }

  return response.json() as Promise<ConviteFichaCriado>
}

export async function listarFichas(
  pagina: number,
  tamanhoPagina: number,
  filtros: FiltrosFichas = {},
  signal?: AbortSignal,
): Promise<FichasPaginadas> {
  const parametros = new URLSearchParams({
    pagina: pagina.toString(),
    tamanhoPagina: tamanhoPagina.toString(),
  })
  adicionarFiltros(parametros, filtros)
  const response = await fetch(`/api/fichas?${parametros}`, {
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : 'Não foi possível consultar as fichas.',
    )
  }

  return response.json() as Promise<FichasPaginadas>
}

function adicionarFiltros(
  parametros: URLSearchParams,
  filtros: Record<string, string | boolean | undefined>,
) {
  Object.entries(filtros).forEach(([chave, valor]) => {
    if (valor !== undefined && valor !== '') {
      parametros.set(chave, String(valor))
    }
  })
}

export async function obterDetalheFicha(
  fichaId: string,
  signal?: AbortSignal,
): Promise<FichaDetalhe> {
  const response = await fetch(
    `/api/fichas/${encodeURIComponent(fichaId)}`,
    {
      credentials: 'same-origin',
      signal,
    },
  )

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : response.status === 404
          ? 'A ficha informada não foi encontrada.'
          : 'Não foi possível consultar os detalhes da ficha.',
    )
  }

  return response.json() as Promise<FichaDetalhe>
}

export async function registrarAtendimento(
  fichaId: string,
  atendimento: RegistrarAtendimentoInput,
  antiforgeryToken: string,
): Promise<Atendimento> {
  const response = await fetch(
    `/api/fichas/${encodeURIComponent(fichaId)}/atendimento`,
    {
      method: 'PUT',
      credentials: 'same-origin',
      headers: {
        'Content-Type': 'application/json',
        'X-CSRF-TOKEN': antiforgeryToken,
      },
      body: JSON.stringify(atendimento),
    },
  )

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblemDetails

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ??
        problem?.title ??
        'Não foi possível registrar os dados do atendimento.',
    )
  }

  return response.json() as Promise<Atendimento>
}

export async function obterFinanceiro(
  pagina: number,
  tamanhoPagina: number,
  filtros: FiltrosFinanceiro = {},
  signal?: AbortSignal,
): Promise<FinanceiroResultado> {
  const parametros = new URLSearchParams({
    pagina: pagina.toString(),
    tamanhoPagina: tamanhoPagina.toString(),
  })
  adicionarFiltros(parametros, filtros)
  const response = await fetch(`/api/financeiro?${parametros}`, {
    credentials: 'same-origin',
    signal,
  })

  if (!response.ok) {
    throw new ApiRequestError(
      response.status,
      response.status === 401
        ? 'Sua sessão profissional expirou.'
        : 'Não foi possível consultar o financeiro.',
    )
  }

  return response.json() as Promise<FinanceiroResultado>
}

export async function criarDespesa(
  despesa: SalvarDespesaInput,
  antiforgeryToken: string,
): Promise<Despesa> {
  return salvarDespesa('/api/financeiro/despesas', 'POST', despesa, antiforgeryToken)
}

export async function atualizarDespesa(
  despesaId: string,
  despesa: SalvarDespesaInput,
  antiforgeryToken: string,
): Promise<Despesa> {
  return salvarDespesa(
    `/api/financeiro/despesas/${encodeURIComponent(despesaId)}`,
    'PUT',
    despesa,
    antiforgeryToken,
  )
}

async function salvarDespesa(
  url: string,
  metodo: 'POST' | 'PUT',
  despesa: SalvarDespesaInput,
  antiforgeryToken: string,
): Promise<Despesa> {
  const response = await fetch(url, {
    method: metodo,
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'X-CSRF-TOKEN': antiforgeryToken,
    },
    body: JSON.stringify(despesa),
  })

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblemDetails

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ?? problem?.title ?? 'Não foi possível salvar a despesa.',
    )
  }

  return response.json() as Promise<Despesa>
}

export async function abrirConviteFicha(
  token: string,
  signal?: AbortSignal,
): Promise<ConviteFichaAberto> {
  const response = await fetch('/api/fichas/convites/abrir', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ token }),
    signal,
  })

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ??
        problem?.title ??
        'Não foi possível validar este convite.',
    )
  }

  return response.json() as Promise<ConviteFichaAberto>
}

export async function responderQuestionarioSaude(
  token: string,
  respostas: ResponderQuestionarioSaudeInput,
): Promise<QuestionarioSaudeRespondido> {
  const response = await fetch('/api/fichas/questionario-saude', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ token, ...respostas }),
  })

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblemDetails

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ??
        problem?.title ??
        'Não foi possível salvar o questionário.',
    )
  }

  return response.json() as Promise<QuestionarioSaudeRespondido>
}

export async function aceitarTermoConsentimento(
  token: string,
  aceite: AceitarTermoConsentimentoInput,
): Promise<TermoConsentimentoAceito> {
  const response = await fetch('/api/fichas/termo-consentimento/aceitar', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ token, ...aceite }),
  })

  if (response.status === 400) {
    const problem = (await response.json()) as ValidationProblemDetails

    if (problem.errors) {
      throw new ApiValidationError(problem.errors)
    }
  }

  if (!response.ok) {
    let problem: ProblemDetails | null = null

    try {
      problem = (await response.json()) as ProblemDetails
    } catch {
      // Algumas falhas de infraestrutura podem não retornar JSON.
    }

    throw new ApiRequestError(
      response.status,
      problem?.detail ??
        problem?.title ??
        'Não foi possível registrar o aceite do termo.',
    )
  }

  return response.json() as Promise<TermoConsentimentoAceito>
}
