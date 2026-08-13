import { type FormEvent, useEffect, useState } from 'react'
import {
  ApiRequestError,
  ApiValidationError,
  abrirConviteFicha,
  aceitarTermoConsentimento,
  responderQuestionarioSaude,
  type ConviteFichaAberto,
  type TermoConsentimentoAceito,
} from '../services/api'
import './FichaPublicaPage.css'

type EstadoAbertura =
  | { tipo: 'sem-token' }
  | { tipo: 'carregando' }
  | { tipo: 'aberto'; convite: ConviteFichaAberto }
  | { tipo: 'erro'; titulo: string; mensagem: string }

type RespostasQuestionario = {
  temDiabetes: boolean | null
  tipoDiabetes: string
  possuiPressaoAlta: boolean | null
  temAlergia: boolean | null
  descricaoAlergia: string
  possuiCondicaoCardiaca: boolean | null
  temEpilepsia: boolean | null
  temHemofilia: boolean | null
  usaMarcaPasso: boolean | null
  estaGravidaOuAmamentando: boolean | null
}

type PerguntaSimNaoProps = {
  nome: string
  pergunta: string
  valor: boolean | null
  aoResponder: (valor: boolean) => void
}

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

function PerguntaSimNao({
  nome,
  pergunta,
  valor,
  aoResponder,
}: PerguntaSimNaoProps) {
  return (
    <fieldset className="binary-question" aria-required="true">
      <legend>{pergunta}</legend>
      <div className="binary-options">
        <label>
          <input
            type="radio"
            name={nome}
            checked={valor === true}
            onChange={() => aoResponder(true)}
          />
          <span>Sim</span>
        </label>
        <label>
          <input
            type="radio"
            name={nome}
            checked={valor === false}
            onChange={() => aoResponder(false)}
          />
          <span>Não</span>
        </label>
      </div>
    </fieldset>
  )
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

function formatarDataHora(dataIso: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'long',
    timeStyle: 'short',
  }).format(new Date(dataIso))
}

let tokenInicialDoConvite = obterTokenDoConvite()

export function FichaPublicaPage() {
  const [tokenDoConvite, setTokenDoConvite] = useState(tokenInicialDoConvite)
  const [tentativa, setTentativa] = useState(0)
  const [estado, setEstado] = useState<EstadoAbertura>(
    tokenDoConvite ? { tipo: 'carregando' } : { tipo: 'sem-token' },
  )
  const [respostas, setRespostas] =
    useState<RespostasQuestionario>(respostasIniciais)
  const [questionarioRespondido, setQuestionarioRespondido] = useState(false)
  const [enviandoQuestionario, setEnviandoQuestionario] = useState(false)
  const [erroQuestionario, setErroQuestionario] = useState<string | null>(null)
  const [nomeAssinante, setNomeAssinante] = useState('')
  const [aceitouTermo, setAceitouTermo] = useState(false)
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
      setRespostas(respostasIniciais)
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

    if (!aceitouTermo) {
      setErroAceite('Confirme que leu e aceita o termo para concluir a ficha.')
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
      })

      setTermoAceito(aceite)
      setNomeAssinante('')
      setAceitouTermo(false)
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

  const conviteAberto = estado.tipo === 'aberto'
  const questionarioConcluido = questionarioRespondido || termoAceito !== null
  const fichaConcluida = termoAceito !== null

  return (
    <main className="public-page-shell">
      <section className="public-card" aria-labelledby="public-page-title">
        <header className="public-header">
          <div className="brand-mark" aria-hidden="true">
            FD
          </div>

          <div>
            <p className="eyebrow">Área segura do cliente</p>
            <h1 id="public-page-title">Sua ficha digital</h1>
            <p className="intro">
              Preencha as informações com calma. Seus dados serão utilizados
              somente para o atendimento no estúdio.
            </p>
          </div>
        </header>

        <ol className="flow-progress" aria-label="Etapas da ficha">
          <li
            className={`flow-progress__item ${
              conviteAberto
                ? 'flow-progress__item--complete'
                : 'flow-progress__item--active'
            }`}
          >
            <span>{conviteAberto ? '✓' : '1'}</span>
            Validar convite
          </li>
          <li
            className={`flow-progress__item ${
              questionarioConcluido
                ? 'flow-progress__item--complete'
                : conviteAberto
                  ? 'flow-progress__item--active'
                  : ''
            }`}
          >
            <span>{questionarioConcluido ? '✓' : '2'}</span>
            Saúde
          </li>
          <li
            className={`flow-progress__item ${
              fichaConcluida
                ? 'flow-progress__item--complete'
                : questionarioConcluido
                  ? 'flow-progress__item--active'
                  : ''
            }`}
          >
            <span>{fichaConcluida ? '✓' : '3'}</span>
            Consentimento
          </li>
        </ol>

        <div className="public-content" aria-live="polite">
          {estado.tipo === 'carregando' && (
            <div className="opening-state" aria-busy="true">
              <span className="loading-indicator" aria-hidden="true" />
              <div>
                <h2>Validando seu convite</h2>
                <p>Isso deve levar apenas alguns segundos.</p>
              </div>
            </div>
          )}

          {estado.tipo === 'sem-token' && (
            <div className="opening-state opening-state--warning" role="alert">
              <span className="state-symbol" aria-hidden="true">
                !
              </span>
              <div>
                <h2>Link incompleto</h2>
                <p>
                  Abra novamente o link completo enviado pelo estúdio. Nenhuma
                  informação foi enviada.
                </p>
              </div>
            </div>
          )}

          {estado.tipo === 'erro' && (
            <div className="opening-state opening-state--error" role="alert">
              <span className="state-symbol" aria-hidden="true">
                !
              </span>
              <div>
                <h2>{estado.titulo}</h2>
                <p>{estado.mensagem}</p>
                <button
                  className="secondary-button compact-button"
                  type="button"
                  onClick={() => setTentativa((valorAtual) => valorAtual + 1)}
                >
                  Tentar novamente
                </button>
              </div>
            </div>
          )}

          {estado.tipo === 'aberto' &&
            !questionarioRespondido &&
            !termoAceito && (
            <form className="health-form" onSubmit={enviarQuestionario}>
              <div className="section-heading">
                <p className="eyebrow">Etapa 2 de 3</p>
                <h2>Histórico de saúde</h2>
                <p>
                  Todas as perguntas precisam ser respondidas. Quando você
                  marcar “Sim”, poderão aparecer informações complementares.
                </p>
              </div>

              <div className="health-notice">
                Use somente dados fictícios nesta versão de desenvolvimento.
              </div>

              <div className="questions-list">
                <PerguntaSimNao
                  nome="temDiabetes"
                  pergunta="Tem diabetes?"
                  valor={respostas.temDiabetes}
                  aoResponder={(valor) => {
                    atualizarResposta('temDiabetes', valor)
                    if (!valor) atualizarResposta('tipoDiabetes', '')
                  }}
                />

                {respostas.temDiabetes === true && (
                  <label className="conditional-field">
                    <span>Qual é o tipo de diabetes? *</span>
                    <input
                      type="text"
                      maxLength={100}
                      required
                      value={respostas.tipoDiabetes}
                      onChange={(event) =>
                        atualizarResposta('tipoDiabetes', event.target.value)
                      }
                    />
                  </label>
                )}

                <PerguntaSimNao
                  nome="possuiPressaoAlta"
                  pergunta="Possui pressão alta?"
                  valor={respostas.possuiPressaoAlta}
                  aoResponder={(valor) =>
                    atualizarResposta('possuiPressaoAlta', valor)
                  }
                />

                <PerguntaSimNao
                  nome="temAlergia"
                  pergunta="Tem alguma alergia?"
                  valor={respostas.temAlergia}
                  aoResponder={(valor) => {
                    atualizarResposta('temAlergia', valor)
                    if (!valor) atualizarResposta('descricaoAlergia', '')
                  }}
                />

                {respostas.temAlergia === true && (
                  <label className="conditional-field">
                    <span>Descreva a alergia *</span>
                    <textarea
                      maxLength={300}
                      required
                      value={respostas.descricaoAlergia}
                      onChange={(event) =>
                        atualizarResposta(
                          'descricaoAlergia',
                          event.target.value,
                        )
                      }
                    />
                  </label>
                )}

                <PerguntaSimNao
                  nome="possuiCondicaoCardiaca"
                  pergunta="Possui alguma condição cardíaca?"
                  valor={respostas.possuiCondicaoCardiaca}
                  aoResponder={(valor) =>
                    atualizarResposta('possuiCondicaoCardiaca', valor)
                  }
                />

                <PerguntaSimNao
                  nome="temEpilepsia"
                  pergunta="Tem epilepsia?"
                  valor={respostas.temEpilepsia}
                  aoResponder={(valor) =>
                    atualizarResposta('temEpilepsia', valor)
                  }
                />

                <PerguntaSimNao
                  nome="temHemofilia"
                  pergunta="Tem hemofilia?"
                  valor={respostas.temHemofilia}
                  aoResponder={(valor) =>
                    atualizarResposta('temHemofilia', valor)
                  }
                />

                <PerguntaSimNao
                  nome="usaMarcaPasso"
                  pergunta="Usa marca-passo?"
                  valor={respostas.usaMarcaPasso}
                  aoResponder={(valor) =>
                    atualizarResposta('usaMarcaPasso', valor)
                  }
                />

                <PerguntaSimNao
                  nome="estaGravidaOuAmamentando"
                  pergunta="Está grávida ou amamentando?"
                  valor={respostas.estaGravidaOuAmamentando}
                  aoResponder={(valor) =>
                    atualizarResposta('estaGravidaOuAmamentando', valor)
                  }
                />
              </div>

              {erroQuestionario && (
                <p className="form-error" role="alert">
                  {erroQuestionario}
                </p>
              )}

              <div className="questionnaire-actions">
                <p>
                  Revise suas respostas. Depois de salvar, elas não poderão ser
                  editadas neste fluxo.
                </p>
                <button type="submit" disabled={enviandoQuestionario}>
                  {enviandoQuestionario
                    ? 'Salvando respostas...'
                    : 'Salvar e continuar'}
                </button>
              </div>
            </form>
          )}

          {estado.tipo === 'aberto' &&
            questionarioRespondido &&
            !termoAceito && (
              <form className="consent-form" onSubmit={enviarAceite}>
                <div className="section-heading">
                  <p className="eyebrow">Etapa 3 de 3</p>
                  <h2>Termo de consentimento</h2>
                  <p>
                    Leia o conteúdo completo antes de confirmar. O sistema
                    registrará esta versão exata do termo junto ao seu aceite.
                  </p>
                </div>

                <div className="health-notice">
                  Este é um termo provisório de desenvolvimento. Continue
                  somente com informações fictícias.
                </div>

                <article
                  className="term-document"
                  aria-label="Conteúdo do termo de consentimento"
                  tabIndex={0}
                >
                  <p className="term-version">
                    Versão {estado.convite.termoConsentimento.versao}
                  </p>
                  <p className="term-document__content">
                    {estado.convite.termoConsentimento.conteudo}
                  </p>
                </article>

                <label className="consent-name-field">
                  <span>Seu nome completo *</span>
                  <input
                    type="text"
                    autoComplete="name"
                    maxLength={150}
                    required
                    value={nomeAssinante}
                    onChange={(event) => {
                      setNomeAssinante(event.target.value)
                      setErroAceite(null)
                    }}
                  />
                  <small>
                    Digite o nome da pessoa que está declarando o aceite.
                  </small>
                </label>

                <label className="consent-checkbox">
                  <input
                    type="checkbox"
                    checked={aceitouTermo}
                    required
                    onChange={(event) => {
                      setAceitouTermo(event.target.checked)
                      setErroAceite(null)
                    }}
                  />
                  <span>
                    Declaro que li o termo acima e confirmo seu aceite para
                    concluir esta ficha.
                  </span>
                </label>

                {erroAceite && (
                  <p className="form-error" role="alert">
                    {erroAceite}
                  </p>
                )}

                <div className="questionnaire-actions">
                  <p>
                    Ao concluir, a ficha ficará fechada para novas respostas
                    por este convite.
                  </p>
                  <button type="submit" disabled={enviandoAceite}>
                    {enviandoAceite
                      ? 'Registrando aceite...'
                      : 'Aceitar e concluir ficha'}
                  </button>
                </div>
              </form>
            )}

          {termoAceito && (
            <div className="completion-panel" role="status">
              <span className="completion-symbol" aria-hidden="true">
                ✓
              </span>
              <p className="eyebrow">Ficha concluída</p>
              <h2>Obrigado. Suas informações foram recebidas.</h2>
              <p>
                O questionário e o aceite foram registrados. O profissional
                responsável poderá consultar a confirmação no sistema.
              </p>
              <dl className="completion-details">
                <div>
                  <dt>Status</dt>
                  <dd>{termoAceito.statusFicha}</dd>
                </div>
                <div>
                  <dt>Concluída em</dt>
                  <dd>{formatarDataHora(termoAceito.aceitoEmUtc)}</dd>
                </div>
              </dl>
              <p className="completion-guidance">
                Você já pode fechar esta página. Não é necessário enviar uma
                captura de tela pelo aplicativo de mensagens.
              </p>
            </div>
          )}
        </div>
      </section>
    </main>
  )
}
