import { useEffect, useState } from 'react'
import {
  ApiRequestError,
  obterDetalheFicha,
  type FichaDetalhe,
} from '../services/api'
import { OperacoesProfissionaisFicha } from '../components/ficha-profissional/OperacoesProfissionaisFicha'
import './FichaDetalhePage.css'

type FichaDetalhePageProps = {
  fichaId: string
}

function formatarDataHora(dataUtc: string) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(dataUtc))
}

function formatarDataNascimento(data: string | null) {
  if (!data) return 'Aguardando preenchimento'

  const [ano, mes, dia] = data.split('-').map(Number)

  return new Intl.DateTimeFormat('pt-BR').format(
    new Date(ano, mes - 1, dia),
  )
}

function formatarSimNao(valor: boolean) {
  return valor ? 'Sim' : 'Não'
}

function formatarMoeda(valor: number) {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(valor)
}

function formatarProcedimento(tipo: FichaDetalhe['tipoProcedimento']) {
  if (tipo === 'Tatuagem') return 'Tatuagem'
  if (tipo === 'Piercing') return 'Piercing'
  return 'Não informado'
}

function obterStatus(ficha: FichaDetalhe) {
  if (ficha.status === 'ConviteEnviado' && ficha.conviteExpirado) {
    return { texto: 'Convite expirado', classe: 'expired' }
  }

  const statusPorCodigo: Record<string, { texto: string; classe: string }> = {
    Rascunho: { texto: 'Rascunho', classe: 'draft' },
    ConviteEnviado: { texto: 'Convite enviado', classe: 'sent' },
    EmPreenchimento: { texto: 'Em preenchimento', classe: 'progress' },
    AnamnesePreenchida: {
      texto: 'Aguardando assinatura do cliente',
      classe: 'progress',
    },
    AguardandoConsentimento: {
      texto: 'Aguardando consentimento',
      classe: 'progress',
    },
    AutorizadaParaProcedimento: {
      texto: 'Autorizada para o procedimento',
      classe: 'sent',
    },
    RevisadaPeloProfissional: {
      texto: 'Revisada · registro pendente',
      classe: 'progress',
    },
    Concluida: { texto: 'Concluída', classe: 'completed' },
    Expirada: { texto: 'Expirada', classe: 'expired' },
    Cancelada: { texto: 'Cancelada', classe: 'cancelled' },
  }

  return statusPorCodigo[ficha.status] ?? {
    texto: ficha.status,
    classe: 'draft',
  }
}

export function FichaDetalhePage({ fichaId }: FichaDetalhePageProps) {
  const [ficha, setFicha] = useState<FichaDetalhe | null>(null)
  const [erro, setErro] = useState<string | null>(null)

  useEffect(() => {
    const abortController = new AbortController()

    obterDetalheFicha(fichaId, abortController.signal)
      .then(setFicha)
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return
        }

        if (error instanceof ApiRequestError && error.status === 401) {
          window.location.replace('/profissional/entrar')
          return
        }

        setErro(
          error instanceof ApiRequestError
            ? error.message
            : 'Não foi possível carregar a ficha.',
        )
      })

    return () => abortController.abort()
  }, [fichaId])

  function atualizarDepoisDaOperacao(novoStatus: string) {
    setFicha((atual) =>
      atual ? { ...atual, status: novoStatus } : atual,
    )

    obterDetalheFicha(fichaId)
      .then(setFicha)
      .catch(() => {
        // O status confirmado pela operação continua visível se a recarga falhar.
      })
  }

  if (erro) {
    return (
      <main className="record-detail-shell">
        <section className="record-detail-state record-detail-state--error">
          <p>{erro}</p>
          <a href="/profissional/fichas">Voltar para as fichas</a>
        </section>
      </main>
    )
  }

  if (!ficha) {
    return (
      <main className="record-detail-shell">
        <div className="record-detail-state" aria-live="polite">
          <span className="record-detail-loading" aria-hidden="true" />
          Carregando detalhes da ficha...
        </div>
      </main>
    )
  }

  const status = obterStatus(ficha)
  const questionario = ficha.questionarioSaude

  return (
    <main className="record-detail-shell">
      <header className="record-detail-header">
        <p className="eyebrow">Área profissional</p>
        <div className="record-detail-title-row">
          <div>
            <h1>{ficha.cliente.nomeParaExibicao}</h1>
            <p>Detalhes da ficha digital e do preenchimento.</p>
          </div>
          <span className={`record-status record-status--${status.classe}`}>
            {status.texto}
          </span>
        </div>
      </header>

      {([
        'AnamnesePreenchida',
        'AguardandoConsentimento',
        'AutorizadaParaProcedimento',
        'RevisadaPeloProfissional',
      ].includes(ficha.status) ||
        (ficha.status === 'Concluida' && ficha.registroProcedimento)) && (
        <OperacoesProfissionaisFicha
          ficha={ficha}
          aoAtualizarStatus={atualizarDepoisDaOperacao}
        />
      )}

      <details className="record-detail-section record-detail-collapsible">
        <summary className="record-detail-section-toggle">
          <div className="record-detail-section-heading">
            <p className="eyebrow">Atendimento</p>
            <h2>Responsável e procedimento</h2>
          </div>
          <span className="record-detail-toggle-action">
            <span className="record-detail-toggle-closed">Ver</span>
            <span className="record-detail-toggle-open">Recolher</span>
            <span aria-hidden="true">⌄</span>
          </span>
        </summary>
        <dl className="record-detail-grid">
          <div>
            <dt>Procedimento</dt>
            <dd>{formatarProcedimento(ficha.tipoProcedimento)}</dd>
          </div>
          <div>
            <dt>Profissional responsável</dt>
            <dd>{ficha.profissionalResponsavelNome}</dd>
          </div>
          <div>
            <dt>Ficha criada em</dt>
            <dd>{formatarDataHora(ficha.criadaEmUtc)}</dd>
          </div>
          <div>
            <dt>Versões preservadas</dt>
            <dd>
              {ficha.versaoModelo
                ? `Modelo ${ficha.versaoModelo} · questionário ${ficha.versaoQuestionario} · termo ${ficha.versaoTermo}`
                : 'Ficha anterior ao versionamento por procedimento'}
            </dd>
          </div>
          <div>
            <dt>CNPJ apresentado</dt>
            <dd>{ficha.cnpjApresentado ?? 'Não registrado nesta versão'}</dd>
          </div>
        </dl>
      </details>

      <details
        className="record-detail-section record-detail-collapsible"
        open={ficha.status === 'RevisadaPeloProfissional'}
      >
        <summary className="record-detail-section-toggle">
          <div className="record-detail-section-heading">
            <p className="eyebrow">Acompanhamento profissional</p>
            <h2>Revisão e registro pós-procedimento</h2>
          </div>
          <span className="record-detail-toggle-action">
            <span className="record-detail-toggle-closed">Ver</span>
            <span className="record-detail-toggle-open">Recolher</span>
            <span aria-hidden="true">⌄</span>
          </span>
        </summary>

        {ficha.revisaoProfissional ? (
          <dl className="record-detail-grid">
            <div>
              <dt>Revisada em</dt>
              <dd>{formatarDataHora(ficha.revisaoProfissional.revisadaEmUtc)}</dd>
            </div>
            <div>
              <dt>Profissional</dt>
              <dd>{ficha.revisaoProfissional.profissionalNome}</dd>
            </div>
            <div>
              <dt>Ficha e identidade conferidas</dt>
              <dd>{formatarSimNao(ficha.revisaoProfissional.dadosDaFichaConferidos)}</dd>
            </div>
          </dl>
        ) : (
          <p className="record-detail-pending">
            A ficha ainda aguarda a revisão do profissional responsável.
          </p>
        )}

        {ficha.registroProcedimento && (
          <div className="record-technical-result">
            <dl className="record-detail-grid">
              {ficha.registroProcedimento.tipoProcedimento === 'Tatuagem' ? (
                <>
                  <div><dt>Arte realizada</dt><dd>{ficha.registroProcedimento.arteEfetivamenteTatuada}</dd></div>
                  <div><dt>Material</dt><dd>{ficha.registroProcedimento.materialUtilizado}</dd></div>
                  <div><dt>Local</dt><dd>{ficha.registroProcedimento.localTatuagem}</dd></div>
                </>
              ) : (
                <>
                  <div><dt>Joia utilizada</dt><dd>{ficha.registroProcedimento.joiaUtilizada}</dd></div>
                  <div><dt>Agulha utilizada</dt><dd>{ficha.registroProcedimento.agulhaUtilizada}</dd></div>
                  <div><dt>Local</dt><dd>{ficha.registroProcedimento.localPerfuracao}</dd></div>
                </>
              )}
              <div><dt>Valor total</dt><dd>{formatarMoeda(ficha.registroProcedimento.valorTotal)}</dd></div>
              <div><dt>Sinal</dt><dd>{formatarMoeda(ficha.registroProcedimento.valorSinal)}</dd></div>
              <div><dt>Restante</dt><dd>{formatarMoeda(ficha.registroProcedimento.valorRestante)}</dd></div>
              <div><dt>Pagamento</dt><dd>{ficha.registroProcedimento.formaPagamento}</dd></div>
              <div><dt>Registrado em</dt><dd>{formatarDataHora(ficha.registroProcedimento.registradoEmUtc)}</dd></div>
            </dl>
            <figure className="record-signature">
              <figcaption>Assinatura do profissional</figcaption>
              <img src={ficha.registroProcedimento.assinaturaDesenhada} alt="Assinatura desenhada pelo profissional" />
            </figure>
          </div>
        )}
      </details>

      <details className="record-detail-section record-detail-collapsible">
        <summary className="record-detail-section-toggle">
          <div className="record-detail-section-heading">
            <p className="eyebrow">Identificação</p>
            <h2>Dados do cliente</h2>
          </div>
          <span className="record-detail-toggle-action">
            <span className="record-detail-toggle-closed">Ver</span>
            <span className="record-detail-toggle-open">Recolher</span>
            <span aria-hidden="true">⌄</span>
          </span>
        </summary>
        <dl className="record-detail-grid">
          <div>
            <dt>Nome de referência</dt>
            <dd>{ficha.cliente.nomeReferencia}</dd>
          </div>
          <div>
            <dt>Nome completo</dt>
            <dd>{ficha.cliente.nomeCompleto ?? 'Aguardando preenchimento'}</dd>
          </div>
          <div>
            <dt>Nome social</dt>
            <dd>{ficha.cliente.nomeSocial ?? 'Não informado'}</dd>
          </div>
          <div>
            <dt>Pronomes</dt>
            <dd>{ficha.cliente.pronomes ?? 'Não informado'}</dd>
          </div>
          <div>
            <dt>Estado civil</dt>
            <dd>{ficha.cliente.estadoCivil ?? 'Não informado'}</dd>
          </div>
          <div>
            <dt>Data de nascimento</dt>
            <dd>{formatarDataNascimento(ficha.cliente.dataNascimento)}</dd>
          </div>
          <div>
            <dt>Celular</dt>
            <dd>{ficha.cliente.celular ?? 'Aguardando preenchimento'}</dd>
          </div>
          <div>
            <dt>CPF</dt>
            <dd>{ficha.cliente.cpf ?? 'Não informado'}</dd>
          </div>
          <div>
            <dt>Telefone adicional</dt>
            <dd>{ficha.cliente.telefoneAdicional ?? 'Não informado'}</dd>
          </div>
          <div>
            <dt>E-mail</dt>
            <dd>{ficha.cliente.email ?? 'Não informado'}</dd>
          </div>
          <div>
            <dt>Instagram</dt>
            <dd>{ficha.cliente.instagram ?? 'Não informado'}</dd>
          </div>
          <div>
            <dt>Contato de emergência</dt>
            <dd>
              {ficha.cliente.contatoEmergenciaNome &&
              ficha.cliente.contatoEmergenciaCelular
                ? `${ficha.cliente.contatoEmergenciaNome} — ${ficha.cliente.contatoEmergenciaCelular}`
                : 'Não informado'}
            </dd>
          </div>
          <div>
            <dt>Endereço confirmado</dt>
            <dd>
              {ficha.cliente.logradouro && ficha.cliente.numero
                ? `${ficha.cliente.logradouro}, ${ficha.cliente.numero}${ficha.cliente.complemento ? ` — ${ficha.cliente.complemento}` : ''} — ${ficha.cliente.bairro}, ${ficha.cliente.cidade}/${ficha.cliente.estado} — CEP ${ficha.cliente.cep}`
                : 'Não informado nesta versão da ficha'}
            </dd>
          </div>
          <div>
            <dt>Dados pessoais</dt>
            <dd>
              {ficha.cliente.dadosPessoaisPreenchidosEmUtc
                ? `Preenchidos em ${formatarDataHora(
                    ficha.cliente.dadosPessoaisPreenchidosEmUtc,
                  )}`
                : 'Aguardando preenchimento pelo cliente'}
            </dd>
          </div>
          <div>
            <dt>Ficha criada em</dt>
            <dd>{formatarDataHora(ficha.criadaEmUtc)}</dd>
          </div>
          <div>
            <dt>Validade do convite</dt>
            <dd>
              {ficha.conviteExpiraEmUtc
                ? formatarDataHora(ficha.conviteExpiraEmUtc)
                : 'Convite não emitido'}
            </dd>
          </div>
        </dl>
      </details>

      <details className="record-detail-section record-detail-collapsible">
        <summary className="record-detail-section-toggle">
          <div className="record-detail-section-heading">
            <p className="eyebrow">Anamnese</p>
            <h2>Questionário de saúde</h2>
            {questionario && (
              <p>
                Versão {questionario.versao} · respondido em{' '}
                {formatarDataHora(questionario.respondidoEmUtc)}
              </p>
            )}
          </div>
          <span className="record-detail-toggle-action">
            <span className="record-detail-toggle-closed">Ver</span>
            <span className="record-detail-toggle-open">Recolher</span>
            <span aria-hidden="true">⌄</span>
          </span>
        </summary>

        {questionario ? (
          <dl className="health-answer-list">
            <div>
              <dt>Diabetes</dt>
              <dd>
                {formatarSimNao(questionario.temDiabetes)}
                {questionario.tipoDiabetes
                  ? ` — ${questionario.tipoDiabetes}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Pressão alta</dt>
              <dd>{formatarSimNao(questionario.possuiPressaoAlta)}</dd>
            </div>
            <div>
              <dt>Anemia</dt>
              <dd>
                {formatarSimNao(questionario.teveAnemia)}
                {questionario.descricaoAnemia
                  ? ` — ${questionario.descricaoAnemia}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Hepatite</dt>
              <dd>
                {formatarSimNao(questionario.teveHepatite)}
                {questionario.tipoHepatite
                  ? ` — ${questionario.tipoHepatite}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Alergia</dt>
              <dd>
                {formatarSimNao(questionario.temAlergia)}
                {questionario.descricaoAlergia
                  ? ` — ${questionario.descricaoAlergia}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Condição cardíaca</dt>
              <dd>{formatarSimNao(questionario.possuiCondicaoCardiaca)}</dd>
            </div>
            <div>
              <dt>Epilepsia</dt>
              <dd>{formatarSimNao(questionario.temEpilepsia)}</dd>
            </div>
            <div>
              <dt>Hemofilia</dt>
              <dd>{formatarSimNao(questionario.temHemofilia)}</dd>
            </div>
            <div>
              <dt>Doença transmissível</dt>
              <dd>
                {formatarSimNao(questionario.possuiDoencaTransmissivel)}
                {questionario.descricaoDoencaTransmissivel
                  ? ` — ${questionario.descricaoDoencaTransmissivel}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Uso de marca-passo</dt>
              <dd>{formatarSimNao(questionario.usaMarcaPasso)}</dd>
            </div>
            <div>
              <dt>Fuma</dt>
              <dd>{formatarSimNao(questionario.fuma)}</dd>
            </div>
            <div>
              <dt>Álcool nas últimas 24 horas</dt>
              <dd>
                {formatarSimNao(
                  questionario.consumiuBebidaAlcoolicaUltimas24Horas,
                )}
              </dd>
            </div>
            <div>
              <dt>Uso de medicação</dt>
              <dd>
                {formatarSimNao(questionario.usaMedicacao)}
                {questionario.descricaoMedicacao
                  ? ` — ${questionario.descricaoMedicacao}`
                  : ''}
              </dd>
            </div>
            <div>
              <dt>Grávida ou amamentando</dt>
              <dd>
                {formatarSimNao(questionario.estaGravidaOuAmamentando)}
              </dd>
            </div>
          </dl>
        ) : (
          <p className="record-detail-pending">
            O questionário de saúde ainda não foi respondido.
          </p>
        )}
      </details>

      <details className="record-detail-section record-detail-collapsible">
        <summary className="record-detail-section-toggle">
          <div className="record-detail-section-heading">
            <p className="eyebrow">Consentimento</p>
            <h2>Resumo do aceite</h2>
          </div>
          <span className="record-detail-toggle-action">
            <span className="record-detail-toggle-closed">Ver</span>
            <span className="record-detail-toggle-open">Recolher</span>
            <span aria-hidden="true">⌄</span>
          </span>
        </summary>

        {ficha.aceiteTermo ? (
          <>
          <dl className="record-detail-grid">
            <div>
              <dt>Nome declarado</dt>
              <dd>{ficha.aceiteTermo.nomeAssinante}</dd>
            </div>
            <div>
              <dt>Versão do termo</dt>
              <dd>{ficha.aceiteTermo.versaoTermo}</dd>
            </div>
            <div>
              <dt>Aceito em</dt>
              <dd>{formatarDataHora(ficha.aceiteTermo.aceitoEmUtc)}</dd>
            </div>
            <div>
              <dt>Leitura e autorização</dt>
              <dd>
                {ficha.aceiteTermo.confirmouLeituraEAutorizacao
                  ? 'Confirmadas pelo cliente'
                  : 'Não confirmadas'}
              </dd>
            </div>
          </dl>
          {ficha.aceiteTermo.assinaturaDesenhada && (
            <figure className="record-signature">
              <figcaption>Assinatura desenhada do cliente</figcaption>
              <img
                src={ficha.aceiteTermo.assinaturaDesenhada}
                alt="Assinatura desenhada pelo cliente"
              />
            </figure>
          )}
          <details className="record-evidence-details">
            <summary>Detalhes técnicos de segurança</summary>
            <dl className="record-detail-grid">
              <div>
                <dt>Integridade da evidência</dt>
                <dd>
                  {ficha.aceiteTermo.evidenciaIntegra
                    ? 'Verificada'
                    : 'Não foi possível verificar'}
                </dd>
              </div>
              <div>
                <dt>Código da evidência</dt>
                <dd className="record-detail-evidence-code">
                  {ficha.aceiteTermo.evidenciaHash}
                </dd>
              </div>
            </dl>
          </details>
          </>
        ) : (
          <p className="record-detail-pending">
            O termo de consentimento ainda não foi aceito.
          </p>
        )}
      </details>
    </main>
  )
}
