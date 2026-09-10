import type { FormEventHandler } from 'react'
import type { ConviteFichaAberto } from '../../services/api'
import type {
  CampoConfirmacao,
  DadosPessoaisFormulario,
  RespostasQuestionario,
} from './model'

type Confirmacoes = Record<CampoConfirmacao, boolean>

type FormularioConsentimentoProps = {
  convite: ConviteFichaAberto
  dados: DadosPessoaisFormulario
  respostas: RespostasQuestionario
  confirmacoes: Confirmacoes
  nomeAssinante: string
  erro: string | null
  enviando: boolean
  aoEnviar: FormEventHandler<HTMLFormElement>
  aoAlterarConfirmacao: (campo: CampoConfirmacao, valor: boolean) => void
  aoAlterarNomeAssinante: (valor: string) => void
}

function formatarDataBrasileira(dataIso: string) {
  if (!dataIso) return 'Não informada'
  const [ano, mes, dia] = dataIso.split('-')
  return `${dia}/${mes}/${ano}`
}

function formatarResposta(resposta: boolean | null) {
  if (resposta === null) return 'Não informado'
  return resposta ? 'Sim' : 'Não'
}

export function FormularioConsentimento({
  convite,
  dados,
  respostas,
  confirmacoes,
  nomeAssinante,
  erro,
  enviando,
  aoEnviar,
  aoAlterarConfirmacao,
  aoAlterarNomeAssinante,
}: FormularioConsentimentoProps) {
  return (
    <form className="consent-form" onSubmit={aoEnviar}>
      <div className="section-heading">
        <p className="eyebrow">Etapa 4 de 4</p>
        <h2>Termo de consentimento</h2>
        <p>
          Leia o conteúdo completo antes de confirmar. O sistema registrará
          esta versão exata do termo junto ao seu aceite.
        </p>
      </div>

      <section className="consent-review" aria-labelledby="review-title">
        <div className="consent-review__heading">
          <p className="eyebrow">Revisão final</p>
          <h3 id="review-title">Dados que ficarão registrados</h3>
          <p>
            Confira novamente. Uma cópia exata destas informações será
            vinculada a este atendimento.
          </p>
        </div>

        <details open>
          <summary>Dados pessoais</summary>
          <dl className="consent-review__grid">
            <div>
              <dt>Nome completo</dt>
              <dd>{dados.nomeCompleto}</dd>
            </div>
            <div>
              <dt>Data de nascimento</dt>
              <dd>{formatarDataBrasileira(dados.dataNascimento)}</dd>
            </div>
            <div>
              <dt>Celular</dt>
              <dd>{dados.celular}</dd>
            </div>
            <div>
              <dt>E-mail</dt>
              <dd>{dados.email || 'Não informado'}</dd>
            </div>
            <div>
              <dt>Nome social</dt>
              <dd>{dados.nomeSocial || 'Não informado'}</dd>
            </div>
            <div>
              <dt>Pronomes</dt>
              <dd>{dados.pronomes || 'Não informado'}</dd>
            </div>
            <div>
              <dt>Instagram</dt>
              <dd>{dados.instagram || 'Não informado'}</dd>
            </div>
            <div>
              <dt>Contato de emergência</dt>
              <dd>
                {dados.contatoEmergenciaNome
                  ? `${dados.contatoEmergenciaNome} — ${dados.contatoEmergenciaCelular}`
                  : 'Não informado'}
              </dd>
            </div>
          </dl>
        </details>

        <details open>
          <summary>Informações de saúde</summary>
          <dl className="consent-review__grid">
            <div>
              <dt>Diabetes</dt>
              <dd>
                {formatarResposta(respostas.temDiabetes)}
                {respostas.temDiabetes && respostas.tipoDiabetes
                  ? ` — ${respostas.tipoDiabetes}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Pressão alta</dt>
              <dd>{formatarResposta(respostas.possuiPressaoAlta)}</dd>
            </div>
            <div>
              <dt>Alergia</dt>
              <dd>
                {formatarResposta(respostas.temAlergia)}
                {respostas.temAlergia && respostas.descricaoAlergia
                  ? ` — ${respostas.descricaoAlergia}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Condição cardíaca</dt>
              <dd>{formatarResposta(respostas.possuiCondicaoCardiaca)}</dd>
            </div>
            <div>
              <dt>Epilepsia</dt>
              <dd>{formatarResposta(respostas.temEpilepsia)}</dd>
            </div>
            <div>
              <dt>Hemofilia</dt>
              <dd>{formatarResposta(respostas.temHemofilia)}</dd>
            </div>
            <div>
              <dt>Uso de marca-passo</dt>
              <dd>{formatarResposta(respostas.usaMarcaPasso)}</dd>
            </div>
            <div>
              <dt>Grávida ou amamentando</dt>
              <dd>{formatarResposta(respostas.estaGravidaOuAmamentando)}</dd>
            </div>
          </dl>
        </details>
      </section>

      <div className="consent-confirmations">
        <label className="consent-checkbox">
          <input
            type="checkbox"
            checked={confirmacoes.maioridade}
            required
            onChange={(event) =>
              aoAlterarConfirmacao('maioridade', event.target.checked)
            }
          />
          <span>Declaro que tenho 18 anos ou mais.</span>
        </label>

        <label className="consent-checkbox">
          <input
            type="checkbox"
            checked={confirmacoes.dadosPessoais}
            required
            onChange={(event) =>
              aoAlterarConfirmacao('dadosPessoais', event.target.checked)
            }
          />
          <span>
            Confirmo que revisei e que meus dados pessoais estão corretos e
            atualizados.
          </span>
        </label>

        <label className="consent-checkbox">
          <input
            type="checkbox"
            checked={confirmacoes.questionarioSaude}
            required
            onChange={(event) =>
              aoAlterarConfirmacao('questionarioSaude', event.target.checked)
            }
          />
          <span>
            Confirmo que as informações de saúde são verdadeiras e completas
            conforme meu conhecimento atual.
          </span>
        </label>
      </div>

      <div className="health-notice">
        Este é um termo provisório de desenvolvimento.
      </div>

      <article
        className="term-document"
        aria-label="Conteúdo do termo de consentimento"
        tabIndex={0}
      >
        <p className="term-version">
          Versão {convite.termoConsentimento.versao}
        </p>
        <p className="term-document__content">
          {convite.termoConsentimento.conteudo}
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
          onChange={(event) => aoAlterarNomeAssinante(event.target.value)}
        />
        <small>Digite o nome da pessoa que está declarando o aceite.</small>
      </label>

      <label className="consent-checkbox">
        <input
          type="checkbox"
          checked={confirmacoes.aceiteTermo}
          required
          onChange={(event) =>
            aoAlterarConfirmacao('aceiteTermo', event.target.checked)
          }
        />
        <span>
          Declaro que li o termo acima e confirmo seu aceite para concluir esta
          ficha.
        </span>
      </label>

      {erro && (
        <p className="form-error" role="alert">
          {erro}
        </p>
      )}

      <div className="questionnaire-actions">
        <p>
          Ao concluir, a ficha ficará fechada para novas respostas por este
          convite.
        </p>
        <button type="submit" disabled={enviando}>
          {enviando ? 'Registrando aceite...' : 'Aceitar e concluir ficha'}
        </button>
      </div>
    </form>
  )
}
