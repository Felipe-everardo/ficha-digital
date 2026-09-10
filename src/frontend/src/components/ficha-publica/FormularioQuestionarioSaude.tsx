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
          nome="usaMarcaPasso"
          pergunta="Usa marca-passo?"
          valor={respostas.usaMarcaPasso}
          aoResponder={(valor) => aoAlterar('usaMarcaPasso', valor)}
        />

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
