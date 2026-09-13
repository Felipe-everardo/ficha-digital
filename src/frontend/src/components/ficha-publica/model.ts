import type { PreencherDadosPessoaisInput } from '../../services/api'

export type RespostasQuestionario = {
  temDiabetes: boolean | null
  tipoDiabetes: string
  teveAnemia: boolean | null
  descricaoAnemia: string
  teveHepatite: boolean | null
  tipoHepatite: string
  possuiPressaoAlta: boolean | null
  temAlergia: boolean | null
  descricaoAlergia: string
  possuiCondicaoCardiaca: boolean | null
  temEpilepsia: boolean | null
  temHemofilia: boolean | null
  possuiDoencaTransmissivel: boolean | null
  descricaoDoencaTransmissivel: string
  usaMarcaPasso: boolean | null
  fuma: boolean | null
  consumiuBebidaAlcoolicaUltimas24Horas: boolean | null
  usaMedicacao: boolean | null
  descricaoMedicacao: string
  estaGravidaOuAmamentando: boolean | null
}

export type DadosPessoaisFormulario = PreencherDadosPessoaisInput
