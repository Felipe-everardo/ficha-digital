import { type FormEvent, useEffect, useState } from 'react'
import { CalendarInput } from '../components/CalendarInput'
import { MonthInput } from '../components/MonthInput'
import {
  ApiRequestError,
  listarFichas,
  type FichasPaginadas,
  type FiltrosFichas,
  type TipoProcedimento,
} from '../services/api'
import { obterMesAno } from '../utils/periodo'
import './FichasPage.css'

const TAMANHO_PAGINA = 10

type TipoPeriodo = 'todos' | 'dia' | 'mes' | 'ano'

function formatarDataHora(data: string | null) {
  if (!data) return 'Ainda não concluída'

  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(data))
}

function formatarProcedimento(tipo: TipoProcedimento) {
  if (tipo === 'Tatuagem') return 'Tatuagem'
  if (tipo === 'Piercing') return 'Piercing'
  return 'Não informado'
}

function classeDoStatus(status: string) {
  if (status === 'Rascunho') return 'draft'
  if (status === 'ConviteEnviado') return 'sent'
  if (status === 'EmPreenchimento') return 'progress'
  if (status === 'Concluida') return 'completed'
  return 'cancelled'
}

function rotuloDoStatus(status: string) {
  if (status === 'Rascunho') return 'Rascunho'
  if (status === 'ConviteEnviado') return 'Convite enviado'
  if (status === 'EmPreenchimento') return 'Em preenchimento'
  if (status === 'Concluida') return 'Concluída'
  return status
}

function obterLimitesDoPeriodo(tipo: TipoPeriodo, valor: string) {
  if (tipo === 'dia' && valor) {
    return { concluidaDe: valor, concluidaAte: valor }
  }

  if (tipo === 'mes') {
    const mesAno = obterMesAno(valor, new Date().getFullYear())
    if (!mesAno) return {}

    const ano = Number(mesAno.ano)
    const mes = Number(mesAno.mes)
    const ultimoDia = new Date(ano, mes, 0).getDate()
    const valorIso = `${mesAno.ano}-${mesAno.mes}`
    return {
      concluidaDe: `${valorIso}-01`,
      concluidaAte: `${valorIso}-${String(ultimoDia).padStart(2, '0')}`,
    }
  }

  if (tipo === 'ano' && /^\d{4}$/.test(valor)) {
    return {
      concluidaDe: `${valor}-01-01`,
      concluidaAte: `${valor}-12-31`,
    }
  }

  return {}
}

function obterRotuloDoValor(tipo: TipoPeriodo) {
  if (tipo === 'dia') return 'Dia da conclusão'
  if (tipo === 'mes') return 'Mês da conclusão'
  if (tipo === 'ano') return 'Ano da conclusão'
  return ''
}

export function FichasPage() {
  const [pagina, setPagina] = useState(1)
  const [resultado, setResultado] = useState<FichasPaginadas | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [tipoPeriodo, setTipoPeriodo] = useState<TipoPeriodo>('todos')
  const [valorPeriodo, setValorPeriodo] = useState('')
  const [periodoAtivo, setPeriodoAtivo] = useState(false)
  const [filtros, setFiltros] = useState<FiltrosFichas>({})

  useEffect(() => {
    const abortController = new AbortController()
    setCarregando(true)
    setErro(null)

    listarFichas(
      pagina,
      TAMANHO_PAGINA,
      filtros,
      abortController.signal,
    )
      .then(setResultado)
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return

        if (error instanceof ApiRequestError && error.status === 401) {
          window.location.replace('/profissional/entrar')
          return
        }

        setErro(
          error instanceof ApiRequestError
            ? error.message
            : 'Não foi possível carregar as fichas.',
        )
      })
      .finally(() => {
        if (!abortController.signal.aborted) setCarregando(false)
      })

    return () => abortController.abort()
  }, [filtros, pagina])

  function aplicarPeriodo(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const novosFiltros = obterLimitesDoPeriodo(tipoPeriodo, valorPeriodo)
    setFiltros(novosFiltros)
    setPeriodoAtivo(Object.keys(novosFiltros).length > 0)
    setPagina(1)
  }

  function limparPeriodo() {
    setTipoPeriodo('todos')
    setValorPeriodo('')
    setFiltros({})
    setPeriodoAtivo(false)
    setPagina(1)
  }

  return (
    <main className="records-shell">
      <header className="records-header">
        <div className="records-period-filter__intro">
          <p className="eyebrow">Arquivo do estúdio</p>
          <h1>Histórico de fichas</h1>
          <p>
            Consulte as fichas enviadas e concluídas, preservando o contexto
            declarado em cada procedimento.
          </p>
        </div>
        <a className="records-primary-link" href="/profissional/clientes">
          Localizar cliente
        </a>
      </header>

      <form className="records-period-filter" onSubmit={aplicarPeriodo}>
        <div>
          <p className="eyebrow">Período</p>
          <h2>Filtrar pela conclusão</h2>
        </div>
        <div className="records-period-filter__fields">
          <label>
            <span>Tipo</span>
            <select
              value={tipoPeriodo}
              onChange={(event) => {
                const tipo = event.target.value as TipoPeriodo
                setTipoPeriodo(tipo)
                setValorPeriodo('')
              }}
            >
              <option value="todos">Todo o histórico</option>
              <option value="dia">Dia</option>
              <option value="mes">Mês</option>
              <option value="ano">Ano</option>
            </select>
          </label>

          {tipoPeriodo !== 'todos' && (
            <label>
              <span>{obterRotuloDoValor(tipoPeriodo)}</span>
              {tipoPeriodo === 'dia' ? (
                <CalendarInput
                  type="date"
                  required
                  value={valorPeriodo}
                  onChange={(event) => setValorPeriodo(event.target.value)}
                />
              ) : tipoPeriodo === 'mes' ? (
                <MonthInput
                  required
                  value={valorPeriodo}
                  onValueChange={setValorPeriodo}
                />
              ) : (
                <input
                  type="number"
                  min="2000"
                  max={String(new Date().getFullYear())}
                  required
                  value={valorPeriodo}
                  onChange={(event) => setValorPeriodo(event.target.value)}
                />
              )}
            </label>
          )}

          <div className="records-period-filter__actions">
            <button type="submit" disabled={carregando}>
              Aplicar
            </button>
            {periodoAtivo && (
              <button type="button" className="secondary-button" onClick={limparPeriodo}>
                Limpar
              </button>
            )}
          </div>
        </div>
      </form>

      <section className="records-content" aria-live="polite">
        {carregando && !resultado ? (
          <div className="records-state records-state--empty">
            <span className="records-loading-indicator" aria-hidden="true" />
            Carregando fichas...
          </div>
        ) : erro ? (
          <div className="records-state records-state--error" role="alert">
            <p>{erro}</p>
            <button type="button" onClick={() => window.location.reload()}>
              Tentar novamente
            </button>
          </div>
        ) : resultado && resultado.itens.length === 0 ? (
          <div className="records-state">
            <h2>Nenhuma ficha encontrada</h2>
            <p>
              {periodoAtivo
                ? 'Nenhuma ficha foi concluída nesse período.'
                : 'As fichas aparecerão aqui quando forem enviadas aos clientes.'}
            </p>
          </div>
        ) : resultado ? (
          <>
            <p className="records-summary">
              <strong>Fichas registradas:</strong>
              <span>
                {resultado.totalItens}{' '}
                {resultado.totalItens === 1 ? 'ficha' : 'fichas'}
              </span>
            </p>

            <div className="records-table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Cliente</th>
                    <th>Procedimento</th>
                    <th>Profissional</th>
                    <th>Conclusão</th>
                    <th>Status</th>
                    <th>Ação</th>
                  </tr>
                </thead>
                <tbody>
                  {resultado.itens.map((ficha) => (
                    <tr key={ficha.id}>
                      <td data-label="Cliente">
                        <strong>{ficha.clienteNome}</strong>
                      </td>
                      <td data-label="Procedimento">
                        {formatarProcedimento(ficha.tipoProcedimento)}
                      </td>
                      <td data-label="Profissional">
                        {ficha.profissionalResponsavelNome}
                      </td>
                      <td data-label="Conclusão">
                        {formatarDataHora(ficha.concluidaEmUtc)}
                      </td>
                      <td data-label="Status">
                        <span className={`record-status record-status--${classeDoStatus(ficha.status)}`}>
                          {rotuloDoStatus(ficha.status)}
                        </span>
                      </td>
                      <td data-label="Ação">
                        <a
                          className="record-detail-link"
                          href={`/profissional/fichas/${ficha.id}`}
                        >
                          Ver ficha
                        </a>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <nav className="records-pagination" aria-label="Paginação">
              <button
                className="secondary-button"
                type="button"
                disabled={pagina <= 1 || carregando}
                onClick={() => setPagina((atual) => atual - 1)}
              >
                Anterior
              </button>
              <span>
                Página {resultado.pagina} de {Math.max(resultado.totalPaginas, 1)}
              </span>
              <button
                className="secondary-button"
                type="button"
                disabled={pagina >= resultado.totalPaginas || carregando}
                onClick={() => setPagina((atual) => atual + 1)}
              >
                Próxima
              </button>
            </nav>
          </>
        ) : null}
      </section>
    </main>
  )
}
