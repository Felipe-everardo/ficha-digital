import type { FormEventHandler } from 'react'
import type { RespostasQuestionario } from './model'

type PerguntaSimNaoProps = {
  nome: string
  pergunta: string
  valor: boolean | null
  aoResponder: (valor: boolean) => void
}

type FormularioQuestionarioSaudeProps = {
  respostas: RespostasQuestionario
  erro: string | null
  enviando: boolean
  aoEnviar: FormEventHandler<HTMLFormElement>
  aoAlterar: <Campo extends keyof RespostasQuestionario>(
    campo: Campo,
    valor: RespostasQuestionario[Campo],
  ) => void
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

export function FormularioQuestionarioSaude({
  respostas,
  erro,
  enviando,
  aoEnviar,
  aoAlterar,
}: FormularioQuestionarioSaudeProps) {
  return (
    <form className="health-form" onSubmit={aoEnviar}>
      <div className="section-heading">
        <p className="eyebrow">Etapa 3 de 4</p>
        <h2>Histórico de saúde</h2>
        <p>
          Todas as perguntas precisam ser respondidas. Quando você marcar
          “Sim”, poderão aparecer informações complementares.
        </p>
      </div>

      <div className="questions-list">
        <PerguntaSimNao
          nome="temDiabetes"
          pergunta="Tem diabetes?"
          valor={respostas.temDiabetes}
          aoResponder={(valor) => {
            aoAlterar('temDiabetes', valor)
            if (!valor) aoAlterar('tipoDiabetes', '')
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
                aoAlterar('tipoDiabetes', event.target.value)
              }
            />
          </label>
        )}

        <PerguntaSimNao
          nome="teveAnemia"
          pergunta="Já teve anemia?"
          valor={respostas.teveAnemia}
          aoResponder={(valor) => {
            aoAlterar('teveAnemia', valor)
            if (!valor) aoAlterar('descricaoAnemia', '')
          }}
        />

        {respostas.teveAnemia === true && (
          <label className="conditional-field">
            <span>Informe o tipo ou os detalhes sobre a anemia *</span>
            <textarea
              maxLength={300}
              required
              value={respostas.descricaoAnemia}
              onChange={(event) =>
                aoAlterar('descricaoAnemia', event.target.value)
              }
            />
          </label>
        )}

        <PerguntaSimNao
          nome="teveHepatite"
          pergunta="Já teve hepatite?"
          valor={respostas.teveHepatite}
          aoResponder={(valor) => {
            aoAlterar('teveHepatite', valor)
            if (!valor) aoAlterar('tipoHepatite', '')
          }}
        />

        {respostas.teveHepatite === true && (
          <label className="conditional-field">
            <span>Qual foi o tipo de hepatite? *</span>
            <input
              type="text"
              maxLength={100}
              required
              value={respostas.tipoHepatite}
              onChange={(event) =>
                aoAlterar('tipoHepatite', event.target.value)
              }
            />
          </label>
        )}

        <PerguntaSimNao
          nome="possuiPressaoAlta"
          pergunta="Possui pressão alta?"
          valor={respostas.possuiPressaoAlta}
          aoResponder={(valor) => aoAlterar('possuiPressaoAlta', valor)}
        />

        <PerguntaSimNao
          nome="temAlergia"
          pergunta="Tem alguma alergia?"
          valor={respostas.temAlergia}
          aoResponder={(valor) => {
            aoAlterar('temAlergia', valor)
            if (!valor) aoAlterar('descricaoAlergia', '')
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
                aoAlterar('descricaoAlergia', event.target.value)
              }
            />
          </label>
        )}

        <PerguntaSimNao
          nome="possuiCondicaoCardiaca"
          pergunta="Possui alguma condição cardíaca?"
          valor={respostas.possuiCondicaoCardiaca}
          aoResponder={(valor) => aoAlterar('possuiCondicaoCardiaca', valor)}
        />

        <PerguntaSimNao
          nome="temEpilepsia"
          pergunta="Tem epilepsia?"
          valor={respostas.temEpilepsia}
          aoResponder={(valor) => aoAlterar('temEpilepsia', valor)}
        />

        <PerguntaSimNao
          nome="temHemofilia"
          pergunta="Tem hemofilia?"
          valor={respostas.temHemofilia}
          aoResponder={(valor) => aoAlterar('temHemofilia', valor)}
        />

        <PerguntaSimNao
          nome="possuiDoencaTransmissivel"
          pergunta="Possui alguma doença transmissível?"
          valor={respostas.possuiDoencaTransmissivel}
          aoResponder={(valor) => {
            aoAlterar('possuiDoencaTransmissivel', valor)
            if (!valor) aoAlterar('descricaoDoencaTransmissivel', '')
          }}
        />

        {respostas.possuiDoencaTransmissivel === true && (
          <label className="conditional-field">
            <span>Qual doença transmissível? *</span>
            <textarea
              maxLength={300}
              required
              value={respostas.descricaoDoencaTransmissivel}
              onChange={(event) =>
                aoAlterar(
                  'descricaoDoencaTransmissivel',
                  event.target.value,
                )
              }
            />
          </label>
        )}

        <PerguntaSimNao
          nome="usaMarcaPasso"
          pergunta="Usa marca-passo?"
          valor={respostas.usaMarcaPasso}
          aoResponder={(valor) => aoAlterar('usaMarcaPasso', valor)}
        />

        <PerguntaSimNao
          nome="fuma"
          pergunta="Fuma?"
          valor={respostas.fuma}
          aoResponder={(valor) => aoAlterar('fuma', valor)}
        />

        <PerguntaSimNao
          nome="consumiuBebidaAlcoolicaUltimas24Horas"
          pergunta="Consumiu bebida alcoólica nas últimas 24 horas?"
          valor={respostas.consumiuBebidaAlcoolicaUltimas24Horas}
          aoResponder={(valor) =>
            aoAlterar('consumiuBebidaAlcoolicaUltimas24Horas', valor)
          }
        />

        <PerguntaSimNao
          nome="usaMedicacao"
          pergunta="Faz uso de alguma medicação?"
          valor={respostas.usaMedicacao}
          aoResponder={(valor) => {
            aoAlterar('usaMedicacao', valor)
            if (!valor) aoAlterar('descricaoMedicacao', '')
          }}
        />

        {respostas.usaMedicacao === true && (
          <label className="conditional-field">
            <span>Qual medicação você utiliza? *</span>
            <textarea
              maxLength={300}
              required
              value={respostas.descricaoMedicacao}
              onChange={(event) =>
                aoAlterar('descricaoMedicacao', event.target.value)
              }
            />
          </label>
        )}

        <PerguntaSimNao
          nome="estaGravidaOuAmamentando"
          pergunta="Está grávida ou amamentando?"
          valor={respostas.estaGravidaOuAmamentando}
          aoResponder={(valor) =>
            aoAlterar('estaGravidaOuAmamentando', valor)
          }
        />
      </div>

      {erro && (
        <p className="form-error" role="alert">
          {erro}
        </p>
      )}

      <div className="questionnaire-actions">
        <p>
          Revise suas respostas. Depois de salvar, elas não poderão ser editadas
          neste fluxo.
        </p>
        <button type="submit" disabled={enviando}>
          {enviando ? 'Salvando respostas...' : 'Salvar e continuar'}
        </button>
      </div>
    </form>
  )
}
