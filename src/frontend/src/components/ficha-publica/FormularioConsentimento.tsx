import type { FormEventHandler } from 'react'
import type { ConviteFichaAberto } from '../../services/api'
import type {
  DadosPessoaisFormulario,
  RespostasQuestionario,
} from './model'
import { CampoAssinatura } from './CampoAssinatura'

type FormularioConsentimentoProps = {
  convite: ConviteFichaAberto
  dados: DadosPessoaisFormulario
  respostas: RespostasQuestionario
  confirmouLeituraEAutorizacao: boolean
  nomeAssinante: string
  assinaturaDesenhada: string | null
  erro: string | null
  enviando: boolean
  aoEnviar: FormEventHandler<HTMLFormElement>
  aoAlterarConfirmacao: (valor: boolean) => void
  aoAlterarNomeAssinante: (valor: string) => void
  aoAlterarAssinatura: (valor: string | null) => void
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
  confirmouLeituraEAutorizacao,
  nomeAssinante,
  assinaturaDesenhada,
  erro,
  enviando,
  aoEnviar,
  aoAlterarConfirmacao,
  aoAlterarNomeAssinante,
  aoAlterarAssinatura,
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
          <summary>Atendimento</summary>
          <dl className="consent-review__grid">
            <div>
              <dt>Profissional responsável</dt>
              <dd>{convite.profissionalResponsavelNome}</dd>
            </div>
            <div>
              <dt>Procedimento</dt>
              <dd>{convite.tipoProcedimento}</dd>
            </div>
          </dl>
        </details>

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
              <dt>Estado civil</dt>
              <dd>{dados.estadoCivil}</dd>
            </div>
            <div>
              <dt>CPF</dt>
              <dd>{dados.cpf}</dd>
            </div>
            <div>
              <dt>Celular</dt>
              <dd>{dados.celular}</dd>
            </div>
            <div>
              <dt>Telefone adicional</dt>
              <dd>{dados.telefoneAdicional || 'Não informado'}</dd>
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
            <div>
              <dt>Endereço</dt>
              <dd>
                {dados.logradouro}, {dados.numero}
                {dados.complemento ? ` — ${dados.complemento}` : ''}
                {` — ${dados.bairro}, ${dados.cidade}/${dados.estado} — CEP ${dados.cep}`}
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
              <dt>Anemia</dt>
              <dd>
                {formatarResposta(respostas.teveAnemia)}
                {respostas.teveAnemia && respostas.descricaoAnemia
                  ? ` — ${respostas.descricaoAnemia}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Hepatite</dt>
              <dd>
                {formatarResposta(respostas.teveHepatite)}
                {respostas.teveHepatite && respostas.tipoHepatite
                  ? ` — ${respostas.tipoHepatite}`
                  : ''}
              </dd>
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
              <dt>Doença transmissível</dt>
              <dd>
                {formatarResposta(respostas.possuiDoencaTransmissivel)}
                {respostas.possuiDoencaTransmissivel &&
                respostas.descricaoDoencaTransmissivel
                  ? ` — ${respostas.descricaoDoencaTransmissivel}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Uso de marca-passo</dt>
              <dd>{formatarResposta(respostas.usaMarcaPasso)}</dd>
            </div>
            <div>
              <dt>Fuma</dt>
              <dd>{formatarResposta(respostas.fuma)}</dd>
            </div>
            <div>
              <dt>Álcool nas últimas 24 horas</dt>
              <dd>
                {formatarResposta(
                  respostas.consumiuBebidaAlcoolicaUltimas24Horas,
                )}
              </dd>
            </div>
            <div>
              <dt>Uso de medicação</dt>
              <dd>
                {formatarResposta(respostas.usaMedicacao)}
                {respostas.usaMedicacao && respostas.descricaoMedicacao
                  ? ` — ${respostas.descricaoMedicacao}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Grávida ou amamentando</dt>
              <dd>{formatarResposta(respostas.estaGravidaOuAmamentando)}</dd>
            </div>
          </dl>
        </details>
      </section>

      <div className="health-notice">
        Este conteúdo corresponde ao modelo selecionado para o procedimento e
        ficará preservado junto ao aceite.
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

      <CampoAssinatura
        valor={assinaturaDesenhada}
        aoAlterar={aoAlterarAssinatura}
      />

      <label className="consent-checkbox">
          <input
            type="checkbox"
            checked={confirmouLeituraEAutorizacao}
            required
            onChange={(event) => aoAlterarConfirmacao(event.target.checked)}
          />
          <span>
            Confirmo que li e entendi os dados e o termo apresentados e autorizo
            a realização do procedimento.
        </span>
      </label>

      {erro && (
        <p className="form-error" role="alert">
          {erro}
        </p>
      )}

      <div className="questionnaire-actions">
        <p>
          Ao autorizar, seus dados e sua assinatura ficarão bloqueados. O
          registro técnico será preenchido pelo profissional após o atendimento.
        </p>
        <button type="submit" disabled={enviando}>
          {enviando ? 'Registrando autorização...' : 'Assinar e autorizar procedimento'}
        </button>
      </div>
    </form>
  )
}
