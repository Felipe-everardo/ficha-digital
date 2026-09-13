import {
  adicionarFiltros,
  ApiRequestError,
  criarErroDaApi,
  criarChaveIdempotencia,
  verificarErroValidacao,
} from './http'

export type TipoProcedimento = 'NaoInformado' | 'Tatuagem' | 'Piercing'

export type ConviteFichaCriado = {
  fichaId: string
  conviteId: string
  linkPreenchimento: string
  expiraEmUtc: string
}

export type FichaClienteResumo = {
  id: string
  status: string
  tipoProcedimento: TipoProcedimento
  profissionalResponsavelId: string | null
  profissionalResponsavelNome: string
  criadaEmUtc: string
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
  conviteExpiraEmUtc: string | null
  conviteExpirado: boolean
}

export type FiltrosFichas = {
  busca?: string
  tipoProcedimento?: TipoProcedimento
  status?: string
  criadaDe?: string
  criadaAte?: string
  concluidaDe?: string
  concluidaAte?: string
}

export type FichasPaginadas = {
  itens: FichaResumo[]
  pagina: number
  tamanhoPagina: number
  totalItens: number
  totalPaginas: number
}

export type ClienteFichaDetalhe = {
  id: string
  nomeReferencia: string
  nomeCompleto: string | null
  nomeSocial: string | null
  nomeParaExibicao: string
  pronomes: string | null
  estadoCivil: string | null
  dataNascimento: string | null
  cpf: string | null
  celular: string | null
  telefoneAdicional: string | null
  email: string | null
  instagram: string | null
  contatoEmergenciaNome: string | null
  contatoEmergenciaCelular: string | null
  cep: string | null
  logradouro: string | null
  numero: string | null
  complemento: string | null
  bairro: string | null
  cidade: string | null
  estado: string | null
  dadosPessoaisPreenchidosEmUtc: string | null
}

export type QuestionarioSaudeDetalhe = {
  versao: number
  temDiabetes: boolean
  tipoDiabetes: string | null
  teveAnemia: boolean
  descricaoAnemia: string | null
  teveHepatite: boolean
  tipoHepatite: string | null
  possuiPressaoAlta: boolean
  temAlergia: boolean
  descricaoAlergia: string | null
  possuiCondicaoCardiaca: boolean
  temEpilepsia: boolean
  temHemofilia: boolean
  possuiDoencaTransmissivel: boolean
  descricaoDoencaTransmissivel: string | null
  usaMarcaPasso: boolean
  fuma: boolean
  consumiuBebidaAlcoolicaUltimas24Horas: boolean
  usaMedicacao: boolean
  descricaoMedicacao: string | null
  estaGravidaOuAmamentando: boolean
  respondidoEmUtc: string
}

export type AceiteTermoResumo = {
  versaoTermo: number
  nomeAssinante: string
  aceitoEmUtc: string
  confirmouLeituraEAutorizacao: boolean
  assinaturaDesenhada: string | null
  evidenciaHash: string
  evidenciaIntegra: boolean
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
  versaoModelo: number | null
  versaoQuestionario: number | null
  versaoTermo: number | null
  cnpjApresentado: string | null
  cliente: ClienteFichaDetalhe
  questionarioSaude: QuestionarioSaudeDetalhe | null
  aceiteTermo: AceiteTermoResumo | null
  revisaoProfissional: {
    profissionalNome: string
    dadosDaFichaConferidos: boolean
    revisadaEmUtc: string
  } | null
  registroProcedimento: {
    tipoProcedimento: TipoProcedimento
    profissionalNome: string
    arteEfetivamenteTatuada: string | null
    materialUtilizado: string | null
    localTatuagem: string | null
    joiaUtilizada: string | null
    agulhaUtilizada: string | null
    localPerfuracao: string | null
    observacoes: string | null
    valorTotal: number
    valorSinal: number
    valorRestante: number
    formaPagamento: string
    nomeProfissionalAssinante: string
    assinaturaDesenhada: string
    registradoEmUtc: string
    evidenciaHash: string
    evidenciaIntegra: boolean
  } | null
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
    estadoCivil: string | null
    dataNascimento: string | null
    cpf: string | null
    celular: string | null
    telefoneAdicional: string | null
    email: string | null
    instagram: string | null
    contatoEmergenciaNome: string | null
    contatoEmergenciaCelular: string | null
    cep: string | null
    logradouro: string | null
    numero: string | null
    complemento: string | null
    bairro: string | null
    cidade: string | null
    estado: string | null
  }
  questionarioSaude: QuestionarioSaudeDetalhe | null
  termoConsentimento: TermoConsentimento
}

export type PreencherDadosPessoaisInput = {
  nomeCompleto: string
  nomeSocial: string
  pronomes: string
  estadoCivil: string
  dataNascimento: string
  cpf: string
  celular: string
  telefoneAdicional: string
  email: string
  instagram: string
  contatoEmergenciaNome: string
  contatoEmergenciaCelular: string
  cep: string
  logradouro: string
  numero: string
  complemento: string
  bairro: string
  cidade: string
  estado: string
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
  teveAnemia: boolean
  descricaoAnemia: string | null
  teveHepatite: boolean
  tipoHepatite: string | null
  possuiPressaoAlta: boolean
  temAlergia: boolean
  descricaoAlergia: string | null
  possuiCondicaoCardiaca: boolean
  temEpilepsia: boolean
  temHemofilia: boolean
  possuiDoencaTransmissivel: boolean
  descricaoDoencaTransmissivel: string | null
  usaMarcaPasso: boolean
  fuma: boolean
  consumiuBebidaAlcoolicaUltimas24Horas: boolean
  usaMedicacao: boolean
  descricaoMedicacao: string | null
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
  confirmouLeituraEAutorizacao: boolean
  assinaturaDesenhada: string
}

export type TermoConsentimentoAceito = {
  aceiteId: string
  fichaId: string
  versaoTermo: number
  aceitoEmUtc: string
  evidenciaHash: string
  statusFicha: string
}

export async function emitirConviteFicha(
  clienteId: string,
  profissionalResponsavelNome: string,
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
        'Idempotency-Key': criarChaveIdempotencia(),
      },
      body: JSON.stringify({ profissionalResponsavelNome, tipoProcedimento }),
    },
  )

  if (!response.ok) {
    throw await criarErroDaApi(response, 'Não foi possível gerar o convite.')
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

export async function abrirConviteFicha(
  token: string,
  signal?: AbortSignal,
): Promise<ConviteFichaAberto> {
  const response = await fetch('/api/fichas/convites/abrir', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Idempotency-Key': criarChaveIdempotencia(),
    },
    body: JSON.stringify({ token }),
    signal,
  })

  if (!response.ok) {
    throw await criarErroDaApi(
      response,
      'Não foi possível validar este convite.',
    )
  }

  return response.json() as Promise<ConviteFichaAberto>
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
      'Idempotency-Key': criarChaveIdempotencia(),
    },
    body: JSON.stringify({
      token,
      nomeCompleto: dados.nomeCompleto,
      nomeSocial: opcionalOuNull(dados.nomeSocial),
      pronomes: opcionalOuNull(dados.pronomes),
      estadoCivil: dados.estadoCivil,
      dataNascimento: dados.dataNascimento,
      cpf: dados.cpf,
      celular: dados.celular,
      telefoneAdicional: opcionalOuNull(dados.telefoneAdicional),
      email: opcionalOuNull(dados.email),
      instagram: opcionalOuNull(dados.instagram),
      contatoEmergenciaNome: opcionalOuNull(dados.contatoEmergenciaNome),
      contatoEmergenciaCelular: opcionalOuNull(
        dados.contatoEmergenciaCelular,
      ),
      cep: dados.cep,
      logradouro: dados.logradouro,
      numero: dados.numero,
      complemento: opcionalOuNull(dados.complemento),
      bairro: dados.bairro,
      cidade: dados.cidade,
      estado: dados.estado,
    }),
  })

  await verificarErroValidacao(response)

  if (!response.ok) {
    throw await criarErroDaApi(
      response,
      'Não foi possível salvar seus dados pessoais.',
    )
  }

  return response.json() as Promise<DadosPessoaisPreenchidos>
}

export async function responderQuestionarioSaude(
  token: string,
  respostas: ResponderQuestionarioSaudeInput,
): Promise<QuestionarioSaudeRespondido> {
  const response = await fetch('/api/fichas/questionario-saude', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Idempotency-Key': criarChaveIdempotencia(),
    },
    body: JSON.stringify({ token, ...respostas }),
  })

  await verificarErroValidacao(response)

  if (!response.ok) {
    throw await criarErroDaApi(
      response,
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
      'Idempotency-Key': criarChaveIdempotencia(),
    },
    body: JSON.stringify({ token, ...aceite }),
  })

  await verificarErroValidacao(response)

  if (!response.ok) {
    throw await criarErroDaApi(
      response,
      'Não foi possível registrar o aceite do termo.',
    )
  }

  return response.json() as Promise<TermoConsentimentoAceito>
}

type OperacaoFichaResultado = { fichaId: string; status: string }

async function executarOperacaoFicha(
  fichaId: string,
  operacao: string,
  dados: unknown,
  antiforgeryToken: string,
): Promise<OperacaoFichaResultado> {
  const response = await fetch(
    `/api/fichas/${encodeURIComponent(fichaId)}/operacoes/${operacao}`,
    {
      method: 'POST',
      credentials: 'same-origin',
      headers: {
        'Content-Type': 'application/json',
        'X-CSRF-TOKEN': antiforgeryToken,
        'Idempotency-Key': criarChaveIdempotencia(),
      },
      body: JSON.stringify(dados),
    },
  )

  if (!response.ok) {
    throw await criarErroDaApi(
      response,
      'Não foi possível registrar a operação.',
    )
  }

  return response.json() as Promise<OperacaoFichaResultado>
}

export function revisarFicha(
  fichaId: string,
  antiforgeryToken: string,
) {
  return executarOperacaoFicha(
    fichaId,
    'revisar',
    { dadosDaFichaConferidos: true },
    antiforgeryToken,
  )
}

export type ConcluirProcedimentoInput = {
  valorTotal: number
  valorSinal: number
  formaPagamento: 'Pix' | 'Dinheiro' | 'Cartao'
  assinaturaDesenhada: string
  tatuagem: {
    arteEfetivamenteTatuada: string
    materialUtilizado: string
    localTatuagem: string
    observacoes: string | null
  } | null
  piercing: {
    joiaUtilizada: string
    agulhaUtilizada: string
    localPerfuracao: string
    observacoes: string | null
  } | null
}

export function concluirProcedimento(
  fichaId: string,
  dados: ConcluirProcedimentoInput,
  antiforgeryToken: string,
) {
  return executarOperacaoFicha(
    fichaId,
    'concluir',
    dados,
    antiforgeryToken,
  )
}
