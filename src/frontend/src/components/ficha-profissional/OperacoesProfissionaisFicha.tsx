import { useState, type FormEvent } from 'react'
import {
  ApiRequestError,
  concluirProcedimento,
  revisarFicha,
  type FichaDetalhe,
} from '../../services/api'
import { obterAntiforgeryToken } from '../../services/autenticacao'
import { CampoAssinatura } from '../ficha-publica/CampoAssinatura'

type OperacoesProfissionaisFichaProps = {
  ficha: FichaDetalhe
  aoAtualizarStatus: (status: string) => void
}

type MensagemOperacao = {
  texto: string
  statusDaFicha: string
  tipo: 'sucesso' | 'erro'
}

const formatadorMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
})

function formatarMoedaDigitada(valor: string) {
  const digitos = valor.replace(/\D/g, '').slice(0, 12)
  if (!digitos) return ''

  return formatadorMoeda.format(Number(digitos) / 100)
}

function converterMoedaParaNumero(valor: string) {
  const digitos = valor.replace(/\D/g, '')
  return digitos ? Number(digitos) / 100 : Number.NaN
}

export function OperacoesProfissionaisFicha({
  ficha,
  aoAtualizarStatus,
}: OperacoesProfissionaisFichaProps) {
  const [dadosConferidos, setDadosConferidos] = useState(false)
  const [assinaturaConclusao, setAssinaturaConclusao] = useState<string | null>(
    null,
  )
  const [executando, setExecutando] = useState(false)
  const [mensagem, setMensagem] = useState<MensagemOperacao | null>(null)

  function informar(texto: string, statusDaFicha = ficha.status) {
    setMensagem({ texto, statusDaFicha, tipo: 'erro' })
  }

  async function executar(
    operacao: (token: string) => Promise<{ status: string }>,
    mensagemSucesso: string,
  ) {
    setExecutando(true)
    setMensagem(null)

    try {
      const token = await obterAntiforgeryToken()
      const resultado = await operacao(token)
      setMensagem({
        texto: mensagemSucesso,
        statusDaFicha: resultado.status,
        tipo: 'sucesso',
      })
      aoAtualizarStatus(resultado.status)
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 401) {
        window.location.replace('/profissional/entrar')
        return
      }

      informar(
        error instanceof ApiRequestError
          ? error.message
          : 'Não foi possível concluir esta ação.',
      )
    } finally {
      setExecutando(false)
    }
  }

  function revisar() {
    if (!dadosConferidos) {
      informar('Confirme que revisou os dados completos da ficha.')
      return
    }

    return executar(
      (token) => revisarFicha(ficha.id, token),
      'Revisão profissional confirmada. O registro pós-procedimento já pode ser preenchido.',
    )
  }

  function concluir(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!assinaturaConclusao) {
      informar('Desenhe a assinatura profissional para concluir a ficha.')
      return
    }

    const dados = new FormData(event.currentTarget)
    const texto = (nome: string) => String(dados.get(nome) ?? '').trim()
    const opcional = (nome: string) => texto(nome) || null
    const ehTatuagem = ficha.tipoProcedimento === 'Tatuagem'

    return executar(
      (token) =>
        concluirProcedimento(
          ficha.id,
          {
            valorTotal: converterMoedaParaNumero(texto('valorTotal')),
            valorSinal: converterMoedaParaNumero(texto('valorSinal')),
            formaPagamento: texto('formaPagamento') as
              | 'Pix'
              | 'Dinheiro'
              | 'Cartao',
            assinaturaDesenhada: assinaturaConclusao,
            tatuagem: ehTatuagem
              ? {
                  arteEfetivamenteTatuada: texto('arte'),
                  materialUtilizado: texto('material'),
                  localTatuagem: texto('local'),
                  observacoes: opcional('observacoes'),
                }
              : null,
            piercing: ehTatuagem
              ? null
              : {
                  joiaUtilizada: texto('joia'),
                  agulhaUtilizada: texto('agulha'),
                  localPerfuracao: texto('local'),
                  observacoes: opcional('observacoes'),
                },
          },
          token,
        ),
      'Registro pós-procedimento salvo e ficha concluída.',
    )
  }

  const mensagemVisivel =
    mensagem?.statusDaFicha === ficha.status ? mensagem.texto : null

  return (
    <section className="record-operation-card record-operation-card--stacked">
      {mensagemVisivel && (
        <p
          className="record-operation-message"
          role={mensagem?.tipo === 'erro' ? 'alert' : 'status'}
        >
          {mensagemVisivel}
        </p>
      )}

      {(ficha.status === 'AnamnesePreenchida' ||
        ficha.status === 'AguardandoConsentimento') && (
        <div>
          <p className="eyebrow">Preenchimento do cliente</p>
          <h2>Aguardando consentimento e assinatura</h2>
          <p>
            O cliente pode concluir o aceite pelo próprio link, sem depender de
            uma liberação do profissional.
          </p>
        </div>
      )}

      {ficha.status === 'AutorizadaParaProcedimento' && (
        <>
          <div>
            <p className="eyebrow">Autorização do cliente concluída</p>
            <h2>Revisar e confirmar a ficha</h2>
            <p>
              Confira os dados pessoais, as respostas de saúde, o termo, a
              assinatura e a identidade do cliente antes do procedimento.
            </p>
          </div>
          <label className="record-operation-check">
            <input
              type="checkbox"
              checked={dadosConferidos}
              onChange={(event) => {
                setDadosConferidos(event.target.checked)
                setMensagem(null)
              }}
            />
            Conferi a ficha completa e a identidade do cliente.
          </label>
          <button type="button" disabled={executando} onClick={revisar}>
            Confirmar revisão profissional
          </button>
        </>
      )}

      {ficha.status === 'RevisadaPeloProfissional' && (
        <div className="record-operation-workflow">
          <div>
            <p className="eyebrow">Revisão profissional concluída</p>
            <h2>Registro pós-procedimento</h2>
            <p>
              Depois de realizar o procedimento, informe os dados técnicos e
              de pagamento para concluir a ficha.
            </p>
          </div>
          <FormularioConclusao
            ficha={ficha}
            assinatura={assinaturaConclusao}
            executando={executando}
            aoAlterarAssinatura={(assinatura) => {
              setAssinaturaConclusao(assinatura)
              setMensagem(null)
            }}
            aoEnviar={concluir}
          />
        </div>
      )}

      {ficha.status === 'Concluida' && ficha.registroProcedimento && (
        <div>
          <p className="eyebrow">Atendimento finalizado</p>
          <h2>Ficha concluída</h2>
          <p>
            O registro pós-procedimento e a assinatura profissional estão
            salvos no histórico.
          </p>
        </div>
      )}

    </section>
  )
}

function FormularioConclusao({
  ficha,
  assinatura,
  executando,
  aoAlterarAssinatura,
  aoEnviar,
}: {
  ficha: FichaDetalhe
  assinatura: string | null
  executando: boolean
  aoAlterarAssinatura: (valor: string | null) => void
  aoEnviar: (event: FormEvent<HTMLFormElement>) => void
}) {
  return (
    <form className="record-operation-form" onSubmit={aoEnviar}>
      {ficha.tipoProcedimento === 'Tatuagem' ? (
        <>
          <CampoTexto nome="arte" rotulo="Arte efetivamente tatuada" />
          <CampoTexto nome="material" rotulo="Material utilizado" />
          <CampoTexto nome="local" rotulo="Local efetivo da tatuagem" />
        </>
      ) : (
        <>
          <CampoTexto nome="joia" rotulo="Joia utilizada" />
          <CampoTexto nome="agulha" rotulo="Agulha utilizada" />
          <CampoTexto nome="local" rotulo="Local efetivo da perfuração" />
        </>
      )}
      <CampoTexto
        nome="observacoes"
        rotulo="Observações"
        obrigatorio={false}
        multilinha
      />
      <CampoMoeda nome="valorTotal" rotulo="Valor total" />
      <CampoMoeda nome="valorSinal" rotulo="Valor do sinal" />
      <label>
        <span>Forma de pagamento *</span>
        <select name="formaPagamento" required defaultValue="">
          <option value="" disabled>Selecione</option>
          <option value="Pix">Pix</option>
          <option value="Dinheiro">Dinheiro</option>
          <option value="Cartao">Cartão</option>
        </select>
      </label>
      <p className="record-operation-form__full">
        Profissional responsável: <strong>{ficha.profissionalResponsavelNome}</strong>
      </p>
      <div className="record-operation-form__full">
        <CampoAssinatura valor={assinatura} aoAlterar={aoAlterarAssinatura} />
      </div>
      <button type="submit" disabled={executando}>
        Salvar registro e concluir ficha
      </button>
    </form>
  )
}

function CampoMoeda({ nome, rotulo }: { nome: string; rotulo: string }) {
  const [valor, setValor] = useState('')

  return (
    <label>
      <span>{rotulo} *</span>
      <input
        name={nome}
        type="text"
        inputMode="numeric"
        autoComplete="off"
        required
        placeholder="R$ 0,00"
        value={valor}
        onChange={(event) => setValor(formatarMoedaDigitada(event.target.value))}
      />
    </label>
  )
}

function CampoTexto({
  nome,
  rotulo,
  obrigatorio = true,
  multilinha = false,
}: {
  nome: string
  rotulo: string
  obrigatorio?: boolean
  multilinha?: boolean
}) {
  return (
    <label>
      <span>{rotulo}{obrigatorio ? ' *' : ''}</span>
      {multilinha ? (
        <textarea name={nome} required={obrigatorio} maxLength={2000} />
      ) : (
        <input
          name={nome}
          type="text"
          required={obrigatorio}
        />
      )}
    </label>
  )
}
