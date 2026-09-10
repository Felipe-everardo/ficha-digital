import { type FormEvent, useEffect, useState } from 'react'
import type {
  CampoConfirmacao,
  DadosPessoaisFormulario,
  RespostasQuestionario,
} from '../components/ficha-publica/model'
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
  possuiPressaoAlta: null,
  temAlergia: null,
  descricaoAlergia: '',
  possuiCondicaoCardiaca: null,
  temEpilepsia: null,
  temHemofilia: null,
  usaMarcaPasso: null,
  estaGravidaOuAmamentando: null,
}

const dadosPessoaisIniciais: DadosPessoaisFormulario = {
  nomeCompleto: '',
  nomeSocial: '',
  pronomes: '',
  dataNascimento: '',
  celular: '',
  email: '',
  instagram: '',
  contatoEmergenciaNome: '',
  contatoEmergenciaCelular: '',
}

function formatarDataParaInput(data: Date) {
  const ano = data.getFullYear()
  const mes = String(data.getMonth() + 1).padStart(2, '0')
  const dia = String(data.getDate()).padStart(2, '0')

  return `${ano}-${mes}-${dia}`
}

function obterTokenDoConvite(): string | null {
  const segmentos = window.location.pathname.split('/').filter(Boolean)
  const tokenNoCaminho =
    segmentos[0] === 'fichas' && segmentos[1] === 'preencher'
      ? segmentos[2]?.trim()
      : undefined

  if (tokenNoCaminho) {
    window.history.replaceState(
      window.history.state,
      '',
      '/fichas/preencher',
    )
  }

  return tokenNoCaminho || null
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
  const [aceitouTermo, setAceitouTermo] = useState(false)
  const [confirmouMaioridade, setConfirmouMaioridade] = useState(false)
  const [confirmouDadosPessoais, setConfirmouDadosPessoais] = useState(false)
  const [confirmouQuestionarioSaude, setConfirmouQuestionarioSaude] =
    useState(false)
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
          dataNascimento: convite.dadosPessoais.dataNascimento ?? '',
          celular: formatarTelefoneBrasileiro(
            convite.dadosPessoais.celular ?? '',
          ),
          email: convite.dadosPessoais.email ?? '',
          instagram: convite.dadosPessoais.instagram ?? '',
          contatoEmergenciaNome:
            convite.dadosPessoais.contatoEmergenciaNome ?? '',
          contatoEmergenciaCelular: formatarTelefoneBrasileiro(
            convite.dadosPessoais.contatoEmergenciaCelular ?? '',
          ),
        })
        if (convite.questionarioSaude) {
          setRespostas({
            temDiabetes: convite.questionarioSaude.temDiabetes,
            tipoDiabetes: convite.questionarioSaude.tipoDiabetes ?? '',
            possuiPressaoAlta:
              convite.questionarioSaude.possuiPressaoAlta,
            temAlergia: convite.questionarioSaude.temAlergia,
            descricaoAlergia:
              convite.questionarioSaude.descricaoAlergia ?? '',
            possuiCondicaoCardiaca:
              convite.questionarioSaude.possuiCondicaoCardiaca,
            temEpilepsia: convite.questionarioSaude.temEpilepsia,
            temHemofilia: convite.questionarioSaude.temHemofilia,
            usaMarcaPasso: convite.questionarioSaude.usaMarcaPasso,
            estaGravidaOuAmamentando:
              convite.questionarioSaude.estaGravidaOuAmamentando,
          })
        }
        // Em um novo atendimento, os dados anteriores precisam ser
        // confirmados novamente antes do questionário de saúde.
        setDadosPessoaisPreenchidos(convite.questionarioRespondido)
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
      possuiPressaoAlta,
      temAlergia,
      possuiCondicaoCardiaca,
      temEpilepsia,
      temHemofilia,
      usaMarcaPasso,
      estaGravidaOuAmamentando,
    } = respostas

    if (
      temDiabetes === null ||
      possuiPressaoAlta === null ||
      temAlergia === null ||
      possuiCondicaoCardiaca === null ||
      temEpilepsia === null ||
      temHemofilia === null ||
      usaMarcaPasso === null ||
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

    setEnviandoQuestionario(true)
    setErroQuestionario(null)

    try {
      await responderQuestionarioSaude(tokenDoConvite, {
        temDiabetes,
        tipoDiabetes: temDiabetes ? respostas.tipoDiabetes.trim() : null,
        possuiPressaoAlta,
        temAlergia,
        descricaoAlergia: temAlergia
          ? respostas.descricaoAlergia.trim()
          : null,
        possuiCondicaoCardiaca,
        temEpilepsia,
        temHemofilia,
        usaMarcaPasso,
        estaGravidaOuAmamentando,
      })

      setQuestionarioRespondido(true)
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

    if (
      !confirmouMaioridade ||
      !confirmouDadosPessoais ||
      !confirmouQuestionarioSaude ||
      !aceitouTermo
    ) {
      setErroAceite(
        'Confirme todas as declarações antes de concluir a ficha.',
      )
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
        aceitouTermo,
        confirmouMaioridade,
        confirmouDadosPessoais,
        confirmouQuestionarioSaude,
      })

      setTermoAceito(aceite)
      setNomeAssinante('')
      setAceitouTermo(false)
      setConfirmouMaioridade(false)
      setConfirmouDadosPessoais(false)
      setConfirmouQuestionarioSaude(false)
      setQuestionarioRespondido(false)
      tokenInicialDoConvite = null
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

  function atualizarConfirmacao(campo: CampoConfirmacao, valor: boolean) {
    const atualizadores = {
      maioridade: setConfirmouMaioridade,
      dadosPessoais: setConfirmouDadosPessoais,
      questionarioSaude: setConfirmouQuestionarioSaude,
      aceiteTermo: setAceitouTermo,
    }

    atualizadores[campo](valor)
    setErroAceite(null)
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
      confirmacoes: {
        maioridade: confirmouMaioridade,
        dadosPessoais: confirmouDadosPessoais,
        questionarioSaude: confirmouQuestionarioSaude,
        aceiteTermo: aceitouTermo,
      },
      nomeAssinante,
      erro: erroAceite,
      enviando: enviandoAceite,
      enviar: enviarAceite,
      alterarConfirmacao: atualizarConfirmacao,
      alterarNomeAssinante: (valor: string) => {
        setNomeAssinante(valor)
        setErroAceite(null)
      },
    },
  }
}
