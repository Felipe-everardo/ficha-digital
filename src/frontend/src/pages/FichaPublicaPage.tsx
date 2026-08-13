import { type FormEvent, useEffect, useState } from 'react'
import {
  ApiRequestError,
  ApiValidationError,
  abrirConviteFicha,
  responderQuestionarioSaude,
  type ConviteFichaAberto,
  type QuestionarioSaudeRespondido,
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

const tokenDoConvite = obterTokenDoConvite()

export function FichaPublicaPage() {
  const [tentativa, setTentativa] = useState(0)
  const [estado, setEstado] = useState<EstadoAbertura>(
    tokenDoConvite ? { tipo: 'carregando' } : { tipo: 'sem-token' },
  )
  const [respostas, setRespostas] =
    useState<RespostasQuestionario>(respostasIniciais)
  const [questionarioRespondido, setQuestionarioRespondido] =
    useState<QuestionarioSaudeRespondido | null>(null)
  const [enviandoQuestionario, setEnviandoQuestionario] = useState(false)
  const [erroQuestionario, setErroQuestionario] = useState<string | null>(null)

  useEffect(() => {
    if (!tokenDoConvite) return

    const abortController = new AbortController()
    setEstado({ tipo: 'carregando' })

    abrirConviteFicha(tokenDoConvite, abortController.signal)
      .then((convite) => setEstado({ tipo: 'aberto', convite }))
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
  }, [tentativa])

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
      const questionario = await responderQuestionarioSaude(tokenDoConvite, {
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

      setQuestionarioRespondido(questionario)
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

  const conviteAberto = estado.tipo === 'aberto'
  const questionarioConcluido = questionarioRespondido !== null

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
              questionarioConcluido ? 'flow-progress__item--active' : ''
            }`}
          >
            <span>3</span>
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

          {estado.tipo === 'aberto' && !questionarioRespondido && (
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

          {estado.tipo === 'aberto' && questionarioRespondido && (
            <div className="questionnaire-complete">
              <div className="opening-state opening-state--success" role="status">
                <span className="state-symbol" aria-hidden="true">
                  ✓
                </span>
                <div>
                  <p className="eyebrow">Questionário salvo</p>
                  <h2>Respostas recebidas com sucesso</h2>
                  <p>
                    Agora falta revisar e aceitar o termo de consentimento para
                    concluir a ficha.
                  </p>
                </div>
              </div>

              <details className="term-preview">
                <summary>Visualizar o termo da etapa final</summary>
                <div className="term-preview__content">
                  <p className="term-version">
                    Versão {estado.convite.termoConsentimento.versao}
                  </p>
                  <p>{estado.convite.termoConsentimento.conteudo}</p>
                </div>
              </details>

              <p className="development-note">
                O aceite do termo será conectado ao backend na próxima entrega.
              </p>
            </div>
          )}
        </div>
      </section>
    </main>
  )
}
