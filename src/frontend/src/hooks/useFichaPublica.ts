import { type FormEvent, useEffect, useState } from 'react'
import type {
  DadosPessoaisFormulario,
  RespostasQuestionario,
} from '../components/ficha-publica/model'
import { formatarCep, formatarCpf } from '../utils/documentos'
import { formatarTelefoneBrasileiro } from '../utils/telefone'
import {
  ApiRequestError,
  ApiValidationError,
  abrirConviteFicha,
  aceitarTermoConsentimento,
  preencherDadosPessoais,
  responderQuestionarioSaude,
  type ConviteFichaAberto,
  type TermoConsentimentoAceito,
} from '../services/api'

type EstadoAbertura =
  | { tipo: 'sem-token' }
  | { tipo: 'carregando' }
  | { tipo: 'aberto'; convite: ConviteFichaAberto }
  | { tipo: 'erro'; titulo: string; mensagem: string }

const respostasIniciais: RespostasQuestionario = {
  temDiabetes: null,
  tipoDiabetes: '',
  teveAnemia: null,
  descricaoAnemia: '',
  teveHepatite: null,
  tipoHepatite: '',
  possuiPressaoAlta: null,
  temAlergia: null,
  descricaoAlergia: '',
  possuiCondicaoCardiaca: null,
  temEpilepsia: null,
  temHemofilia: null,
  possuiDoencaTransmissivel: null,
  descricaoDoencaTransmissivel: '',
  usaMarcaPasso: null,
  fuma: null,
  consumiuBebidaAlcoolicaUltimas24Horas: null,
  usaMedicacao: null,
  descricaoMedicacao: '',
  estaGravidaOuAmamentando: null,
}

const dadosPessoaisIniciais: DadosPessoaisFormulario = {
  nomeCompleto: '',
  nomeSocial: '',
  pronomes: '',
  estadoCivil: '',
  dataNascimento: '',
  cpf: '',
  celular: '',
  telefoneAdicional: '',
  email: '',
  instagram: '',
  contatoEmergenciaNome: '',
  contatoEmergenciaCelular: '',
  cep: '',
  logradouro: '',
  numero: '',
  complemento: '',
  bairro: '',
  cidade: '',
  estado: '',
}

function formatarDataParaInput(data: Date) {
  const ano = data.getFullYear()
  const mes = String(data.getMonth() + 1).padStart(2, '0')
  const dia = String(data.getDate()).padStart(2, '0')

  return `${ano}-${mes}-${dia}`
}

function obterTokenDoConvite(): string | null {
  const chaveSessao = 'ficha-digital.token-convite'
  const segmentos = window.location.pathname.split('/').filter(Boolean)
  const tokenNoCaminho =
    segmentos[0] === 'fichas' && segmentos[1] === 'preencher'
      ? segmentos[2]?.trim()
      : undefined

  if (tokenNoCaminho) {
    window.sessionStorage.setItem(chaveSessao, tokenNoCaminho)
    window.history.replaceState(
      window.history.state,
      '',
      '/fichas/preencher',
    )
  }

  return tokenNoCaminho || window.sessionStorage.getItem(chaveSessao)
}

function obterTituloDoErro(status: number) {
  if (status === 404) return 'Convite não encontrado'
  if (status === 410) return 'Este convite expirou'
  if (status === 409) return 'Ficha indisponível'
  if (status === 429) return 'Muitas tentativas em pouco tempo'

  return 'Não foi possível abrir a ficha'
}

function primeiraMensagemDeValidacao(error: ApiValidationError) {
  return Object.values(error.errors).flat()[0]
}

let tokenInicialDoConvite = obterTokenDoConvite()

export function useFichaPublica() {
  const dataLimiteMaioridade = new Date()
  dataLimiteMaioridade.setFullYear(dataLimiteMaioridade.getFullYear() - 18)
  const dataMaximaNascimento = formatarDataParaInput(dataLimiteMaioridade)
  const [tokenDoConvite, setTokenDoConvite] = useState(tokenInicialDoConvite)
  const [tentativa, setTentativa] = useState(0)
  const [estado, setEstado] = useState<EstadoAbertura>(
    tokenDoConvite ? { tipo: 'carregando' } : { tipo: 'sem-token' },
  )
  const [respostas, setRespostas] =
    useState<RespostasQuestionario>(respostasIniciais)
  const [dadosPessoais, setDadosPessoais] =
    useState<DadosPessoaisFormulario>(dadosPessoaisIniciais)
  const [dadosPessoaisPreenchidos, setDadosPessoaisPreenchidos] =
    useState(false)
  const [enviandoDadosPessoais, setEnviandoDadosPessoais] = useState(false)
  const [erroDadosPessoais, setErroDadosPessoais] = useState<string | null>(
    null,
  )
  const [questionarioRespondido, setQuestionarioRespondido] = useState(false)
  const [enviandoQuestionario, setEnviandoQuestionario] = useState(false)
  const [erroQuestionario, setErroQuestionario] = useState<string | null>(null)
  const [nomeAssinante, setNomeAssinante] = useState('')
  const [confirmouLeituraEAutorizacao, setConfirmouLeituraEAutorizacao] =
    useState(false)
  const [assinaturaDesenhada, setAssinaturaDesenhada] = useState<
    string | null
  >(null)
  const [enviandoAceite, setEnviandoAceite] = useState(false)
  const [erroAceite, setErroAceite] = useState<string | null>(null)
  const [termoAceito, setTermoAceito] =
    useState<TermoConsentimentoAceito | null>(null)

  useEffect(() => {
    if (!tokenDoConvite) return

    const abortController = new AbortController()
    setEstado({ tipo: 'carregando' })

    abrirConviteFicha(tokenDoConvite, abortController.signal)
      .then((convite) => {
        setEstado({ tipo: 'aberto', convite })
        setDadosPessoais({
          nomeCompleto: convite.dadosPessoais.nomeCompleto ?? '',
          nomeSocial: convite.dadosPessoais.nomeSocial ?? '',
          pronomes: convite.dadosPessoais.pronomes ?? '',
          estadoCivil: convite.dadosPessoais.estadoCivil ?? '',
          dataNascimento: convite.dadosPessoais.dataNascimento ?? '',
          cpf: formatarCpf(convite.dadosPessoais.cpf ?? ''),
          celular: formatarTelefoneBrasileiro(
            convite.dadosPessoais.celular ?? '',
          ),
          telefoneAdicional: formatarTelefoneBrasileiro(
            convite.dadosPessoais.telefoneAdicional ?? '',
          ),
          email: convite.dadosPessoais.email ?? '',
          instagram: convite.dadosPessoais.instagram ?? '',
          contatoEmergenciaNome:
            convite.dadosPessoais.contatoEmergenciaNome ?? '',
          contatoEmergenciaCelular: formatarTelefoneBrasileiro(
            convite.dadosPessoais.contatoEmergenciaCelular ?? '',
          ),
          cep: formatarCep(convite.dadosPessoais.cep ?? ''),
          logradouro: convite.dadosPessoais.logradouro ?? '',
          numero: convite.dadosPessoais.numero ?? '',
          complemento: convite.dadosPessoais.complemento ?? '',
          bairro: convite.dadosPessoais.bairro ?? '',
          cidade: convite.dadosPessoais.cidade ?? '',
          estado: convite.dadosPessoais.estado ?? '',
        })
        if (convite.questionarioSaude) {
          setRespostas({
            temDiabetes: convite.questionarioSaude.temDiabetes,
            tipoDiabetes: convite.questionarioSaude.tipoDiabetes ?? '',
            teveAnemia: convite.questionarioSaude.teveAnemia,
            descricaoAnemia:
              convite.questionarioSaude.descricaoAnemia ?? '',
            teveHepatite: convite.questionarioSaude.teveHepatite,
            tipoHepatite: convite.questionarioSaude.tipoHepatite ?? '',
            possuiPressaoAlta:
              convite.questionarioSaude.possuiPressaoAlta,
            temAlergia: convite.questionarioSaude.temAlergia,
            descricaoAlergia:
              convite.questionarioSaude.descricaoAlergia ?? '',
            possuiCondicaoCardiaca:
              convite.questionarioSaude.possuiCondicaoCardiaca,
            temEpilepsia: convite.questionarioSaude.temEpilepsia,
            temHemofilia: convite.questionarioSaude.temHemofilia,
            possuiDoencaTransmissivel:
              convite.questionarioSaude.possuiDoencaTransmissivel,
            descricaoDoencaTransmissivel:
              convite.questionarioSaude.descricaoDoencaTransmissivel ?? '',
            usaMarcaPasso: convite.questionarioSaude.usaMarcaPasso,
            fuma: convite.questionarioSaude.fuma,
            consumiuBebidaAlcoolicaUltimas24Horas:
              convite.questionarioSaude
                .consumiuBebidaAlcoolicaUltimas24Horas,
            usaMedicacao: convite.questionarioSaude.usaMedicacao,
            descricaoMedicacao:
              convite.questionarioSaude.descricaoMedicacao ?? '',
            estaGravidaOuAmamentando:
              convite.questionarioSaude.estaGravidaOuAmamentando,
          })
        }
        setNomeAssinante(convite.dadosPessoais.nomeCompleto ?? '')
        // Em um novo atendimento, os dados anteriores precisam ser
        // confirmados novamente antes do questionário de saúde.
        setDadosPessoaisPreenchidos(convite.dadosPessoaisPreenchidos)
        setQuestionarioRespondido(convite.questionarioRespondido)
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        if (error instanceof ApiRequestError) {
          setEstado({
            tipo: 'erro',
            titulo: obterTituloDoErro(error.status),
            mensagem: error.message,
          })
          return
        }

        setEstado({
          tipo: 'erro',
          titulo: 'Não foi possível acessar o sistema',
          mensagem:
            'Verifique sua conexão e tente novamente em alguns instantes.',
        })
      })

    return () => abortController.abort()
  }, [tentativa, tokenDoConvite])

  function atualizarDadoPessoal(
    campo: keyof DadosPessoaisFormulario,
    valor: string,
  ) {
    setDadosPessoais((dadosAtuais) => ({
      ...dadosAtuais,
      [campo]: valor,
    }))
    setErroDadosPessoais(null)
  }

  async function enviarDadosPessoais(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!tokenDoConvite) return

    const contatoNomeInformado = Boolean(
      dadosPessoais.contatoEmergenciaNome.trim(),
    )
    const contatoCelularInformado = Boolean(
      dadosPessoais.contatoEmergenciaCelular.trim(),
    )

    if (contatoNomeInformado !== contatoCelularInformado) {
      setErroDadosPessoais(
        'Para o contato de emergência, informe o nome e o celular juntos.',
      )
      return
    }

    if (dadosPessoais.dataNascimento > dataMaximaNascimento) {
      setErroDadosPessoais(
        'O estúdio realiza procedimentos somente em pessoas com 18 anos ou mais.',
      )
      return
    }

    setEnviandoDadosPessoais(true)
    setErroDadosPessoais(null)

    try {
      const resultado = await preencherDadosPessoais(
        tokenDoConvite,
        dadosPessoais,
      )

      setDadosPessoaisPreenchidos(true)
      setNomeAssinante(dadosPessoais.nomeCompleto.trim())

      setEstado((estadoAtual) =>
        estadoAtual.tipo === 'aberto'
          ? {
              ...estadoAtual,
              convite: {
                ...estadoAtual.convite,
                dadosPessoaisPreenchidos: true,
                nomeReferencia: resultado.nomeParaExibicao,
              },
            }
          : estadoAtual,
      )
    } catch (error) {
      if (error instanceof ApiValidationError) {
        setErroDadosPessoais(
          primeiraMensagemDeValidacao(error) ??
            'Confira os dados e tente novamente.',
        )
      } else if (error instanceof ApiRequestError) {
        setErroDadosPessoais(error.message)
      } else {
        setErroDadosPessoais(
          'Não foi possível salvar seus dados. Tente novamente.',
        )
      }
    } finally {
      setEnviandoDadosPessoais(false)
    }
  }

  function atualizarResposta<Campo extends keyof RespostasQuestionario>(
    campo: Campo,
    valor: RespostasQuestionario[Campo],
  ) {
    setRespostas((respostasAtuais) => ({
      ...respostasAtuais,
      [campo]: valor,
    }))
    setErroQuestionario(null)
  }

  async function enviarQuestionario(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (!tokenDoConvite) return

    const {
      temDiabetes,
      teveAnemia,
      teveHepatite,
      possuiPressaoAlta,
      temAlergia,
      possuiCondicaoCardiaca,
      temEpilepsia,
      temHemofilia,
      possuiDoencaTransmissivel,
      usaMarcaPasso,
      fuma,
      consumiuBebidaAlcoolicaUltimas24Horas,
      usaMedicacao,
      estaGravidaOuAmamentando,
    } = respostas

    if (
      temDiabetes === null ||
      teveAnemia === null ||
      teveHepatite === null ||
      possuiPressaoAlta === null ||
      temAlergia === null ||
      possuiCondicaoCardiaca === null ||
      temEpilepsia === null ||
      temHemofilia === null ||
      possuiDoencaTransmissivel === null ||
      usaMarcaPasso === null ||
      fuma === null ||
      consumiuBebidaAlcoolicaUltimas24Horas === null ||
      usaMedicacao === null ||
      estaGravidaOuAmamentando === null
    ) {
      setErroQuestionario('Responda todas as perguntas antes de continuar.')
      return
    }

    if (temDiabetes && !respostas.tipoDiabetes.trim()) {
      setErroQuestionario('Informe o tipo de diabetes.')
      return
    }

    if (temAlergia && !respostas.descricaoAlergia.trim()) {
      setErroQuestionario('Descreva a alergia informada.')
      return
    }

    if (teveAnemia && !respostas.descricaoAnemia.trim()) {
      setErroQuestionario('Informe os detalhes sobre a anemia.')
      return
    }

    if (teveHepatite && !respostas.tipoHepatite.trim()) {
      setErroQuestionario('Informe o tipo de hepatite.')
      return
    }

    if (
      possuiDoencaTransmissivel &&
      !respostas.descricaoDoencaTransmissivel.trim()
    ) {
      setErroQuestionario('Informe qual é a doença transmissível.')
      return
    }

    if (usaMedicacao && !respostas.descricaoMedicacao.trim()) {
      setErroQuestionario('Informe qual medicação você utiliza.')
      return
    }

    setEnviandoQuestionario(true)
    setErroQuestionario(null)

    try {
      await responderQuestionarioSaude(tokenDoConvite, {
        temDiabetes,
        tipoDiabetes: temDiabetes ? respostas.tipoDiabetes.trim() : null,
        teveAnemia,
        descricaoAnemia: teveAnemia
          ? respostas.descricaoAnemia.trim()
          : null,
        teveHepatite,
        tipoHepatite: teveHepatite
          ? respostas.tipoHepatite.trim()
          : null,
        possuiPressaoAlta,
        temAlergia,
        descricaoAlergia: temAlergia
          ? respostas.descricaoAlergia.trim()
          : null,
        possuiCondicaoCardiaca,
        temEpilepsia,
        temHemofilia,
        possuiDoencaTransmissivel,
        descricaoDoencaTransmissivel: possuiDoencaTransmissivel
          ? respostas.descricaoDoencaTransmissivel.trim()
          : null,
        usaMarcaPasso,
        fuma,
        consumiuBebidaAlcoolicaUltimas24Horas,
        usaMedicacao,
        descricaoMedicacao: usaMedicacao
          ? respostas.descricaoMedicacao.trim()
          : null,
        estaGravidaOuAmamentando,
      })

      setQuestionarioRespondido(true)
      setEstado((estadoAtual) =>
        estadoAtual.tipo === 'aberto'
          ? {
              ...estadoAtual,
              convite: {
                ...estadoAtual.convite,
                status: 'AnamnesePreenchida',
                questionarioRespondido: true,
              },
            }
          : estadoAtual,
      )
    } catch (error) {
      if (error instanceof ApiValidationError) {
        setErroQuestionario(
          primeiraMensagemDeValidacao(error) ??
            'Confira as respostas e tente novamente.',
        )
      } else if (error instanceof ApiRequestError) {
        setErroQuestionario(error.message)
      } else {
        setErroQuestionario(
          'Não foi possível salvar as respostas. Tente novamente.',
        )
      }
    } finally {
      setEnviandoQuestionario(false)
    }
  }

  async function enviarAceite(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (
      !tokenDoConvite ||
      estado.tipo !== 'aberto' ||
      !questionarioRespondido
    ) {
      return
    }

    const nomeNormalizado = nomeAssinante.trim()

    if (!nomeNormalizado) {
      setErroAceite('Informe seu nome para registrar o aceite.')
      return
    }

    if (!confirmouLeituraEAutorizacao) {
      setErroAceite(
        'Confirme que leu, entendeu e autoriza o procedimento.',
      )
      return
    }

    if (!assinaturaDesenhada) {
      setErroAceite('Desenhe sua assinatura antes de continuar.')
      return
    }

    setEnviandoAceite(true)
    setErroAceite(null)

    const termo = estado.convite.termoConsentimento

    try {
      const aceite = await aceitarTermoConsentimento(tokenDoConvite, {
        versaoTermo: termo.versao,
        conteudoHash: termo.conteudoHash,
        nomeAssinante: nomeNormalizado,
        confirmouLeituraEAutorizacao,
        assinaturaDesenhada,
      })

      setTermoAceito(aceite)
      setNomeAssinante('')
      setAssinaturaDesenhada(null)
      setConfirmouLeituraEAutorizacao(false)
      setQuestionarioRespondido(false)
      tokenInicialDoConvite = null
      window.sessionStorage.removeItem('ficha-digital.token-convite')
      setTokenDoConvite(null)
    } catch (error) {
      if (error instanceof ApiValidationError) {
        setErroAceite(
          primeiraMensagemDeValidacao(error) ??
            'Confira os dados do aceite e tente novamente.',
        )
      } else if (error instanceof ApiRequestError) {
        setErroAceite(error.message)
      } else {
        setErroAceite(
          'Não foi possível concluir a ficha. Tente novamente.',
        )
      }
    } finally {
      setEnviandoAceite(false)
    }
  }

  const conviteAberto = estado.tipo === 'aberto'
  const dadosPessoaisConcluidos =
    dadosPessoaisPreenchidos || questionarioRespondido || termoAceito !== null
  const questionarioConcluido = questionarioRespondido || termoAceito !== null
  const fichaConcluida = termoAceito !== null
  const possuiDadosAnteriores =
    estado.tipo === 'aberto' &&
    estado.convite.dadosPessoaisPreenchidos

  return {
    estado,
    tentarNovamente: () => setTentativa((valorAtual) => valorAtual + 1),
    progresso: {
      conviteAberto,
      dadosPessoaisConcluidos,
      questionarioConcluido,
      fichaConcluida,
    },
    dadosPessoais: {
      preenchidos: dadosPessoaisPreenchidos,
      valores: dadosPessoais,
      possuiDadosAnteriores,
      dataMaximaNascimento,
      erro: erroDadosPessoais,
      enviando: enviandoDadosPessoais,
      enviar: enviarDadosPessoais,
      alterar: atualizarDadoPessoal,
    },
    questionario: {
      respondido: questionarioRespondido,
      respostas,
      erro: erroQuestionario,
      enviando: enviandoQuestionario,
      enviar: enviarQuestionario,
      alterar: atualizarResposta,
    },
    consentimento: {
      termoAceito,
      confirmouLeituraEAutorizacao,
      nomeAssinante,
      assinaturaDesenhada,
      erro: erroAceite,
      enviando: enviandoAceite,
      enviar: enviarAceite,
      alterarConfirmacao: (valor: boolean) => {
        setConfirmouLeituraEAutorizacao(valor)
        setErroAceite(null)
      },
      alterarAssinatura: (valor: string | null) => {
        setAssinaturaDesenhada(valor)
        setErroAceite(null)
      },
      alterarNomeAssinante: (valor: string) => {
        setNomeAssinante(valor)
        setErroAceite(null)
      },
    },
  }
}
