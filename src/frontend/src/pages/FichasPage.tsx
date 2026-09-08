import { type FormEvent, useEffect, useState } from 'react'
import {
  ApiRequestError,
  listarFichas,
  type FormaPagamento,
  type FichasPaginadas,
  type FiltrosFichas,
  type TipoProcedimento,
} from '../services/api'
import { CalendarInput } from '../components/CalendarInput'
import { MonthInput } from '../components/MonthInput'
import { obterMesAno } from '../utils/periodo'
import './FichasPage.css'

const TAMANHO_PAGINA = 10

type TipoPeriodo = 'todos' | 'dia' | 'mes' | 'ano'

function formatarData(data: string) {
  const [ano, mes, dia] = data.split('-').map(Number)
  return new Intl.DateTimeFormat('pt-BR').format(
    new Date(ano, mes - 1, dia),
  )
}

function formatarMoeda(valor: number) {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  }).format(valor)
}

function formatarFormaPagamento(forma: FormaPagamento) {
  const rotulos: Record<FormaPagamento, string> = {
    Dinheiro: 'Dinheiro',
    Pix: 'Pix',
    CartaoDebito: 'Cartão de débito',
    CartaoCredito: 'Cartão de crédito',
    Transferencia: 'Transferência',
    Outro: 'Outro',
  }

  return rotulos[forma]
}

function formatarProcedimento(tipo: TipoProcedimento) {
  if (tipo === 'Tatuagem') return 'Tatuagem'
  if (tipo === 'Piercing') return 'Piercing'
  return 'Não informado'
}

function obterLimitesDoPeriodo(tipo: TipoPeriodo, valor: string) {
  if (tipo === 'todos' || !valor) return null

  if (tipo === 'dia') {
    return { atendimentoDe: valor, atendimentoAte: valor }
  }

  if (tipo === 'mes') {
    const mesAno = obterMesAno(valor, new Date().getFullYear())
    if (!mesAno) return null

    const ano = Number(mesAno.ano)
    const mes = Number(mesAno.mes)
    const ultimoDia = new Date(ano, mes, 0).getDate()
    const valorIso = `${mesAno.ano}-${mesAno.mes}`
    return {
      atendimentoDe: `${valorIso}-01`,
      atendimentoAte: `${valorIso}-${String(ultimoDia).padStart(2, '0')}`,
    }
  }

  return {
    atendimentoDe: `${valor}-01-01`,
    atendimentoAte: `${valor}-12-31`,
  }
}

function obterRotuloDoValor(tipo: TipoPeriodo) {
  if (tipo === 'dia') return 'Escolha o dia'
  if (tipo === 'mes') return 'Escolha o mês'
  return 'Informe o ano'
}

export function FichasPage() {
  const [pagina, setPagina] = useState(1)
  const [resultado, setResultado] = useState<FichasPaginadas | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [erro, setErro] = useState<string | null>(null)
  const [tipoPeriodo, setTipoPeriodo] = useState<TipoPeriodo>('todos')
  const [valorPeriodo, setValorPeriodo] = useState('')
  const [periodoAtivo, setPeriodoAtivo] = useState(false)
  const [filtros, setFiltros] = useState<FiltrosFichas>({
    status: 'Concluida',
  })

  useEffect(() => {
    const abortController = new AbortController()

    setCarregando(true)
    setErro(null)

    listarFichas(pagina, TAMANHO_PAGINA, filtros, abortController.signal)
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
            : 'Não foi possível carregar o histórico de fichas.',
        )
      })
      .finally(() => {
        if (!abortController.signal.aborted) setCarregando(false)
      })

    return () => abortController.abort()
  }, [pagina, filtros])

  function aplicarPeriodo(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const limites = obterLimitesDoPeriodo(tipoPeriodo, valorPeriodo)

    setPagina(1)
    setPeriodoAtivo(limites !== null)
    setFiltros({
      status: 'Concluida',
      ...limites,
    })
  }

  function limparPeriodo() {
    setTipoPeriodo('todos')
    setValorPeriodo('')
    setPagina(1)
    setPeriodoAtivo(false)
    setFiltros({ status: 'Concluida' })
  }

  const totalPaginasExibido = Math.max(resultado?.totalPaginas ?? 1, 1)
  return (
    <main className="records-shell">
      <header className="records-header">
        <div>
          <a
            className="records-back-link desktop-section-navigation"
            href="/profissional"
          >
            ← Voltar ao painel
          </a>
          <p className="eyebrow">Atendimentos realizados</p>
          <h1>Histórico de fichas</h1>
          <p>
            Consulte os procedimentos e valores registrados por dia, mês ou
            ano. Para encontrar uma pessoa ou iniciar um novo atendimento, use
            a área de clientes.
          </p>
        </div>

        <a
          className="records-primary-link desktop-section-navigation"
          href="/profissional/clientes"
        >
          Ir para clientes
        </a>
      </header>

      <form className="records-period-filter" onSubmit={aplicarPeriodo}>
        <div className="records-period-filter__intro">
          <p className="eyebrow">Filtrar por data</p>
          <h2>Quando o procedimento foi realizado?</h2>
          <p>
            O período usa a data do procedimento. Quando o valor ainda não foi
            registrado, usa a data em que o cliente concluiu a ficha.
          </p>
        </div>

        <div className="records-period-filter__fields">
          <label>
            Período
            <select
              value={tipoPeriodo}
              onChange={(event) => {
                setTipoPeriodo(event.target.value as TipoPeriodo)
                setValorPeriodo('')
              }}
            >
              <option value="todos">Todos os períodos</option>
              <option value="dia">Dia</option>
              <option value="mes">Mês</option>
              <option value="ano">Ano</option>
            </select>
          </label>

          {tipoPeriodo !== 'todos' && (
            <label>
              {obterRotuloDoValor(tipoPeriodo)}
              {tipoPeriodo === 'dia' ? (
                <CalendarInput
                  type="date"
                  value={valorPeriodo}
                  required
                  onChange={(event) => setValorPeriodo(event.target.value)}
                />
              ) : tipoPeriodo === 'mes' ? (
                <MonthInput
                  value={valorPeriodo}
                  required
                  onValueChange={setValorPeriodo}
                />
              ) : (
                <input
                  type="number"
                  value={valorPeriodo}
                  min="2000"
                  max={String(new Date().getFullYear())}
                  placeholder="Ex.: 2026"
                  required
                  onChange={(event) => setValorPeriodo(event.target.value)}
                />
              )}
            </label>
          )}

          <div className="records-period-filter__actions">
            <button type="submit" disabled={carregando}>
              Consultar
            </button>
            {periodoAtivo && (
              <button
                className="secondary-button"
                type="button"
                disabled={carregando}
                onClick={limparPeriodo}
              >
                Limpar
              </button>
            )}
          </div>
        </div>
      </form>

      {resultado && !erro && (
        <section
          className="records-financial-summary"
          aria-label="Resumo financeiro do período"
        >
          <article>
            <span>Recebido</span>
            <strong>
              {formatarMoeda(resultado.resumoFinanceiro.totalRecebido)}
            </strong>
            <small>Atendimentos com valor registrado</small>
          </article>
          <article>
            <span>A registrar</span>
            <strong>{resultado.resumoFinanceiro.fichasSemRegistro}</strong>
            <small>Fichas concluídas sem valor informado</small>
          </article>
        </section>
      )}

      <section className="records-content" aria-live="polite">
        {carregando && !resultado && (
          <div className="records-state">
            <span className="records-loading-indicator" aria-hidden="true" />
            Carregando histórico...
          </div>
        )}

        {erro && (
          <div className="records-state records-state--error" role="alert">
            <p>{erro}</p>
            <button type="button" onClick={() => window.location.reload()}>
              Tentar novamente
            </button>
          </div>
        )}

        {!erro && resultado && resultado.itens.length === 0 && (
          <div className="records-state records-state--empty">
            <p className="eyebrow">Nenhum resultado</p>
            <h2>
              {periodoAtivo
                ? 'Nenhum atendimento foi registrado nesse período.'
                : 'Nenhum atendimento foi concluído ainda.'}
            </h2>
            {periodoAtivo ? (
              <button
                className="secondary-button"
                type="button"
                onClick={limparPeriodo}
              >
                Ver todos os períodos
              </button>
            ) : (
              <a className="records-primary-link" href="/profissional/clientes">
                Ir para clientes
              </a>
            )}
          </div>
        )}

        {!erro && resultado && resultado.itens.length > 0 && (
          <>
            <div className="records-summary">
              <strong>{resultado.totalItens}</strong>{' '}
              {resultado.totalItens === 1
                ? 'atendimento concluído'
                : 'atendimentos concluídos'}
              {carregando && <span>Atualizando...</span>}
            </div>

            <div className="records-table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Cliente</th>
                    <th>Procedimento</th>
                    <th>Profissional</th>
                    <th>Data do atendimento</th>
                    <th>Valor final</th>
                    <th>Pagamento</th>
                    <th>Ações</th>
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
                      <td data-label="Data do atendimento">
                        {ficha.atendimento
                          ? formatarData(ficha.atendimento.dataRealizacao)
                          : ficha.concluidaEmUtc
                            ? new Intl.DateTimeFormat('pt-BR').format(
                                new Date(ficha.concluidaEmUtc),
                              )
                            : 'A registrar'}
                      </td>
                      <td data-label="Valor final">
                        {ficha.atendimento
                          ? formatarMoeda(ficha.atendimento.valorFinal)
                          : 'A registrar'}
                      </td>
                      <td data-label="Pagamento">
                        {ficha.atendimento ? (
                          <span
                            className="payment-status payment-status--pago"
                          >
                            Pago ·{' '}
                            {formatarFormaPagamento(
                              ficha.atendimento.formaPagamento,
                            )}
                          </span>
                        ) : (
                          '—'
                        )}
                      </td>
                      <td data-label="Ações">
                        <a
                          className="record-detail-link"
                          href={`/profissional/fichas/${ficha.id}`}
                        >
                          {ficha.atendimento
                            ? 'Ver detalhes'
                            : 'Registrar valor'}
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
                disabled={pagina === 1 || carregando}
                onClick={() => setPagina((atual) => atual - 1)}
              >
                Anterior
              </button>
              <span>
                Página {resultado.pagina} de {totalPaginasExibido}
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
        )}
      </section>
    </main>
  )
}
